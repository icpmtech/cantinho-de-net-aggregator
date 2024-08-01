document.addEventListener('DOMContentLoaded', function () {
  loadDashboardData();
  //loadPortfolioStatistics();
});

async function fetchData(url, method = 'GET', body = null) {
  const options = {
    method,
    headers: {
      'Content-Type': 'application/json'
    }
  };

  if (body) {
    options.body = JSON.stringify(body);
  }

  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(`Failed to fetch: ${response.statusText}`);
  }
  return response.json();
}

async function loadDashboardData() {
  try {
    showSpinner();
    const data = await fetchData('/api/Dashboards/data');
    updateDashboard(data);
    hideSpinner();
  } catch (error) {
    hideSpinner();
    showToast(error.message, 'danger');
  }
}

async function loadPortfolioStatistics() {
  try {
    showSpinner();
    const data = await fetchData('/api/Dashboards/portfolio-statistics');
    updatePortfolioStatistics(data);
    hideSpinner();
  } catch (error) {
    hideSpinner();
    showToast(error.message, 'danger');
  }
}

function updateDashboard(data) {
  document.getElementById('profit').innerText = `€${data.profit?.toFixed(2)}`;
  document.getElementById('dividends').innerText = `€${data.dividends?.toFixed(2)}`;
  document.getElementById('payments').innerText = `${data.payments}`;
  document.getElementById('operations').innerText = `${data.operations}`;

  const profitPercentageElement = document.getElementById('profit_percentage');
  const profitPercentage = data.profitPercentage?.toFixed(3);

  let arrowClass = '';
  let arrowIcon = '';
  if (profitPercentage > 0) {
    arrowClass = 'text-success';
    arrowIcon = 'bx-up-arrow-alt';
  } else if (profitPercentage < 0) {
    arrowClass = 'text-danger';
    arrowIcon = 'bx-down-arrow-alt';
  } else {
    arrowClass = 'text-muted';
    arrowIcon = 'bx-minus';
  }

  profitPercentageElement.className = `text-success fw-medium ${arrowClass}`;
  profitPercentageElement.innerHTML = `<i class='bx ${arrowIcon}'></i> ${profitPercentage}%`;
}

function updatePortfolioStatistics(data) {
  const portfolioStatisticsElement = document.getElementById('portfolioStatistics');


  document.getElementById('totalPortfolios').innerText = `€${data[0].totalInvestment.toFixed(2)}`;
  portfolioStatisticsElement.innerHTML = data.map(portfolio => `
                        <div class="portfolio-section">
                            <h5 class="portfolio-name">${portfolio.name}</h5>
                        </div>
                    `).join('');


}
function showToast(message, type = 'primary') {
  const toastContainer = document.getElementById('toastPlacement');
  const toastElement = document.createElement('div');
  toastElement.className = `bs-toast toast fade show bg-${type}`;
  toastElement.role = 'alert';
  toastElement.ariaLive = 'assertive';
  toastElement.ariaAtomic = 'true';

  toastElement.innerHTML = `
                <div class="toast-header">
                    <i class='bx bx-bell me-2'></i>
                    <div class="me-auto fw-medium">Bootstrap</div>
                    <small>Just now</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            `;

  toastContainer.appendChild(toastElement);

  // Automatically remove the toast after it disappears
  toastElement.addEventListener('hidden.bs.toast', () => {
    toastElement.remove();
  });

  // Initialize the toast
  new bootstrap.Toast(toastElement).show();
}



function showSpinner() {
  document.getElementById('spinner').style.display = 'block';
}

function hideSpinner() {
  document.getElementById('spinner').style.display = 'none';
}
