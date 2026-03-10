using HQBackSite.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace HQBackSite.Tests
{
    [TestClass]
    public class WordHtmlConverterTests
    {
        [TestMethod]
        public void ConvertDocxToHtml_WhenContainsImageAndMergedTable_ShouldRenderImgAndMergeAttributes()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main""
            xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""
            xmlns:wp=""http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing""
            xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main""
            xmlns:pic=""http://schemas.openxmlformats.org/drawingml/2006/picture"">
  <w:body>
    <w:p>
      <w:r><w:t>Title</w:t></w:r>
    </w:p>
    <w:p>
      <w:r>
        <w:drawing>
          <wp:inline>
            <a:graphic>
              <a:graphicData>
                <pic:pic>
                  <pic:blipFill>
                    <a:blip r:embed=""rIdImg1"" />
                  </pic:blipFill>
                </pic:pic>
              </a:graphicData>
            </a:graphic>
          </wp:inline>
        </w:drawing>
      </w:r>
    </w:p>
    <w:tbl>
      <w:tr>
        <w:tc>
          <w:tcPr>
            <w:gridSpan w:val=""2"" />
            <w:vMerge w:val=""restart"" />
          </w:tcPr>
          <w:p><w:r><w:t>A1</w:t></w:r></w:p>
        </w:tc>
        <w:tc>
          <w:p><w:r><w:t>B1</w:t></w:r></w:p>
        </w:tc>
      </w:tr>
      <w:tr>
        <w:tc>
          <w:tcPr>
            <w:gridSpan w:val=""2"" />
            <w:vMerge />
          </w:tcPr>
          <w:p><w:r><w:t>A2</w:t></w:r></w:p>
        </w:tc>
        <w:tc>
          <w:p><w:r><w:t>B2</w:t></w:r></w:p>
        </w:tc>
      </w:tr>
    </w:tbl>
  </w:body>
</w:document>";

            var relationshipsXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rIdImg1""
                Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/image""
                Target=""media/image1.png"" />
</Relationships>";

            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO2Z0y8AAAAASUVORK5CYII=");
            var docxPath = CreateDocx(root, documentXml, relationshipsXml, "word/media/image1.png", imageBytes);

            var imageOutputDir = Path.Combine(root, "images");
            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath, imageOutputDir, "/images/word-html/test/");

            StringAssert.Contains(html, "<img ");
            StringAssert.Contains(html, "/images/word-html/test/");
            StringAssert.Contains(html, "colspan=\"2\"");
            StringAssert.Contains(html, "rowspan=\"2\"");
            Assert.IsTrue(Directory.GetFiles(imageOutputDir).Any(), "Expected extracted image file.");
        }

        [TestMethod]
        public void ConvertDocxToHtml_WhenNoImageOutputPath_ShouldEmbedImageAsDataUri()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main""
            xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""
            xmlns:wp=""http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing""
            xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main""
            xmlns:pic=""http://schemas.openxmlformats.org/drawingml/2006/picture"">
  <w:body>
    <w:p>
      <w:r>
        <w:drawing>
          <wp:inline>
            <a:graphic>
              <a:graphicData>
                <pic:pic>
                  <pic:blipFill>
                    <a:blip r:embed=""rIdImg1"" />
                  </pic:blipFill>
                </pic:pic>
              </a:graphicData>
            </a:graphic>
          </wp:inline>
        </w:drawing>
      </w:r>
    </w:p>
  </w:body>
</w:document>";

            var relationshipsXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rIdImg1""
                Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/image""
                Target=""media/image1.png"" />
</Relationships>";

            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO2Z0y8AAAAASUVORK5CYII=");
            var docxPath = CreateDocx(root, documentXml, relationshipsXml, "word/media/image1.png", imageBytes);

            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath, null, null);

            StringAssert.Contains(html, "data:image/png;base64,");
        }

        [TestMethod]
        public void ConvertDocxToHtml_WhenRunHighlightYellow_ShouldRenderBackgroundColor()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r>
        <w:rPr><w:highlight w:val=""yellow"" /></w:rPr>
        <w:t>highlight text</w:t>
      </w:r>
    </w:p>
  </w:body>
