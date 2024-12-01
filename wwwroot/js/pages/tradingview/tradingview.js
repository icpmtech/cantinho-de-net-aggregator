let chartInstance;

// Function to fetch data from the API
async function fetchData(symbol, startDate, endDate) {
  const url = `/api/YahooFinance/chart-symbol/${symbol}?startDate=${startDate}&endDate=${endDate}`;
  const options = {
    method: 'GET'
  };

  // Show loading spinner and overlay
  document.getElementById('loading-spinner').style.display = 'flex';
  document.getElementById('overlay').style.display = 'block';

  // Reset messages
  document.getElementById('loading-message').style.display = 'block';
  document.getElementById('error-message').style.display = 'none';
  document.getElementById('success-message').style.display = 'none';
  document.getElementById('empty-message').style.display = 'none';

  try {
    const response = await fetch(url, options);

    if (!response.ok) {
      throw new Error('Failed to fetch data');
    }

    const data = await response.json();
    if (!data || data.length === 0) {
      document.getElementById('empty-message').style.display = 'block';
      return [];
    }

    // Map the data to the required format
    const mappedData = data.map((item) => ({
      date: new Date(item.timestamp).getTime(),
      open: item.open,
      close: item.close,
      high: item.high,
      low: item.low,
      volume: item.volume,
    }));

    document.getElementById('success-message').style.display = 'block';
    return mappedData;
  } catch (error) {
    document.getElementById('error-message').style.display = 'block';
    console.error('Error fetching data:', error);
    return [];
  } finally {
    // Hide loading spinner and overlay
    document.getElementById('loading-spinner').style.display = 'none';
    document.getElementById('overlay').style.display = 'none';
  }
}

// Function to create the chart and additional panels for indicators
function createChart(chartData) {
  const root = am5.Root.new("chartdiv");
  root.setThemes([am5themes_Animated.new(root)]);

  // Create main chart container
  const mainChart = root.container.children.push(
    am5xy.XYChart.new(root, {
      panX: true,
      panY: true,
      wheelX: "panX",
      wheelY: "zoomX",
      layout: root.verticalLayout,
    })
  );

  // Create X Axis
  const dateAxis = mainChart.xAxes.push(
    am5xy.DateAxis.new(root, {
      maxDeviation: 0.5,
      baseInterval: { timeUnit: "day", count: 1 },
      renderer: am5xy.AxisRendererX.new(root, {
        minGridDistance: 50
      }),
      tooltip: am5.Tooltip.new(root, {})
    })
  );

  // Create Y Axis for price
  const valueAxis = mainChart.yAxes.push(
    am5xy.ValueAxis.new(root, {
      renderer: am5xy.AxisRendererY.new(root, {})
    })
  );

  // Create Candlestick Series
  const candlestickSeries = mainChart.series.push(
    am5xy.CandlestickSeries.new(root, {
      name: "Candlesticks",
      xAxis: dateAxis,
      yAxis: valueAxis,
      openValueYField: "open",
      valueYField: "close",
      lowValueYField: "low",
      highValueYField: "high",
      valueXField: "date",
      tooltip: am5.Tooltip.new(root, {
        labelText: "Open: {open}\nHigh: {high}\nLow: {low}\nClose: {close}\nVolume: {volume}"
      })
    })
  );

  candlestickSeries.data.setAll(chartData);

  // Add a cursor
  mainChart.set("cursor", am5xy.XYCursor.new(root, {}));

  // Add a legend
  mainChart.set("legend", am5.Legend.new(root, {}));

  return { root, mainChart, candlestickSeries, dateAxis, valueAxis };
}

// Event listener for fetching data
document.getElementById("fetch-data").addEventListener("click", async () => {
  const symbol = document.getElementById("symbol").value.trim();
  const startDate = document.getElementById("start-date").value;
  const endDate = document.getElementById("end-date").value;

  if (!symbol) {
    alert("Please enter a stock symbol.");
    return;
  }

  if (!startDate || !endDate) {
    alert("Please select both start and end dates.");
    return;
  }

  if (new Date(startDate) > new Date(endDate)) {
    alert("Start date cannot be after end date.");
    return;
  }

  const chartData = await fetchData(symbol, startDate, endDate);
  if (chartData.length === 0) return;

  if (chartInstance) {
    chartInstance.root.dispose();
  }
  chartInstance = createChart(chartData);
});

