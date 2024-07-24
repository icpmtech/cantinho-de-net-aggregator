function showAddPortfolioItemModal(portfolioId) {
  document.getElementById('addPortfolioItemForm').reset();
  document.getElementById('portfolioId').value = portfolioId;
  new bootstrap.Modal(document.getElementById('addPortfolioItemModal')).show();
}

async function showEditPortfolioItemModal(itemId) {
  const response = await fetch(`/api/PortfolioItem/item/${itemId}`);
  const item = await response.json();

  if (response.ok) {
    document.getElementById('editPortfolioItemId').value = item.id;
    document.getElementById('portfolioeditPortfolioItemId').value = item.portfolioId;
    document.getElementById('editItemSymbol').value = item.symbol;
    document.getElementById('editItemQuantity').value = item.quantity;
    document.getElementById('editItemPurchasePrice').value = item.purchasePrice;
    const purchaseDate = new Date(item.purchaseDate);
    const formattedDate = purchaseDate.toISOString().substring(0, 10);
    document.getElementById('editItemPurchaseDate').value = formattedDate;
    new bootstrap.Modal(document.getElementById('editPortfolioItemModal')).show();
  } else {
    alert('Failed to load portfolio item');
  }
}


document.getElementById('addPortfolioItemForm').addEventListener('submit', async function (event) {
  event.preventDefault();
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
    alert('Failed to add portfolio item');
  }
});

document.getElementById('editPortfolioItemForm').addEventListener('submit', async function (event) {
  event.preventDefault();
  const id = document.getElementById('editPortfolioItemId').value;
  const portfolioId = document.getElementById('portfolioeditPortfolioItemId').value;
  const symbol = document.getElementById('editItemSymbol').value;
  const quantity = document.getElementById('editItemQuantity').value;
  const purchasePrice = document.getElementById('editItemPurchasePrice').value;
  const purchaseDate = document.getElementById('editItemPurchaseDate').value;

  try {
    const response = await fetch(`/api/PortfolioItem/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ portfolioId, symbol, quantity, purchasePrice, purchaseDate, id })
    });

    if (response.ok) {
      bootstrap.Modal.getInstance(document.getElementById('editPortfolioItemModal')).hide();
      loadPortfolios();
    } else {
      const errorData = await response.json();
      showError(errorData.message || 'Failed to edit portfolio item');
    }
  } catch (error) {
    showError('An error occurred while editing the portfolio item');
  }
});