</w:document>";

            var docxPath = CreateDocx(root, documentXml, null, null, null);
            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath);

            StringAssert.Contains(html, "background-color: #ffff00");
            StringAssert.Contains(html, "highlight text");
        }

        [TestMethod]
        public void ConvertDocxToHtml_WhenRunShadingFill_ShouldRenderBackgroundColor()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r>
        <w:rPr><w:shd w:val=""clear"" w:color=""auto"" w:fill=""FFF200"" /></w:rPr>
        <w:t>shading text</w:t>
      </w:r>
    </w:p>
  </w:body>
</w:document>";

            var docxPath = CreateDocx(root, documentXml, null, null, null);
            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath);

            StringAssert.Contains(html, "background-color: #fff200");
            StringAssert.Contains(html, "shading text");
        }

        [TestMethod]
        public void ConvertDocxToHtml_WhenContainsHyperlink_ShouldRenderAnchorTag()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main""
            xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <w:body>
    <w:p>
      <w:hyperlink r:id=""rIdLink1"">
        <w:r><w:t>Open Link</w:t></w:r>
      </w:hyperlink>
    </w:p>
  </w:body>
</w:document>";

            var relationshipsXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rIdLink1""
                Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink""
                Target=""https://example.com/docs/report.docx""
                TargetMode=""External"" />
</Relationships>";

            var docxPath = CreateDocx(root, documentXml, relationshipsXml, null, null);
            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath);

            StringAssert.Contains(html, "<a href=\"https://example.com/docs/report.docx\"");
            StringAssert.Contains(html, ">Open Link</a>");
        }

        [TestMethod]
        public void ConvertDocxToHtml_WhenForcePdfHyperlinks_ShouldRewriteAnchorHrefToPdf()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main""
            xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <w:body>
    <w:p>
      <w:hyperlink r:id=""rIdLink1"">
        <w:r><w:t>PDF Link</w:t></w:r>
      </w:hyperlink>
    </w:p>
  </w:body>
</w:document>";

            var relationshipsXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rIdLink1""
                Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink""
                Target=""https://example.com/docs/report.docx?download=1#section""
                TargetMode=""External"" />
</Relationships>";

            var docxPath = CreateDocx(root, documentXml, relationshipsXml, null, null);
            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath, null, null, true);

            StringAssert.Contains(html, "<a href=\"https://example.com/docs/report.pdf?download=1#section\"");
            StringAssert.Contains(html, ">PDF Link</a>");
        }

        [TestMethod]
        public void ConvertDocxToHtml_WhenHyperlinkIsFieldCode_ShouldRenderAnchorAndRewriteToPdf()
        {
            var root = Path.Combine(Path.GetTempPath(), "HQBackSite.WordHtmlConverterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r><w:fldChar w:fldCharType=""begin"" /></w:r>
      <w:r><w:instrText xml:space=""preserve""> HYPERLINK ""https://example.com/files/manual.docx"" </w:instrText></w:r>
      <w:r><w:fldChar w:fldCharType=""separate"" /></w:r>
      <w:r><w:t>manual</w:t></w:r>
      <w:r><w:fldChar w:fldCharType=""end"" /></w:r>
    </w:p>
  </w:body>
</w:document>";

            var docxPath = CreateDocx(root, documentXml, null, null, null);
            var html = WordHtmlConverter.ConvertDocxToHtml(docxPath, null, null, true);

            StringAssert.Contains(html, "<a href=\"https://example.com/files/manual.pdf\"");
            StringAssert.Contains(html, ">manual</a>");
        }

        private static string CreateDocx(string root, string documentXml, string relationshipsXml, string mediaPath, byte[] mediaBytes)
        {
            var docxPath = Path.Combine(root, Guid.NewGuid().ToString("N") + ".docx");
            using (var archive = ZipFile.Open(docxPath, ZipArchiveMode.Create))
            {
                WriteTextEntry(archive, "word/document.xml", documentXml);
                if (!string.IsNullOrWhiteSpace(relationshipsXml))
                {
                    WriteTextEntry(archive, "word/_rels/document.xml.rels", relationshipsXml);
                }

                if (!string.IsNullOrWhiteSpace(mediaPath) && mediaBytes != null && mediaBytes.Length > 0)
                {
                    var mediaEntry = archive.CreateEntry(mediaPath);
                    using (var stream = mediaEntry.Open())
                    {
                        stream.Write(mediaBytes, 0, mediaBytes.Length);
                    }
                }
            }

            return docxPath;
        }

        private static void WriteTextEntry(ZipArchive archive, string entryPath, string content)
        {
            var entry = archive.CreateEntry(entryPath);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }
    }
}
