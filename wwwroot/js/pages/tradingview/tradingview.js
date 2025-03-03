let chartInstance;
let pollingTimer = null;

// Define polling intervals based on selected time interval
const POLLING_INTERVALS = {
  '1m': 60000,      // 1 minute
  '5m': 300000,     // 5 minutes
  '15m': 900000,    // 15 minutes
  '30m': 1800000,   // 30 minutes
  '1h': 3600000,    // 1 hour
  '1d': 86400000,   // 1 day,
  '1w': 604800000    // 1 week (optional)
};

// Helper function to format datetime as YYYY-MM-DDTHH:MM in UTC
function formatDateTimeUTC(date) {
  const year = date.getUTCFullYear();
  const month = String(date.getUTCMonth() + 1).padStart(2, '0');
  const day = String(date.getUTCDate()).padStart(2, '0');
  const hours = String(date.getUTCHours()).padStart(2, '0');
  const minutes = String(date.getUTCMinutes()).padStart(2, '0');
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

// Function to fetch initial data
async function fetchData(symbol, startDate, endDate, interval) {
  const url = `/api/YahooFinance/chart-symbol/${symbol}?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}&interval=${interval}`;
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

    // Map the data to the required format with proper validation
    let mappedData = data.map((item) => ({
      date: new Date(item.timestamp).getTime(), // Ensure timestamp is in milliseconds
      open: item.open !== null && item.open !== undefined ? Number(item.open) : null,
      close: item.close !== null && item.close !== undefined ? Number(item.close) : null,
      high: item.high !== null && item.high !== undefined ? Number(item.high) : null,
      low: item.low !== null && item.low !== undefined ? Number(item.low) : null,
      volume: item.volume !== null && item.volume !== undefined ? Number(item.volume) : null,
    }));

    // Check for null or invalid values
    const hasNullValues = mappedData.some(item =>
      item.open === null ||
      item.close === null ||
      item.high === null ||
      item.low === null ||
      item.date === null
    );

    if (hasNullValues) {
      // Convert mappedData to a readable string format
      alert("Data contains null or invalid values:\n" + JSON.stringify(mappedData, null, 2));
      // Optionally, you can filter out invalid data points
      // mappedData = mappedData.filter(item =>
      //   item.open !== null &&
      //   item.close !== null &&
      //   item.high !== null &&
      //   item.low !== null &&
      //   item.date !== null
      // );
      // return mappedData;
    }

    // Further validation: Remove data points with any null values
    const validData = mappedData.filter(item =>
      item.open !== null &&
      item.close !== null &&
      item.high !== null &&
      item.low !== null &&
      item.date !== null
    );

    if (validData.length === 0) {
      document.getElementById('empty-message').style.display = 'block';
      return [];
    }

    // Log mapped data for debugging
    console.log("Mapped Data:", validData);

    document.getElementById('success-message').style.display = 'block';
    return validData;
  } catch (error) {
    document.getElementById('error-message').style.display = 'block';
    console.error('Error fetching data:', error);
    return [];
  } finally {
    // Hide loading spinner and overlay
    document.getElementById('loading-spinner').style.display = 'none';
    document.getElementById('overlay').style.display = 'none';
    document.getElementById('loading-message').style.display = 'none';
  }
}