// Event listener for applying patterns
document.getElementById("apply-patterns").addEventListener("click", () => {
  if (!chartInstance) {
    alert("Please fetch data before applying patterns.");
    return;
  }

  const patterns = Array.from(document.getElementById("menu-select").selectedOptions).map((opt) => opt.value);
  applyPatterns(patterns, chartInstance.mainChart, chartInstance.candlestickSeries);
});

// Event listener for applying indicators
document.getElementById("apply-indicators").addEventListener("click", () => {
  if (!chartInstance) {
    alert("Please fetch data before applying indicators.");
    return;
  }

  const indicators = Array.from(document.getElementById("indicator-select").selectedOptions).map((opt) => opt.value);
  applyIndicators(indicators, chartInstance);
});

// Function to apply patterns
function applyPatterns(patterns, chart, series) {
  const patternColors = {
    highlightBullish: am5.color(0x00ff00),
    highlightBearish: am5.color(0xff0000),
    highlightDoji: am5.color(0x0000ff),
    highlightHammer: am5.color(0xffa500),
    highlightShootingStar: am5.color(0x800080),
    highlightMorningStar: am5.color(0x008080),
  };

  patterns.forEach((pattern) => {
    // Remove existing pattern highlights if any
    const existingSeries = chart.series.values.filter(s => s.get("name") === pattern);
    existingSeries.forEach(s => s.dispose());

    const highlightSeries = chart.series.push(
      am5xy.LineSeries.new(chart.root, { // Use chart.root instead of root
        name: pattern,
        xAxis: chart.xAxes.getIndex(0),
        yAxis: chart.yAxes.getIndex(0),
        valueYField: "close",
        valueXField: "date",
        strokeWidth: 0, // Invisible line
        fill: patternColors[pattern],
        tooltip: am5.Tooltip.new(chart.root, {
          labelText: pattern.replace(/highlight/, '') // Clean name
        })
      })
    );

    const highlights = [];

   
    series.dataItems.forEach((item, index) => {
      const curr = item.dataContext;
      const prev = series.dataItems[index - 1]?.dataContext || {};
      const prev2 = series.dataItems[index - 2]?.dataContext || {};

      switch (pattern) {
        case "highlightBullish":
          if (prev.close < prev.open &&
            curr.close > curr.open &&
            curr.open < prev.close &&
            curr.close > prev.open) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightBearish":
          if (prev.close > prev.open &&
            curr.close < curr.open &&
            curr.open > prev.close &&
            curr.close < prev.open) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightDoji":
          if (Math.abs(curr.close - curr.open) < 0.1 * (curr.high - curr.low)) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightHammer":
          const bodyHammer = Math.abs(curr.close - curr.open);
          const lowerShadowHammer = curr.low - Math.min(curr.close, curr.open);
          if (bodyHammer < 0.3 * (curr.high - curr.low) && lowerShadowHammer > 2 * bodyHammer) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightShootingStar":
          const upperShadow = curr.high - Math.max(curr.close, curr.open);
          if (Math.abs(curr.close - curr.open) < 0.3 * (curr.high - curr.low) && upperShadow > 2 * Math.abs(curr.close - curr.open)) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightMorningStar":
          if (index < 2) return;
          const first = series.dataItems[index - 2].dataContext;
          const second = series.dataItems[index - 1].dataContext;
          const third = curr;
          if (first.close < first.open &&
            Math.abs(second.close - second.open) < 0.1 * (second.high - second.low) &&
            third.close > third.open &&
            third.close > (first.open + first.close) / 2) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        default:
          return;
      }
    });

    if (highlights.length > 0) {
      highlightSeries.data.setAll(highlights);
      highlightSeries.strokes.template.set("stroke", patternColors[pattern]);
      highlightSeries.bullets.push(() => {
        return am5.Bullet.new(chart.root, { // Use chart.root instead of root
          sprite: am5.Circle.new(chart.root, { // Use chart.root instead of root
            radius: 5,
            fill: patternColors[pattern]
          })
        });
      });
    }
  });
}

