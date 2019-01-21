var charts = new Array(0);
var expanded = new Array(0);
var widen = new Array(0);
var chartShown = new Array(0);
var block = true;
var nCharts = 0;
var rozsah = 48;
var endrozsah = 0;
var cleanCharts = false;
var blockExpand = false;
var sliderRef;
var showAll = true;
var bounce = false;
var peopleData = null;

var targetCardWidth = 300;
var prevCardWidth = 300;

var firstSplitCardId = 4;

function chartParams(type) {
    if (type == 1) {


        var chartParamsBAR = {
            type: 'bar',

            name: "",
            easing: 'easeInBounce',
            duration: 500,
            barPercentage: 1.0,
            data: {

                datasets: [{
                    label: "my messages",

                    backgroundColor: "rgba(0, 73, 207, 0.9)",
                    borderColor: "rgba(0, 73, 207, 0.9)",

                },
                {
                    label: "partner messages",

                    backgroundColor: "rgba(200, 73, 0, 0.9)",
                    borderColor: "rgba(200, 73, 0, 0.9)",

                }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 200, // general animation time
                },
                hover: {
                    animationDuration: 400, // duration of animations when hovering an item
                },
                responsiveAnimationDuration: 0,
                layout: {
                    padding: {
                        left: 8,
                        right: 8,
                        top: 0,
                        bottom: 0
                    }
                },
                legend: {
                    display: false,
                    labels: {
                        boxWidth: 10,

                        fontColor: 'rgb(100, 100, 100)',
                        fontSize: 20
                    },
                    title: {
                        display: false,
                        text: ''
                    },
                    scales: {
                        xAxes: [{
                            stacked: true,
                            ticks: {

                                autoSkip: true,

                                callback: function (orig, index, values) {


                                    var day = orig.substr(0, 2) + ". ";
                                    var mon = orig.substr(2, 2) + ".";

                                    return day + mon;



                                }
                            }
                        }],
                        yAxes: [{

                            position: 'left',
                            gridLines: {
                                zeroLineWidth: 2.5
                                //  zeroLineColor: 'rgba(10, 132, 255, 0)'

                            }
                        }
                        ]
                    }

                }
            }
        }

        return chartParamsBAR;

    } else if (type == 2) {


        var chartParamsBAR2 = {
            type: 'horizontalBar',

            name: "",

            barPercentage: 1.0,
            data: {

                datasets: [{
                    label: "my messages",

                    backgroundColor: "rgba(0, 73, 207, 0.9)",
                    borderColor: "rgba(0, 73, 207, 0.9)",

                }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 200, // general animation time
                },
                hover: {
                    animationDuration: 400, // duration of animations when hovering an item
                },
                responsiveAnimationDuration: 0,
                layout: {
                    padding: {
                        left: 8,
                        right: 8,
                        top: 0,
                        bottom: 0
                    }
                },
                legend: {
                    display: false,
                    labels: {
                        boxWidth: 10,

                        fontColor: 'rgb(100, 100, 100)',
                        fontSize: 20
                    },
                    title: {
                        display: false,
                        text: ''
                    },
                    scales: {
                        xAxes: [{
                            stacked: true,
                            ticks: {

                                autoSkip: true,


                            }
                        }],
                        yAxes: [{

                            position: 'left',
                            gridLines: {
                                zeroLineWidth: 2.5
                                //  zeroLineColor: 'rgba(10, 132, 255, 0)'

                            }
                        }
                        ]
                    }
                }


            }

        }
        return chartParamsBAR2;
    } else {

        var chartParamsBAR3 = {
            type: 'horizontalBar',

            name: "",

            barPercentage: 1.0,
            data: {

                datasets: [{
                    label: "my messages",

                    backgroundColor: "rgba(0, 73, 207, 0.9)",
                    borderColor: "rgba(0, 73, 207, 0.9)",

                },
                {
                    label: "partner messages",

                    backgroundColor: "rgba(200, 73, 0, 0.9)",
                    borderColor: "rgba(200, 73, 0, 0.9)",

                }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 200, // general animation time
                },
                hover: {
                    animationDuration: 400, // duration of animations when hovering an item
                },
                responsiveAnimationDuration: 0,
                layout: {
                    padding: {
                        left: 8,
                        right: 8,
                        top: 0,
                        bottom: 0
                    }
                },
                legend: {
                    display: false,
                    labels: {
                        boxWidth: 10,

                        fontColor: 'rgb(100, 100, 100)',
                        fontSize: 20
                    },
                    title: {
                        display: false,
                        text: ''
                    },
                    scales: {
                        xAxes: [{
                            stacked: true,
                            ticks: {

                                autoSkip: true,


                            }
                        }],
                        yAxes: [{

                            position: 'left',
                            gridLines: {
                                zeroLineWidth: 2.5
                                //  zeroLineColor: 'rgba(10, 132, 255, 0)'

                            }
                        }
                        ]
                    }
                }


            }
        
        }
        return chartParamsBAR3;



    }



      


      }