// Function to fetch the latest data point(s)
async function fetchLatestData(symbol, interval) {
  if (!chartInstance || !chartInstance.candlestickSeries || chartInstance.candlestickSeries.data.length === 0) {
    console.warn('No data available to fetch latest data.');
    return null;
  }

  const lastData = chartInstance.candlestickSeries.data[chartInstance.candlestickSeries.data.length - 1];
  const lastDate = new Date(lastData.date);

  // Calculate next expected date based on interval
  let nextDate = new Date(lastDate);
  switch (interval) {
    case '1m':
      nextDate.setMinutes(lastDate.getMinutes() + 1);
      break;
    case '5m':
      nextDate.setMinutes(lastDate.getMinutes() + 5);
      break;
    case '15m':
      nextDate.setMinutes(lastDate.getMinutes() + 15);
      break;
    case '30m':
      nextDate.setMinutes(lastDate.getMinutes() + 30);
      break;
    case '1h':
      nextDate.setHours(lastDate.getHours() + 1);
      break;
    case '1d':
      nextDate.setDate(lastDate.getDate() + 1);
      break;
    case '1w':
      nextDate.setDate(lastDate.getDate() + 7);
      break;
    default:
      console.warn(`Unknown interval: ${interval}`);
      return null;
  }

  const startDate = formatDateTimeUTC(nextDate);
  const endDate = formatDateTimeUTC(new Date());

  const url = `/api/YahooFinance/chart-symbol/${symbol}?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}&interval=${interval}`;
  const options = {
    method: 'GET'
  };

  try {
    const response = await fetch(url, options);
    if (!response.ok) {
      throw new Error('Failed to fetch latest data');
    }

    const data = await response.json();
    if (!data || data.length === 0) {
      console.warn('No new data received.');
      return null;
    }

    // Map the data to the required format with proper validation
    const mappedData = data.map((item) => ({
      date: new Date(item.timestamp).getTime(), // Ensure timestamp is in milliseconds
      open: item.open !== null && item.open !== undefined ? Number(item.open) : null,
      close: item.close !== null && item.close !== undefined ? Number(item.close) : null,
      high: item.high !== null && item.high !== undefined ? Number(item.high) : null,
      low: item.low !== null && item.low !== undefined ? Number(item.low) : null,
      volume: item.volume !== null && item.volume !== undefined ? Number(item.volume) : null,
    }));
    console.log("----UPDATED---: "+mappedData);
    // Validate and filter data
    const validData = mappedData.filter(item =>
      item.open !== null &&
      item.close !== null &&
      item.high !== null &&
      item.low !== null &&
      item.date !== null
    );

    if (validData.length === 0) {
      console.warn('No valid new data received.');
      return null;
    }

    // Log new data for debugging
    console.log("Latest Data:", validData);

    return validData;
  } catch (error) {
    console.error('Error fetching latest data:', error);
    return null;
  }
}

// Function to start polling
function startPolling(symbol, interval) {
  // Clear any existing timer
  if (pollingTimer) {
    clearInterval(pollingTimer);
  }

  // Set polling frequency based on interval
  const pollingFrequency = POLLING_INTERVALS[interval] || 60000; // Default to 1 minute

  // Set up a new timer
  pollingTimer = setInterval(async () => {
    const latestDataArray = await fetchLatestData(symbol, interval);
    if (latestDataArray && latestDataArray.length > 0) {
      latestDataArray.forEach(latestData => {
        updateChart(latestData);
        updateHTMLElements(latestData);
      });
    }
  }, pollingFrequency);
}

// Function to stop polling
function stopPolling() {
  if (pollingTimer) {
    clearInterval(pollingTimer);
    pollingTimer = null;
  }
}

// Function to create the chart
function createChart(chartData, interval) {
  const root = am5.Root.new("chartdiv");
  root.setThemes([am5themes_Animated.new(root)]);

  // Determine base interval for the chart based on selected interval
  let baseInterval;
  switch (interval) {
    case '1m':
      baseInterval = { timeUnit: "minute", count: 1 };
      break;
    case '5m':
      baseInterval = { timeUnit: "minute", count: 5 };
      break;
    case '15m':
      baseInterval = { timeUnit: "minute", count: 15 };
      break;
    case '30m':
      baseInterval = { timeUnit: "minute", count: 30 };
      break;
    case '1h':
      baseInterval = { timeUnit: "hour", count: 1 };
      break;
    case '1d':
    default:
      baseInterval = { timeUnit: "day", count: 1 };
      break;
  }

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
      baseInterval: baseInterval,
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

  // Create Candlestick Series with Modified Tooltip
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
      }),
      tooltipY: 0
    })
  );

  candlestickSeries.data.setAll(chartData);

  // Customize Candlestick Appearance (Optional)
  candlestickSeries.columns.template.setAll({
    strokeWidth: 1,
    fill: am5.color(0xAAAAAA),
    stroke: am5.color(0xAAAAAA),
    width: am5.percent(50) // Adjust width as needed
  });

  // Add a cursor
  mainChart.set("cursor", am5xy.XYCursor.new(root, {}));

  // Add a legend to the main chart
  const mainLegend = mainChart.children.push(
    am5.Legend.new(root, {
      centerX: am5.percent(50),
      x: am5.percent(50),
      marginTop: 15,
      marginBottom: 15
    })
  );
  mainLegend.data.setAll(mainChart.series.values);

  return { root, mainChart, candlestickSeries, dateAxis, valueAxis, mainLegend };
}

