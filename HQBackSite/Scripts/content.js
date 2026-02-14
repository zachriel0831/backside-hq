// ============================================
// Content 表單共用功能
// ============================================

function toggleContentFields() {
    var subtypeSelect = document.getElementById('subtype') || document.querySelector('select[name="subtype"]');
    if (!subtypeSelect) return;

    var subtype = subtypeSelect.value;
    var contentRow = document.getElementById('contentRow');
    var urlRow = document.getElementById('urlRow');

    if (!contentRow || !urlRow) return;

    if (subtype === 'd_dw') {
        contentRow.style.display = 'none';
        urlRow.style.display = 'block';
        if (typeof ckEditor !== 'undefined' && ckEditor) {
            try {
                ckEditor.destroy();
                ckEditor = null;
                if (typeof editorInitialized !== 'undefined') {
                    editorInitialized = false;
                }
            } catch (e) {}
        }
    } else {
        contentRow.style.display = 'block';
        urlRow.style.display = 'none';
        if ((typeof ckEditor === 'undefined' || !ckEditor) && subtype !== 'd_dw') {
            setTimeout(function () {
                if (typeof initCKEditor === 'function') {
                    var textareaId = document.getElementById('content') ? 'content' : (document.getElementById('descpt') ? 'descpt' : null);
                    if (textareaId) {
                        initCKEditor(textareaId);
                    }
                }
            }, 100);
        }
    }
}

function initContentFieldsToggle() {
    function setupToggle() {
        var subtypeSelect = document.getElementById('subtype') || document.querySelector('select[name="subtype"]');
        if (subtypeSelect) {
            toggleContentFields();
            subtypeSelect.addEventListener('change', toggleContentFields);
        } else {
            setTimeout(setupToggle, 100);
        }
    }
    setTimeout(setupToggle, 200);
}

function uploadFileForUrl() {
    var fileInput = document.getElementById('urlFileInput');
    if (fileInput) {
        fileInput.click();
    }
}

async function handleFileUpload(event) {
    var file = event.target.files[0];
    if (!file) return;

    var formData = new FormData();
    formData.append('file', file);

    var uploadBtn = document.getElementById('uploadUrlBtn');
    var originalText = uploadBtn.innerHTML;
    uploadBtn.disabled = true;
    uploadBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 上傳中...';

    try {
        var response = await fetch('/Menu/UploadFileForUrl', {
            method: 'POST',
            body: formData
        });
        var result = await response.json();

        if (result.code === 1 && result.data && result.data.url) {
            var urlInput = document.getElementById('url');
            if (urlInput) {
                urlInput.value = result.data.url;
            }
        } else {
            alert(result.message || '上傳失敗');
        }
    } catch (error) {
        console.error('Upload error:', error);
        alert('上傳失敗：' + error.message);
    } finally {
        uploadBtn.disabled = false;
        uploadBtn.innerHTML = originalText;
        event.target.value = '';
    }
}

function uploadImageForContent() {
    var fileInput = document.getElementById('imageFileInput');
    if (fileInput) {
        fileInput.click();
    }
}

function uploadFileForContent() {
    var fileInput = document.getElementById('contentFileInput');
    if (fileInput) {
        fileInput.click();
    }
}

async function handleImageUpload(event) {
    var file = event.target.files[0];
    if (!file) return;

    if (!file.type.match('image.*')) {
        alert('請選擇圖片檔案');
        event.target.value = '';
        return;
    }

    var formData = new FormData();
    formData.append('file', file);

    var uploadBtn = document.getElementById('uploadImageBtn');
    var originalText = uploadBtn.innerHTML;
    uploadBtn.disabled = true;
    uploadBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 上傳中...';

    try {
        var response = await fetch('/Menu/UploadFileForUrl', {
            method: 'POST',
            body: formData
        });
        var result = await response.json();

        if (result.code === 1 && result.data && result.data.url) {
            var imageUrl = result.data.url;
            if (typeof ckEditor !== 'undefined' && ckEditor) {
                var imgHtml = '<img src="' + imageUrl + '" alt="" />';
                ckEditor.insertHtml(imgHtml);
            } else {
                var textareaId = (typeof currentTextareaId !== 'undefined' && currentTextareaId) 
                    ? currentTextareaId 
                    : (document.getElementById('content') ? 'content' : (document.getElementById('descpt') ? 'descpt' : null));
                if (textareaId) {
                    var textarea = document.getElementById(textareaId);
                    if (textarea) {
                        var currentContent = textarea.value || '';
                        textarea.value = currentContent + '<img src="' + imageUrl + '" alt="" />';
                    }
                }
            }
        } else {
            alert(result.message || '上傳失敗');
        }
    } catch (error) {
        console.error('Upload error:', error);
        alert('上傳失敗：' + error.message);
    } finally {
        uploadBtn.disabled = false;
        uploadBtn.innerHTML = originalText;
        event.target.value = '';
    }
}

async function handleContentFileUpload(event) {
    var file = event.target.files[0];
    if (!file) return;

    var formData = new FormData();
    formData.append('file', file);

    var uploadBtn = document.getElementById('uploadFileBtn');
    var originalText = uploadBtn.innerHTML;
    uploadBtn.disabled = true;
    uploadBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 上傳中...';

    try {
        var response = await fetch('/Menu/UploadFileForUrl', {
            method: 'POST',
            body: formData
        });
        var result = await response.json();

        if (result.code === 1 && result.data && result.data.url) {
            var fileUrl = result.data.url;
            var fileName = file.name;
            if (typeof ckEditor !== 'undefined' && ckEditor) {
                var linkHtml = '<a href="' + fileUrl + '">' + fileName + '</a>';
                ckEditor.insertHtml(linkHtml);
            } else {
                var textareaId = (typeof currentTextareaId !== 'undefined' && currentTextareaId) 
                    ? currentTextareaId 
                    : (document.getElementById('content') ? 'content' : (document.getElementById('descpt') ? 'descpt' : null));
                if (textareaId) {
                    var textarea = document.getElementById(textareaId);
                    if (textarea) {
                        var currentContent = textarea.value || '';
                        textarea.value = currentContent + '<a href="' + fileUrl + '">' + fileName + '</a>';
                    }
                }
            }
        } else {
            alert(result.message || '上傳失敗');
        }
    } catch (error) {
        console.error('Upload error:', error);
        alert('上傳失敗：' + error.message);
    } finally {
        uploadBtn.disabled = false;
        uploadBtn.innerHTML = originalText;
        event.target.value = '';
    }
}

(function () {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initContentFieldsToggle);
    } else {
        initContentFieldsToggle();
    }
})();
