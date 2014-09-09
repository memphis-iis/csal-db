//NOTE: requires jQuery to be previously imported

var CSALCommon = {
    showRows: function (tableSelector, rows) {
        var table = $(tableSelector).DataTable();
        table
            .clear()
            .rows.add(rows)
            .draw()
        ;
    },

    doServerGet: function (url, dataTarget) {
        $.ajax({
            type: "GET",
            url: url,
            dataType: "json"
        })
        .done(function (data, textStatus, jqXHR) {
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
            console.log("Call done [" + url + "]: " + textStatus + ", error:" + errorThrown);
            alert("There was an error [" + url + "] ==> " +
                "[" + textStatus + ": " + errorThrown + "]"
            );
        });
    }
};