$(window).resize(function () {

    resizeAll();

});

function resizeAll() {
   /* if (!bounce) {
        bounce = true;*/
        var cW = ($(window).width() - 50);
        if (cW > 1500) {

            cW -= 100;
        //    cW /= 4;
        }
        else if (cW > 1000) {
            cW -= 50;
          //  cW /= 3;
        }
        else if (cW > 500) {
            cW -= 25;
           // cW /= 2;
        }
        for (i = 0; i < nCharts; i++) {
            var card = document.getElementById("crd" + i);
            //var chart = document.getElementById("canv" + i);
            var chartDiv = document.getElementById("chrtDiv" + i);

            if (card != null) {

                var ccW = cW;
                if (i >= firstSplitCardId) {
               
                   ccW /= 2;
                }

                card.style.width = ccW + "px";
                chartDiv.style.display = "inline-block";
               // chart.style.visibility = "visible";
                if (i >= firstSplitCardId) card.style.height = "10000px";
                else card.style.height = "300px";

                  
            }

        }

  
      

   /*     targetCardWidth = cW;
        prevCardWidth = targetCardWidth;
        setTimeout(function () { bounce = false; }, 250);
    }*/
}


function showCard(c, wait, fromClick = false) {
    var chart = document.getElementById("canv" + c);
    chart.style.visibility = "visible";
    charts[c].resize();

 
}


function widenCard(c, force) {

}


function createChart(i, sgmt, type = 1) {


    var ctx = document.getElementById("charts" + sgmt);

    if (document.getElementById("crd" + i) != null) return;

    var karta = document.createElement('div'); //karta
    karta.id = "crd" + i;
    karta.setAttribute("class", "card");
    // setWidth(karta);
    // karta.setAttribute("onclick", "expandCard(" + i + ")");

    var chrtDiv = document.createElement('div'); //graf s jeho contaimentem
    chrtDiv.id = "chrtDiv" + i;
    chrtDiv.setAttribute("class", "crdChrtDiv");
    var chrt = document.createElement('canvas');
    chrt.id = "canv" + i;
    chrt.setAttribute("class", "chart");



   
    chrtDiv.appendChild(chrt);

  //  chrt.setAttribute("onclick", "widenCard(" + i + ",false)");



    var contaiment = document.createElement('div'); //containment info a grafu
    contaiment.setAttribute("class", "row");


    contaiment.appendChild(chrtDiv);

 

    karta.appendChild(contaiment);

    ctx.appendChild(karta);
    nCharts++;

    var crt = new chartParams(type);
  

    var tempChart = new Chart(chrt, crt);

    charts.push(tempChart);
    expanded.push(false);
    chartShown.push(false);

    widen.push(false);

    
 

}

function demandData(file, callback) {
    var rawFile = new XMLHttpRequest();
   


    rawFile.onreadystatechange = function () {
        if (rawFile.readyState === 4) {
            callback(rawFile.responseText);
        }
    }

    rawFile.open("GET", file, true);
    rawFile.setRequestHeader("GET", "text/plain;charset=UTF-8");
   
    rawFile.send();
}

function dropDownClick() {
    document.getElementById("listLidi").classList.toggle("show");
}









var dataChecker;
var selPersonFile = "";
var selPerson = "";
function personSelected(id) {

    demandData("setPerson$" + id, null);

    selPersonFile = peopleData[id]['realPath'];
    selPerson = peopleData[id]['personName'];

    dataChecker = setInterval(function () {
        demandData("getPrepared", function (text) {

            if (text == selPersonFile) {
                var dropdown = document.getElementById("selPerson");
                if (selPerson.length > 50) selPerson = selPerson.substr(0, 50) + "...";
                dropdown.innerHTML = selPerson;
                clearInterval(dataChecker);
                createChart(0, 1);
                createChart(1, 2);
                createChart(2, 3);
                createChart(3, 4);
                createChart(4, 5, 2);
                createChart(5, 5, 2);
                createChart(6, 6, 3);
                createChart(7, 6, 3);
                resizeAll();
                getGraphData();

            }
            



        });
    }, 500);

}

