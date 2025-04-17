// DividendComponent.js

class DividendComponent {
  /**
   * Creates an instance of DividendComponent.
   * @param {Object} options - Configuration options for the component.
   * @param {string|HTMLElement} options.container - The container element or its selector where the component will be rendered.
   * @param {string} [options.symbol='AAPL'] - The default stock symbol to fetch dividends for.
   * @param {string} [options.apiUrl='https://apimarketsanalyticshub-aeccaahebzamare9.eastus-01.azurewebsites.net/'] - The base API URL for fetching dividends.
   */
  constructor(options) {
    // Destructure and set default options
    const {
      container,
      symbol = 'AAPL',
      apiUrl = 'https://apimarketsanalyticshub-aeccaahebzamare9.eastus-01.azurewebsites.net/dividends'
    } = options;

    // Resolve the container element
    if (typeof container === 'string') {
      this.container = document.querySelector(container);
    } else if (container instanceof HTMLElement) {
      this.container = container;
    } else {
      throw new Error('Invalid container provided. It must be a selector string or an HTMLElement.');
    }

    if (!this.container) {
      throw new Error('Container element not found.');
    }

    this.symbol = symbol;
    this.apiUrl = apiUrl;

    // Initialize the component
    this.init();
  }

  /**
   * Initializes the component by setting up the HTML structure and fetching initial data.
   */
  init() {
    // Create the component's HTML structure
    this.container.innerHTML = `
            <div class="dividend-component">
                <div class="controls">
                    <input type="text" id="symbolInput" placeholder="Enter stock symbol (e.g., AAPL)" value="${this.symbol}" />
                    <button id="fetchButton">Fetch Dividends</button>
                </div>
                <div id="loading" class="loading hidden">Loading dividend data...</div>
                <div id="error" class="error hidden">Failed to load dividend data.</div>
                <table id="dividendsTable" class="dividends-table hidden">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Ex Date</th>
                            <th>Amount</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Dividend data will be inserted here -->
                    </tbody>
                </table>
            </div>
        `;

    // Reference to elements
    this.symbolInput = this.container.querySelector('#symbolInput');
    this.fetchButton = this.container.querySelector('#fetchButton');
    this.loadingDiv = this.container.querySelector('#loading');
    this.errorDiv = this.container.querySelector('#error');
    this.dividendsTable = this.container.querySelector('#dividendsTable');
    this.dividendsTableBody = this.container.querySelector('#dividendsTable tbody');

    // Bind event listeners
    this.fetchButton.addEventListener('click', () => {
      const symbol = this.symbolInput.value.trim().toUpperCase();
      if (symbol) {
        this.updateSymbol(symbol);
      } else {
        this.showError('Please enter a valid stock symbol.');
      }
    });

    // Optionally, fetch dividends for the default symbol on initialization
    this.fetchDividends(this.symbol);
  }

  /**
   * Updates the stock symbol and re-fetches dividend data.
   * @param {string} newSymbol - The new stock symbol to fetch dividends for.
   */
  updateSymbol(newSymbol) {
    this.symbol = newSymbol;
    this.fetchDividends(this.symbol);
  }

  /**
   * Fetches dividend data for a given stock symbol.
   * @param {string} symbol - The stock symbol to fetch dividends for.
   */
  async fetchDividends(symbol) {
    const url = `${this.apiUrl}?symbol=${encodeURIComponent(symbol)}`;

    // Show loading indicator and hide previous results/errors
    this.showLoading();
    this.hideError();
    this.hideTable();

    try {
      const response = await fetch(url);

      if (!response.ok) {
        throw new Error(`Error fetching data: ${response.status} ${response.statusText}`);
      }

      const data = await response.json();

      this.renderDividends(data);
      this.showTable();

    } catch (error) {
      console.error('Fetch Error:', error);
      this.showError(`Failed to load dividend data: ${error.message}`);
    } finally {
      this.hideLoading();
    }
  }

  /**
   * Renders dividend data into the HTML table.
   * @param {Object} data - The dividend data fetched from the API.
   */
  renderDividends(data) {
    // Clear existing data
    this.dividendsTableBody.innerHTML = '';

    if (data.dividends && data.dividends.length > 0) {
      data.dividends.forEach(dividend => {
        const row = document.createElement('tr');
        row.innerHTML = `
                    <td>${this.formatDate(dividend.date)}</td>
                    <td>${this.formatDate(dividend.exDate)}</td>
                    <td>${this.formatAmount(dividend.amount)}</td>
                `;
        this.dividendsTableBody.appendChild(row);
      });
    } else {
      const noDataRow = document.createElement('tr');
      noDataRow.innerHTML = `<td colspan="3">No dividend data available.</td>`;
      this.dividendsTableBody.appendChild(noDataRow);
    }
  }

  /**
   * Formats a date string into a more readable format.
   * @param {string} dateString - The date string to format.
   * @returns {string} - The formatted date string.
   */
  formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    const date = new Date(dateString);
    return isNaN(date) ? dateString : date.toLocaleDateString(undefined, options);
  }

  /**
   * Formats the dividend amount to two decimal places with a dollar sign.
   * @param {number|string} amount - The amount to format.
   * @returns {string} - The formatted amount string.
   */
  formatAmount(amount) {
    const parsedAmount = parseFloat(amount);
    return isNaN(parsedAmount) ? amount : `$${parsedAmount.toFixed(2)}`;
  }

  /**
   * Displays the loading indicator.
   */
  showLoading() {
    this.loadingDiv.classList.remove('hidden');
  }

  /**
   * Hides the loading indicator.
   */
  hideLoading() {
    this.loadingDiv.classList.add('hidden');
  }

  /**
   * Displays the error message.
   * @param {string} message - The error message to display.
   */
  showError(message) {
    this.errorDiv.textContent = message;
    this.errorDiv.classList.remove('hidden');
  }

  /**
   * Hides the error message.
   */
  hideError() {
    this.errorDiv.classList.add('hidden');
  }

  /**
   * Shows the dividends table.
   */
  showTable() {
    this.dividendsTable.classList.remove('hidden');
  }

  /**
   * Hides the dividends table.
   */
  hideTable() {
    this.dividendsTable.classList.add('hidden');
  }
}
