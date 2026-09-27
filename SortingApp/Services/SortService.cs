using System;
using System.Collections.Generic;
using System.Diagnostics;
using SortingApp.Algorithms;
using SortingApp.Models;

namespace SortingApp.Services
{
  public class SortService
  {
    public SortResult Execute(
        ISortAlgorithm algorithm,
        List<double> data,
        bool ascending,
        Action<List<double>, int> onIteration,
        int maxIterations = 0)
    {
      var result = new SortResult
      {
        AlgorithmName = algorithm.Name,
        ElementCount = data.Count,
        Success = false
      };

      try
      {
        var sw = Stopwatch.StartNew();

        var sorted = algorithm.Sort(
            data,
            ascending,
            onIteration,
            maxIterations);

        sw.Stop();

        result.ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds;
        result.Success = true;
      }
      catch (Exception ex)
      {
        result.Success = false;
        result.ErrorMessage = ex.Message;
      }

      return result;
    }
  }
}