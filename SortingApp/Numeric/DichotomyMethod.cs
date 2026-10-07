using System;

namespace SortingApp.Numeric
{
  public class DichotomyResult
  {
    public double XMin { get; set; }
    public double FMin { get; set; }
    public int Iterations { get; set; }
    public double FinalInterval { get; set; }
  }

  public static class DichotomyMethod
  {
    /// <summary>
    /// Метод половинного деления (дихотомии) для поиска минимума.
    /// </summary>
    /// <param name="func">Целевая функция f(x)</param>
    /// <param name="a">Левая граница</param>
    /// <param name="b">Правая граница</param>
    /// <param name="epsilon">Точность</param>
    /// <param name="maxIterations">Максимум итераций (защита от зацикливания)</param>
    public static DichotomyResult FindMinimum(
        Func<double, double> func,
        double a, double b, double epsilon,
        int maxIterations = 10000)
    {
      if (func == null) throw new ArgumentNullException(nameof(func));
      if (a >= b) throw new ArgumentException("a должно быть < b");
      if (epsilon <= 0) throw new ArgumentException("ε должно быть > 0");

      int iterations = 0;

      while ((b - a) > epsilon && iterations < maxIterations)
      {
        double c = (a + b) / 2.0;
        double x1 = c - epsilon / 4.0;
        double x2 = c + epsilon / 4.0;

        double f1 = func(x1);
        double f2 = func(x2);

        if (double.IsNaN(f1) || double.IsInfinity(f1) ||
            double.IsNaN(f2) || double.IsInfinity(f2))
        {
          throw new Exception(
              $"Функция не определена в точке x ≈ {c:F4}");
        }

        if (f1 < f2)
          b = x2;
        else
          a = x1;

        iterations++;
      }

      double xMin = (a + b) / 2.0;
      double fMin = func(xMin);

      return new DichotomyResult
      {
        XMin = xMin,
        FMin = fMin,
        Iterations = iterations,
        FinalInterval = b - a
      };
    }
  }
}