    function myChart() {

        var piePoints = [
            { y: 522885, label: "CREDIT" },
            { y: 752982, label: "DEBIT" },
            { y: 769903, label: "Balance" }
        ];

        var chart = new CanvasJS.Chart("myChart", {
            animationEnabled: true,
            title: { text: "BRANCH ID : 55179       GL-HEAD : 411" },
            data: [{ type: "pie", dataPoints: piePoints }]
        });
        chart.render();
    }
    $(document).ready(function () {
        myChart();
    });


    function myChartbar() {

        var barPoints = [
            { label: "BALANCE", y: 10881 },
            { label: "CREDIT", y: 8896 },
            { label: "DEBIT", y: 10015 }
        ];

        var chartBar = new CanvasJS.Chart("myChartbar", {
            animationEnabled: true,
            title: { text: "BRANCH ID : 55179       GL-HEAD : 1513" },
            axisY: { title: "Value" },
            data: [{ type: "column", dataPoints: barPoints }]
        });
        chartBar.render();
    }
    $(document).ready(function () {
        myChartbar();
    });



    /*    function myChartmline() {

            var lValues = [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000];
            var xValues = [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000];
            var yValues = [200, 300, 400, 500, 600, 700, 800, 900, 950, 1000];
            var zValues = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500];

            new Chart("myChartmline", {
      type: "line",
      data: {
        labels: lValues,
          datasets: [{
              data: xValues,
          borderColor: "red",
          fill: false
          }, {
                  data: yValues,
          borderColor: "green",
          fill: false
              }, {
                  data: zValues,
          borderColor: "blue",
          fill: true
        }]
      },
      options: {
        legend: {display: false
        }
      }
    });

                $(document).ready(function () {
          myChartmline();
      });


    */
