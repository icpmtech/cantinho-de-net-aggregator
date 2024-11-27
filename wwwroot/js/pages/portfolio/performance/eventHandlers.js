// eventHandlers.js

// Import necessary functions
import { fetchPortfolioData } from "./fetchPortfolioData.js";
import { mapData } from './mapData.js';
import { renderCharts } from './chartRenderer.js';
import { renderHistoricalChart } from './renderHistoricalChart.js';
import { showError } from "./showError.js";
import { hideError } from "./hideError.js";
import { showSkeletons, hideSkeletons } from './skeletons.js'; // Import skeleton functions

// Function to initialize event handlers
export function initializeEventHandlers() {
  // Event listener for 'Update Portfolio' button
  document.getElementById('updatePortfolio').addEventListener('click', async () => {
    // Get the necessary DOM elements
    const loadingIndicator = document.getElementById('loading');
    const mainContent = document.getElementById('mainContent');

    // Show the loading indicator and skeletons, hide main content
    loadingIndicator.classList.remove('d-none');
    showSkeletons();
    hideError();

    try {
      // Fetch portfolio data
      const json = await fetchPortfolioData();

      // Map and process the data
      const updatedDataFromApi = await mapData(json);

      // Update the global data variable if necessary
      // If dataFromApi is managed in main.js, consider exporting a setter function
      // For simplicity, we'll assume it's managed within this scope

      // Render charts with the updated data
      renderCharts(updatedDataFromApi);

      // Render historical chart with selected period
      const timePeriodSelect = document.getElementById('timePeriodSelect');
      const selectedPeriod = timePeriodSelect.value || '1Y'; // Default to '1Y' if no value
      renderHistoricalChart(updatedDataFromApi, selectedPeriod);
    } catch (error) {
      console.error('Error:', error);
      showError('Error loading data. Please try again later.');
    } finally {
      // Hide skeletons and show main content
      hideSkeletons();
      mainContent.classList.remove('d-none');

      // Hide the loading indicator
      loadingIndicator.classList.add('d-none');
    }
  });

  // Event listener for time period selection
  document.getElementById('timePeriodSelect').addEventListener('change', async function () {
    const selectedPeriod = this.value;

    // Show the loading indicator and skeletons for the historical chart
    const loadingIndicator = document.getElementById('loading');
    const historicalChartContainer = document.getElementById('historicalPortfolioChart');

    loadingIndicator.classList.remove('d-none');
    showSkeletons();
    hideError();

    try {
      // Assuming dataFromApi is globally accessible or manage via a shared state
      // For this example, we'll fetch and map data again
      const json = await fetchPortfolioData();
      const updatedDataFromApi = await mapData(json);

      // Render the historical chart with the selected period
      renderHistoricalChart(updatedDataFromApi, selectedPeriod);
    } catch (error) {
      console.error('Error:', error);
      showError('Error loading historical data. Please try again later.');
    } finally {
      // Hide skeletons and show main content
      hideSkeletons();
      loadingIndicator.classList.add('d-none');
    }
  });
}
