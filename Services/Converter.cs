using System.Globalization;

namespace MarketAnalyticHub.Services
{
  public static class Converter
  {
    public static int  ConvertToInt(string value)
    {
      // Try to parse the value as a decimal with invariant culture
      if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalValue))
      {
        // Convert the decimal to an integer
        return (int)decimalValue;
      }

      // Define a culture for fallback parsing
      CultureInfo culture = new CultureInfo("en-US");

      // Try to parse the value as a decimal using the provided culture
      if (decimal.TryParse(value, NumberStyles.Number, culture, out decimal decimalValue1))
      {
        // Convert the decimal to an integer
        return (int)decimalValue1;
      }

      // Return a default value if parsing fails
      return 0;
    }

    public static decimal ConvertToDecimal(string value)
    {
      // Try to parse the value as a decimal with invariant culture
      if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalValue))
      {
        return decimalValue;
      }

      // Define a culture for fallback parsing
      CultureInfo culture = new CultureInfo("en-US");

      // Try to parse the value as a decimal using the provided culture
      if (decimal.TryParse(value, NumberStyles.Number, culture, out decimal decimalValue1))
      {
        return decimalValue1;
      }

      // Return a default value if parsing fails
      return 0;
    }

    public static DateTime ConvertToDateTime(string value)
    {
      string[] dateFormats = { "yyyyMMdd", "yyyy/MM/dd", "MM/dd/yyyy", "dd/MM/yyyy" };
      CultureInfo culture = CultureInfo.InvariantCulture;

      // Try to parse the value using the specified date formats
      if (DateTime.TryParseExact(value, dateFormats, culture, DateTimeStyles.None, out DateTime itemPurchaseDate))
      {
        return itemPurchaseDate;
      }

      // Return DateTime.MinValue if parsing fails
      return DateTime.MinValue;
    }
  }

}
