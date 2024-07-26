document.addEventListener('DOMContentLoaded', function () {
  loadPortfolios();
  loadSymbols();
  loadCompanies();
  loadDashboardData();
  loadTotalPortfolioPercentage();
  loadDashboardDataOverAllPortfolio();
});



async function loadCompanies() {
  const response = await fetch('/api/company', {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    const companyies = await response.json();
    const companySelect = document.getElementById('itemCompany');
    const editcompanySelect = document.getElementById('editItemCompany');
    companyies.forEach(company => {
      const option = document.createElement('option');
      option.value = company.id;
      option.text = company.name;
      companySelect.appendChild(option);

      const editOption = document.createElement('option');
      option.value = company.id;
      option.text = company.name
      editcompanySelect.appendChild(editOption);
    });
  } else {
    alert('Failed to load companies');
  }
}


async function loadDashboardDataOverAllPortfolio() {
  try {
    const response = await fetch('/api/Portfolio/portfolio-overall-stats');
    const data = await response.json();

    // Update total market value
    document.getElementById('totalMarketValue').innerText = `€${data.totalMarketValue.toFixed(2)}`;

    // Update total custom market value
    document.getElementById('totalCustMarketValue').innerText = `€${data.totalCustMarketValue.toFixed(2)}`;

    // Update total difference value
    document.getElementById('totalDifferenceValue').innerText = `€${data.totalDifferenceValue.toFixed(2)}`;

    // Update total difference percentage with conditional styling
    const totalDifferencePercentageElement = document.getElementById('totalDifferencePercentage');
    totalDifferencePercentageElement.innerText = `${data.totalDifferencePercentage.toFixed(2)}%`;

    // Add logic for positive or negative difference percentage
    const totalDifferenceIconElement = document.getElementById('totalDifferenceIcon');
    if (data.totalDifferencePercentage >= 0) {
      totalDifferencePercentageElement.classList.remove('text-danger');
      totalDifferencePercentageElement.classList.add('text-success');
      totalDifferenceIconElement.classList.remove('bx-down-arrow-alt');
      totalDifferenceIconElement.classList.add('bx-up-arrow-alt');
    } else {
      totalDifferencePercentageElement.classList.remove('text-success');
      totalDifferencePercentageElement.classList.add('text-danger');
      totalDifferenceIconElement.classList.remove('bx-up-arrow-alt');
      totalDifferenceIconElement.classList.add('bx-down-arrow-alt');
    }
  } catch (error) {
    alert(error.message);
  }
}
async function loadTotalPortfolioOverallStats() {
  try {
    const response = await fetch('/api/Portfolio/portfolio-overall-stats');
    const data = await response.json();

    // Update total market value
    document.getElementById('totalMarketValue').innerText = `€${data.totalMarketValue.toFixed(2)}`;

    // Update total custom market value
    document.getElementById('totalCustMarketValue').innerText = `€${data.totalCustMarketValue.toFixed(2)}`;

    // Update total difference value
    document.getElementById('totalDifferenceValue').innerText = `€${data.totalDifferenceValue.toFixed(2)}`;

    // Update total difference percentage with conditional styling
    const totalDifferencePercentageElement = document.getElementById('totalDifferencePercentage');
    totalDifferencePercentageElement.innerText = `${data.totalDifferencePercentage.toFixed(2)}%`;

    // Add logic for positive or negative difference percentage
    const totalDifferenceIconElement = document.getElementById('totalDifferenceIcon');
    if (data.totalDifferencePercentage >= 0) {
      totalDifferencePercentageElement.classList.remove('text-danger');
      totalDifferencePercentageElement.classList.add('text-success');
      totalDifferenceIconElement.classList.remove('bx-down-arrow-alt');
      totalDifferenceIconElement.classList.add('bx-up-arrow-alt');
    } else {
      totalDifferencePercentageElement.classList.remove('text-success');
      totalDifferencePercentageElement.classList.add('text-danger');
      totalDifferenceIconElement.classList.remove('bx-up-arrow-alt');
      totalDifferenceIconElement.classList.add('bx-down-arrow-alt');
    }
  } catch (error) {
    alert(error.message);
  }
}
function updateDashboardDataOverAllPortfolio(data) {
  document.getElementById('totalMarketValue').innerText = `€${data.totalMarketValue.toFixed(2)}`;
  document.getElementById('totalCustMarketValue').innerText = `€${data.totalCustMarketValue.toFixed(2)}`;
  document.getElementById('totalDifferenceValue').innerText = `€${data.totalDifferenceValue.toFixed(2)}`;
  document.getElementById('totalDifferencePercentage').innerText = `${data.totalDifferencePercentage.toFixed(2)}%`;
}

async function loadDashboardData() {
  try {
    const data = await fetchData('/api/Dashboards/data');
    updateDashboard(data);
  } catch (error) {
    alert(error.message);
  }
}

async function loadTotalPortfolioPercentage() {
  try {
    const data = await fetchData('/api/Portfolio/total-percentage');
    document.getElementById('totalPortfolioPercentage').innerText = `${data.totalPercentage?.toFixed(3)}%`;
    document.getElementById('totalProfitDifferencePercentage').innerText = `${data.totalWithDividendsPercentage?.toFixed(3)}%`;
    document.getElementById('totalProfit').innerText = `€${data.totalProfit?.toFixed(3)}`;
  } catch (error) {
    alert(error.message);
  }
}

function updateDashboard(data) {
  //document.getElementById('profit').innerText = `€${data.profit?.toFixed(2)}`;
  document.getElementById('dividends').innerText = `€${data.dividends?.toFixed(2)}`;
}


