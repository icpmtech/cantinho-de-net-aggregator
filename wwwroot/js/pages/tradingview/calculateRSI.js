// Function to calculate RSI
function calculateRSI(closePrices, period, dates) {
    const rsiData = [];
    let gains = 0;
    let losses = 0;

    // Initialize first RSI value
    for (let i = 1; i <= period; i++) {
        const change = closePrices[i] - closePrices[i - 1];
        if (change > 0) gains += change;
        else losses -= change;
    }
    let averageGain = gains / period;
    let averageLoss = losses / period;
    let rs = averageGain / averageLoss;
    let rsi = averageLoss === 0 ? 100 : 100 - (100 / (1 + rs));
    rsiData[period] = { date: dates[period], value: rsi };

    // Calculate RSI for the rest of the data
    for (let i = period + 1; i < closePrices.length; i++) {
        const change = closePrices[i] - closePrices[i - 1];
        if (change > 0) {
            averageGain = (averageGain * (period - 1) + change) / period;
            averageLoss = (averageLoss * (period - 1)) / period;
        } else {
            averageGain = (averageGain * (period - 1)) / period;
            averageLoss = (averageLoss * (period - 1) - change) / period;
        }
        rs = averageLoss === 0 ? 100 : averageGain / averageLoss;
        rsi = averageLoss === 0 ? 100 : 100 - (100 / (1 + rs));
        rsiData[i] = { date: dates[i], value: rsi };
    }

    // Fill in the initial period with nulls
    for (let i = 0; i < period; i++) {
        rsiData[i] = { date: dates[i], value: null };
    }

    // Remove undefined entries and return
    return rsiData.filter(item => item !== undefined);
}
