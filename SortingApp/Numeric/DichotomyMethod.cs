using System;
using System.Collections.Generic;

namespace SortingApp.Numeric
{
  // ============================================================
  //  ИТЕРАЦИЯ МЕТОДА ДИХОТОМИИ
  // ============================================================
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

  // ============================================================
  //  РЕЗУЛЬТАТ МЕТОДА ДИХОТОМИИ
  // ============================================================
  public class DichotomyResult
  {
    public double XMin { get; set; }
    public double FMin { get; set; }
    public int Iterations { get; set; }
    public double FinalInterval { get; set; }
    public List<DichotomyIteration> Steps { get; set; }
        = new List<DichotomyIteration>();
  }

  // ============================================================
  //  КРИТИЧЕСКАЯ ТОЧКА
  // ============================================================
  public enum CriticalPointType
  {
    Minimum,
    Maximum,
    Inflection
  }

  public class CriticalPoint
  {
    public double X { get; set; }
    public double F { get; set; }
    public CriticalPointType Type { get; set; }
  }

  // ============================================================
  //  КОРЕНЬ ФУНКЦИИ (переход через ноль)
  // ============================================================
  public class RootPoint
  {
    public double X { get; set; }
    public double F { get; set; }

    /// <summary>
    /// Направление перехода:
    /// +1 — с минуса на плюс (↗)
    /// -1 — с плюса на минус (↘)
    /// </summary>
    public int Direction { get; set; }

    public int Iterations { get; set; }
  }

  // ============================================================
  //  ИСКЛЮЧЕНИЕ: НЕУНИМОДАЛЬНАЯ ФУНКЦИЯ
  // ============================================================
  public class NotUnimodalException : Exception
  {
    public int LocalMinimaCount { get; }
    public List<double> LocalMinima { get; }
    public List<CriticalPoint> AllCriticalPoints { get; }

    public NotUnimodalException(string message,
        int localMinimaCount,
        List<double> localMinima,
        List<CriticalPoint> allCriticalPoints = null)
        : base(message)
    {
      LocalMinimaCount = localMinimaCount;
      LocalMinima = localMinima;
      AllCriticalPoints = allCriticalPoints ?? new List<CriticalPoint>();
    }
  }

  // ============================================================
  //  МЕТОД ДИХОТОМИИ
  // ============================================================
  public static class DichotomyMethod
  {
    /// <summary>
    /// Находит все критические точки (минимумы, максимумы, перегибы).
    /// </summary>
    public static List<CriticalPoint> FindCriticalPoints(
        Func<double, double> func,
        double a, double b,
        int samples = 2000)
    {
      var result = new List<CriticalPoint>();

      if (samples < 20) samples = 20;

      double step = (b - a) / samples;

      var values = new double[samples + 1];
      for (int i = 0; i <= samples; i++)
      {
        double x = a + step * i;
        try { values[i] = func(x); }
        catch { values[i] = double.NaN; }
      }

      for (int i = 1; i < samples; i++)
      {
        if (double.IsNaN(values[i - 1]) ||
            double.IsNaN(values[i]) ||
            double.IsNaN(values[i + 1]))
          continue;

        double d1 = values[i] - values[i - 1];
        double d2 = values[i + 1] - values[i];

        if (d1 * d2 < 0)
        {
          double xCrit = a + step * i;
          double fCrit = values[i];

          try
          {
            double localA = a + step * (i - 1);
            double localB = a + step * (i + 1);

            if (d1 > 0 && d2 < 0)
              xCrit = RefineExtremum(func, localA, localB, true);
            else
              xCrit = RefineExtremum(func, localA, localB, false);

            fCrit = func(xCrit);
          }
          catch { }

          result.Add(new CriticalPoint
          {
            X = xCrit,
            F = fCrit,
            Type = d1 > 0 ? CriticalPointType.Maximum
                            : CriticalPointType.Minimum
          });
        }
      }

      var unique = new List<CriticalPoint>();
      foreach (var cp in result)
      {
        bool isDuplicate = false;
        foreach (var u in unique)
        {
          if (Math.Abs(cp.X - u.X) < step * 1.5 && cp.Type == u.Type)
          {
            isDuplicate = true;
            break;
          }
        }
        if (!isDuplicate) unique.Add(cp);
      }

      return unique;
    }

