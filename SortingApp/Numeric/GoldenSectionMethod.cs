using System;
using System.Collections.Generic;

namespace SortingApp.Numeric
{
  public class GoldenIteration
  {
    public int Number { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double X1 { get; set; }
    public double X2 { get; set; }
    public double F1 { get; set; }
    public double F2 { get; set; }
    public double Interval => B - A;
  }

  public class GoldenResult
  {
    public double XExtremum { get; set; }
    public double FExtremum { get; set; }
    public int Iterations { get; set; }
    public double FinalInterval { get; set; }
    public bool IsMaximum { get; set; }
    public List<GoldenIteration> Steps { get; set; } = new List<GoldenIteration>();
  }

  public static class GoldenSectionMethod
  {
    private const double PHI = 0.61803398874989484820;

    /// <summary>
    /// Поиск экстремума (минимума или максимума) методом золотого сечения.
    /// </summary>
    public static GoldenResult FindExtremum(
        Func<double, double> func,
        double a, double b, double epsilon,
        bool findMaximum = false,
        int maxIterations = 10000)
    {
      if (func == null) throw new ArgumentNullException(nameof(func));
      if (a >= b) throw new ArgumentException("a должно быть < b");
      if (epsilon <= 0) throw new ArgumentException("ε должно быть > 0");

      // Проверка унимодальности
      var criticalPoints = DichotomyMethod.FindCriticalPoints(func, a, b, 1000);
      int minCount = 0, maxCount = 0;
      foreach (var cp in criticalPoints)
      {
        if (cp.Type == CriticalPointType.Minimum) minCount++;
        else if (cp.Type == CriticalPointType.Maximum) maxCount++;
      }

      // Если ищем минимум — должен быть 1 минимум и 0 максимумов
      // Если ищем максимум — должен быть 1 максимум и 0 минимумов
      bool isUnimodal = findMaximum
          ? (maxCount == 1 && minCount == 0)
          : (minCount == 1 && maxCount == 0);

      if (!isUnimodal)
      {
        throw new NotUnimodalException(
            $"На интервале [{a}; {b}] функция НЕ УНИМОДАЛЬНА для поиска " +
            $"{(findMaximum ? "МАКСИМУМА" : "МИНИМУМА")}.\n" +
            $"Найдено: {minCount} минимумов, {maxCount} максимумов.",
            minCount,
            new List<double>(),
            criticalPoints);
      }

      var result = new GoldenResult
      {
        IsMaximum = findMaximum
      };

      int iterations = 0;

      // Начальные точки
      double x1 = b - PHI * (b - a);
      double x2 = a + PHI * (b - a);
      double f1 = func(x1);
      double f2 = func(x2);

      result.Steps.Add(new GoldenIteration
      {
        Number = 0,
        A = a,
        B = b,
        X1 = x1,
        X2 = x2,
        F1 = f1,
        F2 = f2
      });

      while ((b - a) > epsilon && iterations < maxIterations)
      {
        bool chooseLeft = findMaximum ? (f1 > f2) : (f1 < f2);

        if (chooseLeft)
        {
          b = x2;
          x2 = x1;
          f2 = f1;
          x1 = b - PHI * (b - a);
          f1 = func(x1);
        }
        else
        {
          a = x1;
          x1 = x2;
          f1 = f2;
          x2 = a + PHI * (b - a);
          f2 = func(x2);
        }

        iterations++;

        result.Steps.Add(new GoldenIteration
        {
          Number = iterations,
          A = a,
          B = b,
          X1 = x1,
          X2 = x2,
          F1 = f1,
          F2 = f2
        });
      }

      result.XExtremum = (a + b) / 2.0;
      result.FExtremum = func(result.XExtremum);
      result.Iterations = iterations;
      result.FinalInterval = b - a;

      return result;
    }
  }
}