using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
  public class InsertionSort : ISortAlgorithm
  {
    public string Name => "Вставками";

    public Tuple<List<double>, int> Sort(
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration = null,
        int maxIterations = 0)
    {
      var arr = new List<double>(data);
      int passes = 0;   // ← число ВСТАВОК

      for (int i = 1; i < arr.Count; i++)
      {
        double key = arr[i];
        int j = i - 1;

        while (j >= 0 && (ascending ? arr[j] > key : arr[j] < key))
        {
          arr[j + 1] = arr[j];
          j--;
        }

        arr[j + 1] = key;

        passes++;   // ← считаем вставку
        onIteration?.Invoke(new List<double>(arr), passes);

        if (maxIterations > 0 && passes >= maxIterations)
          break;
      }

      return Tuple.Create(arr, passes);
    }
  }
}