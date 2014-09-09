//NOTE: requires jQuery to be previously imported

//Helper for jQuery to see if a query returns anything
$.fn.exists = function () {
    return this.length && this.length !== 0;
};

//Actual helpers
var CSALCommon = {
    progressDialogSelector: null,
    progressDialogTextSelector: null,

    showProgress: function (txt) {
        try {
            if (!!CSALCommon.progressDialogSelector) {
                var dlg = $(CSALCommon.progressDialogSelector);
                if (dlg.exists()) {
                    if (!!CSALCommon.progressDialogTextSelector) {
                        console.log(txt);
                        $(CSALCommon.progressDialogTextSelector).text(!!txt ? txt : "Working");
                    }
                    dlg.show();
                }
            }
        }
        catch (e) {
            console.log(e);
        }
    },

    hideProgress: function () {
        try {
            if (!!CSALCommon.progressDialogSelector) {
                var dlg = $(CSALCommon.progressDialogSelector);
                if (dlg.exists()) {
                    dlg.hide();
                }
            }
        }
        catch (e) {
            console.log(e);
        }
    },

    showRows: function (tableSelector, rows) {
        var table = $(tableSelector).DataTable();
        table
            .clear()
            .rows.add(rows)
            .draw()
        ;
    },

    doServerGet: function (url, progressText, dataTarget) {
        CSALCommon.showProgress(progressText);
        $.ajax({
            type: "GET",
            url: url,
            dataType: "json"
        })
        .done(function (data, textStatus, jqXHR) {
            CSALCommon.hideProgress();
            console.log("Call done [" + url + "]: " + textStatus);
            if (textStatus != "success") {
                //OOPS - server didn't like our request
                var errMsg = textStatus + "???";
                try {
                    errMsg = data.errmsg;
                } catch (err) { }
                alert("There was an issue [" + url + "]: " + errMsg);
            }
            else {
                //SUCCESS!
                dataTarget(data);
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            CSALCommon.hideProgress();
            console.log("Call done [" + url + "]: " + textStatus + ", error:" + errorThrown);
            alert("There was an error [" + url + "] ==> " +
                "[" + textStatus + ": " + errorThrown + "]"
            );
        });
    }
};