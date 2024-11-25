import ApexCharts from 'https://cdn.jsdelivr.net/npm/apexcharts@3.37.2/dist/apexcharts.esm.js';
import { calculateHistoricalPortfolioData, historicalChartInstance } from './chartRenderer.js';

export function renderHistoricalChart(data, period) {
    const historicalData = calculateHistoricalPortfolioData(data.items, period);

    const chartOptions = {
        series: [{
            name: 'Portfolio Value',
            data: historicalData.values
        }],
        chart: {
            type: 'line',
            height: 350
        },
        xaxis: {
            categories: historicalData.dates,
            title: {
                text: 'Date'
            }
        },
        yaxis: {
            title: {
                text: 'Market Value ($)'
            }
        },
        title: {
            text: `Portfolio Performance (${period})`,
            align: 'center'
        }
    };

    if (historicalChartInstance) {
        historicalChartInstance.updateOptions(chartOptions);
    } else {
        historicalChartInstance = new ApexCharts(document.querySelector("#historicalPortfolioChart"), chartOptions);
        historicalChartInstance.render();
    }
}
