// Fetch profit data from the API with error handling
async function fetchProfitData() {
  try {
    const response = await fetch('/api/Dashboards/all-time-data', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      }
    });
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Failed to fetch profit data:', error);
    return null; // Return null or an empty object to indicate failure
  }
}

// Validate the data returned from the API to ensure it meets the expected structure
function validateProfitData(data) {
  if (!data || !Array.isArray(data.symbolData)) {
    return false;
  }

  return data.symbolData.every(symbolData =>
    typeof symbolData.symbol === 'string' &&
    Array.isArray(symbolData.data) &&
    symbolData.data.every(d =>
      typeof d.year === 'number' &&
      typeof d.month === 'number' &&
      typeof d.difference === 'number'
    )
  );
}

// Create a heatmap for a portfolio
function createHeatmap(portfolio, index, profitData) {
  const heatmapContainer = document.getElementById('heatmapContainer');
  const heatmapDiv = document.createElement('div');
  heatmapDiv.id = `heatmap${index + 1}`;
  heatmapDiv.classList.add('heatmap');
  heatmapContainer.appendChild(heatmapDiv);

  const stockData = portfolio.items.map(stock => {
    const symbolData = profitData.symbolData.find(p => p.symbol === stock.symbol);
    const data = symbolData ? symbolData.data.map(d => ({
      x: `${d.year}-${d.month.toString().padStart(2, '0')}`,
      y: d.difference
    })) : [];

    return {
      name: stock.symbol,
      data: data
    };
  });

  const options = {
    chart: {
      type: 'heatmap',
      height: 600 // Adjust height dynamically
    },
    series: stockData,
    plotOptions: {
      heatmap: {
        shadeIntensity: 0.5,
        radius: 0,
        useFillColorAsStroke: false,
        colorScale: {
          ranges: [{
            from: -1000000,
            to: -5000,
            color: '#FF0000',
            name: 'severe negative'
          }, {
            from: -5000,
            to: 0,
            color: '#FFA500',
            name: 'negative'
          }, {
            from: 0.001,
            to: 5000,
            color: '#00FF00',
            name: 'positive'
          }, {
            from: 5000,
            to: 10000000000,
            color: '#008000',
            name: 'high positive'
          }]
        }
      }
    },
    dataLabels: {
      enabled: false
    },
    title: {
      text: `Portfolio ${portfolio.name} Performance`
    }
  };

  const chart = new ApexCharts(document.querySelector(`#heatmap${index + 1}`), options);
  chart.render();
}

// Render portfolio heatmaps
async function renderPortfolioHeatMaps(portfolios) {
  const heatmapContainer = document.getElementById('heatmapContainer');
  const portfoliosNumbers = document.getElementById('portfolios-numbers');

  heatmapContainer.innerHTML = '<p>Loading...</p>'; // Display loading text
  portfoliosNumbers.innerHTML = '';

  const profitData = await fetchProfitData();

  if (!profitData) {
    heatmapContainer.innerHTML = '<p>Failed to load data.</p>';
    return;
  }

  if (!validateProfitData(profitData)) {
    heatmapContainer.innerHTML = '<p>Invalid data format.</p>';
    return;
  }

  heatmapContainer.innerHTML = ''; // Clear loading text


  if (portfolios.length === 0) {
    heatmapContainer.innerHTML = '<p>No portfolios available.</p>';
    return;
  }

  portfolios.forEach((portfolio, index) => {
    createHeatmap(portfolio, index, profitData);
  });
}