// Function to update the chart with the latest data
function updateChart(latestData) {
  if (!chartInstance) return;

  const { candlestickSeries } = chartInstance;

  // Append the latest data point
  candlestickSeries.data.push(latestData);

  // Optionally, remove the oldest data point to limit the dataset size
  const MAX_DATA_POINTS = 500; // Adjust as needed
  if (candlestickSeries.data.length > MAX_DATA_POINTS) {
    candlestickSeries.data.shift();
  }

  // If indicators are present, they should automatically update if data bindings are correct
}

// Function to update HTML elements with latest data
function updateHTMLElements(latestData) {
  // Update Latest Price
  document.getElementById('latest-price').textContent = latestData.close !== null ? latestData.close.toFixed(2) : '--';

  // Calculate Change (assuming you have previous close)
  if (chartInstance.candlestickSeries.data.length > 1) {
    const previousData = chartInstance.candlestickSeries.data[chartInstance.candlestickSeries.data.length - 2];
    if (previousData.close !== null && latestData.close !== null) {
      const change = latestData.close - previousData.close;
      const changePercent = (change / previousData.close) * 100;
      document.getElementById('price-change').textContent = `${change.toFixed(2)} (${changePercent.toFixed(2)}%)`;
      document.getElementById('price-change').style.color = change >= 0 ? 'green' : 'red';
    } else {
      document.getElementById('price-change').textContent = '--';
      document.getElementById('price-change').style.color = 'black';
    }
  } else {
    document.getElementById('price-change').textContent = '--';
    document.getElementById('price-change').style.color = 'black';
  }

  // Update Volume
  document.getElementById('latest-volume').textContent = latestData.volume !== null ? latestData.volume.toLocaleString() : '--';
}

// Function to dynamically create color pickers based on selected indicators
function generateColorPickers(selectedIndicators) {
  const colorPickersContainer = document.getElementById('color-pickers');
  colorPickersContainer.innerHTML = ''; // Clear existing color pickers

  selectedIndicators.forEach(indicator => {
    const colorPickerGroup = document.createElement('div');
    colorPickerGroup.className = 'color-picker-group';

    const label = document.createElement('label');
    label.setAttribute('for', `${indicator}-color`);
    label.textContent = `${capitalizeFirstLetter(indicator)} Color:`;

    const input = document.createElement('input');
    input.type = 'color';
    input.id = `${indicator}-color`;
    input.value = getDefaultColor(indicator);

    colorPickerGroup.appendChild(label);
    colorPickerGroup.appendChild(input);
    colorPickersContainer.appendChild(colorPickerGroup);
  });
}

// Helper function to capitalize first letter
function capitalizeFirstLetter(string) {
  return string.charAt(0).toUpperCase() + string.slice(1);
}

// Helper function to get default color based on indicator
function getDefaultColor(indicator) {
  const defaultColors = {
    accelerationBands: "#9966cc",
    bollingerBands: "#ffa500",
    rsi: "#33cc33",
    macd: "#ff3333",
    volume: "#3333ff"
  };
  return defaultColors[indicator] || "#000000";
}

// Event listener for indicator selection to generate color pickers
document.getElementById("indicator-select").addEventListener("change", function () {
  const selectedIndicators = getSelectedIndicators();
  generateColorPickers(selectedIndicators);
});

// Event listener for fetching data
document.getElementById("fetch-data").addEventListener("click", async () => {
  const symbol = document.getElementById("symbol").value.trim();
  const startDate = document.getElementById("start-date").value;
  const endDate = document.getElementById("end-date").value;
  const interval = document.getElementById("interval-select").value;

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

  // Stop any existing polling
  stopPolling();

  const chartData = await fetchData(symbol, startDate, endDate, interval);
  if (chartData.length === 0) return;

  if (chartInstance) {
    chartInstance.root.dispose();
  }
  chartInstance = createChart(chartData, interval);

  // Automatically apply selected indicators
  const indicators = getSelectedIndicators();
  if (indicators.length > 0) {
    applyIndicators(indicators, chartInstance);
  }

  // Start polling for real-time updates
  startPolling(symbol, interval);
});

