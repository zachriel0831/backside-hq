
var g_cus = {
    CommonConnect: function (Url, Paramters, SuccessFunction, FailFunction) {
        var dicData = {
            url: Url,
            Paramters: Paramters,
            success: SuccessFunction,
            error: FailFunction
        };
        g_api.AjaxPost(dicData);
    }
};


var g_api = {
    AjaxPost: function (dicData) {
        'use strict';
        $.ajax({
            type: 'POST',
            url: dicData.url,
            data: (dicData.Paramters !== null) ? JSON.stringify(dicData.Paramters) : null,
            contentType: "application/json",
            // dataType: "json",
            success: dicData.success,
            failure: dicData.error,
            async: false
        });
    }
};
