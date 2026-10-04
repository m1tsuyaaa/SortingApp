using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
  public class ShakerSort : ISortAlgorithm
  {
    public string Name => "Шейкерная";

    public Tuple<List<double>, int> Sort(
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration = null,
        int maxIterations = 0)
    {
      var arr = new List<double>(data);
      int left = 0;
      int right = arr.Count - 1;
      int passes = 0;   // ← число ДВОЙНЫХ ПРОХОДОВ

      while (left < right)
      {
        // Проход слева направо
        for (int i = left; i < right; i++)
        {
          bool needSwap = ascending
              ? arr[i] > arr[i + 1]
              : arr[i] < arr[i + 1];

          if (needSwap)
          {
            double tmp = arr[i];
            arr[i] = arr[i + 1];
            arr[i + 1] = tmp;
          }
        }
        right--;

        // Проход справа налево
        for (int i = right; i > left; i--)
        {
          bool needSwap = ascending
              ? arr[i - 1] > arr[i]
              : arr[i - 1] < arr[i];

          if (needSwap)
          {
            double tmp = arr[i];
            arr[i] = arr[i - 1];
            arr[i - 1] = tmp;
          }
        }
        left++;

        passes++;   // ← считаем ДВОЙНОЙ проход
        onIteration?.Invoke(new List<double>(arr), passes);

        if (maxIterations > 0 && passes >= maxIterations)
          break;
      }

      return Tuple.Create(arr, passes);
    }
  }
}