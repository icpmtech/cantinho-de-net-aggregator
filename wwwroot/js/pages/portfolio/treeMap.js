function renderPortfolioTreeMaps(portfolios) {
  const treeMapContainer = document.getElementById('treeMapContainer');
  treeMapContainer.innerHTML = '';  // Clear any existing content

  portfolios.forEach((portfolio, index) => {
    const treeMapDiv = document.createElement('div');
    treeMapDiv.id = `treeMap${index + 1}`;
    treeMapDiv.classList.add('treeMap');
    treeMapContainer.appendChild(treeMapDiv);

    // Group data by symbol within each portfolio
    const groupedData = portfolio.items.reduce((acc, stock) => {
      if (!acc[stock.symbol]) {
        acc[stock.symbol] = {
          x: stock.symbol,
          y: 0,
          investment: 0,
          revenue: 0,
          quantity: 0,
          stocks: []  // To store individual stock details
        };
      }
      acc[stock.symbol].y += stock.quantity * stock.currentPrice;
      acc[stock.symbol].investment += stock.quantity * stock.purchasePrice;
      acc[stock.symbol].revenue += (stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice);
      acc[stock.symbol].quantity += stock.quantity;
      acc[stock.symbol].percentageChange = ((acc[stock.symbol].y - acc[stock.symbol].investment) / acc[stock.symbol].investment) * 100;
      acc[stock.symbol].stocks.push(stock); // Add individual stock to the array
      return acc;
    }, {});

    // Convert grouped data into array format for ApexCharts
    const treeMapData = Object.values(groupedData);

    const options = {
      chart: {
        type: 'treemap',
        height: 400,
        events: {
          dataPointSelection: function (event, chartContext, config) {
            const data = config.w.config.series[0].data[config.dataPointIndex];
            showModal(data);
          }
        }
      },
      series: [{
        data: treeMapData.map(item => ({
          ...item,
          color: item.percentageChange >= 0 ? '#00FF00' : '#FF0000',
        }))
      }],
      title: {
        text: `Portfolio ${portfolio.name} Tree Map`
      },
      tooltip: {
        y: {
          formatter: function (value, { series, seriesIndex, dataPointIndex, w }) {
            const data = w.globals.initialSeries[seriesIndex].data[dataPointIndex];
            return `
              <div>
                <strong>Value:</strong> €${value.toFixed(2)}<br>
                <strong>Investment:</strong> €${data.investment.toFixed(2)}<br>
                <strong>Revenue:</strong> €${data.revenue.toFixed(2)}
              </div>`;
          }
        }
      },
      plotOptions: {
        treemap: {
          distributed: true,
          enableShades: false,
          useFillColorAsStroke: true,
          dataLabels: {
            style: {
              fontSize: '14px',
              fontWeight: 'bold',
              colors: ['#fff']
            },
            formatter: function (value, { series, seriesIndex, dataPointIndex, w }) {
              const data = w.globals.initialSeries[seriesIndex].data[dataPointIndex];
              const percentageChange = data.percentageChange.toFixed(2);
              return `${value}<br>${percentageChange}%`;
            }
          }
        }
      }
    };

    const chart = new ApexCharts(document.querySelector(`#treeMap${index + 1}`), options);
    chart.render();
  });
}

function showModal(data) {
  const stocksDetails = data.stocks.map(stock => `
    <li class="list-group-item">
      <div>
        <strong>Symbol:</strong> ${stock.symbol}<br>
        <strong>Quantity:</strong> ${stock.quantity}<br>
        <strong>Current Price:</strong> €${stock.currentPrice.toFixed(2)}<br>
        <strong>Purchase Price:</strong> €${stock.purchasePrice.toFixed(2)}<br>
        <strong>Investment:</strong> €${(stock.quantity * stock.purchasePrice).toFixed(2)}<br>
        <strong>Revenue:</strong> €${((stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice)).toFixed(2)}
      </div>
    </li>`).join('');

  const modalContent = `
    <div>
      <strong>Symbol:</strong> ${data.x}<br>
      <strong>Total Value:</strong> €${data.y.toFixed(2)}<br>
      <strong>Total Investment:</strong> €${data.investment.toFixed(2)}<br>
      <strong>Total Revenue:</strong> €${data.revenue.toFixed(2)}<br>
      <strong>Total Quantity:</strong> ${data.quantity}<br>
      <hr>
      <ul class="list-group">
        ${stocksDetails}
      </ul>
    </div>`;

  document.getElementById('modalContent').innerHTML = modalContent;
  const detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
  detailsModal.show();
}
