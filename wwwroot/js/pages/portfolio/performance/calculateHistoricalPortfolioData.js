export function calculateHistoricalPortfolioData(items, startDate, endDate) {
  // Ensure startDate is before endDate
  if (new Date(startDate) > new Date(endDate)) {
    const temp = startDate;
    startDate = endDate;
    endDate = temp;
  }

  // Initialize dateMap with dates and zero values using UTC
  const dateMap = {};
  const start = new Date(startDate);
  const end = new Date(endDate);

  for (let d = new Date(start); d <= end; d.setUTCDate(d.getUTCDate() + 1)) {
    const dateString = d.getUTCFullYear() + '-' +
      String(d.getUTCMonth() + 1).padStart(2, '0') + '-' +
      String(d.getUTCDate()).padStart(2, '0');
    dateMap[dateString] = 0;
  }

  // Sum up the portfolio value for each date
  for (const item of items) {
    const quantity = item.quantity || 0;
    const historicalPrices = item.historicalPricesData || [];

    historicalPrices.forEach(priceData => {
      const date = new Date(priceData.timestamp || priceData.date);
      const dateString = date.getUTCFullYear() + '-' +
        String(date.getUTCMonth() + 1).padStart(2, '0') + '-' +
        String(date.getUTCDate()).padStart(2, '0');

      if (dateMap.hasOwnProperty(dateString) && priceData.close != null && !isNaN(priceData.close)) {
        dateMap[dateString] += priceData.close * quantity;
      }
    });
  }

  // Extract and sort dates
  const sortedDates = Object.keys(dateMap).sort(
    (a, b) => new Date(a) - new Date(b)
  );
  const dates = [];
  const values = [];

  sortedDates.forEach(date => {
    dates.push(date);
    values.push(Number(dateMap[date].toFixed(2)));
  });

  return { dates, values };
}