// Function to get selected indicators from the indicators select
function getSelectedIndicators() {
  const selectedOptions = Array.from(document.getElementById("indicator-select").selectedOptions);
  return selectedOptions.map(option => option.value);
}

// Function to get selected patterns from the patterns select
function getSelectedPatterns() {
  const selectedOptions = Array.from(document.getElementById("menu-select").selectedOptions);
  return selectedOptions.map(option => option.value);
}

// Event listener for applying indicators
document.getElementById("apply-indicators").addEventListener("click", () => {
  if (!chartInstance) {
    alert("Please fetch data before applying indicators.");
    return;
  }

  const indicators = getSelectedIndicators();
  if (indicators.length === 0) {
    alert("Please select at least one indicator to apply.");
    return;
  }

  // Get color values from color pickers
  const indicatorColors = {};
  indicators.forEach(indicator => {
    const colorInput = document.getElementById(`${indicator}-color`);
    if (colorInput) {
      indicatorColors[indicator] = am5.color(colorInput.value);
    }
  });

  applyIndicators(indicators, chartInstance, indicatorColors);
});

// Event listener for applying patterns
document.getElementById("apply-patterns").addEventListener("click", () => {
  if (!chartInstance) {
    alert("Please fetch data before applying patterns.");
    return;
  }

  const patterns = getSelectedPatterns();
  if (patterns.length === 0) {
    alert("Please select at least one pattern to apply.");
    return;
  }

  applyPatterns(patterns, chartInstance.mainChart, chartInstance.candlestickSeries);
});

// Function to apply indicators
function applyIndicators(indicators, chartInstance, indicatorColors = {}) {
  const { root, mainChart, candlestickSeries, dateAxis, valueAxis, mainLegend } = chartInstance;

  indicators.forEach((indicator) => {
    switch (indicator) {
      case "bollingerBands":
        addBollingerBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, indicatorColors[indicator] || am5.color(0xffa500), mainLegend);
        break;

      case "accelerationBands":
        addAccelerationBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, indicatorColors[indicator] || am5.color(0x9966cc), mainLegend);
        break;

      case "rsi":
        addRSI(root, chartInstance, indicatorColors[indicator] || am5.color(0x33cc33), 14);
        break;

      case "macd":
        addMACD(root, chartInstance, indicatorColors[indicator] || am5.color(0xff3333));
        break;

      case "volume":
        addVolume(mainChart, root, dateAxis, valueAxis, candlestickSeries, indicatorColors[indicator] || am5.color(0x3333ff), mainLegend);
        break;

      default:
        console.warn(`Indicator ${indicator} not recognized.`);
        return;
    }
  });
}

// Function to apply patterns (highlighting specific candlesticks)
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

    // Create a highlight series
    const highlightSeries = chart.series.push(
      am5xy.LineSeries.new(chart.root, {
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
          if (
            prev.close < prev.open &&
            curr.close > curr.open &&
            curr.open < prev.close &&
            curr.close > prev.open
          ) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightBearish":
          if (
            prev.close > prev.open &&
            curr.close < curr.open &&
            curr.open > prev.close &&
            curr.close < prev.open
          ) {
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
          if (
            bodyHammer < 0.3 * (curr.high - curr.low) &&
            lowerShadowHammer > 2 * bodyHammer
          ) {
            highlights.push({
              date: curr.date,
              close: curr.close
            });
          }
          break;
        case "highlightShootingStar":
          const upperShadow = curr.high - Math.max(curr.close, curr.open);
          if (
            Math.abs(curr.close - curr.open) < 0.3 * (curr.high - curr.low) &&
            upperShadow > 2 * Math.abs(curr.close - curr.open)
          ) {
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
          if (
            first.close < first.open &&
            Math.abs(second.close - second.open) < 0.1 * (second.high - second.low) &&
            third.close > third.open &&
            third.close > (first.open + first.close) / 2
          ) {
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
        return am5.Bullet.new(chart.root, {
          sprite: am5.Circle.new(chart.root, {
            radius: 5,
            fill: patternColors[pattern]
          })
        });
      });
    }
  });
}