    private static double RefineExtremum(
        Func<double, double> func,
        double a, double b, bool isMax,
        int maxIter = 100)
    {
      const double phi = 0.618033988749895;

      double x1 = b - phi * (b - a);
      double x2 = a + phi * (b - a);
      double f1 = func(x1);
      double f2 = func(x2);

      for (int i = 0; i < maxIter; i++)
      {
        bool chooseLeft = isMax ? f1 > f2 : f1 < f2;

        if (chooseLeft)
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

    /// <summary>
    /// Находит локальные минимумы.
    /// </summary>
    public static List<double> FindLocalMinima(
        Func<double, double> func,
        double a, double b,
        int samples = 1000)
    {
      var minima = new List<double>();
      var criticalPoints = FindCriticalPoints(func, a, b, samples);

      foreach (var cp in criticalPoints)
      {
        if (cp.Type == CriticalPointType.Minimum)
          minima.Add(cp.X);
      }

      return minima;
    }

    /// <summary>
    /// Метод дихотомии для поиска локального минимума.
    /// </summary>
    public static DichotomyResult FindMinimum(
        Func<double, double> func,
        double a, double b, double epsilon,
        int maxIterations = 10000)
    {
      if (func == null) throw new ArgumentNullException(nameof(func));
      if (a >= b) throw new ArgumentException("a должно быть < b");
      if (epsilon <= 0) throw new ArgumentException("ε должно быть > 0");

      var criticalPoints = FindCriticalPoints(func, a, b, 1000);
      var localMinima = new List<double>();
      var localMaxima = new List<double>();

      foreach (var cp in criticalPoints)
      {
        if (cp.Type == CriticalPointType.Minimum)
          localMinima.Add(cp.X);
        else if (cp.Type == CriticalPointType.Maximum)
          localMaxima.Add(cp.X);
      }

      if (localMinima.Count > 1 || localMaxima.Count > 0)
      {
        string extra = localMaxima.Count > 0
            ? $"\nНайдено {localMaxima.Count} локальных максимумов."
            : "";

        throw new NotUnimodalException(
            $"На интервале [{a}; {b}] найдено {localMinima.Count} " +
            $"локальных минимумов.{extra}\n" +
            "Метод дихотомии работает ТОЛЬКО для унимодальных функций.",
            localMinima.Count,
            localMinima,
            criticalPoints);
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

  // ============================================================
  //  ПОИСК КОРНЕЙ ФУНКЦИИ (переход через ноль)
  // ============================================================
  public static class RootFinder
  {
    /// <summary>
    /// Находит все корни функции на отрезке [a, b],
    /// где f(x) меняет знак (переходит через ноль).
    /// </summary>
    public static List<RootPoint> FindRoots(
        Func<double, double> func,
        double a, double b,
        double epsilon = 1e-6,
        int samples = 2000)
    {
      var roots = new List<RootPoint>();

      if (samples < 20) samples = 20;

      double step = (b - a) / samples;

      double prevX = a;
      double prevF = SafeEval(func, a);

      for (int i = 1; i <= samples; i++)
      {
        double currX = a + step * i;
        double currF = SafeEval(func, currX);

        if (double.IsNaN(prevF) || double.IsNaN(currF))
        {
          prevX = currX;
          prevF = currF;
          continue;
        }

        // Смена знака
        if (prevF * currF < 0)
        {
          var root = RefineRoot(func, prevX, currX, epsilon, prevF, currF);
          if (root != null)
            roots.Add(root);
        }
        // Точное попадание в ноль
        else if (Math.Abs(currF) < 1e-12)
        {
          roots.Add(new RootPoint
          {
            X = currX,
            F = 0,
            Direction = 0,
            Iterations = 0
          });
        }

        prevX = currX;
        prevF = currF;
      }

      // Убираем близкие дубликаты
      var unique = new List<RootPoint>();
      foreach (var r in roots)
      {
        bool isDup = false;
        foreach (var u in unique)
        {
          if (Math.Abs(r.X - u.X) < step * 0.5)
          {
            isDup = true;
            break;
          }
        }
        if (!isDup) unique.Add(r);
      }

      return unique;
    }

    /// <summary>
    /// Уточняет корень методом дихотомии.
    /// </summary>
    private static RootPoint RefineRoot(
        Func<double, double> func,
        double a, double b, double epsilon,
        double fa, double fb)
    {
      int direction = Math.Sign(fb) - Math.Sign(fa);
      int iterations = 0;

      double mid = (a + b) / 2;
      double fmid = SafeEval(func, mid);

      while ((b - a) > epsilon && iterations < 200)
      {
        mid = (a + b) / 2;
        fmid = SafeEval(func, mid);

        if (double.IsNaN(fmid)) break;

        if (fa * fmid < 0)
        {
          b = mid;
          fb = fmid;
        }
        else
        {
          a = mid;
          fa = fmid;
        }

        iterations++;
      }

      return new RootPoint
      {
        X = (a + b) / 2,
        F = SafeEval(func, (a + b) / 2),
        Direction = direction,
        Iterations = iterations
      };
    }

    private static double SafeEval(Func<double, double> func, double x)
    {
      try
      {
        double y = func(x);
        if (double.IsNaN(y) || double.IsInfinity(y)) return double.NaN;
        return y;
      }
      catch { return double.NaN; }
    }
  }
}