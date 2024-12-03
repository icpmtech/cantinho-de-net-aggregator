// Function to add MACD
function addMACD(root, chartInstance, color) {
    const { candlestickSeries } = chartInstance;
    const macdData = calculateMACD(
        candlestickSeries.dataItems.map(item => item.close),
        12,
        26,
        9,
        candlestickSeries.dataItems.map(item => item.date)
    );

    // Create a separate panel for MACD
    const macdPanel = chartInstance.root.container.children.push(
        am5xy.XYChart.new(root, {
            height: am5.percent(20),
            panX: true,
            panY: false,
            wheelX: "panX",
            wheelY: "zoomX",
            focusable: false,
            layout: root.verticalLayout
        })
    );

    // Create Legend for MACD Panel
    const macdLegend = macdPanel.children.push(
        am5.Legend.new(root, {
            centerX: am5.percent(50),
            x: am5.percent(50),
            marginTop: 15,
            marginBottom: 15
        })
    );

    // MACD X Axis
    const macdDateAxis = macdPanel.xAxes.push(
        am5xy.DateAxis.new(root, {
            baseInterval: { timeUnit: "day", count: 1 },
            renderer: am5xy.AxisRendererX.new(root, { visible: false }),
            tooltip: am5.Tooltip.new(root, {})
        })
    );

    // MACD Y Axis
    const macdValueAxis = macdPanel.yAxes.push(
        am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererY.new(root, {})
        })
    );

    // MACD Line
    const macdLineSeries = macdPanel.series.push(
        am5xy.LineSeries.new(root, {
            name: "MACD Line",
            xAxis: macdDateAxis,
            yAxis: macdValueAxis,
            valueYField: "macd",
            valueXField: "date",
            stroke: color,
            strokeWidth: 2,
            tooltip: am5.Tooltip.new(root, {
                labelText: "MACD: {macd}"
            })
        })
    );
    macdLineSeries.data.setAll(macdData.macdLine.filter(item => item.macd !== null));

    // Signal Line
    const signalLineSeries = macdPanel.series.push(
        am5xy.LineSeries.new(root, {
            name: "Signal Line",
            xAxis: macdDateAxis,
            yAxis: macdValueAxis,
            valueYField: "signal",
            valueXField: "date",
            stroke: am5.color(0x0000ff),
            strokeDasharray: [4, 4],
            strokeWidth: 2,
            tooltip: am5.Tooltip.new(root, {
                labelText: "Signal: {signal}"
            })
        })
    );
    signalLineSeries.data.setAll(macdData.signalLine.filter(item => item.signal !== null));

    // Histogram
    const histogramSeries = macdPanel.series.push(
        am5xy.ColumnSeries.new(root, {
            name: "Histogram",
            xAxis: macdDateAxis,
            yAxis: macdValueAxis,
            valueYField: "histogram",
            valueXField: "date",
            fill: am5.color(0xff0000),
            stroke: am5.color(0xff0000),
            tooltip: am5.Tooltip.new(root, {
                labelText: "Histogram: {histogram}"
            })
        })
    );
    histogramSeries.data.setAll(macdData.histogram.filter(item => item.histogram !== null));

    // Add to MACD Legend
    macdLegend.data.push(macdLineSeries);
    macdLegend.data.push(signalLineSeries);
    macdLegend.data.push(histogramSeries);
}
