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

        public static double Normalize(double value, bool integerMode)
        {
            return integerMode ? Math.Round(value) : value;
        }

        public static bool IsInteger(double value)
        {
            return Math.Abs(value - Math.Round(value)) < 0.0000001;
        }
    }
}