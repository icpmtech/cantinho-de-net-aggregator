// Function to add Acceleration Bands (Using Bollinger Bands as Placeholder)
function addAccelerationBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, color, mainLegend) {
  const accelerationData = calculateBollingerBands(
    candlestickSeries.dataItems.map(item => item.close),
    20,
    2,
    candlestickSeries.dataItems.map(item => item.date)
  );

  // Upper Band
  const accelUpper = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "Acceleration Upper",
      xAxis: dateAxis,
      yAxis: valueAxis,
      valueYField: "upper",
      valueXField: "date",
      stroke: color,
      strokeDasharray: [4, 4],
      strokeWidth: 2,
      tooltip: am5.Tooltip.new(root, {
        labelText: "Acceleration Upper: {upper}"
      })
    })
  );
  accelUpper.data.setAll(accelerationData.filter(item => item.upper !== null));

  // Lower Band
  const accelLower = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "Acceleration Lower",
      xAxis: dateAxis,
      yAxis: valueAxis,
      valueYField: "lower",
      valueXField: "date",
      stroke: color,
      strokeDasharray: [4, 4],
      strokeWidth: 2,
      tooltip: am5.Tooltip.new(root, {
        labelText: "Acceleration Lower: {lower}"
      })
    })
  );
  accelLower.data.setAll(accelerationData.filter(item => item.lower !== null));

  // Add to Legend
  mainLegend.data.push(accelUpper);
  mainLegend.data.push(accelLower);
}
