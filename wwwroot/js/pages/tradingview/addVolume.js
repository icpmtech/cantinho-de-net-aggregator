// Function to add Volume
function addVolume(mainChart, root, dateAxis, valueAxis, candlestickSeries, color, mainLegend) {
    const volumeData = candlestickSeries.dataItems.map(item => ({
        date: item.dataContext.date,
        volume: item.dataContext.volume
    }));

    const volumeSeries = mainChart.series.push(
        am5xy.ColumnSeries.new(root, {
            name: "Volume",
            xAxis: dateAxis,
            yAxis: valueAxis,
            valueYField: "volume",
            valueXField: "date",
            fill: color,
            stroke: color,
            tooltip: am5.Tooltip.new(root, {
                labelText: "Volume: {volume}"
            })
        })
    );
    volumeSeries.data.setAll(volumeData);

    // Add to Legend
    mainLegend.data.push(volumeSeries);
}
