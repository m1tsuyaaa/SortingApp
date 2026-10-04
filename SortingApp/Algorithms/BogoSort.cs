using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
  public class BogoSort : ISortAlgorithm
  {
    public string Name => "BOGO";

    public Tuple<List<double>, int> Sort(
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration = null,
        int maxIterations = 0)
    {
      var arr = new List<double>(data);
      var random = new Random();
      int shuffles = 0;

      int limit = maxIterations > 0 ? maxIterations : 10000;

      // Сохраняем исходный порядок, чтобы определять прогресс
      onIteration?.Invoke(new List<double>(arr), 0);

      while (!IsSorted(arr, ascending) && shuffles < limit)
      {
        // Перемешивание Фишера-Йетса
        for (int i = arr.Count - 1; i > 0; i--)
        {
          int j = random.Next(i + 1);
          double tmp = arr[i];
          arr[i] = arr[j];
          arr[j] = tmp;
        }

        shuffles++;
        onIteration?.Invoke(new List<double>(arr), shuffles);
      }

      return Tuple.Create(arr, shuffles);
    }

    private bool IsSorted(List<double> arr, bool ascending)
    {
      for (int i = 1; i < arr.Count; i++)
      {
        if (ascending ? arr[i - 1] > arr[i] : arr[i - 1] < arr[i])
          return false;
      }
      return true;
    }
  }
}