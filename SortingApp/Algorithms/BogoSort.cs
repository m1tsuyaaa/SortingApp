using System;
using System.Collections.Generic;

namespace SortingApp.Algorithms
{
    public class BogoSort : ISortAlgorithm
    {
        public string Name => "BOGO";

        public List<double> Sort(
            List<double> data,
            bool ascending,
            Action<List<double>, int> onIteration = null,
            int maxIterations = 0)
        {
            var arr = new List<double>(data);
            var random = new Random();
            int iterations = 0;

            int limit = maxIterations > 0 ? maxIterations : 10000;

            while (!IsSorted(arr, ascending) && iterations < limit)
            {
                for (int i = arr.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    double tmp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = tmp;
                }

                iterations++;
                onIteration?.Invoke(new List<double>(arr), iterations);
            }

            return arr;
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