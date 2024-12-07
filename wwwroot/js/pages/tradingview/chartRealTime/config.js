const configChart = {
  chartContainerId: "chartdiv",
  controlsContainerId: "chartcontrols",
  symbolInputId: "symbolInput",
  suggestionsId: "suggestions",
  updateButtonId: "updateButton",
  updateIntervalInputId: "updateIntervalInput",
  dataIntervalSelectId: "dataIntervalSelect",
  apiEndpoints: {
    historical: (symbol, startDate, endDate, interval) =>
      `/api/YahooFinance/chart-symbol/${encodeURIComponent(symbol)}?startDate=${startDate}&endDate=${endDate}&interval=${encodeURIComponent(interval)}`,
    realTime: (symbol, interval) =>
      `/api/yahoofinance/chart-real-time-symbol/${encodeURIComponent(symbol)}/${encodeURIComponent(interval)}`,
    search: (query) =>
      `/api/yahoofinance/search/${encodeURIComponent(query)}`
  },
  defaultSettings: {
    updateIntervalSec: 10,
    defaultSymbol: "GALP.LS",
    defaultInterval: "5m",
    dateRange: {
      startDate: () => {
        const date = new Date();
        date.setFullYear(date.getFullYear() - 1);
        return date.toISOString().split('T')[0];
      },
      endDate: () => new Date().toISOString().split('T')[0]
    }
  }
};
export default configChart;
