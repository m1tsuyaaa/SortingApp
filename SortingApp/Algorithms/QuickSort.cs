using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
  public class QuickSort : ISortAlgorithm
  {
    public string Name => "Быстрая";

    private Action<List<double>, int> _onIteration;
    private int _iterations;
    private bool _ascending;
    private int _maxIterations;

    public List<double> Sort(
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration = null,
        int maxIterations = 0)
    {
      var arr = new List<double>(data);
      _onIteration = onIteration;
      _iterations = 0;
      _ascending = ascending;
      _maxIterations = maxIterations;

      QuickSortRecursive(arr, 0, arr.Count - 1);
      return arr;
    }

    private void QuickSortRecursive(List<double> arr, int low, int high)
    {
      if (low < high)
      {
        int pi = Partition(arr, low, high);
        QuickSortRecursive(arr, low, pi - 1);
        QuickSortRecursive(arr, pi + 1, high);
      }
    }

    private int Partition(List<double> arr, int low, int high)
    {
      double pivot = arr[high];
      int i = low - 1;

      for (int j = low; j < high; j++)
      {
        bool needSwap = _ascending ? arr[j] < pivot : arr[j] > pivot;

        if (needSwap)
        {
          i++;
          double tmp = arr[i];
          arr[i] = arr[j];
          arr[j] = tmp;
        }

        _iterations++;
        _onIteration?.Invoke(new List<double>(arr), _iterations);

        if (_maxIterations > 0 && _iterations >= _maxIterations)
          return i + 1;
      }

      // Постановка опорного элемента
      double tmp2 = arr[i + 1];
      arr[i + 1] = arr[high];
      arr[high] = tmp2;

      _iterations++;
      _onIteration?.Invoke(new List<double>(arr), _iterations);

      return i + 1;
    }
  }
}