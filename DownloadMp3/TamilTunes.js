//https://masstamilan.in/
var ll = document.getElementsByClassName('single-descriptions')[0].getElementsByTagName('a');
var mp3Data = [];
var allLinks = [];
var names = [];
mp3Data.push(['MovieName', 'MovieLink', 'Mp3Link', 'Mp3Name']);
for (var l in ll) {
    if (ll[l].href) {
        names.push(ll[l].innerHTML);
        allLinks.push(ll[l].href);
    }
}
var openedWindow = null;
var index = 0;
function openWindow() {
    if (index === allLinks.length) {
        var data = mp3Data;
        var csvContent = "data:text/csv;charset=utf-8,";
        data.forEach(function (infoArray, index) {
            dataString = infoArray.join(",");
            csvContent += index < data.length ? dataString + "\n" : dataString;

        });
        var encodedUri = encodeURI(csvContent);
        //document.open(encodedUri, '', null);
        var link = document.createElement("a");
        link.setAttribute("href", encodedUri);
        link.setAttribute("download", "mp3Info.csv");
        document.body.appendChild(link); // Required for FF

        link.click(); 
        return;
    }
    if (!openedWindow || openedWindow.closed) {
        var url = allLinks[index];
        var name = names[index];
        openedWindow = document.open(url, '', null);
        openedWindow.addEventListener('load', windowLoadFunc, false);
        console.log('Opened : ' + name + ' , with Link : ' + url);
    }
    else if (allLinks[index].indexOf("https://masstamilan.in") === -1) {
        mp3Data.push([names[index], allLinks[index], '', '']);
        console.log('Adding CSV : ' + names[index] + ' , with Link : ' + allLinks[index]);
        index++;
        setTimeout(openWindow, 100);
        return;
    }
    setTimeout(openWindow, 2000);
}
function windowLoadFunc() {
    let beforeCount = mp3Data.length;
    let ll = openedWindow.document.getElementById('tlist').getElementsByTagName('a');
    for (var l in ll) {
        if (ll[l] && ll[l].href && ll[l].href.indexOf('.mp3') > 1) {
            let movieName = names[index];
            let title = ll[l].parentNode.parentNode.children[0].children[0].children[1].innerText;
            let localData = [];
            localData.push(movieName);
            localData.push(allLinks[index]);
            localData.push(ll[l].href);
            localData.push(title.replace(' - TamilTunes.pro', '').replace(' - TamilTunes.com', ''));
            console.log('Adding CSV : ' + names[index] + ' , with Link : ' + allLinks[index] + ' MP3 Link : ' + ll[l].href + ' Title : ' + title.replace(' - TamilTunes.com', ''));
            mp3Data.push(localData);
        }
    }
    if (beforeCount === mp3Data.length) {
        console.error("Invalid data for " + names[index] + " " + allLinks[index]);
    } 
    index++;
    openedWindow.close();
    
}
openWindow();