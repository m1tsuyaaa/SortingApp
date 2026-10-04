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
      // Защита от некорректного ввода
      if (count <= 0) return new List<double>();
      if (min > max)
      {
        double tmp = min;
        min = max;
        max = tmp;
      }

      var random = new Random();
      var data = new List<double>(count);

      for (int i = 0; i < count; i++)
      {
        // random.NextDouble() возвращает [0.0, 1.0)
        // Умножаем на диапазон и прибавляем минимум
        double value = min + random.NextDouble() * (max - min);

        // Если запрещены дробные — округляем
        if (!allowFractional)
          value = Math.Round(value);

        // Финальная страховка: гарантируем, что значение в диапазоне
        if (value < min) value = min;
        if (value > max) value = max;

        data.Add(value);
      }

      return data;
    }
  }
}