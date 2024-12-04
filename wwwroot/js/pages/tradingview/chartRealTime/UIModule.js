const UIModule = (() => {
  function setupSymbolSearch(config, onSelect) {
    const symbolInput = document.getElementById(config.symbolInputId);
    const suggestionsBox = document.getElementById(config.suggestionsId);

    symbolInput.addEventListener('input', async () => {
      const query = symbolInput.value.trim();
      if (query.length >= 1) {
        try {
          const data = await DataModule.fetchSymbolSuggestions(config.apiEndpoints.search(query));
          if (data.length > 0) {
            suggestionsBox.innerHTML = '';
            data.forEach(result => {
              const suggestionItem = document.createElement('div');
              suggestionItem.classList.add('suggestion-item');
              const imageUrl = result.img || 'https://via.placeholder.com/40';
              suggestionItem.innerHTML = `
                <img src="${imageUrl}" alt="${result.symbol}">
                <p>${result.symbol} - ${result.shortname}</p>
              `;
              suggestionItem.onclick = () => onSelect(result);
              suggestionsBox.appendChild(suggestionItem);
            });
            suggestionsBox.style.display = 'block';
          } else {
            suggestionsBox.style.display = 'none';
          }
        } catch (error) {
          console.error('Error fetching symbols:', error);
          suggestionsBox.style.display = 'none';
        }
      } else {
        suggestionsBox.style.display = 'none';
      }
    });
  }

  function setupUpdateButton(config, onUpdate) {
    const updateButton = document.getElementById(config.updateButtonId);
    updateButton.addEventListener('click', () => {
      const symbol = document.getElementById(config.symbolInputId).value.trim().toUpperCase();
      if (symbol) {
        onUpdate(symbol);
      }
    });
  }

  function setupIntervalChange(config, onIntervalChange) {
    const intervalInput = document.getElementById(config.updateIntervalInputId);
    intervalInput.addEventListener('change', onIntervalChange);
  }

  function setupDataIntervalSelect(config, onDataIntervalChange) {
    const dataIntervalSelect = document.getElementById(config.dataIntervalSelectId);
    dataIntervalSelect.addEventListener('change', onDataIntervalChange);
  }

  return {
    setupSymbolSearch,
    setupUpdateButton,
    setupIntervalChange,
    setupDataIntervalSelect
  };
})();
