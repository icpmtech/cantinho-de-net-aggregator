
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
function sanitizeSymbol(symbol) {
  return symbol.replace(/[^a-zA-Z0-9-_]/g, '_');
}

function renderPortfolioList(portfolios, chartType = 'bar', dateRange = 'all') {
  const portfolioList = document.getElementById('portfolioList');
  portfolioList.dataset.portfolios = JSON.stringify(portfolios); // Save portfolios data for re-rendering
  portfolioList.dataset.dateRange = dateRange; // Save date range for re-rendering
  portfolioList.innerHTML = '';

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
              <a class="dropdown-item" href="javascript:showAddPortfolioItemModal(${portfolio.id});">Add Operations</a>
              <a class="dropdown-item" href="javascript:showEditPortfolioModal(${portfolio.id}, '${portfolio.name.replace(/'/g, "\\'")}');">Edit Portfolio</a>
              <a class="dropdown-item" href="javascript:deletePortfolio(${portfolio.id});">Delete</a>
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
                  <small class="text-success fw-medium"><i class='bx bx-up-arrow-alt'></i> +€0</small>
                </div>
              </div>
              <button class="btn badge bg-primary text-white" onclick="toggleChart(${portfolio.id})">TOGGLE CHART</button>
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
  return `
    <div class="card mt-3 shadow-sm rounded border-0">
      <div data-bs-toggle="collapse" data-bs-target="#group-${sanitizedSymbol}" aria-expanded="false" aria-controls="group-${sanitizedSymbol}" class="card-header bg-primary text-white d-flex justify-content-between align-items-center rounded" id="group-header-${sanitizedSymbol}">
        <span class="badge bg-white text-primary">${group.symbol}</span>
        <div class="d-flex align-items-center">
          <span class="badge bg-white text-primary ms-1">Op. ${group.items.length}</span>
          <button class="btn btn-link p-0 ms-1" type="button">
            <span class="badge bg-white text-primary ms-1">
              <small class="${group.items[0].change > 0 ? 'text-success' : 'text-danger'} fw-medium">
                <i class='bx ${group.items[0].change > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i> ${group.items[0].change}% </br>Op. €${(group.items[0].purchasePrice * group.items[0].quantity).toFixed(3)}
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
                <i class='bx ${group.items[0].change > 0 ? 'bx-up-arrow-alt' : 'bx-down-arrow-alt'}'></i> ${group.items[0].change}%
              </small>
              <div class="progress mt-2">
                <div class="progress-bar ${group.items[0].change > 0 ? 'bg-success' : 'bg-danger'}" role="progressbar" style="width: ${Math.abs(group.items[0].change)}%;" aria-valuenow="${group.items[0].change}" aria-valuemin="0" aria-valuemax="100"></div>
              </div>
            </p>
            <p><strong>High Price:</strong> ${group.items[0].highPrice}</p>
            <p><strong>Low Price:</strong> ${group.items[0].lowPrice}</p>
            <p><strong>Open Price:</strong> ${group.items[0].openPrice}</p>
            <p><strong>Previous Close Price:</strong> ${group.items[0].previousClosePrice}</p>
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
  const roiValue = item.quantity * item.currentPrice - item.purchasePrice * item.quantity;
  const totalValue =  item.purchasePrice * item.quantity;
  const roiPercentage = ((item.currentPrice - item.purchasePrice) / item.purchasePrice) * 100;

  return `
    <li class="list-group-item bg-info text-white d-flex justify-content-between align-items-center rounded mb-2">
      <div>
        <p class="mb-1"><strong>Quantity:</strong> ${item.quantity}</p>
        <p class="mb-1"><strong>Total Op.:</strong> €${totalValue.toFixed(2) }</p>
        <p class="mb-1"><strong>Purchase Date:</strong> ${formatDate(item.purchaseDate)}</p>
        <p class="mb-1"><strong>Purchase Price:</strong> €${item.purchasePrice.toFixed(2)}</p>
        <p class="mb-1"><strong>ROI (Value):</strong> €${roiValue.toFixed(2)}</p>
        <p class="mb-1"><strong>ROI (Percentage):</strong> ${roiPercentage.toFixed(2)}%</p>
      </div>
      <div class="d-flex flex-column align-items-end">
        <button class="btn btn-primary btn-sm mb-1" onclick="showEditPortfolioItemModal(${item.id})">Edit</button>
        <button class="btn btn-danger btn-sm" onclick="deletePortfolioItem(${item.id})">Delete</button>
      </div>
    </li>
  `;
}



function toggleChart(portfolioId) {
  const chartCanvas = document.getElementById(`chart-${portfolioId}`);
  if (chartCanvas.style.display === 'none') {
    chartCanvas.style.display = 'block';
  } else {
    chartCanvas.style.display = 'none';
  }
}


function toggleChart(portfolioId) {
  const chartCanvas = document.getElementById(`chart-${portfolioId}`);
  if (chartCanvas.style.display === 'none') {
    chartCanvas.style.display = 'block';
  } else {
    chartCanvas.style.display = 'none';
  }
}
