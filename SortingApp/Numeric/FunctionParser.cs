using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SortingApp.Numeric
{
  /// <summary>
  /// Простой парсер математических выражений.
  /// Поддерживает: + - * / ^ ( ) sin cos tan exp log sqrt abs pi e
  /// Переменная: x
  /// </summary>
  public class FunctionParser
  {
    private readonly string _expression;
    private int _pos;

    public FunctionParser(string expression)
    {
      if (string.IsNullOrWhiteSpace(expression))
        throw new ArgumentException("Пустое выражение");

      _expression = expression
          .Replace(" ", "")
          .Replace(",", ".")
          .ToLower();
    }

    /// <summary>
    /// Вычисляет значение функции в точке x.
    /// </summary>
    public double Evaluate(double x)
    {
      _pos = 0;
      double result = ParseExpression(x);

      if (_pos < _expression.Length)
        throw new Exception(
            $"Не удалось разобрать выражение около позиции {_pos}");

      return result;
    }

    // expression = term (('+' | '-') term)*
    private double ParseExpression(double x)
    {
      double result = ParseTerm(x);

      while (_pos < _expression.Length)
      {
        char c = _expression[_pos];
        if (c == '+')
        {
          _pos++;
          result += ParseTerm(x);
        }
        else if (c == '-')
        {
          _pos++;
          result -= ParseTerm(x);
        }
        else break;
      }

      return result;
    }

    // term = factor (('*' | '/') factor)*
    private double ParseTerm(double x)
    {
      double result = ParseFactor(x);

      while (_pos < _expression.Length)
      {
        char c = _expression[_pos];
        if (c == '*')
        {
          _pos++;
          result *= ParseFactor(x);
        }
        else if (c == '/')
        {
          _pos++;
          double divisor = ParseFactor(x);
          if (System.Math.Abs(divisor) < 1e-15)
            throw new DivideByZeroException("Деление на ноль");
          result /= divisor;
        }
        else break;
      }

      return result;
    }

    // factor = unary ('^' factor)?
    private double ParseFactor(double x)
    {
      double baseValue = ParseUnary(x);

      if (_pos < _expression.Length && _expression[_pos] == '^')
      {
        _pos++;
        double exponent = ParseFactor(x);
        return System.Math.Pow(baseValue, exponent);
      }

      return baseValue;
    }

    // unary = ('-' | '+')? primary
    private double ParseUnary(double x)
    {
      if (_pos < _expression.Length)
      {
        char c = _expression[_pos];
        if (c == '-')
        {
          _pos++;
          return -ParseUnary(x);
        }
        if (c == '+')
        {
          _pos++;
          return ParseUnary(x);
        }
      }

      return ParsePrimary(x);
    }

    // primary = number | 'x' | '(' expression ')' | func '(' expression ')'
    private double ParsePrimary(double x)
    {
      if (_pos >= _expression.Length)
        throw new Exception("Неожиданный конец выражения");

      char c = _expression[_pos];

      // Скобки
      if (c == '(')
      {
        _pos++;
        double result = ParseExpression(x);
        if (_pos >= _expression.Length || _expression[_pos] != ')')
          throw new Exception("Не закрыта скобка");
        _pos++;
        return result;
      }

      // Переменная x
      if (c == 'x')
      {
        _pos++;
        return x;
      }

      // Константы
      if (c == 'p' && MatchWord("pi"))
        return System.Math.PI;
      if (c == 'e')
      {
        // Проверяем, что это 'e' как константа, а не начало 'exp'
        if (_pos + 1 < _expression.Length &&
            char.IsLetter(_expression[_pos + 1]) &&
            _expression.Substring(_pos).StartsWith("exp"))
        {
          // не константа e, а функция exp
        }
        else
        {
          _pos++;
          return System.Math.E;
        }
      }

      // Функции
      if (MatchWord("sin")) return ParseFunctionArg(x, System.Math.Sin);
      if (MatchWord("cos")) return ParseFunctionArg(x, System.Math.Cos);
      if (MatchWord("tan")) return ParseFunctionArg(x, System.Math.Tan);
      if (MatchWord("exp")) return ParseFunctionArg(x, System.Math.Exp);
      if (MatchWord("log")) return ParseFunctionArg(x, System.Math.Log);
      if (MatchWord("sqrt")) return ParseFunctionArg(x, System.Math.Sqrt);
      if (MatchWord("abs")) return ParseFunctionArg(x, System.Math.Abs);

      // Число
      if (char.IsDigit(c) || c == '.')
        return ParseNumber();

      throw new Exception($"Неизвестный символ: '{c}'");
    }

    private bool MatchWord(string word)
    {
      if (_pos + word.Length > _expression.Length)
        return false;
      if (_expression.Substring(_pos, word.Length) != word)
        return false;

      _pos += word.Length;
      return true;
    }

    private double ParseFunctionArg(double x, Func<double, double> func)
    {
      // Ожидаем '('
      if (_pos >= _expression.Length || _expression[_pos] != '(')
        throw new Exception("Ожидалась '(' после функции");

      _pos++;
      double arg = ParseExpression(x);

      if (_pos >= _expression.Length || _expression[_pos] != ')')
        throw new Exception("Не закрыта скобка функции");

      _pos++;

      return func(arg);
    }

    private double ParseNumber()
    {
      int start = _pos;

      while (_pos < _expression.Length &&
             (char.IsDigit(_expression[_pos]) || _expression[_pos] == '.'))
      {
        _pos++;
      }

      string numStr = _expression.Substring(start, _pos - start);

      if (!double.TryParse(numStr, NumberStyles.Any,
          CultureInfo.InvariantCulture, out double result))
      {
        throw new Exception($"Некорректное число: '{numStr}'");
      }

      return result;
    }
  }
}