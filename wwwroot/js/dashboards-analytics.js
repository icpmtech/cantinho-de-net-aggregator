/**
 * Dashboard Analytics
 */

'use strict';

(function () {
  let cardColor, headingColor, axisColor, shadeColor, borderColor;

  cardColor = config.colors.cardColor;
  headingColor = config.colors.headingColor;
  axisColor = config.colors.axisColor;
  borderColor = config.colors.borderColor;

  // Define a function to load the revenue data from the backend
  async function loadRevenueData() {
    try {
      const data = await fetchData('/api/Dashboards/revenue');
      updateRevenueChart(data);
    } catch (error) {
      alert('Failed to load revenue data: ' + error.message);
    }
  }

  // Function to update the revenue chart with the fetched data
  function updateRevenueChart(data) {
    const totalRevenueChartEl = document.querySelector('#totalRevenueChart');

    const totalRevenueChartOptions = {
      series: [
        {
          name: '2021',
          data: data.series2021
        },
        {
          name: '2020',
          data: data.series2020
        }
      ],
      chart: {
        height: 300,
        stacked: true,
        type: 'bar',
        toolbar: { show: false }
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '33%',
          borderRadius: 12,
          startingShape: 'rounded',
          endingShape: 'rounded'
        }
      },
      colors: ['#7367F0', '#00CFDD'],
      dataLabels: {
        enabled: false
      },
      stroke: {
        curve: 'smooth',
        width: 6,
        lineCap: 'round',
        colors: ['#FFFFFF']
      },
      legend: {
        show: true,
        horizontalAlign: 'left',
        position: 'top',
        markers: {
          height: 8,
          width: 8,
          radius: 12,
          offsetX: -3
        },
        labels: {
          colors: '#6e6b7b'
        },
        itemMargin: {
          horizontal: 10
        }
      },
      grid: {
        borderColor: '#e9ecef',
        padding: {
          top: 0,
          bottom: -8,
          left: 20,
          right: 20
        }
      },
      xaxis: {
        categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul'],
        labels: {
          style: {
            fontSize: '13px',
            colors: '#6e6b7b'
          }
        },
        axisTicks: {
          show: false
        },
        axisBorder: {
          show: false
        }
      },
      yaxis: {
        labels: {
          style: {
            fontSize: '13px',
            colors: '#6e6b7b'
          }
        }
      },
      responsive: [
        {
          breakpoint: 1700,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '32%'
              }
            }
          }
        },
        {
          breakpoint: 1580,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '35%'
              }
            }
          }
        },
        {
          breakpoint: 1440,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '42%'
              }
            }
          }
        },
        {
          breakpoint: 1300,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '48%'
              }
            }
          }
        },
        {
          breakpoint: 1200,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '40%'
              }
            }
          }
        },
        {
          breakpoint: 1040,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 11,
                columnWidth: '48%'
              }
            }
          }
        },
        {
          breakpoint: 991,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '30%'
              }
            }
          }
        },
        {
          breakpoint: 840,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '35%'
              }
            }
          }
        },
        {
          breakpoint: 768,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '28%'
              }
            }
          }
        },
        {
          breakpoint: 640,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '32%'
              }
            }
          }
        },
        {
          breakpoint: 576,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '37%'
              }
            }
          }
        },
        {
          breakpoint: 480,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '45%'
              }
            }
          }
        },
        {
          breakpoint: 420,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '52%'
              }
            }
          }
        },
        {
          breakpoint: 380,
          options: {
            plotOptions: {
              bar: {
                borderRadius: 10,
                columnWidth: '60%'
              }
            }
          }
        }
      ],
      states: {
        hover: {
          filter: {
            type: 'none'
          }
        },
        active: {
          filter: {
            type: 'none'
          }
        }
      }
    };

    if (totalRevenueChartEl !== null) {
      const totalRevenueChart = new ApexCharts(totalRevenueChartEl, totalRevenueChartOptions);
      totalRevenueChart.render();
    }
  }

  // Load the revenue data when the document is ready
  document.addEventListener('DOMContentLoaded', function () {
    loadRevenueData();
    loadGrowthData();
    loadProfitData();
    loadSymbolsStatisticsData();
    loadIncomeData();
    loadExpensesData();
    loadDividendsData();
    loadProfitDataExpenses();
  });

  // Define a function to load the growth data from the backend
  async function loadGrowthData() {
    try {
      const data = await fetchData('/api/Dashboards/growth');
      updateGrowthChart(data);
    } catch (error) {
      alert('Failed to load growth data: ' + error.message);
    }
  }

  // Function to update the growth chart with the fetched data
  function updateGrowthChart(data) {
    const growthChartEl = document.querySelector('#growthChart');

    const growthChartOptions = {
      series: [data.growth],
      labels: ['Growth'],
      chart: {
        height: 240,
        type: 'radialBar'
      },
      plotOptions: {
        radialBar: {
          size: 150,
          offsetY: 10,
          startAngle: -150,
          endAngle: 150,
          hollow: {
            size: '55%'
          },
          track: {
            background: '#e7e7e7',
            strokeWidth: '100%'
          },
          dataLabels: {
            name: {
              offsetY: 15,
              color: '#5e5873',
              fontSize: '15px',
              fontWeight: '500',
              fontFamily: 'Public Sans'
            },
            value: {
              offsetY: -25,
              color: '#5e5873',
              fontSize: '22px',
              fontWeight: '500',
              fontFamily: 'Public Sans'
            }
          }
        }
      },
      colors: ['#7367F0'],
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'dark',
          shadeIntensity: 0.5,
          gradientToColors: ['#7367F0'],
          inverseColors: true,
          opacityFrom: 1,
          opacityTo: 0.6,
          stops: [30, 70, 100]
        }
      },
      stroke: {
        dashArray: 5
      },
      grid: {
        padding: {
          top: -35,
          bottom: -10
        }
      },
      states: {
        hover: {
          filter: {
            type: 'none'
          }
        },
        active: {
          filter: {
            type: 'none'
          }
        }
      }
    };

    if (growthChartEl !== null) {
      const growthChart = new ApexCharts(growthChartEl, growthChartOptions);
      growthChart.render();
    }
  }

  // Profit Report Line Chart
  // Define a function to load the profit data from the backend
  async function loadProfitData() {
    try {
      const data = await fetchData('/api/Dashboards/profit');
      updateProfitChart(data);
    } catch (error) {
      alert('Failed to load profit data: ' + error.message);
    }
  }

  // Function to update the profit chart with the fetched data
  function updateProfitChart(data) {
    const profileReportChartEl = document.querySelector('#profileReportChart');

    const profileReportChartConfig = {
      chart: {
        height: 80,
        // width: 175,
        type: 'line',
        toolbar: {
          show: false
        },
        dropShadow: {
          enabled: true,
          top: 10,
          left: 5,
          blur: 3,
          color: '#FF9F43', // replace with config.colors.warning
          opacity: 0.15
        },
        sparkline: {
          enabled: true
        }
      },
      grid: {
        show: false,
        padding: {
          right: 8
        }
      },
      colors: ['#FF9F43'], // replace with config.colors.warning
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 5,
        curve: 'smooth'
      },
      series: [
        {
          data: data.data
        }
      ],
      xaxis: {
        show: false,
        lines: {
          show: false
        },
        labels: {
          show: false
        },
        axisBorder: {
          show: false
        }
      },
      yaxis: {
        show: false
      }
    };

    if (profileReportChartEl !== null) {
      const profileReportChart = new ApexCharts(profileReportChartEl, profileReportChartConfig);
      profileReportChart.render();
    }
  }

  // Profit Report Line Chart
  // Define a function to load the profit data from the backend
  async function loadProfitDataExpenses() {
    try {
      const data = await fetchData('/api/Dashboards/profit');
      updateProfitChartExpenses(data);
    } catch (error) {
      alert('Failed to load profit data: ' + error.message);
    }
  }

  // Function to update the profit chart with the fetched data
  function updateProfitChartExpenses(data) {
    const profitChartEl = document.querySelector('#profitChart');

    const profitChartConfig = {
      series: [
        {
          data: data.series
        }
      ],
      chart: {
        height: 215,
        parentHeightOffset: 0,
        parentWidthOffset: 0,
        toolbar: {
          show: false
        },
        type: 'area'
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 2,
        curve: 'smooth'
      },
      legend: {
        show: false
      },
      markers: {
        size: 6,
        colors: 'transparent',
        strokeColors: 'transparent',
        strokeWidth: 4,
        discrete: [
          {
            fillColor: '#ffffff',
            seriesIndex: 0,
            dataPointIndex: 7,
            strokeColor: '#696CFF', // replace with config.colors.primary
            strokeWidth: 2,
            size: 6,
            radius: 8
          }
        ],
        hover: {
          size: 7
        }
      },
      colors: ['#696CFF'], // replace with config.colors.primary
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'dark', // replace with shadeColor
          shadeIntensity: 0.6,
          opacityFrom: 0.5,
          opacityTo: 0.25,
          stops: [0, 95, 100]
        }
      },
      grid: {
        borderColor: '#EBEBEB', // replace with borderColor
        strokeDashArray: 3,
        padding: {
          top: -20,
          bottom: -8,
          left: -10,
          right: 8
        }
      },
      xaxis: {
        categories: data.categories,
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        },
        labels: {
          show: true,
          style: {
            fontSize: '13px',
            colors: '#A1A1A1' // replace with axisColor
          }
        }
      },
      yaxis: {
        labels: {
          show: false
        },
        min: 10,
        max: 50,
        tickAmount: 4
      }
    };

    if (typeof profitChartEl !== undefined && profitChartEl !== null) {
      const profitChart = new ApexCharts(profitChartEl, profitChartConfig);
      profitChart.render();
    }
  }

  // Symbols Statistics Chart
  // --------------------------------------------------------------------
  // Define a function to load the order statistics data from the backend
  async function loadSymbolsStatisticsData() {
    try {
      const data = await fetchData('/api/Dashboards/symbols-statistics');
      updatesymbolStatisticsChart(data);
    } catch (error) {
      alert('Failed to load order statistics data: ' + error.message);
    }
  }

  // Function to update the order statistics chart with the fetched data
  function updatesymbolStatisticsChart(data) {
    const chartOrderStatistics = document.querySelector('#symbolStatisticsChart');

    const orderChartConfig = {
      chart: {
        height: 165,
        width: 130,
        type: 'donut'
      },
      labels: data.labels,
      series: data.series,
      colors: ['#696CFF', '#AFB8E3', '#FFC4B1', '#B8E0D2'], // replace with actual colors
      stroke: {
        width: 5,
        colors: ['#fff'] // replace with actual card color
      },
      dataLabels: {
        enabled: false,
        formatter: function (val, opt) {
          return parseInt(val) + '%';
        }
      },
      legend: {
        show: false
      },
      grid: {
        padding: {
          top: 0,
          bottom: 0,
          right: 15
        }
      },
      states: {
        hover: {
          filter: { type: 'none' }
        },
        active: {
          filter: { type: 'none' }
        }
      },
      plotOptions: {
        pie: {
          donut: {
            size: '75%',
            labels: {
              show: true,
              value: {
                fontSize: '1.5rem',
                fontFamily: 'Public Sans',
                color: '#5E5873', // replace with heading color
                offsetY: -15,
                formatter: function (val) {
                  return parseInt(val) + '%';
                }
              },
              name: {
                offsetY: 20,
                fontFamily: 'Public Sans'
              },
              total: {
                show: true,
                fontSize: '0.8125rem',
                color: '#b9c3cd', // replace with axis color
                label: 'Weekly',
                formatter: function (w) {
                  return '38%';
                }
              }
            }
          }
        }
      }
    };

    if (typeof chartOrderStatistics !== undefined && chartOrderStatistics !== null) {
      const statisticsChart = new ApexCharts(chartOrderStatistics, orderChartConfig);
      statisticsChart.render();
    }
  }


  // Define a function to load the dividends data from the backend
  async function loadDividendsData() {
    try {
      const data = await fetchData('/api/Dashboards/dividends');
      updateDividendsChart(data);
    } catch (error) {
      alert('Failed to load dividends data: ' + error.message);
    }
  }

  // Function to update the dividends chart with the fetched data
  function updateDividendsChart(data) {
    const dividendsChartEl = document.querySelector('#dividendsChart');

    const dividendsChartConfig = {
      series: [
        {
          data: data.series
        }
      ],
      chart: {
        height: 215,
        parentHeightOffset: 0,
        parentWidthOffset: 0,
        toolbar: {
          show: false
        },
        type: 'area'
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 2,
        curve: 'smooth'
      },
      legend: {
        show: false
      },
      markers: {
        size: 6,
        colors: 'transparent',
        strokeColors: 'transparent',
        strokeWidth: 4,
        discrete: [
          {
            fillColor: '#ffffff',
            seriesIndex: 0,
            dataPointIndex: 7,
            strokeColor: '#696CFF', // replace with config.colors.primary
            strokeWidth: 2,
            size: 6,
            radius: 8
          }
        ],
        hover: {
          size: 7
        }
      },
      colors: ['#696CFF'], // replace with config.colors.primary
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'dark', // replace with shadeColor
          shadeIntensity: 0.6,
          opacityFrom: 0.5,
          opacityTo: 0.25,
          stops: [0, 95, 100]
        }
      },
      grid: {
        borderColor: '#EBEBEB', // replace with borderColor
        strokeDashArray: 3,
        padding: {
          top: -20,
          bottom: -8,
          left: -10,
          right: 8
        }
      },
      xaxis: {
        categories: data.categories,
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        },
        labels: {
          show: true,
          style: {
            fontSize: '13px',
            colors: '#A1A1A1' // replace with axisColor
          }
        }
      },
      yaxis: {
        labels: {
          show: false
        },
        min: 10,
        max: 50,
        tickAmount: 4
      }
    };

    if (typeof dividendsChartEl !== undefined && dividendsChartEl !== null) {
      const dividendsChart = new ApexCharts(dividendsChartEl, dividendsChartConfig);
      dividendsChart.render();
    }
  }

  // Define a function to load the income data from the backend
  async function loadIncomeData() {
    try {
      const data = await fetchData('/api/Dashboards/income');
      updateIncomeChart(data);
    } catch (error) {
      alert('Failed to load income data: ' + error.message);
    }
  }

  // Function to update the income chart with the fetched data
  function updateIncomeChart(data) {
    const incomeChartEl = document.querySelector('#incomeChart');

    const incomeChartConfig = {
      series: [
        {
          data: data.series
        }
      ],
      chart: {
        height: 215,
        parentHeightOffset: 0,
        parentWidthOffset: 0,
        toolbar: {
          show: false
        },
        type: 'area'
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 2,
        curve: 'smooth'
      },
      legend: {
        show: false
      },
      markers: {
        size: 6,
        colors: 'transparent',
        strokeColors: 'transparent',
        strokeWidth: 4,
        discrete: [
          {
            fillColor: '#ffffff',
            seriesIndex: 0,
            dataPointIndex: 7,
            strokeColor: '#696CFF', // replace with config.colors.primary
            strokeWidth: 2,
            size: 6,
            radius: 8
          }
        ],
        hover: {
          size: 7
        }
      },
      colors: ['#696CFF'], // replace with config.colors.primary
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'dark', // replace with shadeColor
          shadeIntensity: 0.6,
          opacityFrom: 0.5,
          opacityTo: 0.25,
          stops: [0, 95, 100]
        }
      },
      grid: {
        borderColor: '#EBEBEB', // replace with borderColor
        strokeDashArray: 3,
        padding: {
          top: -20,
          bottom: -8,
          left: -10,
          right: 8
        }
      },
      xaxis: {
        categories: data.categories,
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        },
        labels: {
          show: true,
          style: {
            fontSize: '13px',
            colors: '#A1A1A1' // replace with axisColor
          }
        }
      },
      yaxis: {
        labels: {
          show: false
        },
        min: 10,
        max: 50,
        tickAmount: 4
      }
    };

    if (typeof incomeChartEl !== undefined && incomeChartEl !== null) {
      const incomeChart = new ApexCharts(incomeChartEl, incomeChartConfig);
      incomeChart.render();
    }
  }


  // Define a function to load the expenses data from the backend
  async function loadExpensesData() {
    try {
      const data = await fetchData('/api/Dashboards/expenses');
      updateExpensesChart(data);
    } catch (error) {
      alert('Failed to load expenses data: ' + error.message);
    }
  }

  // Function to update the expenses chart with the fetched data
  function updateExpensesChart(data) {
    const weeklyExpensesEl = document.querySelector('#expensesOfWeek');
    const weeklyExpensesConfig = {
      series: [data.series],
      chart: {
        width: 60,
        height: 60,
        type: 'radialBar'
      },
      plotOptions: {
        radialBar: {
          startAngle: 0,
          endAngle: 360,
          strokeWidth: '8',
          hollow: {
            margin: 2,
            size: '45%'
          },
          track: {
            strokeWidth: '50%',
            background: '#EBEBEB' // replace with borderColor
          },
          dataLabels: {
            show: true,
            name: {
              show: false
            },
            value: {
              formatter: function (val) {
                return '$' + parseInt(val);
              },
              offsetY: 5,
              color: '#697a8d',
              fontSize: '13px',
              show: true
            }
          }
        }
      },
      fill: {
        type: 'solid',
        colors: ['#696CFF'] // replace with config.colors.primary
      },
      stroke: {
        lineCap: 'round'
      },
      grid: {
        padding: {
          top: -10,
          bottom: -15,
          left: -10,
          right: -10
        }
      },
      states: {
        hover: {
          filter: {
            type: 'none'
          }
        },
        active: {
          filter: {
            type: 'none'
          }
        }
      }
    };
    if (typeof weeklyExpensesEl !== undefined && weeklyExpensesEl !== null) {
      const weeklyExpenses = new ApexCharts(weeklyExpensesEl, weeklyExpensesConfig);
      weeklyExpenses.render();
    }
  }
})();
