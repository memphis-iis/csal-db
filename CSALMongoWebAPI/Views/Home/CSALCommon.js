//NOTE: requires jQuery to be previously imported

//Just in case we're on a weird platform without a console
if (!window.console) {
    window.console = { log: function () { } };
}


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

    //Helper to get always-valid, always-trimmed string
    trimmedStr: function (s) {
        var ss = "" + s;
        if (ss && ss != "undefined" && ss != "null" && ss.length && ss.length > 0) {
            return $.trim(ss);
        }
        else {
            return "";
        }
    },

    //For string s, replace all occurrences of fnd with rep - note that we use
    //a global regex, so any regex characters in fnd must be escaped (so you
    //cannot use regex's for this operation)
    replaceAll: function(s, fnd, rep) {
        fnd = ("" + fnd).replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
        fndEx = new RegExp(fnd, 'gi');
        return ("" + s).replace(fndEx, "" + rep);
    },

    safeParseInt: function (s, def) {
        var i = def;
        try {
            i = parseInt(CSALCommon.trimmedStr(s));
            if (isNaN(i))
                i = def;
        }
        catch (e) {
            i = def;
        }

        return i;
    },

    safeParseFloat: function(s, def) {
        var f = def;
        try {
            f = parseFloat(CSALCommon.trimmedStr(s));
            if (isNaN(f))
                f = def;
        }
        catch (e) {
            f = def;
        }

        return f;
    },

    arrLen: function (a) {
        return (a && a.length) ? a.length : 0;
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
    },

    lessonPathMarkup: function (lessonPath) {
        //Given the string lessonPath, return a DOM element suitable for
        //appending for display
        lessonPath = CSALCommon.trimmedStr(lessonPath);
        if (lessonPath.length < 1)
            return $("<span></span>").text(lessonPath);

        var last = "M";
        var component = $("<span></span>");

        for (var i = 0; i < lessonPath.length; ++i) {
            var c = lessonPath[i];
            if (c != last) {
                if (c == "E" || (c == "M" && last == "H")) {
                    component.append($("<span class='glyphicon glyphicon-arrow-down'></span>"))
                }
                else if (c == "H" || (c == "M" && last == "E")) {
                    component.append($("<span class='glyphicon glyphicon-arrow-up'></span>"))
                }
                else {
                    //???
                    component.append($("<span class='glyphicon glyphicon-question-sign'></span>"))
                }
            }
            component.append($("<span></span>").text(c));
        }

        return component;
    },

    correctRateMarkup: function (correctRate) {
        //Given correctRate, which is either a float or a string convertable to a float,
        //return a DOM element suitable for appending for display
        var percent = CSALCommon.safeParseFloat(correctRate, -1.0);
        if (percent < 0.0 || percent > 1.0) {
            return $("<span></span>").text(correctRate); //Just punt
        }

        var correctStyle = percent > 0.67 ? "label-success" : "label-danger";
        var disp = Math.round(percent * 100.0) + "%";

        return $("<span class='label' style='font-weight:normal;'></span>")
            .addClass(correctStyle)
            .text(disp);
    }
};