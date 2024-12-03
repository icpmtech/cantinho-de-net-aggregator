// Function to add RSI with an optional Moving Average (MA) line
function addRSI(root, chartInstance, color, maPeriod = 9) {
  const { candlestickSeries } = chartInstance;

  // Extract close prices and dates from the candlestick series
  const closePrices = candlestickSeries.dataItems.map(item => item.close);
  const dates = candlestickSeries.dataItems.map(item => item.date);

  // Calculate RSI data
  const rsiData = calculateRSI(closePrices, 14, dates);

  // Optionally, calculate Moving Average of RSI
  const maData = calculateMA(rsiData, maPeriod);

  // Create a separate panel for RSI
  const rsiPanel = chartInstance.root.container.children.push(
    am5xy.XYChart.new(root, {
      height: am5.percent(50),
      panX: true,
      panY: false,
      wheelX: "panX",
      wheelY: "zoomX",
      focusable: false,
      layout: root.verticalLayout,
      marginBottom: 50, // Adjust spacing for better appearance
    })
  );
  // Adjust grid color for contrast on black background
  rsiPanel.set("grid", {
    stroke: am5.color(0x666666), // Gray color for grid lines for better contrast
    strokeOpacity: 0.5,
  });
  // Create Legend for RSI Panel
  const rsiLegend = rsiPanel.children.push(
    am5.Legend.new(root, {
      centerX: am5.percent(50),
      x: am5.percent(50),
      marginTop: 15,
      marginBottom: 15
    })
  );

  // RSI X Axis
  const rsiDateAxis = rsiPanel.xAxes.push(
    am5xy.DateAxis.new(root, {
      baseInterval: { timeUnit: "day", count: 1 },
      renderer: am5xy.AxisRendererX.new(root, { visible: false }),
      tooltip: am5.Tooltip.new(root, {})
    })
  );

  // RSI Y Axis
  const rsiValueAxis = rsiPanel.yAxes.push(
    am5xy.ValueAxis.new(root, {
      min: 0,
      max: 100,
      renderer: am5xy.AxisRendererY.new(root, {})
    })
  );

  // RSI Series
  const rsiSeries = rsiPanel.series.push(
    am5xy.LineSeries.new(root, {
      name: "RSI",
      xAxis: rsiDateAxis,
      yAxis: rsiValueAxis,
      valueYField: "value",
      valueXField: "date",
      stroke: color,
      strokeWidth: 2,
      tooltip: am5.Tooltip.new(root, {
        labelText: "RSI: {value}"
      })
    })
  );
  rsiSeries.data.setAll(rsiData.filter(item => item.value !== null));

  // Add RSI Moving Average Series (Optional)
  if (maData && maData.length > 0) {
    const maSeries = rsiPanel.series.push(
      am5xy.LineSeries.new(root, {
        name: `RSI MA (${maPeriod})`,
        xAxis: rsiDateAxis,
        yAxis: rsiValueAxis,
        valueYField: "ma",
        valueXField: "date",
        stroke: am5.color(0x00ff00), // Choose a different color for MA
        strokeWidth: 2,
        strokeDasharray: [5, 5], // Dashed line for distinction
        tooltip: am5.Tooltip.new(root, {
          labelText: `RSI MA (${maPeriod}): {ma}`
        })
      })
    );
    maSeries.data.setAll(maData.filter(item => item.ma !== null));

    // Add MA Series to Legend
    rsiLegend.data.push(maSeries);
  }

  // Add RSI Overbought line at 70
  let overboughtRange = rsiValueAxis.makeDataItem({
    value: 70,
    endValue: 70
  });
  rsiValueAxis.createAxisRange(overboughtRange);
  overboughtRange.get("grid").setAll({
    stroke: am5.color(0xff0000),
    strokeDasharray: [4, 4]
  });
  overboughtRange.get("label").setAll({
    text: "Overbought",
    fontSize: 12,
    fill: color
  });

  // Add RSI Oversold line at 30
  let oversoldRange = rsiValueAxis.makeDataItem({
    value: 30,
    endValue: 30
  });
  rsiValueAxis.createAxisRange(oversoldRange);
  oversoldRange.get("grid").setAll({
    stroke: am5.color(0x0000ff),
    strokeDasharray: [4, 4]
  });
  oversoldRange.get("label").setAll({
    text: "Oversold",
    fontSize: 12,
    fill: color
  });

  // Add RSI Series to Legend
  rsiLegend.data.push(rsiSeries);
}

// Helper function to calculate RSI
function calculateRSI(closePrices, period, dates) {
  const rsi = [];
  let gains = 0;
  let losses = 0;

  for (let i = 1; i < closePrices.length; i++) {
    const change = closePrices[i] - closePrices[i - 1];
    if (change > 0) {
      gains += change;
    } else {
      losses -= change;
    }

    if (i >= period) {
      const averageGain = gains / period;
      const averageLoss = losses / period;
      const rs = averageLoss === 0 ? 100 : averageGain / averageLoss;
      const rsiValue = 100 - 100 / (1 + rs);
      rsi.push({ date: dates[i], value: rsiValue });

      // Subtract the oldest gain/loss
      const oldChange = closePrices[i - period + 1] - closePrices[i - period];
      if (oldChange > 0) {
        gains -= oldChange;
      } else {
        losses += oldChange;
      }
    } else {
      rsi.push({ date: dates[i], value: null });
    }
  }

  return rsi;
}

// Helper function to calculate Moving Average of RSI
function calculateMA(rsiData, period) {
  const ma = [];
  let sum = 0;
  let count = 0;

  for (let i = 0; i < rsiData.length; i++) {
    if (rsiData[i].value !== null) {
      sum += rsiData[i].value;
      count++;
      if (count > period) {
        sum -= rsiData[i - period].value;
        count--;
      }
      if (count === period) {
        ma.push({ date: rsiData[i].date, ma: sum / period });
      } else {
        ma.push({ date: rsiData[i].date, ma: null });
      }
    } else {
      ma.push({ date: rsiData[i].date, ma: null });
    }
  }

  return ma;
}
