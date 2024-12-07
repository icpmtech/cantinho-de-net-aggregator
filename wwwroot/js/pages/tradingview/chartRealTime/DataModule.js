const DataModule = (() => {
  async function fetchHistoricalData(apiUrl) {
    const response = await fetch(apiUrl);
    if (!response.ok) {
      throw new Error(`Error fetching data: ${response.status} ${response.statusText}`);
    }
    const rawData = await response.json();
    return rawData.map(item => ({
      Date: new Date(item.timestamp).getTime(),
      Open: parseFloat(item.open),
      Close: parseFloat(item.close),
      High: parseFloat(item.high),
      Low: parseFloat(item.low),
      Volume: parseInt(item.volume, 10)
    }));
  }

  async function fetchRealTimeData(apiUrl) {
    const response = await fetch(apiUrl);
    if (!response.ok) {
      throw new Error(`Error fetching real-time data: ${response.status} ${response.statusText}`);
    }
    const item = await response.json();
    return {
      Date: new Date(item.Timestamp || item.timestamp).getTime(),
      Open: parseFloat(item.Open || item.open),
      Close: parseFloat(item.Close || item.close),
      High: parseFloat(item.High || item.high),
      Low: parseFloat(item.Low || item.low),
      Volume: parseInt(item.Volume || item.volume, 10)
    };
  }

  async function searchSymbols(apiUrl) {
    const response = await fetch(apiUrl);
    if (!response.ok) {
      throw new Error(`Error searching symbols: ${response.statusText}`);
    }
    return await response.json();
  }

  return {
    fetchHistoricalData,
    fetchRealTimeData,
    searchSymbols
  };
})();
export default DataModule;
