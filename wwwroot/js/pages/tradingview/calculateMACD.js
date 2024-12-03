// Function to calculate MACD
function calculateMACD(closePrices, shortPeriod, longPeriod, signalPeriod, dates) {
    const emaShort = calculateEMA(closePrices, shortPeriod);
    const emaLong = calculateEMA(closePrices, longPeriod);
    const macdLine = [];
    const signalLine = [];
    const histogram = [];

    // Calculate MACD Line
    for (let i = 0; i < closePrices.length; i++) {
        if (emaShort[i] !== null && emaLong[i] !== null) {
            macdLine[i] = { date: dates[i], macd: emaShort[i] - emaLong[i] };
        } else {
            macdLine[i] = { date: dates[i], macd: null };
        }
    }

    // Calculate Signal Line (EMA of MACD Line)
    const macdValues = macdLine.map(item => item.macd);
    const emaSignal = calculateEMA(macdValues, signalPeriod);
    for (let i = 0; i < macdLine.length; i++) {
        if (emaSignal[i] !== null) {
            signalLine[i] = { date: dates[i], signal: emaSignal[i] };
        } else {
            signalLine[i] = { date: dates[i], signal: null };
        }
    }

    // Calculate Histogram
    for (let i = 0; i < macdLine.length; i++) {
        if (macdLine[i].macd !== null && signalLine[i].signal !== null) {
            histogram[i] = { date: dates[i], histogram: macdLine[i].macd - signalLine[i].signal };
        } else {
            histogram[i] = { date: dates[i], histogram: null };
        }
    }

    return { macdLine, signalLine, histogram };
}
