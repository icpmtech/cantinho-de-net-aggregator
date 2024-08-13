async function loadPortfolios(dateRange = '1y') {
  document.getElementById('loadingSpinner').style.display = 'block';
  document.getElementById('portfolioList').style.display = 'none';

  const { startDate, endDate } = getDateRange(dateRange);

  const response = await fetch(`/api/Portfolio`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    const portfolios = await response.json();
    renderPortfolioList(portfolios, 'bar', dateRange);
    renderPortfolioHeatMaps(portfolios);
    renderPortfolioTreeMaps(portfolios);
  } else {
    alert('Failed to load portfolios');
  }

  document.getElementById('loadingSpinner').style.display = 'none';
  document.getElementById('portfolioList').style.display = 'block';
}

async function loadSymbols() {
  const response = await fetch('/api/SymbolsAPI', {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    const symbols = await response.json();
    const symbolSelect = document.getElementById('itemSymbol');
    const editSymbolSelect = document.getElementById('editItemSymbol');
    symbols.forEach(symbol => {
      const option = document.createElement('option');
      option.value = symbol;
      option.text = symbol;
      symbolSelect.appendChild(option);

      const editOption = document.createElement('option');
      editOption.value = symbol;
      editOption.text = symbol;
      editSymbolSelect.appendChild(editOption);
    });
  } else {
    alert('Failed to load symbols');
  }
}

function sanitizeSymbol(symbol) {
  return symbol.replace(/[^a-zA-Z0-9-_]/g, '_');
}

function renderPortfolioList(portfolios, chartType = 'bar', dateRange = 'all') {
  const portfolioList = document.getElementById('portfolioList');
  portfolioList.dataset.portfolios = JSON.stringify(portfolios);
  portfolioList.dataset.dateRange = dateRange;
  portfolioList.innerHTML = '';
  document.getElementById('portfolios-numbers').innerHTML = portfolios.length;
  const renderedSymbols = new Set();

  portfolios.forEach(portfolio => {
    const portfolioDiv = document.createElement('div');
    portfolioDiv.classList.add('portfolio-card', 'mb-4');
    portfolioDiv.innerHTML = generatePortfolioHTML(portfolio);

    portfolioList.appendChild(portfolioDiv);
    renderChart(portfolio, chartType);
    portfolio.groupedItems.forEach(group => {
      const sanitizedSymbol = sanitizeSymbol(group.symbol);
      if (!renderedSymbols.has(sanitizedSymbol)) {
        renderCandlestickChart(`candlestick-chart-${sanitizedSymbol}`, group.symbol, chartType, dateRange);
        renderSparklineChart(`sparkline-chart-${sanitizedSymbol}`, group.symbol);
        renderedSymbols.add(sanitizedSymbol);
      }
    });
  });
}

function generatePortfolioHTML(portfolio) {
  return `
            <div class="card mb-4 shadow-sm border-0 rounded">
                <div class="card-body">
                    <div class="card-title d-flex align-items-center justify-content-between">
                        <h3 class="mb-0">${portfolio.name}</h3>
                        <div class="dropdown">
                            <button class="btn p-0" type="button" id="portfolioActions-${portfolio.id}" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                <i class="bx bx-dots-vertical-rounded"></i>
                            </button>
                            <div class="dropdown-menu dropdown-menu-end" aria-labelledby="portfolioActions-${portfolio.id}">
                                <a class="dropdown-item" href="javascript:toggleChart(${portfolio.id})">
                                    <i class="bx bx-line-chart"></i> View Chart
                                </a>
                                <a class="dropdown-item" href="javascript:showAddPortfolioItemModal(${portfolio.id});">
                                    <i class="bx bx-plus"></i> Add Op.
                                </a>
                                <a class="dropdown-item" href="javascript:showEditPortfolioModal(${portfolio.id}, '${portfolio.name.replace(/'/g, "\\'")}');">
                                    <i class="bx bx-edit"></i> Edit
                                </a>
                                <a class="dropdown-item" href="javascript:deletePortfolio(${portfolio.id});">
                                    <i class="bx bx-trash"></i> Delete
                                </a>
                            </div>
                        </div>
                    </div>
                    <div class="row mt-4">
                        <div class="col-12">
                            <div class="d-flex align-items-center justify-content-between">
                                <div class="d-flex align-items-center">
                                    <div class="avatar flex-shrink-0">
                                        <img src="/img/icons/unicons/chart-success.png" alt="chart success" class="rounded" style="width: 40px; height: 40px;">
                                    </div>
                                    <div class="ms-3">
                                        <span class="fw-medium d-block text-muted">Total Investment</span>
                                        <h3 class="mb-0">€${portfolio.totalInvestment.toFixed(3)}</h3>
                                    </div>
                                </div>
                            </div>
                            <canvas id="chart-${portfolio.id}" style="display: none; width: 100%; height: 200px;" class="mt-3"></canvas>
                        </div>
                        <div class="col-12 mt-4">
                            <div class="d-flex align-items-center">
                                <div class="avatar flex-shrink-0">
                                    <img src="/img/icons/unicons/wallet-info.png" alt="Credit Card" class="rounded" style="width: 40px; height: 40px;">
                                </div>
                                <div class="ms-3">
                                    <span class="fw-medium d-block text-muted">Current Market Value</span>
                                    <h3 class="mb-0">€${portfolio.currentMarketValue?.toFixed(3)}</h3>
                                    <small class="text-${portfolio.portfolioPercentage > 0 ? 'success' : 'danger'} fw-medium"><i class='bx ${portfolio.portfolioPercentage > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i> ${portfolio.portfolioPercentage.toFixed(3)}%</small>
                                </div>
                            </div>
                            <div class="mt-3">
                                ${portfolio.groupedItems.map(group => generateGroupedItemsHTML(group, portfolio.id)).join('')}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
}

function generateGroupedItemsHTML(group, portfolioId) {
  const sanitizedSymbol = sanitizeSymbol(group.symbol);
  const { roi, currentMarketValue, totalInvestment } = calculateROIandValue(group.items);
  return `
            <div class="card mt-3 shadow-sm rounded border-1">
                <div data-bs-toggle="collapse" data-bs-target="#group-${sanitizedSymbol}" aria-expanded="false" aria-controls="group-${sanitizedSymbol}" class="text-white d-flex justify-content-between align-items-center rounded" id="group-header-${sanitizedSymbol}">
                    <span class="badge bg-white text-primary">${group.symbol}</span>
                    <div id="sparkline-chart-${sanitizedSymbol}" class="sparkline-chart mb-3"></div>
                    <div class="d-flex align-items-center">
                        <span class="badge bg-white text-primary ms-1">Op. ${group.items.length}</span>
                        <button class="btn btn-link p-0 ms-1" type="button">
                            <span class="badge bg-white text-primary ms-1">
                                <small class="${group.items[0].change > 0 ? 'text-success' : 'text-danger'} fw-medium">
                                    <i class='bx ${group.items[0].change > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i>
                                    Today. ${group.items[0].change.toFixed(3)}%
                                </small>
                                </br>
                                <small class="${roi > 0 ? 'text-success' : 'text-danger'} fw-medium">
                                    <i class='bx ${roi > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i>
                                    ROI. ${(roi)}%
                                    </br>Invest. V. € ${(totalInvestment)}
                                    </br>Market V. € ${(currentMarketValue)}
                                    </br>Diff V. € ${(currentMarketValue - totalInvestment).toFixed(3)}
                                </small>
                            </span>
                        </button>
                    </div>
                </div>
                <div id="group-${sanitizedSymbol}" class="collapse" aria-labelledby="group-header-${sanitizedSymbol}" data-parent="#details-${portfolioId}">
                    <div class="card-body">
                        <div>
                            <p><strong>Current Price:</strong> ${group.items[0].currentPrice}</p>
                            <p><strong>Change:</strong> ${group.items[0].change}</p>
                            <p>
                                <strong>Percent Change:</strong>
                                <small class="${group.items[0].change > 0 ? 'text-success' : 'text-danger'} fw-medium">
                                    <i class='bx ${group.items[0].change > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i> ${group.items[0].change.toFixed(3)}%
                                </small>
                                <div class="progress mt-2">
                                    <div class="progress-bar ${group.items[0].change > 0 ? 'bg-success' : 'bg-danger'}" role="progressbar" style="width: ${Math.abs(group.items[0].change)}%;" aria-valuenow="${group.items[0].change.toFixed(3)}" aria-valuemin="0" aria-valuemax="100"></div>
                                </div>
                            </p>
                            <p><strong>High Price:</strong> ${group.items[0].highPrice.toFixed(3)}</p>
                            <p><strong>Low Price:</strong> ${group.items[0].lowPrice.toFixed(3)}</p>
                            <p><strong>Open Price:</strong> ${group.items[0].openPrice.toFixed(3)}</p>
                            <p><strong>Previous Close Price:</strong> ${group.items[0].previousClosePrice.toFixed(3)}</p>
                        </div>
                        <div id="candlestick-chart-${sanitizedSymbol}" class="img-fluid mb-3"></div>
                        <ul class="list-group list-group-flush">
                            ${group.items.map(item => generatePortfolioItemHTML(item)).join('')}
                        </ul>
                    </div>
                </div>
            </div>
        `;
}

function generatePortfolioItemHTML(item) {
  const roiValue = (item.quantity * item.currentPrice) - (item.purchasePrice * item.quantity) - item.commission;
  const totalValue = (item.purchasePrice * item.quantity) + item.commission;
  const roiPercentage = (((item.currentPrice - item.purchasePrice) * item.quantity - item.commission) / totalValue) * 100;

  return `
            <li style="background-color:#ff4560;" class="list-group-item text-white d-flex justify-content-between align-items-center rounded mb-2">
                <div>
                    <p class="mb-1"><strong>Commission:</strong> ${item.commission}</p>
                    <p class="mb-1"><strong>Quantity:</strong> ${item.quantity}</p>
                    <p class="mb-1"><strong>Total Op.:</strong> €${totalValue.toFixed(2)}</p>
                    <p class="mb-1"><strong>Purchase Date:</strong> ${formatDate(item.purchaseDate)}</p>
                    <p class="mb-1"><strong>Purchase Price:</strong> €${item.purchasePrice.toFixed(2)}</p>
                    <p class="mb-1"><strong>ROI (Value):</strong> €${roiValue.toFixed(2)}</p>
                    <p class="mb-1"><strong>ROI (Percentage):</strong> ${roiPercentage.toFixed(2)}%</p>
                </div>
                <div class="d-flex flex-column align-items-end">
                    <a href="/PortfolioItems/Edit/${item.id}" class="btn shadow rounded-pill btn-icon btn-primary">
                        <span class="tf-icons bx bx-edit-alt"></span>
                    </a>
                    <a href="/PortfolioItems/Details/${item.id}" class="btn shadow rounded-pill btn-icon btn-primary">
                        <span class="tf-icons bx bx-show-alt"></span>
                    </a>
                    <button type="button" onclick="deletePortfolioItem(${item.id})" class="btn rounded-pill shadow btn-icon btn-danger">
                        <span class="tf-icons bx bx-eraser"></span>
                    </button>
                </div>
            </li>
        `;
}

function calculateROIandValue(groupItems) {
  let totalInvestment = 0;
  let totalCurrentMarketValue = 0;

  groupItems.forEach(transaction => {
    const investment = (transaction.purchasePrice * transaction.quantity) + transaction.commission;
    const currentMarketValue = transaction.currentPrice * transaction.quantity;

    totalInvestment += investment;
    totalCurrentMarketValue += currentMarketValue;
  });

  const roi = ((totalCurrentMarketValue - totalInvestment) / totalInvestment) * 100;

  return {
    roi: roi.toFixed(2),
    currentMarketValue: totalCurrentMarketValue.toFixed(2),
    totalInvestment: totalInvestment.toFixed(2)
  };
}

function toggleChart(portfolioId) {
  const chartCanvas = document.getElementById(`chart-${portfolioId}`);
  if (chartCanvas.style.display === 'none') {
    chartCanvas.style.display = 'block';
  } else {
    chartCanvas.style.display = 'none';
  }
}
async function fetchWithRetry(url, options, retries = 5, delay = 1000) {
  for (let i = 0; i < retries; i++) {
    const response = await fetch(url, options);
    if (response.status !== 429) {
      return response;
    }
    await new Promise(resolve => setTimeout(resolve, delay));
    delay *= 2; // exponential backoff
  }
  throw new Error('Too many requests');
}
async function renderSparklineChart(elementId, symbol) {
  const url = `api/Dashboards/Stock/${symbol}?interval=5m`;

  try {
    const response = await fetch(url, {
      headers: {
        'Content-Type': 'application/json'
      },
      mode: 'cors'
    });

    if (!response.ok) {
      throw new Error('Network response was not ok');
    }

    const data = await response.json();

    // Map data to a format suitable for the area sparkline chart
    const marketData = data.map(item => ({
      x: new Date(item.timestamp), // Date object for x-axis
      y: item.close, // Close price for y-axis
      open: item.open // Open price for comparison
    }));

    // Determine colors based on open and close prices
    const colors = marketData.map(point => point.y >= point.open ? '#00C851' : '#FF4444'); // Green if close >= open, otherwise red

    const options = {
      chart: {
        type: 'area', // Set to area chart
        height: 80, // Height for sparkline
        sparkline: {
          enabled: true // Enable sparkline mode
        }
      },
      stroke: {
        width: 2,
        curve: 'smooth' // Smoother curve for the sparkline
      },
      fill: {
        type: 'gradient', // Gradient fill to mimic the area chart in the image
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.7,
          opacityTo: 0.2,
          stops: [0, 90, 100],
          colorStops: [
            {
              offset: 0,
              color: colors[0], // Start with the color of the first point
              opacity: 0.7
            },
            {
              offset: 100,
              color: colors[colors.length - 1], // End with the color of the last point
              opacity: 0.2
            }
          ]
        }
      },
      xaxis: {
        type: 'datetime', // Use datetime for x-axis
        labels: {
          show: false // Hide x-axis labels for sparkline
        },
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        }
      },
      yaxis: {
        labels: {
          show: false // Hide y-axis labels for sparkline
        }
      },
      series: [{
        name: 'Close Price',
        data: marketData.map(point => ({
          x: point.x,
          y: point.y,
          fillColor: point.y >= point.open ? '#00C851' : '#FF4444' // Set the color based on the trend
        }))
      }],
      colors: [colors[colors.length - 1]], // Line color based on the last point
      tooltip: {
        enabled: false // Disable tooltips
      }
    };

    // Render the chart
    const chart = new ApexCharts(document.querySelector(`#${elementId}`), options);
    chart.render();
  } catch (error) {
    console.error('Error fetching market data:', error);
  }
}