// Function to add Bollinger Bands
function addBollingerBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, color, mainLegend) {
  // Calculate Bollinger Bands (Simple Moving Average ± 2 Standard Deviations)
  // This is a simplified example; for production, consider using a more robust calculation.

  const smaPeriod = 20; // Simple Moving Average period
  const deviation = 2;  // Number of standard deviations

  // Calculate SMA and standard deviation
  const smaData = [];
  const upperBand = [];
  const lowerBand = [];

  for (let i = 0; i < candlestickSeries.data.length; i++) {
    if (i < smaPeriod - 1) {
      smaData.push(null);
      upperBand.push(null);
      lowerBand.push(null);
      continue;
    }

    const slice = candlestickSeries.data.slice(i - smaPeriod + 1, i + 1);
    const closes = slice.map(item => item.close);
    const sum = closes.reduce((a, b) => a + b, 0);
    const sma = sum / smaPeriod;
    const variance = closes.reduce((a, b) => a + Math.pow(b - sma, 2), 0) / smaPeriod;
    const stdDev = Math.sqrt(variance);

    smaData.push(sma);
    upperBand.push(sma + deviation * stdDev);
    lowerBand.push(sma - deviation * stdDev);
  }

  // Add SMA Series
  const smaSeries = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "SMA",
      xAxis: dateAxis,
      yAxis: valueAxis,
      valueYField: "sma",
      valueXField: "date",
      stroke: color,
      tooltip: am5.Tooltip.new(root, {
        labelText: "SMA: {sma}"
      })
    })
  );

  // Prepare SMA Data
  const smaChartData = candlestickSeries.data.map((item, index) => ({
    date: item.date,
    sma: smaData[index]
  }));

  smaSeries.data.setAll(smaChartData);

  // Add Upper Bollinger Band Series
  const upperBandSeries = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "Upper Bollinger Band",
      xAxis: dateAxis,
      yAxis: valueAxis,
      valueYField: "upperBand",
      valueXField: "date",
      stroke: color,
      strokeDasharray: [4, 4],
      tooltip: am5.Tooltip.new(root, {
        labelText: "Upper Band: {upperBand}"
      })
    })
  );

  const upperBandData = candlestickSeries.data.map((item, index) => ({
    date: item.date,
    upperBand: upperBand[index]
  }));

  upperBandSeries.data.setAll(upperBandData);

  // Add Lower Bollinger Band Series
  const lowerBandSeries = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "Lower Bollinger Band",
      xAxis: dateAxis,
      yAxis: valueAxis,
      valueYField: "lowerBand",
      valueXField: "date",
      stroke: color,
      strokeDasharray: [4, 4],
      tooltip: am5.Tooltip.new(root, {
        labelText: "Lower Band: {lowerBand}"
      })
    })
  );

  const lowerBandData = candlestickSeries.data.map((item, index) => ({
    date: item.date,
    lowerBand: lowerBand[index]
  }));

  lowerBandSeries.data.setAll(lowerBandData);

  // Add to Legend
  mainLegend.data.push(smaSeries, upperBandSeries, lowerBandSeries);
}

