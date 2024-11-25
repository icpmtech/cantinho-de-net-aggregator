// chartRenderer.js

// Import ApexCharts
import ApexCharts from 'https://cdn.jsdelivr.net/npm/apexcharts@3.37.2/dist/apexcharts.esm.js';


// Initialize chart instances globally
export let lineChartInstance, radarChartInstance, barChartInstance, pieChartInstance, eventsChartInstance, heatmapChartInstance;

// Function to render charts
export function renderCharts(data) {
  // Update DOM elements with data
  document.getElementById('dayReturn').textContent = `${data.dayReturn}%`;
  document.getElementById('bestWeek').textContent = `${data.bestWeek}%`;
  document.getElementById('bestMonth').textContent = `${data.bestMonth}%`;
  document.getElementById('bestStockName').textContent = data.bestStockName;

  document.getElementById('sharpeRatio').textContent = data.metrics.sharpeRatio;
  document.getElementById('volatility').textContent = `${data.metrics.volatility}%`;
  document.getElementById('maxDrawdown').textContent = `${data.metrics.maxDrawdown}%`;
  document.getElementById('totalGrowth').textContent = `${data.metrics.totalGrowth}%`;
  document.getElementById('todayGrowth').textContent = `${data.todayGrowth}%`;

  // Display today's metrics
  document.getElementById('totalAmountToday').textContent = data.todayMetrics.totalAmountToday;
  document.getElementById('changeInMarketValueToday').textContent = data.todayMetrics.changeInMarketValueToday;
  document.getElementById('percentChangeToday').textContent = data.todayMetrics.percentChangeToday;

  // Radar Chart Options
  const radarOptions = {
    series: [{
      name: "Metrics",
      data: [
        parseFloat(data.metrics.avgReturn),
        parseFloat(data.metrics.volatility),
        parseFloat(data.metrics.sharpeRatio),
        parseFloat(data.metrics.totalGrowth),
        parseFloat(data.diversification)
      ],
    }],
    chart: {
      type: "radar",
      height: 350,
    },
    labels: [
      "Avg Return (%)",
      "Volatility (Risk)",
      "Sharpe Ratio",
      "Total Growth (%)",
      "Diversification (%)",
    ],
    title: {
      text: "Portfolio Performance Metrics",
    },
    fill: {
      opacity: 0.4,
    },
    colors: ["#008FFB"],
  };

  if (radarChartInstance) {
    radarChartInstance.updateOptions(radarOptions);
  } else {
    radarChartInstance = new ApexCharts(
      document.querySelector("#radarChart"),
      radarOptions
    );
    radarChartInstance.render();
  }

  // Render or Update Line Chart
  const lineSeries = [{
    name: 'Portfolio Value',
    data: data.portfolioEvolution.map(e => e.currentMarketValue)
  }];
  const lineCategories = data.portfolioEvolution.map(e => e.date);

  if (lineChartInstance) {
    lineChartInstance.updateOptions({
      xaxis: {
        categories: lineCategories
      },
      series: lineSeries
    });
  } else {
    const lineOptions = {
      series: lineSeries,
      chart: {
        type: 'line',
        height: 350
      },
      xaxis: {
        categories: lineCategories,
        title: {
          text: 'Date'
        }
      },
      yaxis: {
        title: {
          text: 'Market Value ($)'
        }
      },
      title: {
        text: 'Portfolio Evolution',
        align: 'center'
      }
    };
    lineChartInstance = new ApexCharts(document.querySelector("#lineChart"), lineOptions);
    lineChartInstance.render();
  }

  // Render or Update Bar Chart
  const barSeries = [{
    name: 'Stock Returns (%)',
    data: data.monthlyReturns
  }];
  const barCategories = data.months;

  if (barChartInstance) {
    barChartInstance.updateOptions({
      xaxis: {
        categories: barCategories
      },
      series: barSeries
    });
  } else {
    const barOptions = {
      series: barSeries,
      chart: {
        type: 'bar',
        height: 350
      },
      xaxis: {
        categories: barCategories,
        title: {
          text: 'Month'
        }
      },
      yaxis: {
        title: {
          text: 'Returns (%)'
        }
      },
      title: {
        text: 'Monthly Stock Returns',
        align: 'center'
      }
    };
    barChartInstance = new ApexCharts(document.querySelector("#barChart"), barOptions);
    barChartInstance.render();
  }

  // Allocate colors for each symbol
  const pieSeries = data.allocation.map(a => a.value);
  const pieLabels = data.allocation.map(a => a.label);

  // Generate random colors for each symbol
  const generateRandomColor = () => {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 6; i++) {
      color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
  };

  // Create color array for pie chart
  const pieColors = data.allocation.map(() => generateRandomColor());

  if (pieChartInstance) {
    pieChartInstance.updateOptions({
      labels: pieLabels,
      series: pieSeries,
      colors: pieColors // Assign the random colors
    });
  } else {
    const pieOptions = {
      series: pieSeries,
      chart: {
        type: 'pie',
        height: 350
      },
      labels: pieLabels,
      title: {
        text: 'Portfolio Allocation',
        align: 'center'
      },
      colors: pieColors // Assign the random colors
    };
    pieChartInstance = new ApexCharts(document.querySelector("#pieChart"), pieOptions);
    pieChartInstance.render();
  }

  // Render or Update Events Chart
  const eventsSeries = [{
    name: 'Events',
    data: [data.eventSentiments.positive, data.eventSentiments.negative, data.eventSentiments.neutral]
  }];
  const eventsCategories = ['Positive', 'Negative', 'Neutral'];

  if (eventsChartInstance) {
    eventsChartInstance.updateOptions({
      xaxis: {
        categories: eventsCategories
      },
      series: eventsSeries
    });
  } else {
    const eventsOptions = {
      series: eventsSeries,
      chart: {
        type: 'bar',
        height: 350
      },
      xaxis: {
        categories: eventsCategories,
        title: {
          text: 'Sentiment'
        }
      },
      yaxis: {
        title: {
          text: 'Count'
        },
        min: 0
      },
      title: {
        text: 'Event Sentiments',
        align: 'center'
      }
    };
    eventsChartInstance = new ApexCharts(document.querySelector("#eventsChart"), eventsOptions);
    eventsChartInstance.render();
  }

  // Render the heatmap chart for industries
  renderHeatmapChart(data);
}

