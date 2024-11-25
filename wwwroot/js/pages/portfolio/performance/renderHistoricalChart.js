import ApexCharts from 'https://cdn.jsdelivr.net/npm/apexcharts@3.37.2/dist/apexcharts.esm.js';
import { calculateHistoricalPortfolioData } from './calculateHistoricalPortfolioData.js';
// Import or define fetchHistoricalPrices
import { fetchHistoricalPrices } from './dataFetcher.js'; // Ensure this path is correct

// Initialize chart instances globally
export let historicalChartInstance;

/**
 * Helper function to calculate past date based on period
 */
function getPastDate(period, dataItems) {
  const dateNow = new Date();
  let datePast;

  switch (period) {
    case '1D':
      datePast = new Date(dateNow.getTime() - 1 * 24 * 60 * 60 * 1000);
      break;
    case '5D':
      datePast = new Date(dateNow.getTime() - 5 * 24 * 60 * 60 * 1000);
      break;
    case '3M':
      datePast = new Date();
      datePast.setMonth(datePast.getMonth() - 3);
      break;
    case '6M':
      datePast = new Date();
      datePast.setMonth(datePast.getMonth() - 6);
      break;
    case '1Y':
      datePast = new Date();
      datePast.setFullYear(datePast.getFullYear() - 1);
      break;
    case '5Y':
      datePast = new Date();
      datePast.setFullYear(datePast.getFullYear() - 5);
      break;
    case 'AllTime':
      const validDates = dataItems
        .map(item => new Date(item.purchaseDate))
        .filter(date => !isNaN(date));
      datePast = validDates.length ? new Date(Math.min(...validDates)) : new Date(dateNow.getTime() - 1 * 24 * 60 * 60 * 1000);
      break;
    default:
      datePast = new Date(dateNow.getTime() - 1 * 24 * 60 * 60 * 1000);
  }

  return {
    startDate: datePast.toISOString(),
    endDate: dateNow.toISOString()
  };
}

/**
 * Render the historical portfolio chart
 * @param {Object} data - The portfolio data
 * @param {String} period - The period for the chart (e.g., '1D', '5D', '3M', etc.)
 */
export async function renderHistoricalChart(data, period) {
  if (!data || !data.items || !Array.isArray(data.items)) {
    console.error('Invalid data provided to renderHistoricalChart.');
    return;
  }

  const { startDate, endDate } = getPastDate(period, data.items);

  try {
    // Fetch historical prices for each item with caching and date range
    const fetchPromises = data.items.map(async item => {
      item.historicalPricesData = await fetchHistoricalPrices(item.symbol, startDate, endDate);
    });

    // Wait for all fetch operations to complete
    await Promise.all(fetchPromises);

    // Calculate historical portfolio data
    const historicalData = calculateHistoricalPortfolioData(data.items, startDate, endDate);

    if (!historicalData || !historicalData.values || !historicalData.dates) {
      console.error('Invalid historical data calculated.');
      return;
    }

    const chartOptions = {
      series: [{
        name: 'Portfolio Value',
        data: historicalData.values
      }],
      chart: {
        type: 'line',
        height: 350,
        zoom: {
          enabled: false
        }
      },
      xaxis: {
        categories: historicalData.dates,
        title: {
          text: 'Date'
        },
        type: 'datetime', // Ensure the x-axis is treated as datetime
        labels: {
          format: 'dd MMM'
        }
      },
      yaxis: {
        title: {
          text: 'Market Value ($)'
        },
        labels: {
          formatter: function (val) {
            return `$${val.toLocaleString()}`;
          }
        }
      },
      title: {
        text: `Portfolio Performance (${period})`,
        align: 'center'
      },
      tooltip: {
        x: {
          format: 'dd MMM yyyy'
        },
        y: {
          formatter: function (val) {
            return `$${val.toLocaleString()}`;
          }
        }
      },
      stroke: {
        curve: 'smooth'
      },
      markers: {
        size: 0
      },
      grid: {
        borderColor: '#f1f1f1'
      }
    };

    if (historicalChartInstance) {
      historicalChartInstance.updateOptions(chartOptions);
    } else {
      const chartElement = document.querySelector("#historicalPortfolioChart");
      if (!chartElement) {
        console.error('Chart container #historicalPortfolioChart not found.');
        return;
      }
      historicalChartInstance = new ApexCharts(chartElement, chartOptions);
      historicalChartInstance.render();
    }
  } catch (error) {
    console.error('Error rendering historical chart:', error);
  }
}
