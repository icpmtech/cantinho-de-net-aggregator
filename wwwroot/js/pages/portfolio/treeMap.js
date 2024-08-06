
function exportToCsv(filename, data) {
  const csvContent = [
    ["Symbol", "Quantity", "Current Price", "Purchase Price", "Investment", "Revenue"],
    ...data.map(stock => [
      stock.symbol,
      stock.quantity,
      stock.currentPrice.toFixed(2),
      stock.purchasePrice.toFixed(2),
      (stock.quantity * stock.purchasePrice).toFixed(2),
      ((stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice)).toFixed(2)
    ])
  ].map(e => e.join(",")).join("\n");

  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement("a");
  const url = URL.createObjectURL(blob);
  link.setAttribute("href", url);
  link.setAttribute("download", filename);
  link.style.visibility = 'hidden';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

function exportToExcel(filename, data) {
  const ws = XLSX.utils.json_to_sheet(data.map(stock => ({
    Symbol: stock.symbol,
    Quantity: stock.quantity,
    "Current Price": stock.currentPrice.toFixed(2),
    "Purchase Price": stock.purchasePrice.toFixed(2),
    Investment: (stock.quantity * stock.purchasePrice).toFixed(2),
    Revenue: ((stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice)).toFixed(2)
  })));

  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, "Portfolio Details");
  XLSX.writeFile(wb, filename);
}

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
            symbol: stock.symbol,
            y: 0,
            investment: 0,
            revenue: 0,
            percentageChange: 0,
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
      const treeMapData = Object.values(groupedData).map(data => ({
        x: `${data.symbol}-
        €${data.revenue.toFixed(2)}`,
        y: data.revenue,
        investment: data.investment,
        revenue: data.revenue,
        percentageChange: data.percentageChange,
        quantity: data.quantity,
        stocks: data.stocks
      }));

      // Calculate min and max values for color scale
      const minRevenue = Math.min(...treeMapData.map(data => data.revenue));
      const maxRevenue = Math.max(...treeMapData.map(data => data.revenue));

      const options = {
        chart: {
          type: 'treemap',
          height: 600,
          events: {
            dataPointSelection: function (event, chartContext, config) {
              const data = config.w.config.series[0].data[config.dataPointIndex];
              showModal(data);
            }
          }
        },
        series: [{
          data: treeMapData
        }],
        title: {
          text: `Portfolio ${portfolio.name} revenue`
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
            colorScale: {
              ranges: [
                {
                  from: minRevenue,
                  to: 0,
                  color: '#CD363A' // Red for negative values
                },
                {
                  from: 0.001,
                  to: maxRevenue,
                  color: '#52B12C' // Green for positive values
                }
              ]
            },
            enableShades: false,
            dataLabels: {
              style: {
                fontSize: '14px',
                fontWeight: 'bold',
                colors: ['#fff']
              }
            }
          }
        }
      };

      const chart = new ApexCharts(document.querySelector(`#treeMap${index + 1}`), options);
      chart.render();
    });
  renderRevenueTreeMap(portfolios);
}

 


function renderRevenueTreeMap(portfolios) {
  const treeMapRevenueContainer = document.getElementById('treeMapRevenueContainer');
  treeMapRevenueContainer.innerHTML = '';  // Clear any existing content

  portfolios.forEach((portfolio, index) => {
    const treeMapDiv = document.createElement('div');
    treeMapDiv.id = `treeMapRevenue${index + 1}`;
    treeMapDiv.classList.add('treeMap');
    treeMapRevenueContainer.appendChild(treeMapDiv);

    // Directly map the items to the tree map data format
    const treeMapData = portfolio.items.map(stock => ({
      x: stock.symbol + ' P/L. €' + ((stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice)).toFixed(2),
      y: (stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice),
      investment: stock.quantity * stock.purchasePrice,
      revenue: (stock.quantity * stock.currentPrice) - (stock.quantity * stock.purchasePrice),
      percentageChange: ((stock.currentPrice - stock.purchasePrice) / stock.purchasePrice) * 100,
      stocks: [stock]  // Store individual stock details for the modal
    }));

    // Calculate min and max values for color scale
    const minRevenue = Math.min(...treeMapData.map(data => data.revenue));
    const maxRevenue = Math.max(...treeMapData.map(data => data.revenue));

    const options = {
      chart: {
        type: 'treemap',
        height: 800,
        events: {
          dataPointSelection: function (event, chartContext, config) {
            const data = config.w.config.series[0].data[config.dataPointIndex];
            showModal(data);
          }
        }
      },
      series: [{
        data: treeMapData
      }],
      title: {
        text: `Portfolio ${portfolio.name} transactions revenue`
      },
      tooltip: {
        y: {
          formatter: function (value, { series, seriesIndex, dataPointIndex, w }) {
            const data = w.globals.initialSeries[seriesIndex].data[dataPointIndex];
            return `
              <div>
                <strong>Revenue:</strong> €${value.toFixed(2)}<br>
                <strong>Investment:</strong> €${data.investment.toFixed(2)}<br>
                <strong>Value:</strong> €${data.y.toFixed(2)}
              </div>`;
          }
        }
      },
      plotOptions: {
        treemap: {
          distributed: true,
          colorScale: {
            ranges: [
              {
                from: minRevenue,
                to: 0,
                color: '#CD363A' // Red for negative values
              },
              {
                from: 0.001,
                to: maxRevenue,
                color: '#52B12C' // Green for positive values
              }
            ]
          },
          enableShades: false,
          useFillColorAsStroke: false,
          dataLabels: {
            style: {
              fontSize: '14px',
              fontWeight: 'bold',
              colors: ['#fff']
            },
            formatter: function (value, { series, seriesIndex, dataPointIndex, w }) {
              const data = w.globals.initialSeries[seriesIndex].data[dataPointIndex];
              const percentageChange = data.percentageChange.toFixed(2);
              const revenue = data.revenue.toFixed(2);
              return `${data.x}<br>${percentageChange}%<br>€${revenue}`;
            }
          }
        }
      }
    };

    const chart = new ApexCharts(document.querySelector(`#treeMapRevenue${index + 1}`), options);
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