// Function to render the Industry Market Value Heatmap
export function renderHeatmapChart(data) {
  const heatmapSeries = [{
    name: "Market Value",
    data: data.industryHeatmapData.map(item => ({
      x: item.x, // Industry name
      y: item.y   // Market value for the industry
    }))
  }];

  const heatmapOptions = {
    series: heatmapSeries,
    chart: {
      type: 'heatmap',
      height: 350
    },
    plotOptions: {
      heatmap: {
        shadeIntensity: 0.5,
        colorScale: {
          ranges: [
            {
              from: 0,
              to: 10000,
              name: 'Low',
              color: '#00A100'
            },
            {
              from: 10001,
              to: 50000,
              name: 'Medium',
              color: '#FFB200'
            },
            {
              from: 50001,
              to: 100000,
              name: 'High',
              color: '#FF0000'
            }
          ]
        }
      }
    },
    xaxis: {
      type: 'category',
      title: {
        text: 'Industry'
      }
    },
    yaxis: {
      title: {
        text: 'Market Value ($)'
      }
    },
    dataLabels: {
      enabled: false
    },
    title: {
      text: 'Industry Market Value Heatmap',
      align: 'center'
    }
  };

  if (heatmapChartInstance) {
    heatmapChartInstance.updateOptions(heatmapOptions);
  } else {
    heatmapChartInstance = new ApexCharts(document.querySelector("#industryHeatmap"), heatmapOptions);
    heatmapChartInstance.render();
  }
}
