const ChartModule = (() => {
  let root, stockChart, mainPanel, valueSeries, volumeSeries, sbSeries;
  let dateAxis, valueAxis, volumeValueAxis, sbDateAxis, sbValueAxis;
  let valueLegend;

  let currentSeriesType = "candlestick";

  function initializeChart(config) {
    if (!config || !config.chartContainerId) {
      console.error("Configuration object with 'chartContainerId' is required.");
      return;
    }

    root = am5.Root.new(config.chartContainerId);

    const myTheme = am5.Theme.new(root);
    myTheme.rule("Grid", ["scrollbar", "minor"]).setAll({ visible: false });
    root.setThemes([am5themes_Animated.new(root), myTheme]);

    stockChart = root.container.children.push(am5stock.StockChart.new(root, { paddingRight: 0 }));
    root.numberFormatter.set("numberFormat", "#,###.00");

    mainPanel = stockChart.panels.push(am5stock.StockPanel.new(root, {
      wheelY: "zoomX",
      panX: true,
      panY: true
    }));

    setupAxes();
    setupSeries();
    setupScrollbar();
    animateSeries();

    return {
      root,
      stockChart,
      mainPanel,
      valueSeries,
      volumeSeries,
      sbSeries,
      dateAxis,
      valueAxis,
      volumeValueAxis,
      sbDateAxis,
      sbValueAxis,
      valueLegend
    };
  }

  function setupAxes() {
    valueAxis = mainPanel.yAxes.push(am5xy.ValueAxis.new(root, {
      renderer: am5xy.AxisRendererY.new(root, { pan: "zoom" }),
      extraMin: 0.1,
      tooltip: am5.Tooltip.new(root, {}),
      numberFormat: "#,###.00",
      extraTooltipPrecision: 2
    }));

    dateAxis = mainPanel.xAxes.push(am5xy.GaplessDateAxis.new(root, {
      baseInterval: { timeUnit: "minute", count: 1 },
      renderer: am5xy.AxisRendererX.new(root, { minorGridEnabled: true }),
      tooltip: am5.Tooltip.new(root, {})
    }));

    const volumeAxisRenderer = am5xy.AxisRendererY.new(root, { inside: true, pan: "zoom" });
    volumeAxisRenderer.labels.template.set("forceHidden", true);
    volumeAxisRenderer.grid.template.set("forceHidden", true);

    volumeValueAxis = mainPanel.yAxes.push(am5xy.ValueAxis.new(root, {
      numberFormat: "#.#a",
      height: am5.percent(20),
      y: am5.percent(100),
      centerY: am5.percent(100),
      renderer: volumeAxisRenderer
    }));
  }

  function setupSeries() {
    valueSeries = mainPanel.series.push(am5xy.CandlestickSeries.new(root, {
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
      legendValueText: "Open: [bold]{openValueY}[/] High: [bold]{highValueY}[/] Low: [bold]{lowValueY}[/] Close: [bold]{valueY}[/]"
    }));

    volumeSeries = mainPanel.series.push(am5xy.ColumnSeries.new(root, {
      name: "Volume",
      clustered: false,
      valueXField: "Date",
      valueYField: "Volume",
      xAxis: dateAxis,
      yAxis: volumeValueAxis,
      legendValueText: "[bold]{valueY.formatNumber('#,###.0a')}[/]"
    }));

    volumeSeries.columns.template.setAll({ strokeOpacity: 0, fillOpacity: 0.5 });

    valueSeries.setAll({ interpolationDuration: 500, interpolationEasing: am5.ease.linear });
    volumeSeries.setAll({ interpolationDuration: 500, interpolationEasing: am5.ease.linear });

    valueLegend = mainPanel.plotContainer.children.push(am5stock.StockLegend.new(root, {
      stockChart: stockChart
    }));
    valueLegend.data.setAll([valueSeries, volumeSeries]);

    stockChart.set("stockSeries", valueSeries);
    stockChart.set("volumeSeries", volumeSeries);
  }

  function setupScrollbar() {
    const scrollbar = mainPanel.set("scrollbarX", am5xy.XYChartScrollbar.new(root, {
      orientation: "horizontal",
      height: 50
    }));

    stockChart.toolsContainer.children.push(scrollbar);

    sbDateAxis = scrollbar.chart.xAxes.push(am5xy.GaplessDateAxis.new(root, {
      baseInterval: { timeUnit: "minute", count: 1 },
      renderer: am5xy.AxisRendererX.new(root, { minorGridEnabled: true })
    }));

    sbValueAxis = scrollbar.chart.yAxes.push(am5xy.ValueAxis.new(root, {
      renderer: am5xy.AxisRendererY.new(root, {})
    }));

    sbSeries = scrollbar.chart.series.push(am5xy.LineSeries.new(root, {
      valueYField: "Close",
      valueXField: "Date",
      xAxis: sbDateAxis,
      yAxis: sbValueAxis
    }));

    sbSeries.fills.template.setAll({ visible: true, fillOpacity: 0.3 });
    sbSeries.setAll({ interpolationDuration: 500, interpolationEasing: am5.ease.linear });
  }

  function animateSeries() {
    valueSeries.appear(1000);
    volumeSeries.appear(1000);
    sbSeries.appear(1000);
    dateAxis.appear(1000);
    valueAxis.appear(1000);
    volumeValueAxis.appear(1000);
  }

  function updateSeriesData(newData, isUpdate = false) {
    if (!valueSeries || !volumeSeries || !sbSeries) {
      console.error("Series not initialized. Call initializeChart first.");
      return;
    }

    if (isUpdate) {
      const newDataPoint = newData[0];
      if (!newDataPoint || isNaN(newDataPoint.Date) || isNaN(newDataPoint.Close)) {
        console.error("Invalid data for real-time update.");
        return;
      }

      const lastDataPoint = valueSeries.data.getIndex(valueSeries.data.length - 1);
      if (lastDataPoint && newDataPoint.Date > lastDataPoint.Date) {
        valueSeries.data.push(newDataPoint);
        volumeSeries.data.push(newDataPoint);
        sbSeries.data.push(newDataPoint);
      } else {
        valueSeries.data.setIndex(valueSeries.data.length - 1, newDataPoint);
        volumeSeries.data.setIndex(volumeSeries.data.length - 1, newDataPoint);
        sbSeries.data.setIndex(sbSeries.data.length - 1, newDataPoint);
      }
    } else {
      valueSeries.data.setAll(newData);
      volumeSeries.data.setAll(newData);
      sbSeries.data.setAll(newData);
    }
  }

  function adjustBaseInterval(interval) {
    let timeUnit = "minute", count = 1;

    const match = interval.match(/(\d+)([mhdwmo]+)/);
    if (match) {
      count = parseInt(match[1], 10);
      switch (match[2]) {
        case "m": timeUnit = "minute"; break;
        case "h": timeUnit = "hour"; break;
        case "d": timeUnit = "day"; break;
        case "w": timeUnit = "week"; break;
        case "mo": timeUnit = "month"; break;
        default: timeUnit = "day";
      }
    }

    dateAxis.set("baseInterval", { timeUnit, count });
    sbDateAxis.set("baseInterval", { timeUnit, count });
  }

  return {
    initializeChart,
    updateSeriesData,
    adjustBaseInterval
  };
})();

export default ChartModule;