// Function to add RSI
function addRSI(root, chartInstance, color, period = 14) {
  const { mainChart, dateAxis, valueAxis } = chartInstance;

  const rsiAxis = mainChart.yAxes.push(
    am5xy.ValueAxis.new(root, {
      renderer: am5xy.AxisRendererY.new(root, {
        opposite: true
      }),
      extraMin: 0.1,
      extraMax: 0.1,
      tooltip: am5.Tooltip.new(root, {})
    })
  );

  const rsiSeries = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "RSI",
      xAxis: dateAxis,
      yAxis: rsiAxis,
      valueYField: "rsi",
      valueXField: "date",
      stroke: color,
      tooltip: am5.Tooltip.new(root, {
        labelText: "RSI: {rsi}"
      })
    })
  );

  // Calculate RSI
  const rsiData = [];
  for (let i = 0; i < chartInstance.candlestickSeries.data.length; i++) {
    if (i < period) {
      rsiData.push(null);
      continue;
    }

    let gains = 0;
    let losses = 0;
    for (let j = i - period + 1; j <= i; j++) {
      const change = chartInstance.candlestickSeries.data[j].close - chartInstance.candlestickSeries.data[j - 1].close;
      if (change > 0) {
        gains += change;
      } else {
        losses -= change;
      }
    }

    const averageGain = gains / period;
    const averageLoss = losses / period;

    const rs = averageLoss === 0 ? 100 : averageGain / averageLoss;
    const rsi = averageLoss === 0 ? 100 : 100 - (100 / (1 + rs));

    rsiData.push(rsi);
  }

  const rsiChartData = chartInstance.candlestickSeries.data.map((item, index) => ({
    date: item.date,
    rsi: rsiData[index] !== null ? Number(rsiData[index].toFixed(2)) : null
  }));

  rsiSeries.data.setAll(rsiChartData);

  // Add to Legend
  chartInstance.mainLegend.data.push(rsiSeries);
}

// Function to add MACD
function addMACD(root, chartInstance, color) {
  const { mainChart, dateAxis, valueAxis } = chartInstance;

  const macdAxis = mainChart.yAxes.push(
    am5xy.ValueAxis.new(root, {
      renderer: am5xy.AxisRendererY.new(root, {
        opposite: true
      }),
      extraMin: 0.1,
      extraMax: 0.1,
      tooltip: am5.Tooltip.new(root, {})
    })
  );

  const macdLineSeries = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "MACD",
      xAxis: dateAxis,
      yAxis: macdAxis,
      valueYField: "macd",
      valueXField: "date",
      stroke: color,
      tooltip: am5.Tooltip.new(root, {
        labelText: "MACD: {macd}"
      })
    })
  );

  const signalLineSeries = mainChart.series.push(
    am5xy.LineSeries.new(root, {
      name: "Signal Line",
      xAxis: dateAxis,
      yAxis: macdAxis,
      valueYField: "signal",
      valueXField: "date",
      stroke: am5.color(0xff0000),
      tooltip: am5.Tooltip.new(root, {
        labelText: "Signal Line: {signal}"
      })
    })
  );

  const histogramSeries = mainChart.series.push(
    am5xy.ColumnSeries.new(root, {
      name: "Histogram",
      xAxis: dateAxis,
      yAxis: macdAxis,
      valueYField: "histogram",
      valueXField: "date",
      fill: color,
      stroke: color,
      tooltip: am5.Tooltip.new(root, {
        labelText: "Histogram: {histogram}"
      })
    })
  );

  // Calculate MACD
  const ema12 = calculateEMA(chartInstance.candlestickSeries.data, 12);
  const ema26 = calculateEMA(chartInstance.candlestickSeries.data, 26);
  const macd = [];
  const signal = [];
  const histogram = [];

  for (let i = 0; i < chartInstance.candlestickSeries.data.length; i++) {
    if (i < 25) { // Minimum data points for EMA26
      macd.push(null);
      signal.push(null);
      histogram.push(null);
      continue;
    }

    const currentMACD = ema12[i] - ema26[i];
    macd.push(Number(currentMACD.toFixed(2)));

    if (i < 25 + 9) { // Minimum for signal line (9-period EMA of MACD)
      signal.push(null);
      histogram.push(null);
      continue;
    }

    // Calculate signal line as EMA of MACD
    const signalSlice = macd.slice(i - 8, i + 1);
    const sumSignal = signalSlice.reduce((a, b) => a + (b !== null ? b : 0), 0);
    const currentSignal = sumSignal / 9;
    signal.push(Number(currentSignal.toFixed(2)));

    // Histogram
    histogram.push(Number((currentMACD - currentSignal).toFixed(2)));
  }

  const macdChartData = chartInstance.candlestickSeries.data.map((item, index) => ({
    date: item.date,
    macd: macd[index],
    signal: signal[index],
    histogram: histogram[index]
  }));

  macdLineSeries.data.setAll(macdChartData);
  signalLineSeries.data.setAll(macdChartData);
  histogramSeries.data.setAll(macdChartData);

  // Add to Legend
  chartInstance.mainLegend.data.push(macdLineSeries, signalLineSeries, histogramSeries);
}

