function contains(obj, dis) {
    for (var dat in obj) {
        if (obj[dat] == dis) {
            return true;
        }
    }
    return false;
}

function isInt(n) {
    return parseFloat(n) == parseInt(n, 10) && !isNaN(parseInt(n));
}

function strContains(sr, dis) {
    for (var dat = 0; dat < sr.length; dat++) {
        if(dat+dis.length-1 < sr.length){
            if (sr.substring(dat, dat+dis.length) == dis) {
                return true;
            }
        }
    }
    return false;
}

function pandaWalk() {
    if (document.getElementById("panda").conorcode != "1") {
        document.getElementById('mag').src = "/Content/pics/pandaWalk.gif";
        document.getElementById('panda').className = "";
        document.getElementById('field').className = 'pandamation';
        document.getElementById('field').setAttribute("style", "visibility: visibile");
        setTimeout(function () {
            document.getElementById('mag').src = "/Content/pics/stillPandaWalk.png";
            document.getElementById("panda").setAttribute("onclick", "execSearch()");
        }, 4000);
        document.getElementById("panda").conorcode = "1";
    }
}

function execSearch() {
    window.location.href = "https://www.google.com/#q=" + $("#field").val();
}

function activateSideNav(id) {
    $('#sideNavNews').removeClass('active');
    $('#sideNavCalendar').removeClass('active');
    $('#sideNavMessages').removeClass('active');
    $('#sideNavStocks').removeClass('active');
    $(id).addClass('active');
}

function updateChart(code, date1, date2){
    document.getElementById("datChartDoh").src = "./StockChart?code="+code+"&date1="+date1+"&date2="+date2;
}

function setNote(stock, note) {
    $.ajax({
        type: "POST",
        url: "NoteIt?code=" + stock + "&note=" + note,
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    });
    $("#datNote p").html(note);
    console.log("Noting Pandas");
}

function buy(stock, shares) {
    if (isInt(shares) && parseInt(shares) > 0) {
        $.ajax({
            type: "POST",
            url: "BuyIt?code="+stock+"&shares="+shares,
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        });
        var result = parseInt(document.getElementById("shareSet").innerHTML.split("<b> ")[1].split("<")[0]) + parseInt(shares);
        document.getElementById("shareSet").innerHTML = "<label>Shares Owned: </label><b> " + result + "</b>";
        $("#shareCount").val("");
        console.log("Buying Pandas");
    }
}

function sell(stock, shares) {
    console.log(shares);
    if (isInt(shares) && parseInt(shares)>0) {
        $.ajax({
            type: "POST",
            url: "SellIt?code=" + stock + "&shares=" + shares,
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        });
        var result = parseInt(document.getElementById("shareSet").innerHTML.split("<b> ")[1].split("<")[0]) - parseInt(shares);
        document.getElementById("shareSet").innerHTML = "<label>Shares Owned: </label><b> " + (result<0 ? 0 : result) + "</b>";
        $("#shareCount").val("");
        console.log("Selling Pandas");
    }
}

function favorite(stock, bool) {
    $.ajax({
        type: "POST",
        url: "FavoriteIt?favor="+bool+"&stock="+stock,
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    });
    $("#heart .bef").css("background", (!bool ? 'rgb(0, 0, 255)' : 'rgb(255, 0, 0)'));
    $("#heart .aft").css("background", (!bool ? 'rgb(0, 0, 255)' : 'rgb(255, 0, 0)'));
    console.log("Yay Pandas");
}