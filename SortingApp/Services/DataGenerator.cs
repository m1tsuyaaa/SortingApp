using System;
using System.Collections.Generic;

namespace SortingApp.Services
{
  public static class DataGenerator
  {
    public static List<double> Generate(
        int count,
        double min,
        double max,
        bool allowFractional)
    {
      var random = new Random();
      var data = new List<double>(count);

      for (int i = 0; i < count; i++)
      {
        double value = min + random.NextDouble() * (max - min);

        if (!allowFractional)
          value = Math.Round(value);

        data.Add(value);
      }

      return data;
    }
  }
}