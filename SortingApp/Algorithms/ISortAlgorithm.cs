using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
  public interface ISortAlgorithm
  {
    string Name { get; }

    /// <summary>
    /// Сортировка списка.
    /// </summary>
    /// <param name="data">Исходные данные</param>
    /// <param name="ascending">true — по возрастанию</param>
    /// <param name="onIteration">Callback на каждую итерацию (массив, номер итерации)</param>
    /// <param name="maxIterations">Ограничение итераций (0 = без ограничения)</param>
    List<double> Sort(
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration = null,
        int maxIterations = 0);
  }
}