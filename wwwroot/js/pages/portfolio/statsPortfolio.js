async function loadPortfolios(dateRange = '1y') {
  // Show the loading spinner
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
    renderPortfolioList(portfolios, 'bar', dateRange); // Pass dateRange to renderPortfolioList
  } else {
    alert('Failed to load portfolios');
  }

  // Hide the loading spinner and show the portfolio list
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

function renderPortfolioList(portfolios, chartType = 'bar', dateRange = 'all') {
  const portfolioList = document.getElementById('portfolioList');
  portfolioList.dataset.portfolios = JSON.stringify(portfolios); // Save portfolios data for re-rendering
  portfolioList.dataset.dateRange = dateRange; // Save date range for re-rendering
  portfolioList.innerHTML = '';

  portfolios.forEach(portfolio => {
    const portfolioDiv = document.createElement('div');
    portfolioDiv.classList.add('portfolio-card', 'mb-4');

    portfolioDiv.innerHTML = `
                <div class="card">
                    <div class="card-body">
                        <div class="card-title d-flex align-items-center justify-content-between">
                            <h3>${portfolio.name}</h3>
                            <div class="dropdown">
                                <button class="btn p-0" type="button" id="portfolioActions-${portfolio.id}" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    <i class="bx bx-dots-vertical-rounded"></i>
                                </button>
                                <div class="dropdown-menu dropdown-menu-end" aria-labelledby="portfolioActions-${portfolio.id}">
                                    <a class="dropdown-item" href="javascript:showAddPortfolioItemModal(${portfolio.id});">Add Operations</a>
                                    <a class="dropdown-item" href="javascript:showEditPortfolioModal(${portfolio.id}, '${portfolio.name.replace(/'/g, "\\'")}');">Edit Portfolio</a>
                                    <a class="dropdown-item" href="javascript:deletePortfolio(${portfolio.id});">Delete</a>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="d-flex align-items-center">
                                    <div class="avatar flex-shrink-0">
                                        <img src="/img/icons/unicons/chart-success.png" alt="chart success" class="rounded">
                                    </div>
                                    <div class="ms-3">
                                        <span class="fw-medium d-block">Total Investment</span>
                                        <h3>€${portfolio.totalInvestment.toFixed(3)}</h3>
                                        <small class="text-success fw-medium"><i class='bx bx-up-arrow-alt'></i> +€0</small>
                                    </div>
                                </div>
                                <button class="btn btn-outline-primary btn-sm mt-3" onclick="toggleChart(${portfolio.id})">Toggle Chart</button>
                                <canvas id="chart-${portfolio.id}" style="display: none;"></canvas>
                            </div>
                            <div class="col-md-6">
                                <div class="d-flex align-items-center">
                                    <div class="avatar flex-shrink-0">
                                        <img src="/img/icons/unicons/wallet-info.png" alt="Credit Card" class="rounded">
                                    </div>
                                    <div class="ms-3">
                                        <span>Current Market Value</span>
                                        <h3>€${portfolio.currentMarketValue?.toFixed(3)}</h3>
                                                    <small class="text-success fw-medium"><i class='bx bx-up-arrow-alt'></i> ${portfolio.portfolioPercentage.toFixed(3)}%</small>
                                    </div>
                                </div>
                                <button class="btn btn-outline-primary btn-sm mt-3" data-bs-toggle="collapse" data-bs-target="#details-${portfolio.id}" aria-expanded="false" aria-controls="details-${portfolio.id}">
                                    ${portfolio.items.length > 0 ? 'View More' : 'No Items to Display'}
                                </button>
                                <div id="details-${portfolio.id}" class="collapse mt-3">
                                    <ul class="list-group list-group-flush">
                                        ${portfolio.items.map(item => `
                                            <li class="list-group-item">
                                                <div class="d-flex justify-content-between align-items-start">
                                                    <div>
                                                        <p><strong>Symbol:</strong> ${item.symbol}</p>
                                                        <p><strong>Quantity:</strong> ${item.quantity}</p>
                                                        <p><strong>Current Price:</strong> ${item.currentPrice}</p>
                                                        <p><strong>Change:</strong> ${item.change}</p>
                                                        <p>
                                                            <strong>Percent Change:</strong>
                                                            <small class="${item.percentChange > 0 ? 'text-success' : 'text-danger'} fw-medium">
                                                                <i class='bx ${item.percentChange > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i> ${item.percentChange}%
                                                            </small>
                                                            <div class="progress">
                                                                <div class="progress-bar ${item.percentChange > 0 ? 'bg-success' : 'bg-danger'}" role="progressbar" style="width: ${Math.abs(item.percentChange)}%;" aria-valuenow="${item.percentChange}" aria-valuemin="0" aria-valuemax="100"></div>
                                                            </div>
                                                        </p>
                                                        <p><strong>High Price:</strong> ${item.highPrice}</p>
                                                        <p><strong>Low Price:</strong> ${item.lowPrice}</p>
                                                        <p><strong>Open Price:</strong> ${item.openPrice}</p>
                                                        <p><strong>Previous Close Price:</strong> ${item.previousClosePrice}</p>
                                                        <p><strong>Purchase Date:</strong> ${formatDate(item.purchaseDate)}</p>
                                                    </div>
                                                    <div class="mt-3">
                                                        <div id="candlestick-chart-${item.id}" class="img-fluid"></div>
                                                        <div class="btn-group mb-3" role="group" aria-label="Date Range">
                                                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="renderCandlestickChart(${item.id}, '${item.symbol}', '${chartType}', '1d')">1 Day</button>
                                                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="renderCandlestickChart(${item.id}, '${item.symbol}', '${chartType}', '5d')">5 Days</button>
                                                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="renderCandlestickChart(${item.id}, '${item.symbol}', '${chartType}', '1m')">1 Month</button>
                                                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="renderCandlestickChart(${item.id}, '${item.symbol}', '${chartType}', '1y')">1 Year</button>
                                                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="renderCandlestickChart(${item.id}, '${item.symbol}', '${chartType}', '5y')">5 Years</button>
                                                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="renderCandlestickChart(${item.id}, '${item.symbol}', '${chartType}', 'all')">All</button>
                                                        </div>
                                                        <div class="d-flex flex-column justify-content-start">
                                                            <button class="btn btn-primary btn-sm mb-1" onclick="showEditPortfolioItemModal(${item.id})">Edit</button>
                                                             <button class="btn btn-primary btn-sm mb-1" onclick="showEditPortfolioItemTransactionModal(${item.id})">Edit Transaction</button>
                                                            <button class="btn btn-primary btn-sm mb-1" onclick="showAddPortfolioItemTransactionModal(${item.id})">Add Transaction</button>
                                                            <button class="btn btn-danger btn-sm" onclick="deletePortfolioItem(${item.id})">Delete</button>
                                                        </div>
                                                    </div>
                                                </div>
                                            </li>
                                        `).join('')}
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
    portfolioList.appendChild(portfolioDiv);
    renderChart(portfolio, chartType);
    portfolio.items.forEach(item => renderCandlestickChart(item.id, item.symbol, chartType, dateRange));
  });
}


