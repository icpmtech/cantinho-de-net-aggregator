function showAddPortfolioItemModal(portfolioId) {
  document.getElementById('addPortfolioItemForm').reset();
  document.getElementById('portfolioId').value = portfolioId;
  new bootstrap.Modal(document.getElementById('addPortfolioItemModal')).show();
}

function showAIAnalisysPortfolioItemModal(portfolioId) {
  document.getElementById('aiPortfolioItemForm').reset();
  document.getElementById('portfolioId').value = portfolioId;
  new bootstrap.Modal(document.getElementById('aiPortfolioItemModal')).show();
}

async function showEditPortfolioItemModal(itemId) {
  const response = await fetch(`/api/PortfolioItem/item/${itemId}`);
  const item = await response.json();

  if (response.ok) {
    document.getElementById('editPortfolioItemId').value = item.id;
    document.getElementById('portfolioeditPortfolioItemId').value = item.portfolioId;
    document.getElementById('editItemSymbol').value = item.symbol;
    document.getElementById('editItemCompany').value = item.companyId;
    document.getElementById('editItemQuantity').value = item.quantity;
    document.getElementById('editItemPurchasePrice').value = item.purchasePrice;
    document.getElementById('editItemCommission').value = item.commission;
    document.getElementById('editItemOperationType').value = item.operationType;
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
  const company = document.getElementById('itemCompany').value;
  const quantity = document.getElementById('itemQuantity').value;
  const purchasePrice = document.getElementById('itemPurchasePrice').value;
  const purchaseDate = document.getElementById('itemPurchaseDate').value;
  const commission = document.getElementById('itemCommission').value;
  const operationType = document.getElementById('itemOperationType').value;
  const response = await fetch('/api/PortfolioItem', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ portfolioId, symbol, quantity, purchasePrice, purchaseDate, commission, operationType, company })
  });

  if (response.ok) {
    bootstrap.Modal.getInstance(document.getElementById('addPortfolioItemModal')).hide();
    loadPortfolios();
  } else {
    alert('Failed to add portfolio item');
  }
});
document.getElementById('aiPortfolioItemForm').addEventListener('submit', async function (event) {
  event.preventDefault();

  // Extract and validate the Portfolio ID
  const portfolioIdValue = document.getElementById('portfolioId').value;
  const portfolioId = parseInt(portfolioIdValue, 10);

  if (isNaN(portfolioId) || portfolioId <= 0) {
    alert('Please enter a valid Portfolio ID.');
    return;
  }

  // Extract the prompt text
  const promptTextValue = document.getElementById('promptText').value.trim();

 
 

  try {
    // Show loading spinner
    document.getElementById('loadingSpinner').style.display = 'block';
    // Disable the submit button to prevent multiple submissions
    const submitButton = event.target.querySelector('button[type="submit"]');
    submitButton.disabled = true;

    // Prepare the payload
    const payload = {
      PortfolioId: portfolioId,
      PromptInput: promptTextValue || null // Send null if promptText is empty
    };

    // Send the POST request to analyze sentiment
    const response = await fetch('/api/Llm/analyze-sentiment-for-portfolio', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      // Assuming the server returns an HTML string
      const sentimentReport = await response.text();

      // Inject the sentiment report into the container
      const reportContainer = document.getElementById('sentimentReportContainer');
      if (reportContainer) {
        reportContainer.querySelector('.card-body').innerHTML = sentimentReport;
        reportContainer.style.display = 'block';
      }

      // Hide the loading spinner
      document.getElementById('loadingSpinner').style.display = 'none';

 
    } else {
      // Attempt to parse and display the error message from the server
      let errorMessage = 'Failed to analyze portfolio sentiment.';
      try {
        errorMessage = await response.text();
      } catch (e) {
        console.error('Failed to parse error message:', e);
      }
      alert(`Error: ${errorMessage}`);
    }
  } catch (error) {
    console.error('Network or unexpected error:', error);
    alert('An unexpected error occurred while analyzing sentiment.');
  } finally {
    // Hide loading spinner and re-enable the submit button
    document.getElementById('loadingSpinner').style.display = 'none';
    const submitButton = event.target.querySelector('button[type="submit"]');
    submitButton.disabled = false;
  }
});


document.getElementById('editPortfolioItemForm').addEventListener('submit', async function (event) {
  event.preventDefault();
  const id = document.getElementById('editPortfolioItemId').value;
  const portfolioId = document.getElementById('portfolioeditPortfolioItemId').value;
  const symbol = document.getElementById('editItemSymbol').value;
  const company = document.getElementById('editItemCompany').value;
  const quantity = document.getElementById('editItemQuantity').value;
  const purchasePrice = document.getElementById('editItemPurchasePrice').value;
  const purchaseDate = document.getElementById('editItemPurchaseDate').value;
  const commission = document.getElementById('editItemCommission').value;
  const operationType = document.getElementById('editItemOperationType').value;
  try {
    const response = await fetch(`/api/PortfolioItem/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ portfolioId, symbol, quantity, purchasePrice, purchaseDate, id, commission, operationType, company })
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
