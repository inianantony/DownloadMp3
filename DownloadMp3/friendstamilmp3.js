console.clear = null;
console.dir = null;
(function() {
    //https://friendstamilmp3.net/
    var ll = document.getElementById('a-zlist-affix').getElementsByTagName('a');
    var mp3Data = [];
    var allLinks = [];
    var names = [];
    mp3Data.push(['MovieName', 'MovieLink', 'Mp3Link', 'Mp3Name']);
    for (var l2 in ll) {
        if (ll[l2].href) {
            names.push(ll[l2].innerHTML);
            allLinks.push(ll[l2].href);
        }
    }
    var openedWindow = null;
    var index = 0;

    async function openWindow() {
        if (index === allLinks.length) {
            var data = mp3Data;
            var csvContent = "data:text/csv;charset=utf-8,";
            data.forEach(function(infoArray, index) {
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
        } else if (allLinks[index].indexOf("https://friendstamilmp3.net") === -1) {
            mp3Data.push([names[index], allLinks[index], '', '']);
            console.log('Adding CSV : ' + names[index] + ' , with Link : ' + allLinks[index]);
            index++;
            setTimeout(openWindow, 100);
            return;
        }
        setTimeout(openWindow, 2000);
    }

    function resolveAfterSomeSeconds(x) {
        return new Promise(resolve => {
            setTimeout(() => {
                    resolve(x);
                },
                2000);
        });
    }

    async function windowLoadFunc() {
        var x = await resolveAfterSomeSeconds(99990);
        let beforeCount = mp3Data.length;
        let ll = openedWindow.document.getElementById('zipForm').getElementsByTagName('a');
        for (var l1 in ll) {
            if (ll[l1] && ll[l1].href && ll[l1].href.indexOf('.mp3') > 1) {
                let movieName = names[index].replace(/^\s+|\s+$/g, '');
                let title = ll[l1].parentNode.nextElementSibling.innerText.split('.mp3')[0].replace(/^\s+|\s+$/g, '');
                let localData = [];
                localData.push(movieName);
                localData.push(allLinks[index].replace(/^\s+|\s+$/g, ''));
                localData.push(ll[l1].href);
                localData.push(title);
                console.log('Adding CSV : ' +
                    movieName +
                    ' , with Link : ' +
                    allLinks[index] +
                    ' MP3 Link : ' +
                    ll[l1].href +
                    ' Title : ' +
                    title);
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
})();

