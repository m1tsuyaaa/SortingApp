using System;
using System.Collections.Generic;

namespace SortingApp.Numeric
{
  public class NewtonIteration
  {
    public int Number { get; set; }
    public double X { get; set; }
    public double FX { get; set; }
    public double DFX { get; set; }
    public double D2FX { get; set; }
    public double DeltaX { get; set; }
  }

  public class NewtonResult
  {
    public double XResult { get; set; }
    public double FResult { get; set; }
    public int Iterations { get; set; }
    public bool IsMaximum { get; set; }
    public bool IsRoot { get; set; }
    public List<NewtonIteration> Steps { get; set; } = new List<NewtonIteration>();
  }

  public static class NewtonMethod
  {
    private const double H = 1e-5; // Шаг для численного дифференцирования

    /// Численная первая производная.
    public static double Derivative(Func<double, double> func, double x)
    {
      return (func(x + H) - func(x - H)) / (2 * H);
    }

    /// Численная вторая производная.
    public static double SecondDerivative(Func<double, double> func, double x)
    {
      return (func(x + H) - 2 * func(x) + func(x - H)) / (H * H);
    }

    /// Поиск экстремума (min/max) методом Ньютона.
    public static NewtonResult FindExtremum(
    Func<double, double> func,
    double x0, double epsilon,
    bool findMaximum = false,
    int maxIterations = 100)
    {
      if (func == null) throw new ArgumentNullException(nameof(func));
      if (epsilon <= 0) throw new ArgumentException("ε должно быть > 0");

      var result = new NewtonResult
      {
        IsMaximum = findMaximum,
        IsRoot = false
      };

      double x = x0;
      int iter = 0;

      while (iter < maxIterations)
      {
        double fx = func(x);
        double dfx = Derivative(func, x);
        double d2fx = SecondDerivative(func, x);

        // ЗАЩИТА 1: Значения улетели в бесконечность, NaN или аномально большие (разрыв)
        if (double.IsNaN(fx) || double.IsInfinity(fx) || Math.Abs(fx) > 1e6 || Math.Abs(x) > 1000000)
        {
          throw new Exception("Метод разошелся: значение x или f(x) улетело в бесконечность.\n" +
                              "Возможно, у функции нет экстремума (гипербола, прямая или разрыв).");
        }

        // ЗАЩИТА 2: Производные аномально большие (признак разрыва или асимптоты)
        if (Math.Abs(dfx) > 1e10 || Math.Abs(d2fx) > 1e12)
        {
          throw new Exception("Производная слишком большая.\n" +
                              "Функция имеет разрыв в этой области.\n" +
                              "Измените начальную точку x₀.");
        }

        // ЗАЩИТА 3: Вторая производная близка к нулю (перегиб или пологая функция)
        if (Math.Abs(d2fx) < 1e-8)
        {
          throw new Exception($"Вторая производная близка к нулю (f'' = {d2fx:E3}).\n" +
                              "Метод Ньютона не может найти экстремум в этой точке (возможен перегиб).");
        }

        double deltaX = dfx / d2fx;
        double xNext = x - deltaX;

        result.Steps.Add(new NewtonIteration
        {
          Number = iter,
          X = x,
          FX = fx,
          DFX = dfx,
          D2FX = d2fx,
          DeltaX = deltaX
        });

        // Проверка сходимости по шагу
        if (Math.Abs(deltaX) < epsilon)
        {
          x = xNext;
          iter++;
          break;
        }

        x = xNext;
        iter++;
      }

      if (iter >= maxIterations)
      {
        throw new Exception($"Метод не сошелся за {maxIterations} итераций.\n" +
                            "Попробуйте изменить начальную точку x₀ или увеличить лимит.");
      }

      double finalFx = func(x);
      double finalD2 = SecondDerivative(func, x);

      if (Math.Abs(finalD2) < 1e-8)
      {
        throw new Exception("Вторая производная в найденной точке близка к нулю.\n" +
                            "Это не минимум и не максимум (возможно, точка перегиба).");
      }

      bool actuallyMax = finalD2 < 0;

      if (actuallyMax != findMaximum)
      {
        throw new Exception(
            $"Найден {(actuallyMax ? "МАКСИМУМ" : "МИНИМУМ")}, " +
            $"но запрашивался {(findMaximum ? "МАКСИМУМ" : "МИНИМУМ")}.\n" +
            $"f''(x) = {finalD2:F4}\n\n" +
            $"Попробуйте изменить начальную точку x₀, чтобы попасть в нужную 'яму'.");
      }

      result.Steps.Add(new NewtonIteration
      {
        Number = iter,
        X = x,
        FX = finalFx,
        DFX = Derivative(func, x),
        D2FX = finalD2,
        DeltaX = 0
      });

      result.XResult = x;
      result.FResult = finalFx;
      result.Iterations = iter;
      result.IsMaximum = actuallyMax;

      return result;
    }

    public static NewtonResult FindRoot(
        Func<double, double> func,
        double x0, double epsilon,
        int maxIterations = 100)
    {
      if (func == null) throw new ArgumentNullException(nameof(func));
      if (epsilon <= 0) throw new ArgumentException("ε должно быть > 0");

      var result = new NewtonResult
      {
        IsMaximum = false,
        IsRoot = true
      };

      double x = x0;
      int iter = 0;

      while (iter < maxIterations)
      {
        double fx = func(x);
        double dfx = Derivative(func, x);

        // ЗАЩИТА 1: Значения улетели в бесконечность
        if (double.IsNaN(fx) || double.IsInfinity(fx) || Math.Abs(fx) > 1e12 || Math.Abs(x) > 1000000)
        {
          throw new Exception("Метод разошелся: значение улетело в бесконечность.\n" +
                              "Попробуйте другое начальное приближение x₀.");
        }

        // ЗАЩИТА 2: Производная близка к нулю (касательная горизонтальна, метод не сработает)
        if (Math.Abs(dfx) < 1e-8)
        {
          throw new Exception($"Производная близка к нулю (f' = {dfx:E3}) в точке x = {x:F4}.\n" +
                              "Метод Ньютона не может продолжить (деление на ноль).");
        }

        double deltaX = fx / dfx;
        double xNext = x - deltaX;

        result.Steps.Add(new NewtonIteration
        {
          Number = iter,
          X = x,
          FX = fx,
          DFX = dfx,
          D2FX = SecondDerivative(func, x),
          DeltaX = deltaX
        });

        if (Math.Abs(deltaX) < epsilon)
        {
          x = xNext;
          iter++;
          break;
        }

        x = xNext;
        iter++;
      }

      if (iter >= maxIterations)
      {
        throw new Exception($"Метод не сошелся за {maxIterations} итераций.\n" +
                            "Попробуйте изменить x₀.");
      }

      double finalFx = func(x);
      double finalDfx = Derivative(func, x);

      result.Steps.Add(new NewtonIteration
      {
        Number = iter,
        X = x,
        FX = finalFx,
        DFX = finalDfx,
        D2FX = SecondDerivative(func, x),
        DeltaX = 0
      });

      result.XResult = x;
      result.FResult = finalFx;
      result.Iterations = iter;

      return result;
    }
  }
}