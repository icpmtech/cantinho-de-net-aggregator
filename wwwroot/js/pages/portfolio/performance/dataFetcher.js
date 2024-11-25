// Cache object to store API responses
const cache = {};

// Function to fetch historical prices with optional startDate and endDate
export async function fetchHistoricalPrices(symbol, startDate = null, endDate = null) {
  // Generate a unique cache key based on function parameters
  const cacheKey = `${symbol}_${startDate || 'null'}_${endDate || 'null'}`;
  const now = Date.now();
  const oneHourInMs = 3600000; // 1 hour in milliseconds

  // Check if data is in cache and not expired (valid for 1 hour)
  if (cache[cacheKey] && (now - cache[cacheKey].timestamp < oneHourInMs)) {
    console.log(`Using cached data for ${symbol}`);
    return cache[cacheKey].data;
  }

  try {
    // Build the query parameters
    const params = new URLSearchParams();
    if (startDate) {
      params.append('startDate', startDate);
    }
    if (endDate) {
      params.append('endDate', endDate);
    }

    // Construct the full URL
    const url = `/api/YahooFinance/chart-symbol/${encodeURIComponent(symbol)}${params.toString() ? `?${params.toString()}` : ''}`;

    // Fetch the data
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Server error: ${response.statusText}`);
    }
    const data = await response.json();

    // Process the data
    const historicalPrices = data.map(entry => ({
      date: new Date(entry.timestamp), // Ensure date is correctly parsed
      open: entry.open,
      high: entry.high,
      low: entry.low,
      close: entry.close,
      volume: entry.volume,
    }));

    // Store the data in cache with the current timestamp
    cache[cacheKey] = {
      data: historicalPrices,
      timestamp: now,
    };

    return historicalPrices;
  } catch (error) {
    console.error(`Error fetching historical prices for ${symbol}:`, error.message);
    return [];
  }
}

