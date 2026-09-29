using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
    public interface ISortAlgorithm
    {
        string Name { get; }

        List<double> Sort(
            List<double> data,
            bool ascending,
            Action<List<double>, int> onIteration = null,
            int maxIterations = 0);
    }
}