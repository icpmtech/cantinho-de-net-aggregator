async function fetchPurchaseDates(symbol, startDate, endDate) {
  const response = await fetch(`/api/Portfolio/purchase-dates-for-symbol?symbol=${symbol}&startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`);
  const data = await response.json();
  return data;
}

async function renderCandlestickChart(containerId, symbol, chartType = 'candlestick', dateRange = '1y') {
  // Get the start and end dates for the date range
  const { startDate, endDate } = getDateRange(dateRange);

  // API URL to fetch the data
  const apiUrl = `/api/Portfolio/historical-data?symbol=${symbol}&startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;

  try {
    // Fetch the data from the API
    const response = await fetch(apiUrl);
    const dataObj = await response.json();

    // Map the data to the required format
    const data = dataObj.dates.map((dateStr, i) => ({
      timestamp: new Date(dateStr),
      open: dataObj.opens[i],
      high: dataObj.highs[i],
      low: dataObj.lows[i],
      close: dataObj.closes[i],
      volume: dataObj.volumes[i]
    }));

    // Filter data according to startDate and endDate
    const filteredData = data.filter(item => item.timestamp >= startDate && item.timestamp <= endDate);

    // Prepare the seriesData for the chart
    let seriesData;

    if (chartType === 'candlestick') {
      seriesData = filteredData.map(item => ({
        x: item.timestamp,
        y: [item.open, item.high, item.low, item.close]
      }));
    } else {
      seriesData = filteredData.map(item => ({
        x: item.timestamp,
        y: item.close
      }));
    }

    // Fetch purchase dates for annotations (if available)
    const purchaseDates = await fetchPurchaseDates(symbol, startDate, endDate);
    let annotations = purchaseDates.map(purchase => ({
      x: new Date(purchase.date).getTime(),
      borderColor: purchase.operationType === 'Buy' ? '#00FF00' : '#FF4560',
      label: {
        borderColor: purchase.operationType === 'Buy' ? '#00FF00' : '#FF4560',
        style: {
          color: '#fff',
          background: purchase.operationType === 'Buy' ? '#00FF00' : '#FF4560'
        },
        text: `Qty: ${purchase.quantity} - Op: ${purchase.operationType}`
      }
    }));

    // Define the options for ApexCharts
    let options = {
      series: [{
        data: seriesData
      }],
      chart: {
        type: chartType,
        height: 350
      },
      title: {
        text: symbol,
        align: 'left'
      },
      xaxis: {
        type: 'datetime'
      },
      yaxis: {
        tooltip: {
          enabled: true
        }
      },
      annotations: {
        xaxis: annotations
      }
    };

    // Adjust options for bar and line charts
    if (chartType === 'bar') {
      options.plotOptions = {
        bar: {
          horizontal: false,
        }
      };
      options.dataLabels = {
        enabled: false
      };
    } else if (chartType === 'line') {
      options.stroke = {
        curve: 'smooth'
      };
      options.dataLabels = {
        enabled: false
      };
    }

    // Render the chart using ApexCharts
    const chart = new ApexCharts(document.querySelector(`#${containerId}`), options);
    chart.render();
  } catch (error) {
    console.error('Error fetching data:', error);
  }
}
function getDateRange(dateRange) {
  const endDate = new Date();
  let startDate;

  switch (dateRange) {
    case '1d':
      startDate = new Date();
      startDate.setDate(endDate.getDate() - 1);
      break;
    case '5d':
      startDate = new Date();
      startDate.setDate(endDate.getDate() - 5);
      break;
    case '1m':
      startDate = new Date();
      startDate.setMonth(endDate.getMonth() - 1);
      break;
    case '1y':
      startDate = new Date();
      startDate.setFullYear(endDate.getFullYear() - 1);
      break;
    case '5y':
      startDate = new Date();
      startDate.setFullYear(endDate.getFullYear() - 5);
      break;
    default:
      startDate = new Date(0); // Default to all-time
      break;
  }

  return { startDate, endDate };
}

function formatDate(date) {
  const d = new Date(date);
  return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
}


function getDateRange(dateRange) {
  const endDate = new Date();
  let startDate;

  switch (dateRange) {
    case '1d':
      startDate = new Date();
      startDate.setDate(endDate.getDate() - 1);
      break;
    case '5d':
      startDate = new Date();
      startDate.setDate(endDate.getDate() - 5);
      break;
    case '1m':
      startDate = new Date();
      startDate.setMonth(endDate.getMonth() - 1);
      break;
    case '1y':
      startDate = new Date();
      startDate.setFullYear(endDate.getFullYear() - 1);
      break;
    case '5y':
      startDate = new Date();
      startDate.setFullYear(endDate.getFullYear() - 5);
      break;
    default:
      startDate = new Date(0); // Default to all-time
      break;
  }

  return { startDate, endDate };
}

function formatDate(date) {
  const d = new Date(date);
  return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
}
function getDateRange(dateRange) {
  const endDate = new Date();
  let startDate;

  switch (dateRange) {
    case '1d':
      startDate = new Date();
      startDate.setDate(endDate.getDate() - 1);
      break;
    case '5d':
      startDate = new Date();
      startDate.setDate(endDate.getDate() - 5);
      break;
    case '1m':
      startDate = new Date();
      startDate.setMonth(endDate.getMonth() - 1);
      break;
    case '1y':
      startDate = new Date();
      startDate.setFullYear(endDate.getFullYear() - 1);
      break;
    case '5y':
      startDate = new Date();
      startDate.setFullYear(endDate.getFullYear() - 5);
      break;
    default:
      startDate = new Date(0); // Default to all-time
      break;
  }

  return { startDate, endDate };
}

function formatDate(date) {
  const d = new Date(date);
  return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
}

function renderChart(portfolio, chartType) {
  const ctx = document.getElementById(`chart-${portfolio.id}`).getContext('2d');

  if (chartType === 'candlestick') {
    new Chart(ctx, {
      type: 'bar',
      data: {
        labels: portfolio.items.map(item => item.symbol),
        datasets: [{
          label: 'Current Market Value',
          data: portfolio.items.map(item => item.currentMarketValue),
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          borderColor: 'rgba(75, 192, 192, 1)',
          borderWidth: 1
        }, {
          label: 'Total Investment',
          data: portfolio.items.map(item => item.totalInvestment),
          backgroundColor: 'rgba(153, 102, 255, 0.2)',
          borderColor: 'rgba(153, 102, 255, 1)',
          borderWidth: 1
        }]
      },
      options: {
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    });
  } else {
    new Chart(ctx, {
      type: chartType,
      data: {
        labels: portfolio.items.map(item => item.symbol),
        datasets: [{
          label: 'Current Market Value',
          data: portfolio.items.map(item => item.currentMarketValue),
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          borderColor: 'rgba(75, 192, 192, 1)',
          borderWidth: 1
        }, {
          label: 'Total Investment',
          data: portfolio.items.map(item => item.totalInvestment),
          backgroundColor: 'rgba(153, 102, 255, 0.2)',
          borderColor: 'rgba(153, 102, 255, 1)',
          borderWidth: 1
        }]
      },
      options: {
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    });
  }
}

function changeChartType(event) {
  const chartType = event.target.value;
  const portfolios = JSON.parse(document.getElementById('portfolioList').dataset.portfolios);
  const dateRange = document.getElementById('portfolioList').dataset.dateRange;
  renderPortfolioList(portfolios, chartType, dateRange);
}
