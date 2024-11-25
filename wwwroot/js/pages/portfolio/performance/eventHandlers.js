// eventHandlers.js

// Import necessary functions
import { fetchPortfolioData } from "./fetchPortfolioData.js";
import { mapData } from './mapData.js';
import { renderCharts } from './chartRenderer.js';
import { renderHistoricalChart } from './renderHistoricalChart.js';
import { showError } from "./showError.js";
import { hideError } from "./hideError.js";

// Function to initialize event handlers
export function initializeEventHandlers(dataFromApi) {
  // Event listener for 'Update Portfolio' button
  document.getElementById('updatePortfolio').addEventListener('click', async () => {
   
    // Get the loading indicator element
    const loadingIndicator = document.getElementById('loading');
    // Show the loading indicator
    loadingIndicator.classList.remove('d-none');
    hideError();

    try {
      const json = await fetchPortfolioData();
      dataFromApi = await mapData(json);
      renderCharts(dataFromApi);
    } catch (error) {
      console.error('Error:', error);
      showError('Error loading data. Please try again later.');
    } finally {
      loadingIndicator.classList.add('d-none');
    }
  });

  // Event listener for time period selection
  document.getElementById('timePeriodSelect').addEventListener('change', function () {
    const selectedPeriod = this.value;
    renderHistoricalChart(dataFromApi, selectedPeriod);
  });
}
