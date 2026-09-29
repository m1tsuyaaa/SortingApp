using System.Collections.Generic;

namespace SortingApp.Models
{
    public class SortResult
    {
        public string AlgorithmName { get; set; }
        public double ElapsedMilliseconds { get; set; }
        public int Iterations { get; set; }
        public int ElementCount { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        // Снапшоты для анимации
        public List<List<double>> Snapshots { get; set; } = new List<List<double>>();
    }
}