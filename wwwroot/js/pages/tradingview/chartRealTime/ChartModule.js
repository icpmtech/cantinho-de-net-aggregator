const ChartModule = (() => {
  let root, stockChart, mainPanel, valueSeries, volumeSeries, sbSeries;

  function initializeChart(config) {
    root = am5.Root.new(config.chartContainerId);

    // Apply themes
    root.setThemes([
      am5themes_Animated.new(root),
      // Add more themes if needed
    ]);

    // Create stock chart
    stockChart = root.container.children.push(am5stock.StockChart.new(root, { paddingRight: 0 }));

    // Set global number format
    root.numberFormatter.set("numberFormat", "#,###.00");

    // Create main stock panel
    mainPanel = stockChart.panels.push(am5stock.StockPanel.new(root, {
      wheelY: "zoomX",
      panX: true,
      panY: true
    }));

    // Initialize axes and series here...
    // Similar to your existing code but using variables from this module

    return { root, stockChart, mainPanel, valueSeries, volumeSeries, sbSeries };
  }

  function updateSeriesData(newData, isUpdate = false) {
    if (isUpdate) {
      // Update existing series with newData
    } else {
      // Set new data for series
    }
  }

  function setSeriesType(seriesType) {
    // Implement series type switching
  }

  return {
    initializeChart,
    updateSeriesData,
    setSeriesType
    // Add more functions as needed
  };
})();
