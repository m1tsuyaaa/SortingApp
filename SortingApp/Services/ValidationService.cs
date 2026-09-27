using System;
using System.Globalization;

namespace SortingApp.Services
{
  public static class ValidationService
  {
    public static bool TryParseDouble(string input, out double value)
    {
      return double.TryParse(
          input,
          NumberStyles.Any,
          CultureInfo.InvariantCulture,
          out value);
    }

    public static bool TryParseInt(string input, out int value)
    {
      return int.TryParse(input, out value);
    }

    /// <summary>
    /// Округляет значение до целого, если integerMode = true.
    /// </summary>
    public static double Normalize(double value, bool integerMode)
    {
      return integerMode ? Math.Round(value) : value;
    }

    /// <summary>
    /// Проверяет, является ли число целым (для валидации).
    /// </summary>
    public static bool IsInteger(double value)
    {
      return Math.Abs(value - Math.Round(value)) < 0.0000001;
    }
  }
}