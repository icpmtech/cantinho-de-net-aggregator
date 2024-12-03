// Function to add Bollinger Bands
function addBollingerBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, color, mainLegend) {
    const bollingerData = calculateBollingerBands(
        candlestickSeries.dataItems.map(item => item.close),
        20,
        2,
        candlestickSeries.dataItems.map(item => item.date)
    );

    // Upper Band
    const upperBand = mainChart.series.push(
        am5xy.LineSeries.new(root, {
            name: "Bollinger Upper",
            xAxis: dateAxis,
            yAxis: valueAxis,
            valueYField: "upper",
            valueXField: "date",
            stroke: color,
            strokeDasharray: [5, 5],
            strokeWidth: 2,
            tooltip: am5.Tooltip.new(root, {
                labelText: "Bollinger Upper: {upper}"
            })
        })
    );
    upperBand.data.setAll(bollingerData.filter(item => item.upper !== null));

    // Lower Band
    const lowerBand = mainChart.series.push(
        am5xy.LineSeries.new(root, {
            name: "Bollinger Lower",
            xAxis: dateAxis,
            yAxis: valueAxis,
            valueYField: "lower",
            valueXField: "date",
            stroke: color,
            strokeDasharray: [5, 5],
            strokeWidth: 2,
            tooltip: am5.Tooltip.new(root, {
                labelText: "Bollinger Lower: {lower}"
            })
        })
    );
    lowerBand.data.setAll(bollingerData.filter(item => item.lower !== null));

    // Add to Legend
    mainLegend.data.push(upperBand);
    mainLegend.data.push(lowerBand);
}
