// main.js

// Import necessary modules
import { fetchPortfolioData } from "./fetchPortfolioData.js";
import { mapData } from './mapData.js';
import { renderCharts } from './chartRenderer.js';
import { renderHistoricalChart } from './renderHistoricalChart.js';
import { initializeEventHandlers } from './eventHandlers.js';
import { hideError } from "./hideError.js";
import { showError } from "./showError.js";
import { showSkeletons, hideSkeletons } from './skeletons.js'; // Import skeleton functions

// Global variable to store data
let dataFromApi;
async function initialize() {
  // Get the necessary DOM elements
  const loadingIndicator = document.getElementById('loading');
  const mainContent = document.getElementById('mainContent');

  // Show the loading indicator and main content skeletons
  loadingIndicator.classList.remove('d-none');
  showSkeletons();
  hideError();

  try {
    // Fetch portfolio data
    const json = await fetchPortfolioData();

    // Map and process the data
    dataFromApi = await mapData(json);

    // Render charts with the processed data
    renderCharts(dataFromApi);

    // Render historical chart with selected period
    const timePeriodSelect = document.getElementById('timePeriodSelect');
    const selectedPeriod = timePeriodSelect.value || '1Y'; // Default to '1Y' if no value
    renderHistoricalChart(dataFromApi, selectedPeriod);

    // Initialize event handlers
    initializeEventHandlers(dataFromApi);

    // After successful data loading, hide skeletons and show content
    hideSkeletons();
    mainContent.classList.remove('d-none');
  } catch (error) {
    console.error('Error:', error);
    showError('Error loading data. Please try again later.');
  } finally {
    // Hide the loading indicator
    loadingIndicator.classList.add('d-none');
  }
}

// Call the initialization function when the DOM is fully loaded
document.addEventListener('DOMContentLoaded', () => {
  initialize();
});
