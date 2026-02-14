// ============================================
// CKEditor 統一管理模組
// ============================================

window.CKEDITOR_BASEPATH = 'https://cdn.ckeditor.com/4.22.1/full/';

var ckEditor = null;
var editorInitialized = false;
var currentTextareaId = null;

// ============================================
// 工具函數
// ============================================

function hideTextarea(textareaId) {
    if (!textareaId) return;
    var textarea = document.getElementById(textareaId);
    if (textarea && !ckEditor) {
        textarea.style.visibility = 'hidden';
        textarea.style.position = 'absolute';
        textarea.style.opacity = '0';
    }
}

function normalizeRelativePath(path) {
    if (!path) return path;
    while (path.indexOf('../') === 0) {
        path = path.replace(/^\.\.\//, '');
    }
    if (path.indexOf('./') === 0) {
        path = path.replace(/^\.\//, '');
    }
    return path;
}

// 檢查圖片是否存在，如果不存在則使用備用域名
function checkImageUrl(url) {
    return new Promise(function(resolve) {
        if (!url || /^(https?:\/\/|\/\/|data:)/i.test(url)) {
            // 如果是完整URL，檢查是否為 hq.skm.com.tw，如果是則檢查備用域名
            if (url && url.indexOf('hq.skm.com.tw') !== -1) {
                var img = new Image();
                img.onload = function() {
                    resolve(url);
                };
                img.onerror = function() {
                    var fallbackUrl = url.replace('hq.skm.com.tw', 'hq_admin.skm.com.tw:8082');
                    resolve(fallbackUrl);
                };
                img.src = url;
            } else {
                resolve(url);
            }
            return;
        }

        var normalized = normalizeRelativePath(url);
        var primaryUrl = normalized.startsWith('/')
            ? 'https://hq.skm.com.tw' + normalized
            : 'https://hq.skm.com.tw/' + normalized;
        
        var img = new Image();
        img.onload = function() {
            resolve(primaryUrl);
        };
        img.onerror = function() {
            var fallbackUrl = normalized.startsWith('/')
                ? 'https://hq_admin.skm.com.tw:8082' + normalized
                : 'https://hq_admin.skm.com.tw:8082/' + normalized;
            resolve(fallbackUrl);
        };
        img.src = primaryUrl;
    });
}

// 處理HTML內容中的連結，為相對路徑加上完整域名
function processLinks(htmlContent) {
    if (!htmlContent) return htmlContent;
    var processed = htmlContent;

    // 處理 <img> 標籤的 src 屬性
    processed = processed.replace(/<img([^>]*?)\s+src\s*=\s*["']([^"']+)["']([^>]*)>/gi, function (match, before, src, after) {
        if (!/^(https?:\/\/|\/\/|data:)/i.test(src)) {
            var normalized = normalizeRelativePath(src);
            var newSrc = normalized.startsWith('/')
                ? 'https://hq.skm.com.tw' + normalized
                : 'https://hq.skm.com.tw/' + normalized;
            return '<img' + before + ' src="' + newSrc + '"' + after + '>';
        }
        return match;
    });

    // 處理 <a> 標籤的 href 屬性
    processed = processed.replace(/<a([^>]*?)\s+href\s*=\s*["']([^"']+)["']([^>]*)>/gi, function (match, before, href, after) {
        if (!/^(https?:\/\/|\/\/|mailto:|tel:|#|javascript:)/i.test(href)) {
            var normalized = normalizeRelativePath(href);
            var newHref = normalized.startsWith('/')
                ? 'https://hq.skm.com.tw' + normalized
                : 'https://hq.skm.com.tw/' + normalized;
            return '<a' + before + ' href="' + newHref + '"' + after + '>';
        }
        return match;
    });

    return processed;
}

// 處理圖片URL，檢查並使用備用域名
async function processImageUrls(htmlContent) {
    if (!htmlContent) return htmlContent;
    
    var imgRegex = /<img([^>]*?)\s+src\s*=\s*["']([^"']+)["']([^>]*)>/gi;
    var matches = [];
    var match;
    
    while ((match = imgRegex.exec(htmlContent)) !== null) {
        matches.push({
            full: match[0],
            before: match[1],
            src: match[2],
            after: match[3],
            index: match.index
        });
    }
    
    if (matches.length === 0) return htmlContent;
    
    var promises = matches.map(function(m) {
        return checkImageUrl(m.src).then(function(checkedUrl) {
            return {
                original: m.full,
                replacement: '<img' + m.before + ' src="' + checkedUrl + '"' + m.after + '>'
            };
        });
    });
    
    var results = await Promise.all(promises);
    var processed = htmlContent;
    results.forEach(function(result) {
        processed = processed.replace(result.original, result.replacement);
    });
    
    return processed;
}

function applyTableStyles(editor) {
    try {
        var editable = editor.editable();
        if (!editable || !editable.$) return;

        var tables = editable.$.querySelectorAll('table');
        tables.forEach(function (table) {
            if (table.getAttribute('bordercolor')) {
                table.style.borderColor = table.getAttribute('bordercolor');
            }
            if (table.getAttribute('border')) {
                var border = table.getAttribute('border');
                table.style.borderWidth = border + 'px';
                table.style.borderStyle = 'solid';
            }
            if (table.getAttribute('height')) {
                var height = table.getAttribute('height');
                table.style.height = (isNaN(height) ? height : height + 'px');
            }
            if (table.getAttribute('width')) {
                var width = table.getAttribute('width');
                table.style.width = (isNaN(width) ? width : width + 'px');
            }
            if (table.getAttribute('cellspacing')) {
                table.style.borderSpacing = table.getAttribute('cellspacing') + 'px';
                table.style.borderCollapse = 'separate';
            }
            if (table.getAttribute('cellpadding')) {
                var padding = table.getAttribute('cellpadding') + 'px';
                var cells = table.querySelectorAll('td, th');
                cells.forEach(function (cell) {
                    cell.style.padding = padding;
                });
            }

            var cells = table.querySelectorAll('td, th');
            cells.forEach(function (cell) {
                if (cell.getAttribute('height')) {
                    var height = cell.getAttribute('height');
                    cell.style.height = (isNaN(height) ? height : height + 'px');
                }
                if (cell.getAttribute('width')) {
                    var width = cell.getAttribute('width');
                    cell.style.width = (isNaN(width) ? width : width + 'px');
                }
                if (cell.getAttribute('valign')) {
                    cell.style.verticalAlign = cell.getAttribute('valign');
                }
            });
        });
    } catch (e) {}
}

function hideImagePreview(dialogElement) {
    if (!dialogElement || !dialogElement.$) return;
    var dialogDom = dialogElement.$;
    
    var previewBox = dialogDom.querySelector('.ImagePreviewBox');
    if (previewBox) {
        previewBox.innerHTML = '';
        previewBox.style.display = 'none';
        previewBox.style.visibility = 'hidden';
        previewBox.style.height = '0';
        previewBox.style.overflow = 'hidden';
    }
    
    var allElements = dialogDom.querySelectorAll('*');
    allElements.forEach(function (el) {
        var text = (el.textContent || '').trim();
        if (text === '預覽' || text.indexOf('預覽') === 0) {
            var parent = el.parentElement;
            if (parent) {
                var container = parent.closest('td, div.cke_dialog_ui_vbox, div.cke_dialog_ui_hbox');
                if (container) {
                    container.style.display = 'none';
                }
            }
        }
        if (text.indexOf('Lorem ipsum') !== -1 || 
            text.indexOf('consectetuer adipiscing') !== -1 ||
            (text.length > 200 && text.indexOf('Maecenas') !== -1)) {
            el.style.display = 'none';
            el.style.visibility = 'hidden';
            el.style.height = '0';
            el.style.overflow = 'hidden';
        }
    });
    
    var htmlElements = dialogDom.querySelectorAll('.cke_dialog_ui_html, [class*="html"]');
    htmlElements.forEach(function (el) {
        el.style.display = 'none';
    });
}

// ============================================
// CKEditor 初始化
// ============================================

function initCKEditor(textareaId) {
    if (!textareaId) return;
    var contentTextarea = document.getElementById(textareaId);
    if (!contentTextarea) return;
    
    if (editorInitialized && ckEditor && currentTextareaId === textareaId) {
        return;
    }
    
    if (editorInitialized && ckEditor && currentTextareaId !== textareaId) {
        editorInitialized = false;
    }

    if (ckEditor) {
        try {
            ckEditor.destroy();
        } catch (e) {}
        ckEditor = null;
        currentTextareaId = null;
    }

    var originalContent = contentTextarea.value || '';
    if (originalContent) {
        originalContent = processLinks(originalContent);
        contentTextarea.value = originalContent;
    }

    hideTextarea(textareaId);

    try {
        ckEditor = CKEDITOR.replace(textareaId, {
            height: 350,
            language: 'zh',
            versionCheck: false,
            allowedContent: true,
            enterMode: CKEDITOR.ENTER_BR,
            shiftEnterMode: CKEDITOR.ENTER_P,
            removeEmpty: false,
            extraAllowedContent: '*{*}',
            pasteFromWordRemoveFontStyles: false,
            pasteFromWordRemoveStyles: false,
            filebrowserImageUploadUrl: '',
            filebrowserImageBrowseUrl: '',
            filebrowserUploadUrl: '',
            filebrowserBrowseUrl: '',
            linkShowAdvancedTab: false,
            linkShowTargetTab: true,
            autoHeightEnabled: false,
            toolbar: [
                ['Source', '-', 'Undo', 'Redo'],
                ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'],
                ['TextColor', 'BGColor'],
                ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent'],
                ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                ['Table', 'HorizontalRule', 'SpecialChar'],
                ['Styles', 'Format', 'Font', 'FontSize'],
                ['Maximize']
            ],
            font_names: 'Arial/Arial, Helvetica, sans-serif;Comic Sans MS/Comic Sans MS, cursive;Courier New/Courier New, Courier, monospace;Georgia/Georgia, serif;Tahoma/Tahoma, Geneva, sans-serif;Times New Roman/Times New Roman, Times, serif;Verdana/Verdana, Geneva, sans-serif;微軟正黑體/Microsoft JhengHei, 微軟正黑體, sans-serif;新細明體/PMingLiU, serif;標楷體/DFKai-SB, serif',
            fontSize_sizes: '8/8px;9/9px;10/10px;11/11px;12/12px;14/14px;16/16px;18/18px;20/20px;24/24px;36/36px;48/48px;72/72px',
            table_defaultAttributes: { border: '1' },
            startupFocus: false,
            contentsCss: 'body { font-family: Arial, "Microsoft JhengHei", "微軟正黑體", sans-serif; font-size: 14px; line-height: 1.6; } ' +
                'p { margin: 0 0 10px 0; } ' +
                'h1, h2, h3, h4, h5, h6 { margin: 10px 0; } ' +
                'table { border-collapse: separate !important; } ' +
                'table td, table th { border: 1px solid #ddd; padding: 8px; } ' +
                'img { max-width: 100%; height: auto; }',
            on: {
                instanceReady: function (ev) {
                    editorInitialized = true;
                    currentTextareaId = textareaId;
                    var editor = ev.editor;

                    hideTextarea(textareaId);

                    setTimeout(function() {
                        try {
                            var contentsElement = editor.container && editor.container.$.querySelector('.cke_contents');
                            if (contentsElement) {
                                contentsElement.style.height = '350px';
                            }
                        } catch(e) {}
                    }, 100);

                    editor.on('dialogDefinition', function (ev) {
                        var dialogName = ev.data.name;
                        var dialogDefinition = ev.data.definition;

                        if (dialogName === 'link') {
                            try {
                                dialogDefinition.removeContents('advanced');
                            } catch (e) {}
                        }

                        if (dialogName === 'image') {
                            try {
                                dialogDefinition.removeContents('Upload');
                                dialogDefinition.removeContents('advanced');
                                
                                var infoTab = dialogDefinition.getContents('info');
                                if (infoTab && infoTab.elements) {
                                    var elements = infoTab.elements;
                                    for (var i = elements.length - 1; i >= 0; i--) {
                                        var element = elements[i];
                                        if (element.type === 'html' || 
                                            element.id === 'htmlPreview' ||
                                            (element.html && element.html.indexOf('preview') !== -1)) {
                                            elements.splice(i, 1);
                                        }
                                    }
                                }
                            } catch (e) {}
                        }
                    });

                    editor.on('dialogShow', function (ev) {
                        setTimeout(function () {
                            try {
                                var dialog = ev.data;
                                var dialogElement = dialog.getElement();
                                if (dialogElement) {
                                    var inputs = dialogElement.$.querySelectorAll('input[type="text"], input[type="url"], textarea');
                                    inputs.forEach(function (input) {
                                        input.removeAttribute('disabled');
                                        input.removeAttribute('readonly');
                                    });
                                    
                                    if (dialog.getName() === 'image') {
                                        hideImagePreview(dialogElement);
                                        setTimeout(function() { hideImagePreview(dialogElement); }, 100);
                                        setTimeout(function() { hideImagePreview(dialogElement); }, 300);
                                    }
                                }
                            } catch (e) {}
                        }, 50);
                    });

                    if (originalContent) {
                        // 處理圖片URL，檢查並使用備用域名
                        processImageUrls(originalContent).then(function(processedContent) {
                            editor.setData(processedContent);
                            setTimeout(function () { applyTableStyles(editor); }, 300);
                        }).catch(function() {
                            editor.setData(originalContent);
                            setTimeout(function () { applyTableStyles(editor); }, 300);
                        });
                    }

                    editor.on('change', function () {
                        applyTableStyles(editor);
                        var content = editor.getData();
                        var processed = processLinks(content);
                        if (content !== processed) {
                            editor.setData(processed);
                            setTimeout(function () { applyTableStyles(editor); }, 100);
                        }
                        var textarea = document.getElementById(textareaId);
                        if (textarea) {
                            textarea.value = processed || content;
                        }
                    });

                    editor.on('dataReady', function () {
                        applyTableStyles(editor);
                        var data = editor.getData();
                        var processed = processLinks(data);
                        if (data !== processed) {
                            editor.setData(processed);
                            setTimeout(function () { applyTableStyles(editor); }, 100);
                        } else {
                            // 檢查圖片URL並使用備用域名
                            processImageUrls(data).then(function(processedUrls) {
                                if (data !== processedUrls) {
                                    editor.setData(processedUrls);
                                    setTimeout(function () { applyTableStyles(editor); }, 100);
                                }
                            });
                        }
                    });

                    editor.on('selectionChange', function () {
                        applyTableStyles(editor);
                    });
                }
            }
        });
    } catch (e) {
        console.error('CKEditor init error:', e);
    }
}

window.initCKEditor = initCKEditor;
