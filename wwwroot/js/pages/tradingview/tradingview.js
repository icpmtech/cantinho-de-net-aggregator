 const apiKey = '60c4b7c12emshfe21a6a1ec58d8bp1a396cjsn6bc88b0f02f5';
        const apiHost = 'seeking-alpha.p.rapidapi.com';

        let chartInstance;

        async function fetchData(symbol, startDate, endDate) {
            const url = `https://${apiHost}/symbols/get-historical-prices?symbol=${symbol}&show_by=week&start=${startDate}&end=${endDate}&sort=as_of_date`;
            const options = {
                method: 'GET',
                headers: {
                    'x-rapidapi-key': apiKey,
                    'x-rapidapi-host': apiHost,
                },
            };

            try {
                const response = await fetch(url, options);
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                const data = await response.json();

                return data.data.map((item) => ({
                    date: new Date(item.attributes.as_of_date).getTime(),
                    open: item.attributes.open,
                    close: item.attributes.close,
                    high: item.attributes.high,
                    low: item.attributes.low,
                    volume: item.attributes.volume,
                }));
            } catch (error) {
                console.error('Error fetching data:', error);
                alert('Failed to fetch data. Please try again.');
                return [];
            }
        }

        function createChart(chartData) {
            const root = am5.Root.new("chartdiv");
            root.setThemes([am5themes_Animated.new(root)]);

            const chart = root.container.children.push(
                am5xy.XYChart.new(root, {
                    panX: true,
                    wheelX: "panX",
                    wheelY: "zoomX",
                    layout: root.verticalLayout,
                })
            );

            const dateAxis = chart.xAxes.push(
                am5xy.DateAxis.new(root, {
                    groupData: true,
                    baseInterval: { timeUnit: "day", count: 1 },
                    renderer: am5xy.AxisRendererX.new(root, {}),
                })
            );

            const valueAxis = chart.yAxes.push(
                am5xy.ValueAxis.new(root, {
                    renderer: am5xy.AxisRendererY.new(root, {}),
                    height: am5.percent(70),
                })
            );

            const valueSeries = chart.series.push(
                am5xy.CandlestickSeries.new(root, {
                    xAxis: dateAxis,
                    yAxis: valueAxis,
                    valueYField: "close",
                    openValueYField: "open",
                    lowValueYField: "low",
                    highValueYField: "high",
                    valueXField: "date",
                    tooltip: am5.Tooltip.new(root, {
                        labelText: "Open: {open}\nHigh: {high}\nLow: {low}\nClose: {close}",
                    }),
                })
            );

            chart.set("legend", am5.Legend.new(root, {}));

            return { root, chart, valueSeries };
        }

        function updateNotes(patterns) {
            const notes = {
                highlightBullish: "Bullish Engulfing: A pattern where a smaller bearish candle is followed by a larger bullish candle.",
                highlightBearish: "Bearish Engulfing: A pattern where a smaller bullish candle is followed by a larger bearish candle.",
                highlightDoji: "Doji: A candlestick pattern where the open and close prices are nearly equal, indicating indecision.",
            };

            const selectedNotes = patterns.map((pattern) => notes[pattern]).filter(Boolean);
            document.getElementById("pattern-notes").querySelector("span").textContent = selectedNotes.join(" | ") || "No pattern selected.";
        }

        function applyPatterns(patterns, chart, valueSeries) {
            const patternColors = {
                highlightBullish: am5.color(0x00ff00),
                highlightBearish: am5.color(0xff0000),
                highlightDoji: am5.color(0x0000ff),
            };

            patterns.forEach((pattern) => {
                const highlightSeries = chart.series.push(
                    am5xy.LineSeries.new(chart.root, {
                        xAxis: chart.xAxes.getIndex(0),
                        yAxis: chart.yAxes.getIndex(0),
                        valueYField: "close",
                        valueXField: "date",
                        strokeWidth: 2,
                        stroke: patternColors[pattern],
                        name: pattern,
                    })
                );

                const highlights = valueSeries.dataItems
                    .map((item, idx, arr) => {
                        const curr = item.dataContext;
                        const prev = arr[idx - 1]?.dataContext || {};

                        if (pattern === "highlightBullish") {
                            return prev.close < prev.open && curr.close > curr.open && curr.open < prev.close && curr.close > prev.open ? curr : null;
                        } else if (pattern === "highlightBearish") {
                            return prev.close > prev.open && curr.close < curr.open && curr.open > prev.close && curr.close < prev.open ? curr : null;
                        } else if (pattern === "highlightDoji") {
                            return Math.abs(curr.close - curr.open) < 0.1 ? curr : null;
                        }
                        return null;
                    })
                    .filter(Boolean);

                highlightSeries.data.setAll(highlights);
            });
        }

        async function initializeChart() {
            const symbol = document.getElementById("symbol").value;
            const startDate = document.getElementById("start-date").value;
            const endDate = document.getElementById("end-date").value;

            const chartData = await fetchData(symbol, startDate, endDate);

            if (!chartData.length) return;

            if (chartInstance) {
                chartInstance.root.dispose();
            }

            chartInstance = createChart(chartData);
            chartInstance.valueSeries.data.setAll(chartData);
        }

        document.getElementById("fetch-data").addEventListener("click", initializeChart);

        document.getElementById("apply-action").addEventListener("click", function () {
            if (!chartInstance) {
                alert("Please fetch data first!");
                return;
            }

            const { chart, valueSeries } = chartInstance;
            const patterns = Array.from(document.getElementById("menu-select").selectedOptions).map((opt) => opt.value);

            updateNotes(patterns);
            applyPatterns(patterns, chart, valueSeries);
        });

        initializeChart();
