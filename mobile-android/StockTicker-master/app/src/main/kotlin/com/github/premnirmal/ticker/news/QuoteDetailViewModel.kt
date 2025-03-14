package com.github.premnirmal.ticker.news

import android.app.Application
import androidx.annotation.StringRes
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.asLiveData
import androidx.lifecycle.viewModelScope
import com.github.premnirmal.ticker.format
import com.github.premnirmal.ticker.formatBigNumbers
import com.github.premnirmal.ticker.formatDate
import com.github.premnirmal.ticker.model.FetchResult
import com.github.premnirmal.ticker.model.HistoryProvider
import com.github.premnirmal.ticker.model.HistoryProvider.Range
import com.github.premnirmal.ticker.model.StocksProvider
import com.github.premnirmal.ticker.network.NewsProvider
import com.github.premnirmal.ticker.network.StocksApi
import com.github.premnirmal.ticker.network.data.DataPoint
import com.github.premnirmal.ticker.network.data.Quote
import com.github.premnirmal.ticker.network.data.QuoteSummary
import com.github.premnirmal.ticker.news.NewsFeedItem.ArticleNewsFeed
import com.github.premnirmal.ticker.widget.WidgetData
import com.github.premnirmal.ticker.widget.WidgetDataProvider
import com.github.premnirmal.tickerwidget.R
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.shareIn
import kotlinx.coroutines.flow.transform
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class QuoteDetailViewModel @Inject constructor(
  application: Application,
  private val stocksProvider: StocksProvider,
  private val stocksApi: StocksApi,
  private val newsProvider: NewsProvider,
  private val historyProvider: HistoryProvider,
  private val widgetDataProvider: WidgetDataProvider
) : AndroidViewModel(application) {

  private val _quote = MutableSharedFlow<FetchResult<QuoteWithSummary>>()
  val quote: LiveData<FetchResult<QuoteWithSummary>>
    get() = _quote.asLiveData()
  private val _data = MutableLiveData<List<DataPoint>>()
  val data: LiveData<List<DataPoint>>
    get() = _data
  private val _dataFetchError = MutableLiveData<Throwable>()
  val dataFetchError: LiveData<Throwable>
    get() = _dataFetchError
  private val _newsData = MutableLiveData<List<ArticleNewsFeed>>()
  val newsData: LiveData<List<ArticleNewsFeed>>
    get() = _newsData
  private val _newsError = MutableLiveData<Throwable>()
  val newsError: LiveData<Throwable>
    get() = _newsError

  private var quoteSummary: QuoteSummary? = null

  val details: Flow<List<QuoteDetail>> = _quote.transform { summary ->
    if (summary.wasSuccessful) {
      val quote = summary.data.quote
      val quoteSummary = summary.data.quoteSummary
      val details = mutableListOf<QuoteDetail>()
      quote.open?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_open,
                quote.priceFormat.format(it)
            )
        )
      }
      if (quote.dayLow != null && quote.dayHigh != null) {
        details.add(
            QuoteDetail(
                R.string.quote_details_day_range,
                "${quote.dayLow!!.format()} - ${quote.dayHigh!!.format()}"
            )
        )
      }
      if (quote.fiftyTwoWeekLow != null && quote.fiftyTwoWeekHigh != null) {
        details.add(
            QuoteDetail(
                R.string.quote_details_ftw_range,
                "${quote.fiftyTwoWeekLow!!.format()} - ${quote.fiftyTwoWeekHigh!!.format()}"
            )
        )
      }
      quote.regularMarketVolume?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_volume,
                it.format()
            )
        )
      }
      quote.marketCap?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_market_cap,
                it.formatBigNumbers(application)
            )
        )
      }
      quote.trailingPE?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_pe_ratio,
                it.format()
            )
        )
      }
      quote.earningsTimestamp?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_earnings_date,
                it.formatDate(application.getString(R.string.date_format_long))
            )
        )
      }
      if (quote.annualDividendRate > 0f && quote.annualDividendYield > 0f) {
        details.add(
            QuoteDetail(
                R.string.quote_details_dividend_rate,
                quote.dividendInfo()
            )
        )
      }
      quote.dividendDate?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_dividend_date,
                it.formatDate(application.getString(R.string.date_format_long))
            )
        )
      }
      quoteSummary?.financialData?.earningsGrowth?.fmt?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_earnings_growth,
                it
            )
        )
      }
      quoteSummary?.financialData?.revenueGrowth?.fmt?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_revenue_growth,
                it
            )
        )
      }
      quoteSummary?.financialData?.profitMargins?.fmt?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_profit_margins,
                it
            )
        )
      }
      quoteSummary?.financialData?.grossMargins?.fmt?.let {
        details.add(
            QuoteDetail(
                R.string.quote_details_gross_margins,
                it
            )
        )
      }
      emit(details)
    }
  }.shareIn(viewModelScope, SharingStarted.WhileSubscribed(), 1)

  fun loadQuote(ticker: String) = viewModelScope.launch {
    _quote.emit(FetchResult.success(QuoteWithSummary(checkNotNull(stocksProvider.getStock(ticker)), quoteSummary)))
  }

  fun fetchQuote(ticker: String) {
    viewModelScope.launch {
      val fetchStock = stocksProvider.fetchStock(ticker)
      if (fetchStock.wasSuccessful) {
        val fetchDetails = stocksApi.getQuoteDetails(ticker)
        if (fetchDetails.wasSuccessful) {
          quoteSummary = fetchDetails.data
          _quote.emit(FetchResult.success(QuoteWithSummary(fetchStock.data, quoteSummary)))
        } else {
          _quote.emit(FetchResult.success(QuoteWithSummary(fetchStock.data, quoteSummary)))
        }
      } else {
        _quote.emit(FetchResult.failure(fetchStock.error))
      }
    }
  }

  fun fetchQuoteInRealTime(
    symbol: String
  ) {
    viewModelScope.launch {
      do {
        var isMarketOpen = false
        val result = stocksProvider.fetchStock(symbol, allowCache = false)
        if (result.wasSuccessful) {
          isMarketOpen = result.data.isMarketOpen
          _quote.emit(FetchResult.success(QuoteWithSummary(result.data, quoteSummary)))
        }
        delay(StocksProvider.DEFAULT_INTERVAL_MS)
      } while (isActive && result.wasSuccessful && isMarketOpen)
    }
  }

  /**
   * true = show remove
   * false = show add
   */
  fun showAddOrRemove(ticker: String): Boolean {
    return if (widgetDataProvider.widgetCount > 1) {
      false
    } else {
      isInPortfolio(ticker)
    }
  }

  fun isInPortfolio(ticker: String): Boolean {
    return stocksProvider.hasTicker(ticker)
  }

  fun removeStock(ticker: String) {
    val widgetData = widgetDataProvider.widgetDataWithStock(ticker)
    widgetData.forEach { it.removeStock(ticker) }
    viewModelScope.launch {
      stocksProvider.removeStock(ticker)
    }
  }

  fun fetchChartData(symbol: String, range: Range) {
    viewModelScope.launch {
      val result = historyProvider.fetchDataByRange(symbol, range)
      if (result.wasSuccessful) {
        _data.value = result.data
      } else {
        _dataFetchError.postValue(result.error)
      }
    }
  }

  fun fetchNews(quote: Quote) {
    viewModelScope.launch {
      if (_newsData.value != null) {
        _newsData.postValue(_newsData.value)
        return@launch
      }
      val query = quote.newsQuery()
      val result = newsProvider.fetchNewsForQuery(query)
      when {
        result.wasSuccessful -> {
          _newsData.value = result.data.map { ArticleNewsFeed(it) }
        }
        else -> {
          _newsError.value = result.error
        }
      }
    }
  }

  fun getWidgetDatas(): List<WidgetData> {
    val widgetIds = widgetDataProvider.getAppWidgetIds()
    return widgetIds.map { widgetDataProvider.dataForWidgetId(it) }
        .sortedBy { it.widgetName() }
  }

  fun hasWidget(): Boolean {
    return widgetDataProvider.hasWidget()
  }

  fun addTickerToWidget(
    ticker: String,
    widgetId: Int
  ): Boolean {
    val widgetData = widgetDataProvider.dataForWidgetId(widgetId)
    return if (!widgetData.hasTicker(ticker)) {
      widgetData.addTicker(ticker)
      widgetDataProvider.broadcastUpdateWidget(widgetId)
      true
    } else {
      false
    }
  }

  data class QuoteDetail(
    @StringRes
    val title: Int,

    val data: String
  )

  data class QuoteWithSummary(
    val quote: Quote,
    val quoteSummary: QuoteSummary?
  )
}