// Function to apply indicators
function applyIndicators(indicators, chartInstance) {
  const { root, mainChart, candlestickSeries, dateAxis, valueAxis } = chartInstance;
  const indicatorColors = {
    accelerationBands: am5.color(0x9966cc),
    bollingerBands: am5.color(0xffa500),
    rsi: am5.color(0x33cc33),
    macd: am5.color(0xff3333),
    volume: am5.color(0x3333ff),
  };

  indicators.forEach((indicator) => {
    // Remove existing indicator series if any
    const existingSeries = mainChart.series.values.filter(s => s.get("name").toLowerCase().includes(indicator));
    existingSeries.forEach(s => s.dispose());

    switch (indicator) {
      case "bollingerBands":
        const bollingerData = calculateBollingerBands(
          candlestickSeries.dataItems.map(item => item.dataContext.close),
          20,
          2,
          candlestickSeries.dataItems.map(item => item.dataContext.date)
        );

        // Upper Band
        const upperBand = mainChart.series.push(
          am5xy.LineSeries.new(root, {
            name: "Bollinger Upper",
            xAxis: dateAxis,
            yAxis: valueAxis,
            valueYField: "upper",
            valueXField: "date",
            stroke: indicatorColors[indicator],
            strokeDasharray: [5, 5],
            strokeWidth: 1,
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
            stroke: indicatorColors[indicator],
            strokeDasharray: [5, 5],
            strokeWidth: 1,
            tooltip: am5.Tooltip.new(root, {
              labelText: "Bollinger Lower: {lower}"
            })
          })
        );
        lowerBand.data.setAll(bollingerData.filter(item => item.lower !== null));
        break;

      case "accelerationBands":
        // Placeholder: Using Bollinger Bands calculation for Acceleration Bands
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
            stroke: indicatorColors[indicator],
            strokeDasharray: [5, 5],
            strokeWidth: 1,
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
            stroke: indicatorColors[indicator],
            strokeDasharray: [5, 5],
            strokeWidth: 1,
            tooltip: am5.Tooltip.new(root, {
              labelText: "Acceleration Lower: {lower}"
            })
          })
        );
        accelLower.data.setAll(accelerationData.filter(item => item.lower !== null));
        break;

      case "rsi":
        const rsiData = calculateRSI(
          candlestickSeries.dataItems.map(item => item.close),
          14,
          candlestickSeries.dataItems.map(item => item.date)
        );

        // Create a separate panel for RSI
        const rsiPanel = root.container.children.push(
          am5xy.XYChart.new(root, {
            height: am5.percent(20),
            panX: true,
            panY: false,
            wheelX: "panX",
            wheelY: "zoomX",
            focusable: false
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
            stroke: indicatorColors[indicator],
            tooltip: am5.Tooltip.new(root, {
              labelText: "RSI: {value}"
            })
          })
        );
        rsiSeries.data.setAll(rsiData.filter(item => item.value !== null));

        // Add RSI Overbought and Oversold lines
        const overbought = rsiPanel.yAxes.getIndex(0).axisRanges.push(am5xy.AxisRange.new(root, {
          value: 70,
          endValue: 70,
          label: am5.Label.new(root, { text: "Overbought", fontSize: 12, fill: indicatorColors[indicator] }),
          grid: { stroke: am5.color(0xff0000), strokeDasharray: [4, 4] }
        }));

        const oversold = rsiPanel.yAxes.getIndex(0).axisRanges.push(am5xy.AxisRange.new(root, {
          value: 30,
          endValue: 30,
          label: am5.Label.new(root, { text: "Oversold", fontSize: 12, fill: indicatorColors[indicator] }),
          grid: { stroke: am5.color(0x0000ff), strokeDasharray: [4, 4] }
        }));
        break;

      case "macd":
        const macdData = calculateMACD(
          candlestickSeries.dataItems.map(item => item.close),
          12,
          26,
          9,
          candlestickSeries.dataItems.map(item => item.date)
        );

        // Create a separate panel for MACD
        const macdPanel = root.container.children.push(
          am5xy.XYChart.new(root, {
            height: am5.percent(20),
            panX: true,
            panY: false,
            wheelX: "panX",
            wheelY: "zoomX",
            focusable: false
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
            stroke: indicatorColors[indicator],
            tooltip: am5.Tooltip.new(root, {
              labelText: "MACD: {macd}"
            })
          })
        );
        macdLineSeries.data.setAll(macdData.macdLine.filter(item => item.value !== null));

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
            tooltip: am5.Tooltip.new(root, {
              labelText: "Signal: {signal}"
            })
          })
        );
        signalLineSeries.data.setAll(macdData.signalLine.filter(item => item.value !== null));

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
        histogramSeries.data.setAll(macdData.histogram.filter(item => item.value !== null));
        break;

      case "volume":
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
            fill: indicatorColors[indicator],
            stroke: indicatorColors[indicator],
            tooltip: am5.Tooltip.new(root, {
              labelText: "Volume: {volume}"
            })
          })
        );
        volumeSeries.data.setAll(volumeData);
        break;

      default:
        console.warn(`Indicator ${indicator} not recognized.`);
        return;
    }
  });
}

