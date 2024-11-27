using YahooFinanceApi;

namespace MarketAnalyticHub.Services
{

    // Utility methods for validation and conversion
    public static class Utility
    {
      public static string GetValidString(string value, string defaultValue)
      {
        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
      }

      public static string GetValidString(object value, string defaultValue)
      {
        return value != null && !string.IsNullOrWhiteSpace(value.ToString()) ? value.ToString() : defaultValue;
      }

      public static decimal GetValidDecimal(object value, decimal defaultValue)
      {
        return value != null && decimal.TryParse(value.ToString(), out var result) ? result : defaultValue;
      }

      public static double GetValidDouble(object value, double defaultValue)
      {
        return value != null && double.TryParse(value.ToString(), out var result) ? result : defaultValue;
      }

      public static long GetValidLong(object value, long defaultValue)
      {
        return value != null && long.TryParse(value.ToString(), out var result) ? result : defaultValue;
      }
    }
  public static class DictionaryUtils
  {
    public static T GetValueOrDefault<T>(Dictionary<string, Security?> dictionary, string key, Func<Security?, T> selector, T defaultValue)
    {
      if (dictionary != null && dictionary.ContainsKey(key))
      {
        var value = dictionary[key];
        if (value != null && selector != null)
        {
          try
          {
            return selector(value);
          }
          catch
          {
            // Log opcional para debug
            Console.WriteLine($"Erro ao processar a chave '{key}'.");
          }
        }
      }
      return defaultValue;
    }
  }

  public static class SafeValueUtil
  {
    /// <summary>
    /// Safely retrieves a value from a potentially null or invalid source, returning a default value in case of failure.
    /// </summary>
    /// <typeparam name="T">The type of the value to return.</typeparam>
    /// <param name="getter">A function to retrieve the value.</param>
    /// <param name="defaultValue">The default value to return in case of failure.</param>
    /// <returns>The retrieved value or the default value if an exception occurs.</returns>
    public static T GetValueOrDefault<T>(Func<T> getter, T defaultValue)
    {
      try
      {
        return getter();
      }
      catch
      {
        return defaultValue;
      }
    }
  }

}
