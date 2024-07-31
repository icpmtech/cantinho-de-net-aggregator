// Generate random stock data for the heatmaps
function generateStockData(count, range) {
    let series = [];
    for (let i = 0; i < count; i++) {
        let x = `Month ${i + 1}`;
        let y = Math.floor(Math.random() * (range.max - range.min + 1)) + range.min;
        series.push({ x: x, y: y });
    }
    return series;
}
function renderPortfolioHeatMaps(portfolios) {
    const heatmapContainer = document.getElementById('heatmapContainer');
    heatmapContainer.innerHTML = ''; // Clear any existing content

    document.getElementById('portfolios-numbers').innerHTML = portfolios.length;

    portfolios.forEach((portfolio, index) => {
        const heatmapDiv = document.createElement('div');
        heatmapDiv.id = `heatmap${index + 1}`;
        heatmapDiv.classList.add('heatmap');
        heatmapContainer.appendChild(heatmapDiv);

        const stockData = portfolio.items.map(stock => ({
            name: stock.symbol,
            data: generateStockData(12, { min: -10, max: 10 })
        }));

        const options = {
            chart: {
                type: 'heatmap',
                height: 800
            },
            series: stockData,
            plotOptions: {
                heatmap: {
                    shadeIntensity: 0.5,
                    radius: 0,
                    useFillColorAsStroke: false,
                    colorScale: {
                        ranges: [{
                            from: -10,
                            to: 0,
                            color: '#FF0000',
                            name: 'negative'
                        }, {
                            from: 1,
                            to: 10,
                            color: '#00FF00',
                            name: 'positive'
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
    });
}
