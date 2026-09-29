using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
    public class InsertionSort : ISortAlgorithm
    {
        public string Name => "Вставками";

        public List<double> Sort(
            List<double> data,
            bool ascending,
            Action<List<double>, int> onIteration = null,
            int maxIterations = 0)
        {
            var arr = new List<double>(data);
            int iterations = 0;

            for (int i = 1; i < arr.Count; i++)
            {
                double key = arr[i];
                int j = i - 1;

                while (j >= 0 && (ascending ? arr[j] > key : arr[j] < key))
                {
                    arr[j + 1] = arr[j];
                    j--;
                    iterations++;
                    onIteration?.Invoke(new List<double>(arr), iterations);

                    if (maxIterations > 0 && iterations >= maxIterations)
                        return arr;
                }

                arr[j + 1] = key;
                iterations++;
                onIteration?.Invoke(new List<double>(arr), iterations);

                if (maxIterations > 0 && iterations >= maxIterations)
                    return arr;
            }

            return arr;
        }
    }
}