using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace HQBackSite.Utils
{
    /// <summary>
    /// Word(.docx) 轉 HTML 的輕量轉換器。
    /// 目標是保留可讀結構（段落/標題/表格/圖片/超連結），供後台測試頁預覽使用。
    /// </summary>
    public static class WordHtmlConverter
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly Regex HeadingRegex = new Regex("^Heading([1-6])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SafeTokenRegex = new Regex("[^A-Za-z0-9_-]", RegexOptions.Compiled);
        private static readonly Regex FieldHyperlinkAnchorRegex = new Regex(
            "HYPERLINK\\s+\\\\l\\s+\"(?<anchor>[^\"]+)\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FieldHyperlinkUrlRegex = new Regex(
            "HYPERLINK\\s+(?:\"(?<url>[^\"]+)\"|(?<url>\\S+))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private enum VerticalMergeState
        {
            None = 0,
            Restart = 1,
            Continue = 2
        }

        private sealed class ConversionContext
        {
            public ZipArchive Archive { get; set; }
            public Dictionary<string, string> ImageRelationships { get; set; }
            public Dictionary<string, string> HyperlinkRelationships { get; set; }
            public string ImageOutputDirectory { get; set; }
            public string ImageUrlPrefix { get; set; }
            public bool ForcePdfHyperlinks { get; set; }
        }

        private sealed class TableCellRenderInfo
        {
            public XElement Cell { get; set; }
            public int ColSpan { get; set; } = 1;
            public int RowSpan { get; set; } = 1;
            public bool IsContinuation { get; set; }
        }

        private sealed class FieldHyperlinkState
        {
            public StringBuilder InstructionText { get; } = new StringBuilder();
            public StringBuilder ResultHtml { get; } = new StringBuilder();
            public string TargetHref { get; set; }
            public bool IsInResultSection { get; set; }
        }

        public static string ConvertDocxToHtml(string docxPath)
        {
            return ConvertDocxToHtml(docxPath, null, null, false);
        }

        public static string ConvertDocxToHtml(string docxPath, string imageOutputDirectory, string imageUrlPrefix)
        {
            return ConvertDocxToHtml(docxPath, imageOutputDirectory, imageUrlPrefix, false);
        }

        public static string ConvertDocxToHtml(string docxPath, string imageOutputDirectory, string imageUrlPrefix, bool forcePdfHyperlinks)
        {
            // A. 入參檢核：路徑必填且檔案必須存在
            if (string.IsNullOrWhiteSpace(docxPath))
            {
                throw new ArgumentException("File path is required.", nameof(docxPath));
            }

            if (!File.Exists(docxPath))
            {
                throw new FileNotFoundException("DOCX file not found.", docxPath);
            }

            XDocument documentXml;
            // B. 開啟 DOCX(Zip) 並讀取主文件 XML
            using (var archive = ZipFile.OpenRead(docxPath))
            {
                var docEntry = archive.GetEntry("word/document.xml");
                if (docEntry == null)
                {
                    throw new InvalidOperationException("Invalid DOCX: word/document.xml was not found.");
                }

                using (var stream = docEntry.Open())
                {
                    documentXml = XDocument.Load(stream);
                }

                if (!string.IsNullOrWhiteSpace(imageOutputDirectory))
                {
                    Directory.CreateDirectory(imageOutputDirectory);
                }

                // C. 建立轉換上下文：圖片對照、超連結對照、輸出設定
                var context = new ConversionContext
                {
                    Archive = archive,
                    ImageRelationships = LoadImageRelationships(archive),
                    HyperlinkRelationships = LoadHyperlinkRelationships(archive),
                    ImageOutputDirectory = imageOutputDirectory,
                    ImageUrlPrefix = NormalizeImageUrlPrefix(imageUrlPrefix),
                    ForcePdfHyperlinks = forcePdfHyperlinks
                };

                var body = documentXml.Root?.Element(W + "body");
                if (body == null)
                {
                    throw new InvalidOperationException("Invalid DOCX: body node was not found.");
                }

                // D. 建立 HTML 骨架與預設樣式
                var sb = new StringBuilder(4096);
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html lang=\"zh-Hant\">");
                sb.AppendLine("<head>");
                sb.AppendLine("  <meta charset=\"utf-8\" />");
                sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
                sb.AppendLine("  <title>Word Converted HTML</title>");
                sb.AppendLine("  <style>");
                sb.AppendLine("    body { font-family: \"Segoe UI\", \"Microsoft JhengHei\", sans-serif; line-height: 1.7; margin: 24px; color: #222; }");
                sb.AppendLine("    h1,h2,h3,h4,h5,h6 { margin: 1.1em 0 0.6em; }");
                sb.AppendLine("    p { margin: 0 0 1em; }");
                sb.AppendLine("    table { border-collapse: collapse; margin: 1em 0; width: 100%; }");
                sb.AppendLine("    td, th { border: 1px solid #ccc; padding: 8px; vertical-align: top; }");
                sb.AppendLine("    img { max-width: 100%; height: auto; }");
                sb.AppendLine("  </style>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");

                // E. 逐區塊輸出：僅處理段落與表格
                foreach (var block in body.Elements())
                {
                    if (block.Name == W + "p")
                    {
                        AppendParagraph(sb, block, context);
                    }
                    else if (block.Name == W + "tbl")
                    {
                        AppendTable(sb, block, context);
                    }
                }

                sb.AppendLine("</body>");
                sb.AppendLine("</html>");

                return sb.ToString();
            }
        }

        private static void AppendParagraph(StringBuilder sb, XElement paragraph, ConversionContext context)
        {
            var tag = GetParagraphTag(paragraph);
            var align = GetParagraphAlignment(paragraph);
            var content = BuildParagraphInnerHtml(paragraph, context);
            if (string.IsNullOrWhiteSpace(content))
            {
                content = "&nbsp;";
            }

            if (string.IsNullOrWhiteSpace(align))
            {
                sb.Append('<').Append(tag).Append('>').Append(content).Append("</").Append(tag).AppendLine(">");
            }
            else
            {
                sb.Append('<').Append(tag).Append(" style=\"text-align: ").Append(align).Append(";\">")
                  .Append(content).Append("</").Append(tag).AppendLine(">");
            }
        }

        private static void AppendTable(StringBuilder sb, XElement table, ConversionContext context)
        {
            sb.AppendLine("<table>");

            var tableRows = BuildTableRenderRows(table);
            foreach (var row in tableRows)
            {
                sb.AppendLine("  <tr>");
                foreach (var cell in row)
                {
                    if (cell.IsContinuation)
                    {
                        continue;
                    }

                    sb.Append("    <td");
                    if (cell.ColSpan > 1)
                    {
                        sb.Append(" colspan=\"").Append(cell.ColSpan).Append("\"");
                    }
                    if (cell.RowSpan > 1)
                    {
                        sb.Append(" rowspan=\"").Append(cell.RowSpan).Append("\"");
                    }

                    sb.Append(">");
                    sb.Append(BuildTableCellInnerHtml(cell.Cell, context));
                    sb.AppendLine("</td>");
                }

                sb.AppendLine("  </tr>");
            }

            sb.AppendLine("</table>");
        }

        private static string GetParagraphTag(XElement paragraph)
        {
            var pStyleVal = (string)paragraph
                .Element(W + "pPr")?
                .Element(W + "pStyle")?
                .Attribute(W + "val");

            if (string.IsNullOrWhiteSpace(pStyleVal))
            {
                return "p";
            }

            var match = HeadingRegex.Match(pStyleVal);
            if (!match.Success)
            {
                return "p";
            }

            return "h" + match.Groups[1].Value;
        }

        private static string GetParagraphAlignment(XElement paragraph)
        {
            var alignment = (string)paragraph
                .Element(W + "pPr")?
                .Element(W + "jc")?
                .Attribute(W + "val");

            switch (alignment)
            {
                case "center":
                    return "center";
                case "right":
                    return "right";
                case "both":
                    return "justify";
                default:
                    return null;
            }
        }

        private static string BuildRunInnerHtml(XElement run, ConversionContext context)
        {
            var sb = new StringBuilder();

            foreach (var node in run.Elements())
            {
                if (node.Name == W + "t")
                {
                    sb.Append(HttpUtility.HtmlEncode(node.Value));
                }
                else if (node.Name == W + "tab")
                {
                    sb.Append("&emsp;");
                }
                else if (node.Name == W + "br")
                {
                    sb.Append("<br/>");
                }
                else if (node.Name == W + "drawing")
                {
                    sb.Append(BuildDrawingInnerHtml(node, context));
                }
                else if (node.Name.LocalName == "pict")
                {
                    sb.Append(BuildLegacyPictInnerHtml(node, context));
                }
            }

            return sb.ToString();
        }

        private static string BuildParagraphInnerHtml(XElement paragraph, ConversionContext context)
        {
            // 段落內採狀態機處理：同時支援一般 run、w:hyperlink 與 field code hyperlink
            var output = new StringBuilder();
            FieldHyperlinkState fieldState = null;

            foreach (var node in paragraph.Nodes())
            {
                if (!(node is XElement element))
                {
                    continue;
                }

                if (element.Name == W + "hyperlink")
                {
                    FlushFieldHyperlinkState(output, context, ref fieldState);
                    output.Append(BuildHyperlinkInnerHtml(element, context));
                    continue;
                }

                if (element.Name == W + "fldSimple")
                {
                    FlushFieldHyperlinkState(output, context, ref fieldState);
                    output.Append(BuildSimpleFieldHyperlinkHtml(element, context));
                    continue;
                }

                if (element.Name == W + "r")
                {
                    if (TryHandleFieldHyperlinkControlRun(element, context, output, ref fieldState))
                    {
                        continue;
                    }

                    var runHtml = BuildFormattedRunHtml(element, context);
                    if (string.IsNullOrEmpty(runHtml))
                    {
                        continue;
                    }

                    if (fieldState != null && fieldState.IsInResultSection)
                    {
                        fieldState.ResultHtml.Append(runHtml);
                    }
                    else
                    {
                        output.Append(runHtml);
                    }

                    continue;
                }

                var nestedHtml = BuildInlineNodesHtml(element.Nodes(), context);
                if (string.IsNullOrEmpty(nestedHtml))
                {
                    continue;
                }

                if (fieldState != null && fieldState.IsInResultSection)
                {
                    fieldState.ResultHtml.Append(nestedHtml);
                }
                else
                {
                    output.Append(nestedHtml);
                }
            }

            FlushFieldHyperlinkState(output, context, ref fieldState);
            return output.ToString();
        }

        private static string BuildInlineNodesHtml(IEnumerable<XNode> nodes, ConversionContext context)
        {
            var sb = new StringBuilder();

            foreach (var node in nodes)
            {
                if (!(node is XElement element))
                {
                    continue;
                }

                if (element.Name == W + "r")
                {
                    sb.Append(BuildFormattedRunHtml(element, context));
                }
                else if (element.Name == W + "hyperlink")
                {
                    sb.Append(BuildHyperlinkInnerHtml(element, context));
                }
                else
                {
                    sb.Append(BuildInlineNodesHtml(element.Nodes(), context));
                }
            }

            return sb.ToString();
        }

        private static string BuildFormattedRunHtml(XElement run, ConversionContext context)
        {
            var runProperties = run.Element(W + "rPr");
            var runContent = BuildRunInnerHtml(run, context);
            if (string.IsNullOrEmpty(runContent))
            {
                return string.Empty;
            }

            if (IsOn(runProperties?.Element(W + "b")))
            {
                runContent = $"<strong>{runContent}</strong>";
            }

            if (IsOn(runProperties?.Element(W + "i")))
            {
                runContent = $"<em>{runContent}</em>";
            }

            if (IsOn(runProperties?.Element(W + "u")))
            {
                runContent = $"<u>{runContent}</u>";
            }

            var backgroundColor = GetRunBackgroundColor(runProperties);
            if (!string.IsNullOrWhiteSpace(backgroundColor))
            {
                runContent = $"<span style=\"background-color: {backgroundColor};\">{runContent}</span>";
            }

            return runContent;
        }

        private static string BuildHyperlinkInnerHtml(XElement hyperlinkElement, ConversionContext context)
        {
            // 處理標準 w:hyperlink 節點
            var linkTextHtml = BuildInlineNodesHtml(hyperlinkElement.Nodes(), context);
            if (string.IsNullOrWhiteSpace(linkTextHtml))
            {
                return string.Empty;
            }

            var href = ResolveHyperlinkHref(hyperlinkElement, context);
            if (string.IsNullOrWhiteSpace(href))
            {
                return linkTextHtml;
            }

            return BuildAnchorHtml(href, linkTextHtml, context);
        }

        private static bool TryHandleFieldHyperlinkControlRun(
            XElement run,
            ConversionContext context,
            StringBuilder output,
            ref FieldHyperlinkState fieldState)
        {
            // 處理 HYPERLINK field code：begin -> instrText -> separate -> result -> end
            var fldChar = run.Element(W + "fldChar");
            if (fldChar != null)
            {
                var type = ((string)fldChar.Attribute(W + "fldCharType") ?? string.Empty).Trim();
                if (string.Equals(type, "begin", StringComparison.OrdinalIgnoreCase))
                {
                    FlushFieldHyperlinkState(output, context, ref fieldState);
                    fieldState = new FieldHyperlinkState();
                    return true;
                }

                if (string.Equals(type, "separate", StringComparison.OrdinalIgnoreCase))
                {
                    if (fieldState != null)
                    {
                        fieldState.IsInResultSection = true;
                    }

                    return true;
                }

                if (string.Equals(type, "end", StringComparison.OrdinalIgnoreCase))
                {
                    FlushFieldHyperlinkState(output, context, ref fieldState);
                    return true;
                }
            }

            if (fieldState == null || fieldState.IsInResultSection)
            {
                return false;
            }

            var instrValues = run.Elements(W + "instrText")
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (instrValues.Count == 0)
            {
                return false;
            }

            foreach (var value in instrValues)
            {
                fieldState.InstructionText.Append(value);
            }

            if (string.IsNullOrWhiteSpace(fieldState.TargetHref)
                && TryExtractFieldHyperlinkTarget(fieldState.InstructionText.ToString(), out var href))
            {
                fieldState.TargetHref = href;
            }

            return true;
        }

        private static void FlushFieldHyperlinkState(StringBuilder output, ConversionContext context, ref FieldHyperlinkState fieldState)
        {
            if (fieldState == null)
            {
                return;
            }

            var resultHtml = fieldState.ResultHtml.ToString();
            if (!string.IsNullOrWhiteSpace(resultHtml))
            {
                if (!string.IsNullOrWhiteSpace(fieldState.TargetHref))
                {
                    output.Append(BuildAnchorHtml(fieldState.TargetHref, resultHtml, context));
                }
                else
                {
                    output.Append(resultHtml);
                }
            }

            fieldState = null;
        }

        private static string BuildSimpleFieldHyperlinkHtml(XElement fieldSimple, ConversionContext context)
        {
            // 處理 w:fldSimple 的超連結格式
            var content = BuildInlineNodesHtml(fieldSimple.Nodes(), context);
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var instruction = (string)fieldSimple.Attribute(W + "instr");
            if (!TryExtractFieldHyperlinkTarget(instruction, out var href))
            {
                return content;
            }

            return BuildAnchorHtml(href, content, context);
        }

        private static bool TryExtractFieldHyperlinkTarget(string instruction, out string href)
        {
            href = null;
            if (string.IsNullOrWhiteSpace(instruction))
            {
                return false;
            }

            var normalized = Regex.Replace(instruction, "\\s+", " ").Trim();
            var anchorMatch = FieldHyperlinkAnchorRegex.Match(normalized);
            if (anchorMatch.Success)
            {
                var anchor = anchorMatch.Groups["anchor"].Value.Trim();
                if (!string.IsNullOrWhiteSpace(anchor))
                {
                    href = "#" + anchor;
                    return true;
                }
            }

            var urlMatch = FieldHyperlinkUrlRegex.Match(normalized);
            if (!urlMatch.Success)
            {
                return false;
            }

            var target = urlMatch.Groups["url"].Value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            href = target;
            return true;
        }

        private static string BuildAnchorHtml(string href, string innerHtml, ConversionContext context)
        {
            // 統一輸出 <a>，並依設定決定是否套用 PDF 導向改寫
            if (string.IsNullOrWhiteSpace(innerHtml))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(href))
            {
                return innerHtml;
            }

            var finalHref = href;
            if (context != null && context.ForcePdfHyperlinks)
            {
                finalHref = ConvertLinkToPdfUrl(finalHref);
            }

            return "<a href=\"" + HttpUtility.HtmlAttributeEncode(finalHref) + "\" target=\"_blank\" rel=\"noopener\">" + innerHtml + "</a>";
        }

        private static string ResolveHyperlinkHref(XElement hyperlinkElement, ConversionContext context)
        {
            var relationshipId = (string)hyperlinkElement.Attribute(R + "id");
            var anchor = (string)hyperlinkElement.Attribute(W + "anchor");

            if (!string.IsNullOrWhiteSpace(relationshipId)
                && context?.HyperlinkRelationships != null
                && context.HyperlinkRelationships.TryGetValue(relationshipId, out var relationshipTarget)
                && !string.IsNullOrWhiteSpace(relationshipTarget))
            {
                return relationshipTarget;
            }

            if (!string.IsNullOrWhiteSpace(anchor))
            {
                return "#" + anchor;
            }

            return null;
        }

        private static string GetRunBackgroundColor(XElement runProperties)
        {
            if (runProperties == null)
            {
                return null;
            }

            var highlight = (string)runProperties.Element(W + "highlight")?.Attribute(W + "val");
            var fromHighlight = MapWordColorNameToCss(highlight);
            if (!string.IsNullOrWhiteSpace(fromHighlight))
            {
                return fromHighlight;
            }

            // Fallback: shading fill (hex in many Word files)
            var shadingFill = (string)runProperties.Element(W + "shd")?.Attribute(W + "fill");
            if (string.IsNullOrWhiteSpace(shadingFill))
            {
                return null;
            }

            if (string.Equals(shadingFill, "auto", StringComparison.OrdinalIgnoreCase)
                || string.Equals(shadingFill, "none", StringComparison.OrdinalIgnoreCase)
                || string.Equals(shadingFill, "FFFFFF", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (Regex.IsMatch(shadingFill, "^[0-9A-Fa-f]{6}$"))
            {
                return "#" + shadingFill.ToLowerInvariant();
            }

            return null;
        }

        private static string MapWordColorNameToCss(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "yellow":
                    return "#ffff00";
                case "green":
                    return "#00ff00";
                case "cyan":
                    return "#00ffff";
                case "magenta":
                    return "#ff00ff";
                case "blue":
                    return "#0000ff";
                case "red":
                    return "#ff0000";
                case "darkblue":
                    return "#00008b";
                case "darkred":
                    return "#8b0000";
                case "darkgreen":
                    return "#006400";
                case "darkcyan":
                    return "#008b8b";
                case "darkmagenta":
                    return "#8b008b";
                case "darkyellow":
                    return "#b8860b";
                case "lightgray":
                    return "#d3d3d3";
                case "darkgray":
                    return "#a9a9a9";
                case "black":
                    return "#000000";
                case "white":
                case "none":
                case "auto":
                    return null;
                default:
                    return null;
            }
        }

        private static string ConvertLinkToPdfUrl(string href)
        {
            // 只改寫一般可導航網址；錨點與 mailto/tel/javascript 保持原樣
            if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#", StringComparison.Ordinal))
            {
                return href;
            }

            if (href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return href;
            }

            return ConvertPathToPdfTarget(href);
        }

        private static string ConvertPathToPdfTarget(string url)
        {
            var fragmentIndex = url.IndexOf('#');
            var fragment = fragmentIndex >= 0 ? url.Substring(fragmentIndex) : string.Empty;
            var withoutFragment = fragmentIndex >= 0 ? url.Substring(0, fragmentIndex) : url;

            var queryIndex = withoutFragment.IndexOf('?');
            var query = queryIndex >= 0 ? withoutFragment.Substring(queryIndex) : string.Empty;
            var path = queryIndex >= 0 ? withoutFragment.Substring(0, queryIndex) : withoutFragment;

            if (string.IsNullOrWhiteSpace(path) || path.EndsWith("/", StringComparison.Ordinal))
            {
                return url;
            }

            if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            var lastSlash = path.LastIndexOf('/');
            var segment = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
            var dotIndex = segment.LastIndexOf('.');

            if (dotIndex > 0)
            {
                path = path.Substring(0, path.Length - (segment.Length - dotIndex)) + ".pdf";
            }
            else
            {
                path += ".pdf";
            }

            return path + query + fragment;
        }

        private static string BuildDrawingInnerHtml(XElement drawingNode, ConversionContext context)
        {
            var sb = new StringBuilder();
            var blips = drawingNode.Descendants().Where(x => x.Name.LocalName == "blip");
            foreach (var blip in blips)
            {
                var relId = (string)blip.Attribute(R + "embed") ?? (string)blip.Attribute(R + "link");
                var imageTag = CreateImageTagFromRelation(relId, context);
                if (!string.IsNullOrWhiteSpace(imageTag))
                {
                    sb.Append(imageTag);
                }
            }

            return sb.ToString();
        }

        private static string BuildLegacyPictInnerHtml(XElement pictNode, ConversionContext context)
        {
            var sb = new StringBuilder();
            var imageDataNodes = pictNode.Descendants().Where(x => x.Name.LocalName == "imagedata");
            foreach (var imageData in imageDataNodes)
            {
                var relId = (string)imageData.Attribute(R + "id");
                var imageTag = CreateImageTagFromRelation(relId, context);
                if (!string.IsNullOrWhiteSpace(imageTag))
                {
                    sb.Append(imageTag);
                }
            }

            return sb.ToString();
        }

        private static string CreateImageTagFromRelation(string relId, ConversionContext context)
        {
            // 依 relationship id 找到圖片實體，輸出為檔案 URL 或 data URI
            if (string.IsNullOrWhiteSpace(relId) || context == null || context.ImageRelationships == null)
            {
                return string.Empty;
            }

            if (!context.ImageRelationships.TryGetValue(relId, out var targetPath) || string.IsNullOrWhiteSpace(targetPath))
            {
                return string.Empty;
            }

            var entry = context.Archive?.GetEntry(targetPath.Replace('\\', '/'));
            if (entry == null)
            {
                return string.Empty;
            }

            var originalName = Path.GetFileName(entry.FullName);
            var outputName = BuildOutputImageName(originalName, relId);
            if (string.IsNullOrWhiteSpace(outputName))
            {
                return string.Empty;
            }

            string imageSrc;
            if (!string.IsNullOrWhiteSpace(context.ImageOutputDirectory))
            {
                var outputPath = Path.Combine(context.ImageOutputDirectory, outputName);
                if (!File.Exists(outputPath))
                {
                    using (var source = entry.Open())
                    using (var target = File.Create(outputPath))
                    {
                        source.CopyTo(target);
                    }
                }

                imageSrc = CombineUrl(context.ImageUrlPrefix, outputName);
            }
            else
            {
                byte[] bytes;
                using (var source = entry.Open())
                using (var ms = new MemoryStream())
                {
                    source.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                var mime = GetMimeTypeByExtension(Path.GetExtension(outputName));
                imageSrc = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
            }

            return $"<img src=\"{HttpUtility.HtmlAttributeEncode(imageSrc)}\" alt=\"image\" />";
        }

        private static string BuildTableCellInnerHtml(XElement cell, ConversionContext context)
        {
            var sb = new StringBuilder();
            var hasContent = false;

            foreach (var child in cell.Elements())
            {
                if (child.Name == W + "p")
                {
                    if (hasContent)
                    {
                        sb.Append("<br/>");
                    }

                    var content = BuildParagraphInnerHtml(child, context);
                    sb.Append(string.IsNullOrWhiteSpace(content) ? "&nbsp;" : content);
                    hasContent = true;
                }
                else if (child.Name == W + "tbl")
                {
                    if (hasContent)
                    {
                        sb.Append("<div style=\"height:8px\"></div>");
                    }

                    var nested = new StringBuilder();
                    AppendTable(nested, child, context);
                    sb.Append(nested.ToString());
                    hasContent = true;
                }
            }

            return hasContent ? sb.ToString() : "&nbsp;";
        }

        private static List<List<TableCellRenderInfo>> BuildTableRenderRows(XElement table)
        {
            // 先計算每格最終 rowspan/colspan，後續再輸出 HTML 表格
            var rows = new List<List<TableCellRenderInfo>>();
            var activeVerticalAnchors = new Dictionary<int, TableCellRenderInfo>();

            foreach (var row in table.Elements(W + "tr"))
            {
                var rowCells = new List<TableCellRenderInfo>();
                var columnIndex = 0;

                foreach (var cell in row.Elements(W + "tc"))
                {
                    var info = new TableCellRenderInfo
                    {
                        Cell = cell,
                        ColSpan = GetGridSpan(cell),
                        RowSpan = 1,
                        IsContinuation = false
                    };

                    var mergeState = GetVerticalMergeState(cell);
                    if (mergeState == VerticalMergeState.Continue)
                    {
                        var touched = new HashSet<TableCellRenderInfo>();
                        var allAnchorsFound = true;

                        for (var i = 0; i < info.ColSpan; i++)
                        {
                            if (!activeVerticalAnchors.TryGetValue(columnIndex + i, out var anchor))
                            {
                                allAnchorsFound = false;
                                break;
                            }

                            if (touched.Add(anchor))
                            {
                                anchor.RowSpan++;
                            }
                        }

                        info.IsContinuation = allAnchorsFound;
                        if (!allAnchorsFound)
                        {
                            for (var i = 0; i < info.ColSpan; i++)
                            {
                                activeVerticalAnchors.Remove(columnIndex + i);
                            }
                        }
                    }
                    else if (mergeState == VerticalMergeState.Restart)
                    {
                        for (var i = 0; i < info.ColSpan; i++)
                        {
                            activeVerticalAnchors[columnIndex + i] = info;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < info.ColSpan; i++)
                        {
                            activeVerticalAnchors.Remove(columnIndex + i);
                        }
                    }

                    rowCells.Add(info);
                    columnIndex += info.ColSpan;
                }

                rows.Add(rowCells);
            }

            return rows;
        }

        private static int GetGridSpan(XElement cell)
        {
            var value = (string)cell
                .Element(W + "tcPr")?
                .Element(W + "gridSpan")?
                .Attribute(W + "val");

            if (!int.TryParse(value, out var span) || span <= 0)
            {
                return 1;
            }

            return span;
        }

        private static VerticalMergeState GetVerticalMergeState(XElement cell)
        {
            var mergeNode = cell.Element(W + "tcPr")?.Element(W + "vMerge");
            if (mergeNode == null)
            {
                return VerticalMergeState.None;
            }

            var value = (string)mergeNode.Attribute(W + "val");
            if (string.IsNullOrWhiteSpace(value))
            {
                return VerticalMergeState.Continue;
            }

            if (string.Equals(value, "restart", StringComparison.OrdinalIgnoreCase))
            {
                return VerticalMergeState.Restart;
            }

            if (string.Equals(value, "continue", StringComparison.OrdinalIgnoreCase))
            {
                return VerticalMergeState.Continue;
            }

            return VerticalMergeState.None;
        }

        private static Dictionary<string, string> LoadImageRelationships(ZipArchive archive)
        {
            // 讀取 document.xml.rels 中的圖片映射：rId -> word/media/*
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var relEntry = archive.GetEntry("word/_rels/document.xml.rels");
            if (relEntry == null)
            {
                return result;
            }

            XDocument relDoc;
            using (var stream = relEntry.Open())
            {
                relDoc = XDocument.Load(stream);
            }

            var relNs = (XNamespace)"http://schemas.openxmlformats.org/package/2006/relationships";
            foreach (var rel in relDoc.Root?.Elements(relNs + "Relationship") ?? Enumerable.Empty<XElement>())
            {
                var id = (string)rel.Attribute("Id");
                var type = (string)rel.Attribute("Type");
                var target = (string)rel.Attribute("Target");
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(target))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(type) || !type.EndsWith("/image", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result[id] = ResolveWordRelativePath(target);
            }

            return result;
        }

        private static Dictionary<string, string> LoadHyperlinkRelationships(ZipArchive archive)
        {
            // 讀取 document.xml.rels 中的超連結映射：rId -> 目標 URL
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var relEntry = archive.GetEntry("word/_rels/document.xml.rels");
            if (relEntry == null)
            {
                return result;
            }

            XDocument relDoc;
            using (var stream = relEntry.Open())
            {
                relDoc = XDocument.Load(stream);
            }

            var relNs = (XNamespace)"http://schemas.openxmlformats.org/package/2006/relationships";
            foreach (var rel in relDoc.Root?.Elements(relNs + "Relationship") ?? Enumerable.Empty<XElement>())
            {
                var id = (string)rel.Attribute("Id");
                var type = (string)rel.Attribute("Type");
                var target = (string)rel.Attribute("Target");
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(target))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(type) || !type.EndsWith("/hyperlink", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result[id] = target;
            }

            return result;
        }

        private static string ResolveWordRelativePath(string target)
        {
            var baseUri = new Uri("http://local/word/document.xml");
            var resolved = new Uri(baseUri, target.Replace('\\', '/'));
            return resolved.AbsolutePath.TrimStart('/');
        }

        private static string NormalizeImageUrlPrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Replace('\\', '/');
            if (!normalized.Contains("://") && !normalized.StartsWith("/"))
            {
                normalized = "/" + normalized;
            }

            if (!normalized.EndsWith("/"))
            {
                normalized += "/";
            }

            return normalized;
        }

        private static string BuildOutputImageName(string originalName, string relId)
        {
            var safeBase = SanitizeToken(Path.GetFileNameWithoutExtension(originalName));
            var safeRel = SanitizeToken(relId);
            var ext = Path.GetExtension(originalName)?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(safeBase))
            {
                safeBase = "image";
            }
            if (string.IsNullOrWhiteSpace(safeRel))
            {
                safeRel = "rel";
            }
            if (string.IsNullOrWhiteSpace(ext))
            {
                ext = ".bin";
            }

            return $"{safeBase}_{safeRel}{ext}";
        }

        private static string SanitizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var token = SafeTokenRegex.Replace(value, "_").Trim('_');
            return token;
        }

        private static string CombineUrl(string prefix, string fileName)
        {
            var encoded = Uri.EscapeDataString(fileName);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return encoded;
            }

            return prefix + encoded;
        }

        private static string GetMimeTypeByExtension(string extension)
        {
            switch ((extension ?? string.Empty).ToLowerInvariant())
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".svg":
                    return "image/svg+xml";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream";
            }
        }

        private static bool IsOn(XElement element)
        {
            if (element == null)
            {
                return false;
            }

            var raw = (string)element.Attribute(W + "val");
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            return !string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(raw, "off", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase);
        }
    }
}
