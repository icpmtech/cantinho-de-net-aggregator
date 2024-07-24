
function showAddPortfolioModal() {
  document.getElementById('addPortfolioForm').reset();
  new bootstrap.Modal(document.getElementById('addPortfolioModal')).show();
}

function showEditPortfolioModal(id, name) {
  document.getElementById('editPortfolioId').value = id;
  document.getElementById('editPortfolioName').value = name;
  new bootstrap.Modal(document.getElementById('editPortfolioModal')).show();
}


async function savePortfolioItem() {
  const portfolioId = document.getElementById('portfolioId').value;
  const symbol = document.getElementById('itemSymbol').value;
  const quantity = document.getElementById('itemQuantity').value;
  const purchasePrice = document.getElementById('itemPurchasePrice').value;
  const purchaseDate = document.getElementById('itemPurchaseDate').value;
  const response = await fetch('/api/PortfolioItem', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ portfolioId, symbol, quantity, purchasePrice, purchaseDate })
  });

  if (response.ok) {
    bootstrap.Modal.getInstance(document.getElementById('addPortfolioItemModal')).hide();
    loadPortfolios();
  } else {
    alert('Failed to save portfolio item');
  }
}

async function deletePortfolioItem(id) {
  const response = await fetch(`/api/PortfolioItem/${id}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    loadPortfolios();
  } else {
    alert('Failed to delete portfolio item');
  }
}

async function deletePortfolio(id) {
  const response = await fetch(`/api/Portfolio/${id}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    loadPortfolios();
  } else {
    alert('Failed to delete portfolio');
  }
}

document.getElementById('addPortfolioForm').addEventListener('submit', async function (event) {
  event.preventDefault();
  const name = document.getElementById('portfolioName').value;

  const response = await fetch('/api/Portfolio', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ name })
  });

  if (response.ok) {
    bootstrap.Modal.getInstance(document.getElementById('addPortfolioModal')).hide();
    loadPortfolios();
  } else {
    alert('Failed to add portfolio');
  }
});

document.getElementById('editPortfolioForm').addEventListener('submit', async function (event) {
  event.preventDefault();
  const id = document.getElementById('editPortfolioId').value;
  const name = document.getElementById('editPortfolioName').value;

  const response = await fetch(`/api/Portfolio/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ id, name })
  });

  if (response.ok) {
    bootstrap.Modal.getInstance(document.getElementById('editPortfolioModal')).hide();
    loadPortfolios();
  } else {
    alert('Failed to edit portfolio');
  }
});