async function fetchWithRetry(url, options, retries = 5, delay = 1000) {
  for (let i = 0; i < retries; i++) {
    const response = await fetch(url, options);
    if (response.status !== 429) {
      return response;
    }
    await new Promise(resolve => setTimeout(resolve, delay));
    delay *= 2; // exponential backoff
  }
  throw new Error('Too many requests');
}
async function fetchMarketData() {
  const symbol = 'AAPL';
  const url = `https://query1.finance.yahoo.com/v8/finance/chart/${symbol}?interval=5m`;

  try {
    const response = await fetch(url);
    const data = await response.json();
    const marketData = data.map(point => point.close).filter(price => price !== null);

    return marketData;
  } catch (error) {
    console.error('Error fetching market data:', error);
    return [];
  }
}

async function renderChart() {
  const marketData = await fetchMarketData();

  const options = {
    chart: {
      type: 'line',
      height: 80,
      sparkline: {
        enabled: true
      }
    },
    stroke: {
      width: 2
    },
    series: [{
      name: 'Market Data',
      data: marketData
    }],
    colors: ['#FF1654'],
    title: {
      text: 'Market Data for Today',
      align: 'center',
      style: {
        fontSize: '20px'
      }
    }
  };

  const chart = new ApexCharts(document.querySelector("#sparkline-chart"), options);
  chart.render();
}


