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

/**
 * Renders a heatmap chart using ApexCharts with enhanced color scales.
 *
 * @param {Object} data - The data object containing items.
 * @param {Array} data.items - Array of objects with 'sectorActivity', 'symbol', and 'currentMarketValue'.
 */
export function renderHeatmapChart(data) {
  // Validate input data
  if (!data || !Array.isArray(data.items)) {
    console.error('Invalid data format. Expected data.items as an array.');
    return;
  }

  // Step 1: Aggregate the total market value per sectorActivity and symbol
  const aggregatedData = {};

  data.items.forEach(item => {
    const { sectorActivity, symbol, currentMarketValue } = item;

    if (!sectorActivity || !symbol || typeof currentMarketValue !== 'number') {
      // Skip invalid entries
      return;
    }

    // Initialize nested objects if they don't exist
    if (!aggregatedData[sectorActivity]) {
      aggregatedData[sectorActivity] = {};
    }

    if (!aggregatedData[sectorActivity][symbol]) {
      aggregatedData[sectorActivity][symbol] = 0;
    }

    // Sum the market values
    aggregatedData[sectorActivity][symbol] += currentMarketValue;
  });

  // Step 2: Extract unique industries and symbols
  const industries = Object.keys(aggregatedData);
  const symbolsSet = new Set();

  industries.forEach(industry => {
    Object.keys(aggregatedData[industry]).forEach(symbol => {
      symbolsSet.add(symbol);
    });
  });

  const symbols = Array.from(symbolsSet);

  // Step 3: Prepare series data for ApexCharts
  const heatmapSeries = industries.map(industry => {
    const dataPoints = symbols.map(symbol => {
      const value = aggregatedData[industry][symbol] || 0;
      return {
        x: symbol,
        y: value
      };
    });

    return {
      name: industry,
      data: dataPoints
    };
  });

  // Step 4: Define heatmap chart options with enhanced color scales
  const heatmapOptions = {
    series: heatmapSeries,
    chart: {
      type: 'heatmap',
      height: 450,
      toolbar: {
        show: true
      }
    },
    plotOptions: {
      heatmap: {
        shadeIntensity: 0.8,
        radius: 0,
        useFillColorAsStroke: true,
        colorScale: {
          ranges: [
            {
              from: 0,
              to: 1000,
              name: 'Very Low',
              color: '#D4EFDF' // Light Green
            },
            {
              from: 1001,
              to: 5000,
              name: 'Low',
              color: '#ABEBC6' // Green
            },
            {
              from: 5001,
              to: 10000,
              name: 'Medium',
              color: '#F9E79F' // Light Yellow
            },
            {
              from: 10001,
              to: 50000,
              name: 'High',
              color: '#F4D03F' // Yellow
            },
            {
              from: 50001,
              to: 100000,
              name: 'Very High',
              color: '#E67E22' // Orange
            },
            {
              from: 100001,
              to: 1000000,
              name: 'Extreme',
              color: '#CB4335' // Red
            }
          ]
        }
      }
    },
    dataLabels: {
      enabled: true,
      style: {
        colors: ['#000000']
      },
      formatter: function (val) {
        return `$${val.toLocaleString()}`;
      }
    },
    stroke: {
      width: 1
    },
    title: {
      text: 'Industry Market Value Heatmap',
      align: 'center'
    },
    xaxis: {
      type: 'category',
      categories: symbols,
      title: {
        text: 'Stock Symbol'
      }
    },
    yaxis: {
      title: {
        text: 'Industry'
      }
    },
    tooltip: {
      y: {
        formatter: function (val) {
          return `Market Value: $${val.toLocaleString()}`;
        }
      }
    }
  };

  // Step 5: Render or update the heatmap chart
  if (heatmapChartInstance) {
    heatmapChartInstance.updateOptions(heatmapOptions);
  } else {
    const chartElement = document.querySelector("#industryHeatmap");
    if (!chartElement) {
      console.error('Chart container with id "industryHeatmap" not found.');
      return;
    }
    heatmapChartInstance = new ApexCharts(chartElement, heatmapOptions);
    heatmapChartInstance.render();
  }
}