// Helper function to calculate Exponential Moving Average (EMA)
function calculateEMA(data, period) {
  const k = 2 / (period + 1);
  const ema = [];

  // Start by calculating the Simple Moving Average for the first 'period' data points
  let sum = 0;
  for (let i = 0; i < data.length; i++) {
    sum += data[i].close;
    if (i === period - 1) {
      ema.push(sum / period);
    } else if (i >= period) {
      ema.push((data[i].close - ema[i - 1]) * k + ema[i - 1]);
    } else {
      ema.push(null);
    }
  }

  return ema;
}

// Function to add Volume
function addVolume(mainChart, root, dateAxis, valueAxis, candlestickSeries, color, mainLegend) {
  const volumeAxis = mainChart.yAxes.push(
    am5xy.ValueAxis.new(root, {
      renderer: am5xy.AxisRendererY.new(root, {
        opposite: true
      }),
      extraMin: 0.1,
      extraMax: 0.1,
      tooltip: am5.Tooltip.new(root, {})
    })
  );

  const volumeSeries = mainChart.series.push(
    am5xy.ColumnSeries.new(root, {
      name: "Volume",
      xAxis: dateAxis,
      yAxis: volumeAxis,
      valueYField: "volume",
      valueXField: "date",
      fill: color,
      stroke: color,
      tooltip: am5.Tooltip.new(root, {
        labelText: "Volume: {volume}"
      })
    })
  );

  volumeSeries.data.setAll(candlestickSeries.data);

  // Add to Legend
  mainLegend.data.push(volumeSeries);
}

// Placeholder Function for Acceleration Bands
function addAccelerationBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, color, mainLegend) {
  // Acceleration Bands implementation goes here
  console.warn("Acceleration Bands not implemented.");
  // Implement the actual calculation and rendering here.
}

document.addEventListener("DOMContentLoaded", () => {
  // Get references to the datetime input elements
  const endDateInput = document.getElementById("end-date");
  const startDateInput = document.getElementById("start-date");

  // Get the current date and one week ago in UTC
  const now = new Date();
  const oneWeekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);

  // Set the default values in UTC
  endDateInput.value = formatDateTimeUTC(now);
  startDateInput.value = formatDateTimeUTC(oneWeekAgo);

  // Optionally, set min and max attributes to prevent invalid date selections
  endDateInput.max = formatDateTimeUTC(now);
  startDateInput.max = formatDateTimeUTC(now);

  // Initialize color pickers based on default selected indicators
  const initialIndicators = getSelectedIndicators();
  generateColorPickers(initialIndicators);

  // Optionally, validate and set date pickers constraints
  // For example, ensuring start date is always before end date
  startDateInput.addEventListener("change", () => {
    if (new Date(startDateInput.value) > new Date(endDateInput.value)) {
      alert("Start date cannot be after end date.");
      startDateInput.value = formatDateTimeUTC(oneWeekAgo);
    }
  });

  endDateInput.addEventListener("change", () => {
    if (new Date(startDateInput.value) > new Date(endDateInput.value)) {
      alert("End date cannot be before start date.");
      endDateInput.value = formatDateTimeUTC(now);
    }
  });

  // Add event listener to fetch-data button
  const fetchDataButton = document.getElementById("fetch-data");
  fetchDataButton.click();

  // Add event listeners for Zoom Controls
  document.getElementById("zoom-in").addEventListener("click", () => {
    if (!chartInstance) return;
    const { dateAxis } = chartInstance;
    zoomAxis(dateAxis, 0.2); // Zoom in by 20%
  });

  document.getElementById("zoom-out").addEventListener("click", () => {
    if (!chartInstance) return;
    const { dateAxis } = chartInstance;
    zoomAxis(dateAxis, -0.2); // Zoom out by 20%
  });

  document.getElementById("reset-zoom").addEventListener("click", () => {
    if (!chartInstance) return;
    const { dateAxis } = chartInstance;
    dateAxis.zoom(0, 1); // Reset zoom to show all data
    updateZoomLevelDisplay(dateAxis); // Update the zoom level display
  });
});

