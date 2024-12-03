// Helper function to calculate Exponential Moving Average (EMA)
function calculateEMA(data, period) {
    const ema = [];
    const k = 2 / (period + 1);
    let emaPrev = null;

    for (let i = 0; i < data.length; i++) {
        if (data[i] === null || data[i] === undefined || isNaN(data[i])) {
            ema[i] = null;
            continue;
        }

        if (i < period - 1) {
            ema[i] = null;
            continue;
        }

        if (i === period - 1) {
            const sum = data.slice(0, period).reduce((a, b) => a + b, 0);
            emaPrev = sum / period;
            ema[i] = emaPrev;
        } else {
            emaPrev = data[i] * k + emaPrev * (1 - k);
            ema[i] = emaPrev;
        }
    }
    return ema;
}
