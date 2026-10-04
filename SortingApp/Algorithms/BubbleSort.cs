using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
  public class BubbleSort : ISortAlgorithm
  {
    public string Name => "Пузырьковая";

    public Tuple<List<double>, int> Sort(
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration = null,
        int maxIterations = 0)
    {
      var arr = new List<double>(data);
      int n = arr.Count;
      int passes = 0;   // ← число ПРОХОДОВ (а не сравнений!)

      // Максимум n-1 проходов
      for (int i = 0; i < n - 1; i++)
      {
        bool swapped = false;

        for (int j = 0; j < n - 1 - i; j++)
        {
          bool needSwap = ascending
              ? arr[j] > arr[j + 1]
              : arr[j] < arr[j + 1];

          if (needSwap)
          {
            double tmp = arr[j];
            arr[j] = arr[j + 1];
            arr[j + 1] = tmp;
            swapped = true;
          }
        }

        passes++;   // ← считаем ПРОХОД
        onIteration?.Invoke(new List<double>(arr), passes);

        if (maxIterations > 0 && passes >= maxIterations)
          break;

        if (!swapped) break;   // уже отсортировано
      }

      return Tuple.Create(arr, passes);
    }
  }
}