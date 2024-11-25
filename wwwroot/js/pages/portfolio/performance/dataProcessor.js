// dataProcessor.js

// Import necessary functions

// Function to calculate maximum drawdown
function calculateMaxDrawdown(cumulativeReturns) {
  let maxDrawdown = 0;
  let peak = cumulativeReturns[0];

  for (let i = 1; i < cumulativeReturns.length; i++) {
    if (cumulativeReturns[i] > peak) {
      peak = cumulativeReturns[i];
    }
    const drawdown = (peak - cumulativeReturns[i]) / peak;
    if (drawdown > maxDrawdown) {
      maxDrawdown = drawdown;
    }
  }

  return (maxDrawdown * 100).toFixed(2);
}

// Function to calculate portfolio metrics
function calculateMetrics(data) {
  const returns = data.monthlyReturns;
  const avgReturn = returns.length ? (returns.reduce((a, b) => a + b, 0) / returns.length) : 0;
  const stdDev = returns.length > 1
    ? Math.sqrt(returns.reduce((a, b) => a + Math.pow(b - avgReturn, 2), 0) / (returns.length - 1))
    : 0;
  const sharpeRatio = stdDev !== 0 ? (avgReturn / stdDev) : "N/A"; // Assuming risk-free rate is zero

  // Compute cumulative returns
  const cumulativeReturns = returns.reduce((acc, ret, idx) => {
    const prevValue = acc[idx - 1] || 1;
    const newValue = prevValue * (1 + ret / 100);
    acc.push(newValue);
    return acc;
  }, []);

  const totalGrowth = cumulativeReturns.length > 0
    ? ((cumulativeReturns[cumulativeReturns.length - 1] - 1) * 100).toFixed(2)
    : "0.00";

  const maxDrawdown = cumulativeReturns.length > 0
    ? calculateMaxDrawdown(cumulativeReturns)
    : "0.00";

  return {
    sharpeRatio: sharpeRatio === "N/A" ? "N/A" : sharpeRatio.toFixed(2),
    avgReturn: avgReturn.toFixed(2),
    volatility: stdDev.toFixed(2) || "N/A",
    maxDrawdown,
    totalGrowth
  };
}

// Function to calculate diversification index
function calculateDiversification(allocation) {
  const totalValue = allocation.reduce((sum, item) => sum + item.value, 0);
  if (totalValue === 0) return "0.00";

  const hhi = allocation.reduce((sum, item) => {
    const marketShare = item.value / totalValue;
    return sum + Math.pow(marketShare, 2);
  }, 0);

  const diversificationIndex = (1 - hhi) * 100;
  return diversificationIndex.toFixed(2);
}

// Function to calculate today's growth
function calculateTodayGrowth(portfolioEvolution) {
  if (portfolioEvolution.length < 2) return "0.00";

  const startOfDayValue = portfolioEvolution[portfolioEvolution.length - 2]?.currentMarketValue || 0;
  const endOfDayValue = portfolioEvolution[portfolioEvolution.length - 1]?.currentMarketValue || 0;

  if (startOfDayValue === 0) return "0.00";

  const growth = ((endOfDayValue - startOfDayValue) / startOfDayValue) * 100;
  return growth.toFixed(2);
}

// Function to calculate today's metrics
function calculateTodayMetrics(items) {
  let totalPreviousMarketValue = 0;
  let totalCurrentMarketValue = 0;

  items.forEach(item => {
    const quantity = item.quantity || 0;
    const currentPrice = item.currentPrice || 0;
    const previousClosePrice = item.previousClosePrice || 0;

    // Calculate previous and current market values for each item
    const previousMarketValue = previousClosePrice * quantity;
    const currentMarketValue = currentPrice * quantity;

    totalPreviousMarketValue += previousMarketValue;
    totalCurrentMarketValue += currentMarketValue;
  });

  const changeInMarketValue = totalCurrentMarketValue - totalPreviousMarketValue;
  const percentChangeToday = totalPreviousMarketValue !== 0 ? (changeInMarketValue / totalPreviousMarketValue) * 100 : 0;

  return {
    totalAmountToday: totalCurrentMarketValue.toFixed(2),
    changeInMarketValueToday: changeInMarketValue.toFixed(2),
    percentChangeToday: percentChangeToday.toFixed(2)
  };
}

// Function to map industry heatmap data
function mapIndustryHeatmapData(json) {
  if (!json || json.length === 0) {
    console.warn('No data available.');
    return [];
  }

  const items = json[0]?.items || [];

  // Group data by industry and sum the market value for each industry
  const industryMap = items.reduce((acc, item) => {
    const industry = item.industry || 'Unknown'; // Default to 'Unknown' if no industry
    const marketValue = typeof item.currentMarketValue === 'number' ? item.currentMarketValue : 0;

    // Add the market value to the corresponding industry
    if (acc[industry]) {
      acc[industry] += marketValue;
    } else {
      acc[industry] = marketValue;
    }

    return acc;
  }, {});

  // Convert the industry map into an array of objects
  const industryHeatmapData = Object.keys(industryMap).map(industry => ({
    x: industry,   // Industry name (X-axis)
    y: industryMap[industry]  // Total market value for the industry (Y-axis)
  }));

  return industryHeatmapData;
}

// Export other helper functions if needed
export {
  calculateMetrics,
  calculateDiversification,
  calculateTodayGrowth,
  calculateTodayMetrics,
  mapIndustryHeatmapData,
  calculateMaxDrawdown
};
