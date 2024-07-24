async function exportPortfolios(fileType) {
  const response = await fetch(`/api/Portfolio/Export?fileType=${fileType}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = fileType === 'xlsx' ? 'portfolios.xlsx' : 'portfolios.csv';
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
  } else {
    alert('Failed to export portfolios');
  }
}

async function importPortfolios(event) {
  const file = event.target.files[0];
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('/api/Portfolio/Import', {
    method: 'POST',
    body: formData
  });

  if (response.ok) {
    loadPortfolios();
  } else {
    alert('Failed to import portfolios');
  }
}

