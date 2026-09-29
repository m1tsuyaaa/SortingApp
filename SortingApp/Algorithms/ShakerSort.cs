using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
    public class ShakerSort : ISortAlgorithm
    {
        public string Name => "Шейкерная";

        public List<double> Sort(
            List<double> data,
            bool ascending,
            Action<List<double>, int> onIteration = null,
            int maxIterations = 0)
        {
            var arr = new List<double>(data);
            int left = 0;
            int right = arr.Count - 1;
            int iterations = 0;

            while (left < right)
            {
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

                    iterations++;
                    onIteration?.Invoke(new List<double>(arr), iterations);

                    if (maxIterations > 0 && iterations >= maxIterations)
                        return arr;
                }
                right--;

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

                    iterations++;
                    onIteration?.Invoke(new List<double>(arr), iterations);

                    if (maxIterations > 0 && iterations >= maxIterations)
                        return arr;
                }
                left++;
            }

            return arr;
        }
    }
}