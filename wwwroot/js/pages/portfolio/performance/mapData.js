import { fetchHistoricalPrices } from './dataFetcher.js';
import { calculateTodayMetrics, calculateMetrics, calculateTodayGrowth, calculateDiversification, mapIndustryHeatmapData } from './dataProcessor.js';

// Function to map data

export async function mapData(json) {
    if (!json || json.length === 0) {
        console.warn('No data available.');
        return {
            monthlyReturns: [],
            allocation: [],
            dayReturn: "0.00",
            bestWeek: 0,
            bestMonth: 0,
            bestStockName: "N/A",
            eventSentiments: { positive: 0, negative: 0, neutral: 0 },
            portfolioEvolution: [],
            metrics: { sharpeRatio: "N/A", volatility: "N/A", maxDrawdown: "0%", totalGrowth: "0%" },
            todayGrowth: "0",
            industryHeatmapData: []
        };
    }

    const items = json[0]?.items || [];
    let cumulativeInvestment = 0;
    let cumulativeMarketValue = 0;

    // Fetch historical prices for each item
    for (const item of items) {
        item.historicalPricesData = await fetchHistoricalPrices(item.symbol);
    }

    const portfolioEvolution = items
        .map(item => {
            let formattedDate;
            if (item.purchaseDate) {
                const dateObj = new Date(item.purchaseDate);
                if (!isNaN(dateObj)) {
                    formattedDate = dateObj.toISOString().split('T')[0];
                } else {
                    formattedDate = null;
                }
            } else {
                formattedDate = null;
            }

            const investment = typeof item.totalInvestment === 'number' ? item.totalInvestment : 0;
            const currentValue = typeof item.currentMarketValue === 'number' ? item.currentMarketValue : 0;

            cumulativeInvestment += investment;
            cumulativeMarketValue += currentValue;

            return {
                date: formattedDate,
                investment: cumulativeInvestment,
                currentMarketValue: cumulativeMarketValue,
                operationType: item.operationType || "Unknown"
            };
        })
        .filter(event => event.date !== null)
        .sort((a, b) => new Date(a.date) - new Date(b.date));

    // Group items by symbol and sum their market values
    const allocationMap = items.reduce((acc, item) => {
        const symbol = item.symbol || 'Unknown';
        const currentValue = typeof item.currentMarketValue === 'number' ? item.currentMarketValue : 0;

        if (acc[symbol]) {
            acc[symbol] += currentValue; // Add to existing symbol
        } else {
            acc[symbol] = currentValue; // Start new entry for this symbol
        }

        return acc;
    }, {});

    // Convert the allocation map into an array of objects with label and value
    const allocation = Object.keys(allocationMap).map(symbol => ({
        label: symbol,
        value: allocationMap[symbol]
    }));

    const monthlyReturnsMap = {};

    items.forEach(item => {
        if (item.stockEvents && Array.isArray(item.stockEvents)) {
            item.stockEvents.forEach(event => {
                if (event.date && event.priceChange !== undefined) {
                    const eventDate = new Date(event.date);
                    if (!isNaN(eventDate)) {
                        const month = eventDate.toLocaleString('default', { month: 'short' });
                        monthlyReturnsMap[month] = (monthlyReturnsMap[month] || 0) + (event.priceChange || 0);
                    }
                }
            });
        }
    });

    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const monthlyReturns = months.map(month => monthlyReturnsMap[month] || 0);

    const sentimentCounts = items.flatMap(item => item.stockEvents || []).reduce((counts, event) => {
        const sentiment = (event.sentiment || 'Neutral').toLowerCase();
        if (sentiment === 'positive') counts.positive += 1;
        else if (sentiment === 'negative') counts.negative += 1;
        else counts.neutral += 1;
        return counts;
    }, { positive: 0, negative: 0, neutral: 0 });

    const eventSentiments = {
        positive: sentimentCounts.positive,
        negative: sentimentCounts.negative,
        neutral: sentimentCounts.neutral
    };

    const bestStock = items.reduce((best, current) => {
        return (current.percentChange > (best?.percentChange || -Infinity)) ? current : best;
    }, null);

    const bestStockName = bestStock?.symbol || "N/A";
    const dayReturn = bestStock?.percentChange?.toFixed(2) || "0.00";
    const bestWeek = Math.max(...items.flatMap(item => (item.stockEvents || []).map(event => event.priceChange || 0)), 0);
    const bestMonth = Math.max(...monthlyReturns, 0);

    // Calculate today's metrics
    const todayMetrics = calculateTodayMetrics(items);
    const metrics = calculateMetrics({ monthlyReturns });
    const todayGrowth = calculateTodayGrowth(portfolioEvolution);
    const diversification = calculateDiversification(allocation);
    const industryHeatmapData = mapIndustryHeatmapData(json);

    return {
        portfolioEvolution,
        items,
        monthlyReturns,
        allocation,
        dayReturn,
        bestWeek,
        bestMonth,
        bestStockName,
        eventSentiments,
        metrics,
        months,
        todayGrowth,
        diversification,
        industryHeatmapData,
        todayMetrics,
    };
}