// Function to calculate Bollinger Bands
function calculateBollingerBands(closePrices, period, stdDevMultiplier, dates) {
  const bollingerData = [];
  for (let i = 0; i < closePrices.length; i++) {
    if (i < period - 1) {
      bollingerData.push({ date: dates[i], upper: null, lower: null });
      continue;
    }
    const slice = closePrices.slice(i - period + 1, i + 1);
    const sum = slice.reduce((a, b) => a + b, 0);
    const mean = sum / period;
    const variance = slice.reduce((a, b) => a + Math.pow(b - mean, 2), 0) / period;
    const stdDev = Math.sqrt(variance);
    bollingerData.push({
      date: dates[i],
      upper: mean + stdDevMultiplier * stdDev,
      lower: mean - stdDevMultiplier * stdDev
    });
  }
  return bollingerData;
}

// Function to calculate RSI
function calculateRSI(closePrices, period, dates) {
  const rsiData = [];
  let gains = 0;
  let losses = 0;

  // Initialize first RSI value
  for (let i = 1; i <= period; i++) {
    const change = closePrices[i] - closePrices[i - 1];
    if (change > 0) gains += change;
    else losses -= change;
  }
  let averageGain = gains / period;
  let averageLoss = losses / period;
  let rs = averageGain / averageLoss;
  let rsi = averageLoss === 0 ? 100 : 100 - (100 / (1 + rs));
  rsiData[period] = { date: dates[period], value: rsi };

  // Calculate RSI for the rest of the data
  for (let i = period + 1; i < closePrices.length; i++) {
    const change = closePrices[i] - closePrices[i - 1];
    if (change > 0) {
      averageGain = (averageGain * (period - 1) + change) / period;
      averageLoss = (averageLoss * (period - 1)) / period;
    } else {
      averageGain = (averageGain * (period - 1)) / period;
      averageLoss = (averageLoss * (period - 1) - change) / period;
    }
    rs = averageLoss === 0 ? 100 : averageGain / averageLoss;
    rsi = averageLoss === 0 ? 100 : 100 - (100 / (1 + rs));
    rsiData[i] = { date: dates[i], value: rsi };
  }

  // Fill in the initial period with nulls
  for (let i = 0; i < period; i++) {
    rsiData[i] = { date: dates[i], value: null };
  }

  // Remove undefined entries and return
  return rsiData.filter(item => item !== undefined);
}

// Function to calculate MACD
function calculateMACD(closePrices, shortPeriod, longPeriod, signalPeriod, dates) {
  const emaShort = calculateEMA(closePrices, shortPeriod);
  const emaLong = calculateEMA(closePrices, longPeriod);
  const macdLine = [];
  const signalLine = [];
  const histogram = [];

  // Calculate MACD Line
  for (let i = 0; i < closePrices.length; i++) {
    if (emaShort[i] !== null && emaLong[i] !== null) {
      macdLine[i] = { date: dates[i], value: emaShort[i] - emaLong[i] };
    } else {
      macdLine[i] = { date: dates[i], value: null };
    }
  }

  // Calculate Signal Line (EMA of MACD Line)
  const macdValues = macdLine.map(item => item.value);
  const emaSignal = calculateEMA(macdValues, signalPeriod);
  for (let i = 0; i < macdLine.length; i++) {
    if (emaSignal[i] !== null) {
      signalLine[i] = { date: dates[i], value: emaSignal[i] };
    } else {
      signalLine[i] = { date: dates[i], value: null };
    }
  }

  // Calculate Histogram
  for (let i = 0; i < macdLine.length; i++) {
    if (macdLine[i].value !== null && signalLine[i].value !== null) {
      histogram[i] = { date: dates[i], value: macdLine[i].value - signalLine[i].value };
    } else {
      histogram[i] = { date: dates[i], value: null };
    }
  }

  return { macdLine, signalLine, histogram };
}

// Helper function to calculate Exponential Moving Average (EMA)
function calculateEMA(data, period) {
  const ema = [];
  const k = 2 / (period + 1);
  let emaPrev = null;

  for (let i = 0; i < data.length; i++) {
    if (data[i] === null || data[i] === undefined) {
      ema[i] = null;
      continue;
    }

    if (i < period - 1) {
      ema[i] = null;
      continue;
    }

    if (i === period - 1) {
      const sum = data.slice(0, period).reduce((a, b) => a + b, 0);
      emaPrev = sum / period;
      ema[i] = emaPrev;
    } else {
      emaPrev = data[i] * k + emaPrev * (1 - k);
      ema[i] = emaPrev;
    }
  }
  return ema;
}
