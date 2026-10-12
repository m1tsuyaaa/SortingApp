using System;
using System.Collections.Generic;

namespace SortingApp.Numeric
{
  public class DichotomyIteration
  {
    public int Number { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double C { get; set; }
    public double X1 { get; set; }
    public double X2 { get; set; }
    public double F1 { get; set; }
    public double F2 { get; set; }
    public double Interval => B - A;
  }

  public class DichotomyResult
  {
    public double XMin { get; set; }
    public double FMin { get; set; }
    public int Iterations { get; set; }
    public double FinalInterval { get; set; }
    public List<DichotomyIteration> Steps { get; set; }
        = new List<DichotomyIteration>();
  }

  /// <summary>
  /// Исключение при попытке применить дихотомию к немонотонной функции.
  /// </summary>
  public class NotUnimodalException : Exception
  {
    public int LocalMinimaCount { get; }
    public List<double> LocalMinima { get; }

    public NotUnimodalException(string message,
        int localMinimaCount,
        List<double> localMinima)
        : base(message)
    {
      LocalMinimaCount = localMinimaCount;
      LocalMinima = localMinima;
    }
  }

  public static class DichotomyMethod
  {
    /// <summary>
    /// Проверяет, является ли функция унимодальной на отрезке [a, b].
    /// Унимодальная функция имеет только 1 локальный минимум.
    /// </summary>
    /// <param name="func">Функция</param>
    /// <param name="a">Левая граница</param>
    /// <param name="b">Правая граница</param>
    /// <param name="samples">Число точек для анализа</param>
    /// <returns>Список локальных минимумов (если их больше 1 — функция немонотонна)</returns>
    public static List<double> FindLocalMinima(
        Func<double, double> func,
        double a, double b,
        int samples = 1000)
    {
      var minima = new List<double>();

      if (samples < 10) samples = 10;

      double step = (b - a) / samples;

      // Считаем значения
      var values = new double[samples + 1];
      for (int i = 0; i <= samples; i++)
      {
        double x = a + step * i;
        try { values[i] = func(x); }
        catch { values[i] = double.NaN; }
      }

      // Ищем локальные минимумы:
      // точка i — локальный минимум, если values[i-1] > values[i] < values[i+1]
      for (int i = 1; i < samples; i++)
      {
        if (double.IsNaN(values[i - 1]) ||
            double.IsNaN(values[i]) ||
            double.IsNaN(values[i + 1]))
          continue;

        if (values[i - 1] > values[i] && values[i] < values[i + 1])
        {
          // Локальный минимум в точке i
          double xMin = a + step * i;

          // Уточняем дихотомией в узком диапазоне
          try
          {
            double localA = a + step * (i - 1);
            double localB = a + step * (i + 1);
            double refined = RefineMinimum(func, localA, localB);
            minima.Add(refined);
          }
          catch
          {
            minima.Add(xMin);
          }
        }
      }

      // Убираем близкие дубликаты (на всякий случай)
      var unique = new List<double>();
      foreach (var m in minima)
      {
        bool isDuplicate = false;
        foreach (var u in unique)
        {
          if (Math.Abs(m - u) < step * 1.5)
          {
            isDuplicate = true;
            break;
          }
        }
        if (!isDuplicate) unique.Add(m);
      }

      return unique;
    }

    /// <summary>
    /// Уточнение минимума методом золотого сечения (быстрее дихотомии).
    /// </summary>
    private static double RefineMinimum(
        Func<double, double> func,
        double a, double b,
        int maxIter = 100)
    {
      const double phi = 0.618033988749895;
      double x1 = b - phi * (b - a);
      double x2 = a + phi * (b - a);
      double f1 = func(x1);
      double f2 = func(x2);

      for (int i = 0; i < maxIter; i++)
      {
        if (f1 < f2)
        {
          b = x2;
          x2 = x1;
          f2 = f1;
          x1 = b - phi * (b - a);
          f1 = func(x1);
        }
        else
        {
          a = x1;
          x1 = x2;
          f1 = f2;
          x2 = a + phi * (b - a);
          f2 = func(x2);
        }
      }

      return (a + b) / 2.0;
    }

    public static DichotomyResult FindMinimum(
        Func<double, double> func,
        double a, double b, double epsilon,
        int maxIterations = 10000)
    {
      if (func == null) throw new ArgumentNullException(nameof(func));
      if (a >= b) throw new ArgumentException("a должно быть < b");
      if (epsilon <= 0) throw new ArgumentException("ε должно быть > 0");

      // ✅ ПРОВЕРКА УНИМОДАЛЬНОСТИ
      var localMinima = FindLocalMinima(func, a, b, 1000);

      if (localMinima.Count > 1)
      {
        throw new NotUnimodalException(
            $"На интервале [{a}; {b}] найдено {localMinima.Count} локальных минимумов.\n" +
            "Метод дихотомии работает ТОЛЬКО для унимодальных функций " +
            "(с одним минимумом на отрезке).\n\n" +
            "Сузьте интервал так, чтобы на нём был только один минимум,\n" +
            "или используйте другой метод оптимизации.",
            localMinima.Count,
            localMinima);
      }

      var result = new DichotomyResult();
      int iterations = 0;

      result.Steps.Add(new DichotomyIteration
      {
        Number = 0,
        A = a,
        B = b,
        C = (a + b) / 2,
        X1 = a,
        X2 = b,
        F1 = func(a),
        F2 = func(b)
      });

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
          throw new Exception($"Функция не определена в точке x ≈ {c:F4}");
        }

        result.Steps.Add(new DichotomyIteration
        {
          Number = iterations + 1,
          A = a,
          B = b,
          C = c,
          X1 = x1,
          X2 = x2,
          F1 = f1,
          F2 = f2
        });

        if (f1 < f2) b = x2;
        else a = x1;

        iterations++;
      }

      result.XMin = (a + b) / 2.0;
      result.FMin = func(result.XMin);
      result.Iterations = iterations;
      result.FinalInterval = b - a;

      return result;
    }
  }
}