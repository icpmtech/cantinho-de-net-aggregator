function showError(message) {
  const errorElement = document.createElement('div');
  errorElement.className = 'alert alert-danger';
  errorElement.textContent = message;
  document.getElementById('editPortfolioItemForm').prepend(errorElement);

  setTimeout(() => {
    errorElement.remove();
  }, 5000);
}
function toggleChart(portfolioId) {
  const chart = document.getElementById(`chart-${portfolioId}`);
  chart.style.display = chart.style.display === 'none' ? 'block' : 'none';
}
function getDateRange(range) {
  const endDate = new Date();
  const startDate = new Date();

  switch (range) {
    case '1d':
      startDate.setDate(endDate.getDate() - 1);
      break;
    case '5d':
      startDate.setDate(endDate.getDate() - 5);
      break;
    case '1m':
      startDate.setMonth(endDate.getMonth() - 1);
      break;
    case '1y':
      startDate.setFullYear(endDate.getFullYear() - 1);
      break;
    case '5y':
      startDate.setFullYear(endDate.getFullYear() - 5);
      break;
    case 'all':
      startDate.setFullYear(1900);
      break;
    default:
      startDate.setFullYear(2000); // Or any default start date
      break;
  }

  return { startDate, endDate };
}
function formatDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
}

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
