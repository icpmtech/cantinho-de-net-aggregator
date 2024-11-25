// main.js

// Import necessary modules
import { fetchPortfolioData } from "./fetchPortfolioData.js";
import { mapData } from './mapData.js';
import { renderCharts } from './chartRenderer.js';
import { renderHistoricalChart } from './renderHistoricalChart.js';
import { initializeEventHandlers } from './eventHandlers.js';
import { hideError } from "./hideError.js";
import { showError } from "./showError.js";

// Global variable to store data
let dataFromApi;

async function initialize() {
  // Get the loading indicator element
  const loadingIndicator = document.getElementById('loading');
  // Show the loading indicator
  loadingIndicator.classList.remove('d-none');
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
