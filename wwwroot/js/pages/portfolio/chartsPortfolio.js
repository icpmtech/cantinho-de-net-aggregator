function renderCandlestickChart(itemId, symbol, chartType = 'candlestick', dateRange = '1y') {
  const { startDate, endDate } = getDateRange(dateRange);

  const apiUrl = `/api/Portfolio/historical-data?symbol=${symbol}&startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;

  fetch(apiUrl)
    .then(response => response.json())
    .then(data => {
      let seriesData;

      if (chartType === 'candlestick') {
        seriesData = data.map(item => ({
          x: new Date(item.date),
          y: [item.open, item.high, item.low, item.close]
        }));
      } else {
        seriesData = data.map(item => ({
          x: new Date(item.date),
          y: item.close
        }));
      }

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

      const chart = new ApexCharts(document.querySelector(`#candlestick-chart-${itemId}`), options);
      chart.render();
    })
    .catch(error => {
      console.error('Error fetching data:', error);
    });
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