demandData("getDropdowns", function (text) {

    peopleData = JSON.parse(text);

    var dropdown = document.getElementById("listLidi");
    dropdown.innerHTML = "";
 
    
    for (var f = 0; f < peopleData.length; f++) {

        var newItem = document.createElement("a");

        var name = peopleData[f]['personName'];
        if (name.length > 40) name = name.substr(0, 40) + "...";
        name = name + " - " + peopleData[f]['fileSize'] + " MB";
        newItem.innerHTML = name;
        newItem.setAttribute("onClick", "personSelected(" + f + ")");
        newItem.setAttribute("class", "dropdown-item");
        dropdown.appendChild(newItem);
    }
    


});



function getGraphData() {


    var c = 0;
    for (c = 0; c < 4; c++) {
        let chrt = c;
        setTimeout(function () {

            demandData("json$" + chrt, function (text) {

                var jsonData = JSON.parse(text);

                charts[chrt].canvas.parentNode.style.height = '300px';
                charts[chrt].data.datasets[1].data = jsonData['count1'];
                charts[chrt].data.datasets[0].data = jsonData['count2'];
                charts[chrt].data.labels = jsonData['dates'];
                charts[chrt].update(0, false);

                setTimeout(function () {
                    showCard(chrt, true);
                   
                }, 100);



            });



        }, 500 + c * 250);
    }

    demandData("json$8", function (text) {

        var jsonData = JSON.parse(text);
        document.getElementById('stat1').innerHTML = "first message: " + jsonData['firstTime'];
        document.getElementById('stat2').innerHTML = "last message: " + jsonData['lastTime'];

        document.getElementById('stat3').innerHTML = "my messages total count: " + jsonData['totalCountMy'];
        document.getElementById('stat4').innerHTML = "partner messages total count: " + jsonData['totalCountP'];

        document.getElementById('stat5').innerHTML = "messages total count: " + (jsonData['totalCountP'] + jsonData['totalCountMy']);



    });

    setTimeout(function () { getWordCount(); }, 2000);

    function getWordCount() {
        demandData("json$4", function (text) {

            if (text == null || text == "null") {

                setTimeout(function () { getWordCount(); }, 500);
                return;
            }

            var jsonData = JSON.parse(text);

            charts[4].canvas.parentNode.style.height = '10000px';
            charts[4].data.datasets[0].data = jsonData['count1'];
            charts[4].data.labels = jsonData['dates'];
            charts[4].update(0, false);
            setTimeout(function () {
                showCard(4, true);
              

                demandData("json$5", function (text) {

                    var jsonData = JSON.parse(text);
                    charts[5].canvas.parentNode.style.height = '10000px';
                    charts[5].data.datasets[0].data = jsonData['count1'];
                    charts[5].data.labels = jsonData['dates'];
                    charts[5].data.datasets[0].label = "partner messages";
                    charts[5].data.datasets[0].backgroundColor = "rgba(200, 73, 0, 0.9)";
                    charts[5].data.datasets[0].borderColor = "rgba(200, 73, 0, 0.9)";
                    charts[5].update(0, false);

                    setTimeout(function () {
                        showCard(5, true);
                       
                        demandData("json$6", function (text) {

                            var jsonData = JSON.parse(text);
                            charts[6].canvas.parentNode.style.height = '10000px';
                            charts[6].data.datasets[0].data = jsonData['count1'];
                            charts[6].data.datasets[1].data = jsonData['count2'];
                            charts[6].data.labels = jsonData['dates'];
                            charts[6].update(0, false);

                            setTimeout(function () {
                                showCard(6, true);
                             
                                demandData("json$7", function (text) {

                                    var jsonData = JSON.parse(text);
                                    charts[7].canvas.parentNode.style.height = '10000px';
                                    charts[7].data.datasets[0].label = "partner messages";
                                    charts[7].data.datasets[1].label = "my messages";
                                    charts[7].data.datasets[0].data = jsonData['count1'];
                                    charts[7].data.datasets[1].backgroundColor = charts[7].data.datasets[0].backgroundColor;
                                    charts[7].data.datasets[1].borderColor = charts[7].data.datasets[0].borderColor;
                                    charts[7].data.datasets[0].backgroundColor = "rgba(200, 73, 0, 0.9)";
                                    charts[7].data.datasets[0].borderColor = "rgba(200, 73, 0, 0.9)";
                                    charts[7].data.datasets[1].data = jsonData['count2'];
                                    charts[7].data.labels = jsonData['dates'];
                                    charts[7].update(0, false);

                                    setTimeout(function () {
                                        showCard(7, true);
                                        
                                    }, 250);

                                    
                                });
                            }, 250);
                        });
                    }, 250);







                });

            }, 250);

        });

    }
}

window.onclick = function (event) {
    
    if (!event.target.matches('ul.topnav li button')) {
        var dropdown = document.getElementById("listLidi");
        if (dropdown.classList.contains('show')) {
           dropdown.classList.remove('show');
        }
       
    }
}