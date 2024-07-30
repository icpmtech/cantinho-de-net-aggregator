/**
 * Dashboard Analytics Portfolio
 */

'use strict';

(function () {

    document.addEventListener('DOMContentLoaded', function () {
      loadDashboardData();
    loadPortfolioStatistics();
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
                const data = await fetchData('/api/Dashboards/data');
    updateDashboard(data);
            } catch (error) {
      alert(error.message);
            }
        }

    async function loadPortfolioStatistics() {
            try {
                const data = await fetchData('/api/Dashboards/portfolio-statistics');
    updatePortfolioStatistics(data);
            } catch (error) {
      alert(error.message);
            }
        }

    function updateDashboard(data) {
      document.getElementById('profit').innerText = `€${data.profit?.toFixed(2)}`;
    document.getElementById('dividends').innerText = `€${data.dividends?.toFixed(2)}`;
    document.getElementById('payments').innerText = `${data.payments}`;
    document.getElementById('operations').innerText = `${data.operations}`;
           // document.getElementById('totalRevenue').innerText = `$${data.totalRevenue?.toFixed(2)}`;
           // document.getElementById('growth').innerText = `${data.growth}%`;
           // document.getElementById('portfolioGrowth').innerText = `${data.portfolioGrowth}% Portfolio Growth`;
          //  document.getElementById('yearlyReport').innerText = `$${data.yearlyReport?.toFixed(2)}`;
         
            
        }
  let allStatistics = [];
  let currentPage = 0;
  const itemsPerPage = 5;
  function updatePortfolioStatistics(data) {
    const portfolioStatisticsElement = document.getElementById('portfolioStatistics');

    const totalInvestment = data.reduce((sum, portfolio) => sum + portfolio.items.reduce((portfolioSum, item) => portfolioSum + item.totalInvestment, 0), 0);
    const currentMarketValue = data.reduce((sum, portfolio) => sum + portfolio.items.reduce((portfolioSum, item) => portfolioSum + item.currentMarketValue, 0), 0);
    const progressPercentage = (currentMarketValue / totalInvestment) * 100;

    document.getElementById('totalPortfolios').innerText = `€${totalInvestment.toFixed(2)}`;
    document.getElementById('currentMarketValue').innerText = `€${currentMarketValue.toFixed(2)}`;
    document.getElementById('progressPercentage').innerText = `${progressPercentage.toFixed(2)}%`;

    const displayData = data.slice(0, (currentPage + 1) * itemsPerPage);

    portfolioStatisticsElement.innerHTML = displayData.map(portfolio => `
            <div class="portfolio-section">
              <h5 class="portfolio-name">${portfolio.name}</h5>
              <ul class="list-unstyled">
                ${portfolio.items.map(item => `
                  <li class="d-flex mb-4 pb-1">
                    <div class="avatar flex-shrink-0 me-3">
                      <span class="avatar-initial rounded bg-label-primary"><i class='bx bx-mobile-alt'></i></span>
                    </div>
                    <div class="d-flex w-100 flex-wrap align-items-center justify-content-between gap-2">
                      <div class="me-2">
                        <h6 class="mb-0">${item.symbol}</h6>
                        <small class="text-muted">Total Investment: €${item.totalInvestment.toFixed(2)}</small>
                        <small class="text-muted">Current Market Value: €${item.currentMarketValue.toFixed(2)}</small>
                        <small class="text-muted">Dividends: €${item.dividends.toFixed(2)}</small>
                      </div>
                    </div>
                  </li>
                `).join('')}
              </ul>
            </div>
        `).join('');
  }
  function viewMore() {
    currentPage++;
    updatePortfolioStatistics(allStatistics);
    toggleViewMoreButton();
  }

  function toggleViewMoreButton() {
    const viewMoreBtn = document.getElementById('viewMoreBtn');
    if ((currentPage + 1) * itemsPerPage < allStatistics.length) {
      viewMoreBtn.style.display = 'block';
    } else {
      viewMoreBtn.style.display = 'none';
    }
  }



})();