// Optionally, handle window unload to stop polling
window.addEventListener('beforeunload', () => {
  stopPolling();
});

// Function to zoom the axis by a certain percentage
function zoomAxis(axis, zoomChange) {
  // Get current zoom
  const start = axis.get("start");
  const end = axis.get("end");

  // Calculate the range
  const range = end - start;

  // Calculate new start and end based on zoomChange
  let newStart = start + (range * zoomChange) / 2;
  let newEnd = end - (range * zoomChange) / 2;

  // Define minimum and maximum zoom levels
  const MIN_RANGE = 0.05; // 5%
  const MAX_RANGE = 1;     // 100%

  // Calculate new range
  const newRange = newEnd - newStart;

  // Enforce minimum range
  if (newRange < MIN_RANGE) {
    const midpoint = (newStart + newEnd) / 2;
    newStart = midpoint - MIN_RANGE / 2;
    newEnd = midpoint + MIN_RANGE / 2;

    // Clamp values
    newStart = Math.max(0, newStart);
    newEnd = Math.min(1, newEnd);
  }

  // Enforce maximum range
  if (newRange > MAX_RANGE) {
    newStart = 0;
    newEnd = 1;
  }

  // Apply new zoom with animation
  axis.animate({
    key: "zoom",
    to: { start: newStart, end: newEnd },
    duration: 500, // Duration in milliseconds
    easing: am5.ease.linear
  }).then(() => {
    // Update zoom level display after animation
    updateZoomLevelDisplay(axis);
  });
}

// Function to update zoom level display
function updateZoomLevelDisplay(axis) {
  const start = axis.get("start");
  const end = axis.get("end");

  const startDate = axis.get("startDate");
  const endDate = axis.get("endDate");

  // Calculate visible range based on current zoom
  const visibleStartTimestamp = startDate.getTime() + start * (endDate.getTime() - startDate.getTime());
  const visibleEndTimestamp = startDate.getTime() + end * (endDate.getTime() - startDate.getTime());

  const visibleStartDate = new Date(visibleStartTimestamp);
  const visibleEndDate = new Date(visibleEndTimestamp);

  // Format dates for display
  const formatOptions = { year: 'numeric', month: 'short', day: 'numeric' };
  const formattedStartDate = visibleStartDate.toLocaleDateString(undefined, formatOptions);
  const formattedEndDate = visibleEndDate.toLocaleDateString(undefined, formatOptions);

  const zoomLevelText = `Zoom Level: ${formattedStartDate} - ${formattedEndDate}`;
  let zoomDisplay = document.getElementById("zoom-level");

  if (!zoomDisplay) {
    // If the zoom-level element doesn't exist, create it
    zoomDisplay = document.createElement('div');
    zoomDisplay.id = "zoom-level";
    zoomDisplay.style.marginTop = "10px";
    zoomDisplay.textContent = zoomLevelText;
    document.body.insertBefore(zoomDisplay, document.getElementById("chartdiv"));
  } else {
    // Update existing zoom-level element
    zoomDisplay.textContent = zoomLevelText;
  }
}

// Function to apply indicators
function applyIndicators(indicators, chartInstance, indicatorColors = {}) {
  const { root, mainChart, candlestickSeries, dateAxis, valueAxis, mainLegend } = chartInstance;

  indicators.forEach((indicator) => {
    switch (indicator) {
      case "bollingerBands":
        addBollingerBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, indicatorColors[indicator] || am5.color(0xffa500), mainLegend);
        break;

      case "accelerationBands":
        addAccelerationBands(mainChart, root, dateAxis, valueAxis, candlestickSeries, indicatorColors[indicator] || am5.color(0x9966cc), mainLegend);
        break;

      case "rsi":
        addRSI(root, chartInstance, indicatorColors[indicator] || am5.color(0x33cc33), 14);
        break;

      case "macd":
        addMACD(root, chartInstance, indicatorColors[indicator] || am5.color(0xff3333));
        break;

      case "volume":
        addVolume(mainChart, root, dateAxis, valueAxis, candlestickSeries, indicatorColors[indicator] || am5.color(0x3333ff), mainLegend);
        break;

      default:
        console.warn(`Indicator ${indicator} not recognized.`);
        return;
    }
  });
}
