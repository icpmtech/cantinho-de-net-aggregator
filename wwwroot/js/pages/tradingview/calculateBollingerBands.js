// Function to calculate Bollinger Bands
function calculateBollingerBands(closePrices, period, stdDevMultiplier, dates) {
    const bollingerData = [];
    for (let i = 0; i < closePrices.length; i++) {
        if (i < period - 1) {
            bollingerData.push({ date: dates[i], upper: null, lower: null });
            continue;
        }
        const slice = closePrices.slice(i - period + 1, i + 1);
        const sum = slice.reduce((a, b) => a + b, 0);
        const mean = sum / period;
        const variance = slice.reduce((a, b) => a + Math.pow(b - mean, 2), 0) / period;
        const stdDev = Math.sqrt(variance);
        bollingerData.push({
            date: dates[i],
            upper: mean + stdDevMultiplier * stdDev,
            lower: mean - stdDevMultiplier * stdDev
        });
    }
    return bollingerData;
}
