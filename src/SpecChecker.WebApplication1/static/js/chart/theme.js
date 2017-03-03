
Highcharts.theme = {
   //colors: ['#f00', '#058DC7', '#50B432', '#ED561B', '#DDDF00', '#24CBE5', '#64E572', '#FF9655', '#FFF263', '#6AF9C4'],
   colors: ["#FF7F50", "#8A2BE2", "#00CED1", "#00E00D", "#CD5C5C", "#FF69B4", "#00FFFF", "#FF00FF", "#FFA500"],
   chart: {
//      backgroundColor: {
//         linearGradient: { x1: 0, y1: 0, x2: 1, y2: 1 },
//         stops: [
//            [0, 'rgb(255, 255, 255)'],
//            [1, 'rgb(240, 240, 255)']
//         ]
//      },
      borderWidth: 0    ,
      //plotBackgroundColor: 'rgba(255, 255, 255, .9)',
      //plotShadow: true,
      //plotBorderWidth: 1
   },
   title: {
      style: {
         color: '#000',
         font: 'bold 16px 微软雅黑, Consolas'
      }
   },
   subtitle: {
      style: {
         color: '#666666',
         font: 'bold 12px 微软雅黑, Consolas'
      }
   },
   xAxis: {
      gridLineWidth: 1,
      lineColor: '#000',
      tickColor: '#000',
      labels: {
         style: {
            color: '#000',
            font: '10px 微软雅黑, Consolas'
         }
      },
      title: {
         style: {
            color: '#333',
            fontWeight: 'bold',
            fontSize: '12px',
            fontFamily: '微软雅黑, Consolas'

         }
      }
   },
   yAxis: {
      minorTickInterval: 'auto',
      lineColor: '#000',
      lineWidth: 1,
      tickWidth: 1,
      tickColor: '#000',
      labels: {
         style: {
            color: '#000',
            font: '11px 微软雅黑, Consolas'
         }
      },
      title: {
         style: {
            color: '#333',
            fontWeight: 'bold',
            fontSize: '12px',
            fontFamily: '微软雅黑, Consolas'
         }
      }
   },
   legend: {
      itemStyle: {
         font: '9pt 微软雅黑, Consolas',
         color: 'black'

      },
      itemHoverStyle: {
         color: '#039'
      },
      itemHiddenStyle: {
         color: 'gray'
      }
   },
   labels: {
      style: {
         color: '#99b'
      }
   },

   navigation: {
      buttonOptions: {
         theme: {
            stroke: '#CCCCCC'
         }
      }
   }
};

// Apply the theme
var highchartsOptions = Highcharts.setOptions(Highcharts.theme);
