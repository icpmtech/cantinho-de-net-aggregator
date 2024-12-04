am5.ready(async function () {
  // Create root element
  const root = am5.Root.new("chartdiv");

  // Apply themes
  const myTheme = am5.Theme.new(root);
  myTheme.rule("Grid", ["scrollbar", "minor"]).setAll({
    visible: false
  });

  root.setThemes([
    am5themes_Animated.new(root),
    myTheme
  ]);

  // Create a stock chart
  const stockChart = root.container.children.push(am5stock.StockChart.new(root, {
    paddingRight: 0
  }));

  // Set global number format
  root.numberFormatter.set("numberFormat", "#,###.00");

  // Create main stock panel
  const mainPanel = stockChart.panels.push(am5stock.StockPanel.new(root, {
    wheelY: "zoomX",
    panX: true,
    panY: true
  }));

  // Create value axis
  const valueAxis = mainPanel.yAxes.push(am5xy.ValueAxis.new(root, {
    renderer: am5xy.AxisRendererY.new(root, {
      pan: "zoom"
    }),
    extraMin: 0.1,
    tooltip: am5.Tooltip.new(root, {}),
    numberFormat: "#,###.00",
    extraTooltipPrecision: 2
  }));

  // Create date axis
  const dateAxis = mainPanel.xAxes.push(am5xy.GaplessDateAxis.new(root, {
    baseInterval: {
      timeUnit: "minute",
      count: 1
    },
    renderer: am5xy.AxisRendererX.new(root, {
      minorGridEnabled: true
    }),
    tooltip: am5.Tooltip.new(root, {})
  }));

  // Add value series (Candlestick)
  const valueSeries = mainPanel.series.push(am5xy.CandlestickSeries.new(root, {
    name: "Stock Data",
    clustered: false,
    valueXField: "Date",
    valueYField: "Close",
    highValueYField: "High",
    lowValueYField: "Low",
    openValueYField: "Open",
    calculateAggregates: true,
    xAxis: dateAxis,
    yAxis: valueAxis,
    legendValueText: "Open: [bold]{openValueY}[/] High: [bold]{highValueY}[/] Low: [bold]{lowValueY}[/] Close: [bold]{valueY}[/]",
    legendRangeValueText: ""
  }));

  // Set interpolation for valueSeries
  valueSeries.set("interpolationDuration", 500);
  valueSeries.set("interpolationEasing", am5.ease.linear);

  // Set main value series
  stockChart.set("stockSeries", valueSeries);

  // Add a stock legend
  const valueLegend = mainPanel.plotContainer.children.push(am5stock.StockLegend.new(root, {
    stockChart: stockChart
  }));

  // Create volume axis
  const volumeAxisRenderer = am5xy.AxisRendererY.new(root, {
    inside: true,
    pan: "zoom"
  });

  volumeAxisRenderer.labels.template.set("forceHidden", true);
  volumeAxisRenderer.grid.template.set("forceHidden", true);

  const volumeValueAxis = mainPanel.yAxes.push(am5xy.ValueAxis.new(root, {
    numberFormat: "#.#a",
    height: am5.percent(20),
    y: am5.percent(100),
    centerY: am5.percent(100),
    renderer: volumeAxisRenderer
  }));

  // Add volume series
  const volumeSeries = mainPanel.series.push(am5xy.ColumnSeries.new(root, {
    name: "Volume",
    clustered: false,
    valueXField: "Date",
    valueYField: "Volume",
    xAxis: dateAxis,
    yAxis: volumeValueAxis,
    legendValueText: "[bold]{valueY.formatNumber('#,###.0a')}[/]"
  }));

  volumeSeries.columns.template.setAll({
    strokeOpacity: 0,
    fillOpacity: 0.5
  });

  // Color volume columns based on stock rules
  volumeSeries.columns.template.adapters.add("fill", function (fill, target) {
    const dataItem = target.dataItem;
    if (dataItem) {
      return stockChart.getVolumeColor(dataItem);
    }
    return fill;
  });

  // Set interpolation for volumeSeries
  volumeSeries.set("interpolationDuration", 500);
  volumeSeries.set("interpolationEasing", am5.ease.linear);

  // Set main volume series
  stockChart.set("volumeSeries", volumeSeries);

  // Add series to legend
  valueLegend.data.setAll([valueSeries, volumeSeries]);

  // Add cursor
  mainPanel.set("cursor", am5xy.XYCursor.new(root, {
    yAxis: valueAxis,
    xAxis: dateAxis,
    snapToSeries: [valueSeries],
    snapToSeriesBy: "y!"
  }));

  // Add scrollbar
  const scrollbar = mainPanel.set("scrollbarX", am5xy.XYChartScrollbar.new(root, {
    orientation: "horizontal",
    height: 50
  }));
  stockChart.toolsContainer.children.push(scrollbar);

  const sbDateAxis = scrollbar.chart.xAxes.push(am5xy.GaplessDateAxis.new(root, {
    baseInterval: {
      timeUnit: "minute",
      count: 1
    },
    renderer: am5xy.AxisRendererX.new(root, {
      minorGridEnabled: true
    })
  }));

  const sbValueAxis = scrollbar.chart.yAxes.push(am5xy.ValueAxis.new(root, {
    renderer: am5xy.AxisRendererY.new(root, {})
  }));

  const sbSeries = scrollbar.chart.series.push(am5xy.LineSeries.new(root, {
    valueYField: "Close",
    valueXField: "Date",
    xAxis: sbDateAxis,
    yAxis: sbValueAxis
  }));

  sbSeries.fills.template.setAll({
    visible: true,
    fillOpacity: 0.3
  });

  // Set interpolation for sbSeries
  sbSeries.set("interpolationDuration", 500);
  sbSeries.set("interpolationEasing", am5.ease.linear);

  // Animate series appearance
  valueSeries.appear(1000);
  volumeSeries.appear(1000);
  sbSeries.appear(1000);

  // Animate axes appearance
  dateAxis.appear(1000);
  valueAxis.appear(1000);
  volumeValueAxis.appear(1000);

  // Set up series type switcher
  const seriesSwitcher = am5stock.SeriesTypeControl.new(root, {
    stockChart: stockChart
  });

  seriesSwitcher.events.on("selected", function (ev) {
    setSeriesType(ev.item.id);
  });

  function getNewSettings(series) {
    const newSettings = {};
    am5.array.each(["name", "valueYField", "highValueYField", "lowValueYField", "openValueYField", "calculateAggregates", "valueXField", "xAxis", "yAxis", "legendValueText", "legendRangeValueText", "stroke", "fill"], function (setting) {
      newSettings[setting] = series.get(setting);
    });
    return newSettings;
  }

  function setSeriesType(seriesType) {
    // Get current series and its settings
    const currentSeries = stockChart.get("stockSeries");
    const newSettings = getNewSettings(currentSeries);

    // Remove previous series
    const data = currentSeries.data.values;
    mainPanel.series.removeValue(currentSeries);

    // Create new series
    let series;
    switch (seriesType) {
      case "line":
        series = mainPanel.series.push(am5xy.LineSeries.new(root, newSettings));
        break;
      case "candlestick":
      case "procandlestick":
        newSettings.clustered = false;
        series = mainPanel.series.push(am5xy.CandlestickSeries.new(root, newSettings));
        if (seriesType === "procandlestick") {
          series.columns.template.get("themeTags").push("pro");
        }
        break;
      case "ohlc":
        newSettings.clustered = false;
        series = mainPanel.series.push(am5xy.OHLCSeries.new(root, newSettings));
        break;
    }

    // Set new series as stockSeries
    if (series) {
      valueLegend.data.removeValue(currentSeries);
      series.data.setAll(data);
      stockChart.set("stockSeries", series);
      const cursor = mainPanel.get("cursor");
      if (cursor) {
        cursor.set("snapToSeries", [series]);
      }
      valueLegend.data.insertIndex(0, series);
    }
  }

  // Stock toolbar
  const toolbar = am5stock.StockToolbar.new(root, {
    container: document.getElementById("chartcontrols"),
    stockChart: stockChart,
    controls: [
      am5stock.IndicatorControl.new(root, {
        stockChart: stockChart,
        legend: valueLegend
      }),
      am5stock.DateRangeSelector.new(root, {
        stockChart: stockChart
      }),
      am5stock.PeriodSelector.new(root, {
        stockChart: stockChart
      }),
      seriesSwitcher,
      am5stock.DrawingControl.new(root, {
        stockChart: stockChart
      }),
      am5stock.DataSaveControl.new(root, {
        stockChart: stockChart
      }),
      am5stock.ResetControl.new(root, {
        stockChart: stockChart
      }),
      am5stock.SettingsControl.new(root, {
        stockChart: stockChart
      })
    ]
  });

  // Tooltip for events (optional)
  const tooltip = am5.Tooltip.new(root, {
    getStrokeFromSprite: false,
    getFillFromSprite: false
  });

  tooltip.get("background").setAll({
    strokeOpacity: 1,
    stroke: am5.color(0x000000),
    fillOpacity: 1,
    fill: am5.color(0xffffff)
  });

  // Variables for real-time updates
  let lastTimestamp = null;
  let updateIntervalId = null;
  let symbol = document.getElementById('symbolInput').value.trim().toUpperCase();

  // Function to start the auto-update with the specified interval
  function startAutoUpdate() {
    // Clear any existing interval
    if (updateIntervalId) {
      clearInterval(updateIntervalId);
    }
    // Get the interval from input
    let intervalSec = parseInt(document.getElementById('updateIntervalInput').value);
    if (isNaN(intervalSec) || intervalSec < 1) {
      intervalSec = 60; // Default to 60 seconds if invalid
    }
    const intervalMs = intervalSec * 1000;

    // Set up interval to update data every intervalMs milliseconds
    updateIntervalId = setInterval(async () => {
      await fetchData(symbol, true);
    }, intervalMs);
  }

  // Fetch and process data from API
  async function fetchData(symbol, isUpdate = false) {
    try {
      // Get the selected interval
      const interval = document.getElementById('dataIntervalSelect').value;

      // Adjust the baseInterval of date axes according to the selected interval
      adjustBaseInterval(interval);

      let apiUrl;

      if (isUpdate) {
        // For real-time update, call the new endpoint
        apiUrl = `/api/yahoofinance/chart-real-time-symbol/${encodeURIComponent(symbol)}/${encodeURIComponent(interval)}`;
      } else {
        // Get startDate and endDate from input fields
        let startDateInput = document.getElementById('startDateInput').value;
        let endDateInput = document.getElementById('endDateInput').value;

        // If inputs are empty, initialize to default values
        if (!startDateInput) {
          const oneYearAgo = new Date();
          oneYearAgo.setDate(oneYearAgo.getDate() - 365);
          startDateInput = oneYearAgo.toISOString().split('T')[0];
        }
        if (!endDateInput) {
          const today = new Date();
          endDateInput = today.toISOString().split('T')[0];
        }

        const startDate = startDateInput; // Format: YYYY-MM-DD
        const endDate = endDateInput;     // Format: YYYY-MM-DD

        apiUrl = `/api/YahooFinance/chart-symbol/${encodeURIComponent(symbol)}?startDate=${startDate}&endDate=${endDate}&interval=${encodeURIComponent(interval)}`;
      }

      // Fetch data from the API
      const response = await fetch(apiUrl);

      if (!response.ok) {
        throw new Error(`Error fetching data: ${response.status} ${response.statusText}`);
      }

      const rawData = await response.json();

      if (isUpdate) {
        // Process the data from real-time endpoint
        const item = rawData;
        const newDataPoint = {
          Date: new Date(item.Timestamp || item.timestamp).getTime(), // Convert to milliseconds
          Open: parseFloat(item.Open || item.open),
          Close: parseFloat(item.Close || item.close),
          High: parseFloat(item.High || item.high),
          Low: parseFloat(item.Low || item.low),
          Volume: parseInt(item.Volume || item.volume, 10)
        };

        // Validate newDataPoint
        if (isNaN(newDataPoint.Date) || isNaN(newDataPoint.Close)) {
          console.error("Invalid data received for real-time update.");
          return;
        }

        // Get the last data point in the series
        const lastDataPoint = valueSeries.data.getIndex(valueSeries.data.length - 1);

        if (lastDataPoint) {
          if (newDataPoint.Date > lastDataPoint.Date) {
            // Append new data point
            valueSeries.data.push(newDataPoint);
            volumeSeries.data.push(newDataPoint);
            sbSeries.data.push(newDataPoint);
          } else if (newDataPoint.Date === lastDataPoint.Date) {
            // Replace the last data point
            valueSeries.data.setIndex(valueSeries.data.length - 1, newDataPoint);
            volumeSeries.data.setIndex(volumeSeries.data.length - 1, newDataPoint);
            sbSeries.data.setIndex(sbSeries.data.length - 1, newDataPoint);
          }
        } else {
          // No data in series, set data
          valueSeries.data.push(newDataPoint);
          volumeSeries.data.push(newDataPoint);
          sbSeries.data.push(newDataPoint);
        }

        // Update lastTimestamp
        lastTimestamp = newDataPoint.Date;

      } else {
        if (!Array.isArray(rawData)) {
          throw new Error("Invalid data format received from API.");
        }

        // Process the data from historical endpoint
        const processedData = rawData.map(item => ({
          Date: new Date(item.timestamp).getTime(), // Convert to milliseconds
          Open: parseFloat(item.open),
          Close: parseFloat(item.close),
          High: parseFloat(item.high),
          Low: parseFloat(item.low),
          Volume: parseInt(item.volume, 10)
        }));

        if (processedData.length > 0) {
          // Update lastTimestamp
          lastTimestamp = processedData[processedData.length - 1].Date;

          // Set data to series for the first time
          valueSeries.data.setAll(processedData);
          volumeSeries.data.setAll(processedData);
          sbSeries.data.setAll(processedData);
        }
      }

    } catch (error) {
      console.error("Error fetching or processing data:", error);
    }
  }

  // Function to adjust baseInterval based on selected data interval
  function adjustBaseInterval(interval) {
    let timeUnit = 'minute';
    let count = 1;

    if (interval.endsWith('m')) {
      timeUnit = 'minute';
      count = parseInt(interval.replace('m', ''));
    } else if (interval.endsWith('h')) {
      timeUnit = 'hour';
      count = parseInt(interval.replace('h', ''));
    } else if (interval.endsWith('d')) {
      timeUnit = 'day';
      count = parseInt(interval.replace('d', ''));
    } else if (interval.endsWith('wk')) {
      timeUnit = 'week';
      count = parseInt(interval.replace('wk', ''));
    } else if (interval.endsWith('mo')) {
      timeUnit = 'month';
      count = parseInt(interval.replace('mo', ''));
    } else {
      // Default to day if unrecognized
      timeUnit = 'day';
      count = 1;
    }

    dateAxis.set("baseInterval", {
      timeUnit: timeUnit,
      count: count
    });

    sbDateAxis.set("baseInterval", {
      timeUnit: timeUnit,
      count: count
    });
  }

  // Initial data load and start auto-update
  await fetchData(symbol);
  startAutoUpdate();

  // Search and suggestion functions
  async function search() {
    const query = document.getElementById('symbolInput').value.trim();
    const suggestionsBox = document.getElementById('suggestions');
    suggestionsBox.innerHTML = ''; // Clear previous suggestions

    if (query.length >= 1) { // Start searching after 1 character
      try {
        const url = `/api/yahoofinance/search/${encodeURIComponent(query)}`;
        const response = await fetch(url);

        if (response.ok) {
          const data = await response.json();
          if (data.length > 0) {
            suggestionsBox.style.display = 'block';
            data.forEach(result => {
              const suggestionItem = document.createElement('div');
              suggestionItem.classList.add('suggestion-item');

              // Set placeholder image or use real image if available
              const imageUrl = result.img ? result.img : 'https://via.placeholder.com/40';

              suggestionItem.innerHTML = `
                    <img src="${imageUrl}" alt="${result.symbol}">
                    <p>${result.symbol} - ${result.shortname}</p>
                  `;

              suggestionItem.onclick = () => selectSuggestion(result);
              suggestionsBox.appendChild(suggestionItem);
            });
          } else {
            suggestionsBox.style.display = 'none';
          }
        } else {
          console.error('Error fetching symbols:', response.statusText);
          suggestionsBox.style.display = 'none';
        }
      } catch (error) {
        console.error('Error fetching symbols:', error);
        suggestionsBox.style.display = 'none';
      }
    } else {
      suggestionsBox.style.display = 'none';
    }
  }

  async function selectSuggestion(result) {
    document.getElementById('symbolInput').value = result.symbol;
    document.getElementById('suggestions').style.display = 'none';

    // Update the chart with the selected symbol
    symbol = result.symbol;
    lastTimestamp = null; // Reset lastTimestamp when symbol changes
    await fetchData(symbol);
    startAutoUpdate(); // Restart auto-update with new symbol
  }

  // Event listeners
  const symbolInput = document.getElementById('symbolInput');
  symbolInput.addEventListener('input', search);

  document.getElementById('updateButton').addEventListener('click', async function () {
    symbol = symbolInput.value.trim().toUpperCase();
    if (symbol) {
      lastTimestamp = null; // Reset lastTimestamp when symbol changes
      await fetchData(symbol);
      startAutoUpdate(); // Restart auto-update with new symbol
    }
  });

  document.getElementById('updateIntervalInput').addEventListener('change', function () {
    startAutoUpdate();
  });

  document.getElementById('dataIntervalSelect').addEventListener('change', async function () {
    lastTimestamp = null; // Reset lastTimestamp when interval changes
    await fetchData(symbol);
  });

}); // end am5.ready()
