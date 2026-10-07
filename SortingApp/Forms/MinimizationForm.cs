using SortingApp.Numeric;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace SortingApp.Forms
{
  public class MinimizationForm : Form
  {
    // ================== Компоненты ==================
    private MenuStrip menuStrip;

    private TextBox txtA;
    private TextBox txtB;
    private TextBox txtEpsilon;
    private TextBox txtFunction;

    private Label lblResult;
    private Panel graphPanel;

    private Button btnExample1;
    private Button btnExample2;
    private Button btnExample3;

    // Данные последнего расчёта
    private Func<double, double> _lastFunc;
    private double _lastA, _lastB;
    private DichotomyResult _lastResult;

    // ================== Конструктор ==================
    public MinimizationForm()
    {
      InitializeUI();
    }

    // ================== Построение интерфейса ==================
    private void InitializeUI()
    {
      Text = "Поиск минимума методом дихотомии";
      Width = 1200;
      Height = 700;
      StartPosition = FormStartPosition.CenterScreen;
      MinimumSize = new Size(900, 600);
      Font = new Font("Segoe UI", 9);

      // ---------- MenuStrip ----------
      menuStrip = new MenuStrip();

      var fileMenu = new ToolStripMenuItem("Файл");
      fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

      var actionMenu = new ToolStripMenuItem("Действия");
      actionMenu.DropDownItems.Add("Рассчитать", null, Calculate);
      actionMenu.DropDownItems.Add("Очистить", null, ClearAll);
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Пример 1: x² - 4x + 3", null, (s, e) => SetExample(1));
      actionMenu.DropDownItems.Add("Пример 2: sin(x) + x²/10", null, (s, e) => SetExample(2));
      actionMenu.DropDownItems.Add("Пример 3: x⁴ - 3x³ + 2", null, (s, e) => SetExample(3));

      var helpMenu = new ToolStripMenuItem("Справка");
      helpMenu.DropDownItems.Add("О методе", null, (s, e) =>
          MessageBox.Show(
              "Метод половинного деления (дихотомии)\n\n" +
              "Алгоритм:\n" +
              "1. Делим отрезок [a, b] пополам: c = (a+b)/2\n" +
              "2. Считаем f(c - ε/4) и f(c + ε/4)\n" +
              "3. Сужаем отрезок в сторону меньшего значения\n" +
              "4. Повторяем, пока (b - a) > ε\n\n" +
              "Работает для унимодальных функций.",
              "О методе",
              MessageBoxButtons.OK,
              MessageBoxIcon.Information));

      menuStrip.Items.Add(fileMenu);
      menuStrip.Items.Add(actionMenu);
      menuStrip.Items.Add(helpMenu);
      MainMenuStrip = menuStrip;
      Controls.Add(menuStrip);

      // ---------- Панель ввода ----------
      var inputPanel = new Panel
      {
        Left = 10,
        Top = 30,
        Width = 320,
        Height = 620,
        BorderStyle = BorderStyle.FixedSingle,
        Anchor = AnchorStyles.Top | AnchorStyles.Left
      };

      int y = 15;

      // Заголовок
      var lblTitle = new Label
      {
        Text = "Параметры задачи:",
        Left = 10,
        Top = y,
        Width = 280,
        Font = new Font("Segoe UI", 11, FontStyle.Bold)
      };
      inputPanel.Controls.Add(lblTitle);
      y += 35;

      // a
      var lblA = new Label
      {
        Text = "Левая граница a:",
        Left = 10,
        Top = y,
        Width = 130
      };
      inputPanel.Controls.Add(lblA);

      txtA = new TextBox
      {
        Left = 150,
        Top = y - 3,
        Width = 140,
        Text = "0"
      };
      inputPanel.Controls.Add(txtA);
      y += 35;

      // b
      var lblB = new Label
      {
        Text = "Правая граница b:",
        Left = 10,
        Top = y,
        Width = 130
      };
      inputPanel.Controls.Add(lblB);

      txtB = new TextBox
      {
        Left = 150,
        Top = y - 3,
        Width = 140,
        Text = "5"
      };
      inputPanel.Controls.Add(txtB);
      y += 35;

      // ε
      var lblEps = new Label
      {
        Text = "Точность ε:",
        Left = 10,
        Top = y,
        Width = 130
      };
      inputPanel.Controls.Add(lblEps);

      txtEpsilon = new TextBox
      {
        Left = 150,
        Top = y - 3,
        Width = 140,
        Text = "0.001"
      };
      inputPanel.Controls.Add(txtEpsilon);
      y += 35;

      // f(x)
      var lblFunc = new Label
      {
        Text = "Функция f(x):",
        Left = 10,
        Top = y,
        Width = 280
      };
      inputPanel.Controls.Add(lblFunc);
      y += 25;

      txtFunction = new TextBox
      {
        Left = 10,
        Top = y,
        Width = 280,
        Text = "x*x - 4*x + 3"
      };
      inputPanel.Controls.Add(txtFunction);
      y += 35;

      // Подсказка
      var lblHint = new Label
      {
        Text = "Поддерживается:\n" +
                 "  + - * / ^  ( )\n" +
                 "  sin cos tan exp log sqrt abs\n" +
                 "  pi, e, x",
        Left = 10,
        Top = y,
        Width = 280,
        Height = 90,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 8, FontStyle.Italic)
      };
      inputPanel.Controls.Add(lblHint);
      y += 100;

      // Примеры (быстрые кнопки)
      var lblExamples = new Label
      {
        Text = "Примеры:",
        Left = 10,
        Top = y,
        Width = 280,
        Font = new Font("Segoe UI", 9, FontStyle.Bold)
      };
      inputPanel.Controls.Add(lblExamples);
      y += 25;

      btnExample1 = new Button
      {
        Left = 10,
        Top = y,
        Width = 280,
        Height = 25,
        Text = "x² - 4x + 3   (min в x=2)"
      };
      btnExample1.Click += (s, e) => SetExample(1);
      inputPanel.Controls.Add(btnExample1);
      y += 30;

      btnExample2 = new Button
      {
        Left = 10,
        Top = y,
        Width = 280,
        Height = 25,
        Text = "sin(x) + x²/10"
      };
      btnExample2.Click += (s, e) => SetExample(2);
      inputPanel.Controls.Add(btnExample2);
      y += 30;

      btnExample3 = new Button
      {
        Left = 10,
        Top = y,
        Width = 280,
        Height = 25,
        Text = "x⁴ - 3x³ + 2"
      };
      btnExample3.Click += (s, e) => SetExample(3);
      inputPanel.Controls.Add(btnExample3);
      y += 35;

      // Результат
      lblResult = new Label
      {
        Text = "Результат: —",
        Left = 10,
        Top = y,
        Width = 300,
        Height = 100,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.DarkGreen
      };
      inputPanel.Controls.Add(lblResult);

      Controls.Add(inputPanel);

      // ---------- Панель графика ----------
      graphPanel = new Panel
      {
        Left = 340,
        Top = 30,
        Width = 840,
        Height = 620,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
      };
      graphPanel.Paint += GraphPanel_Paint;
      Controls.Add(graphPanel);
    }

    // ================== Меню: Рассчитать ==================
    private void Calculate(object sender, EventArgs e)
    {
      // 1. Парсим a
      if (!TryParseDouble(txtA.Text, out double a))
      {
        MessageBox.Show("Параметр 'a' — не число!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      // 2. Парсим b
      if (!TryParseDouble(txtB.Text, out double b))
      {
        MessageBox.Show("Параметр 'b' — не число!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      // 3. Проверяем порядок
      if (a >= b)
      {
        MessageBox.Show("Должно быть a < b!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      // 4. Парсим ε
      if (!TryParseDouble(txtEpsilon.Text, out double eps))
      {
        MessageBox.Show("Параметр 'ε' — не число!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (eps <= 0)
      {
        MessageBox.Show("Точность ε должна быть > 0!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (eps > (b - a))
      {
        MessageBox.Show("Точность ε должна быть меньше (b - a)!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      // 5. Парсим функцию
      FunctionParser parser;
      try
      {
        parser = new FunctionParser(txtFunction.Text);
        // Проверяем, что функция вычисляется в середине
        double test = parser.Evaluate((a + b) / 2);
        if (double.IsNaN(test) || double.IsInfinity(test))
          throw new Exception("Функция не определена на отрезке");
      }
      catch (Exception ex)
      {
        MessageBox.Show(
            $"Ошибка в формуле: {ex.Message}",
            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      Func<double, double> func = x => parser.Evaluate(x);

      // 6. Запускаем метод дихотомии
      try
      {
        _lastFunc = func;
        _lastA = a;
        _lastB = b;
        _lastResult = DichotomyMethod.FindMinimum(func, a, b, eps);

        // Проверка на достижение maxIterations
        if (_lastResult.FinalInterval > eps * 10)
        {
          MessageBox.Show(
              "Метод не сошёлся за разумное число итераций.\n" +
              "Возможно, функция не унимодальна на отрезке.",
              "Предупреждение",
              MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        lblResult.Text =
            $"Результат:\n" +
            $"x* = {_lastResult.XMin:F6}\n" +
            $"f(x*) = {_lastResult.FMin:F6}\n" +
            $"Итераций: {_lastResult.Iterations}\n" +
            $"Точность достигнута: {_lastResult.FinalInterval:E3}";

        // Перерисовываем график
        graphPanel.Invalidate();
      }
      catch (Exception ex)
      {
        MessageBox.Show(
            $"Ошибка расчёта:\n{ex.Message}",
            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    // ================== Меню: Очистить ==================
    private void ClearAll(object sender, EventArgs e)
    {
      var result = MessageBox.Show(
          "Очистить все поля и график?",
          "Подтверждение",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question);

      if (result != DialogResult.Yes) return;

      txtA.Text = "0";
      txtB.Text = "5";
      txtEpsilon.Text = "0.001";
      txtFunction.Text = "";
      lblResult.Text = "Результат: —";
      _lastFunc = null;
      _lastResult = null;
      graphPanel.Invalidate();
    }

    // ================== Примеры ==================
    private void SetExample(int num)
    {
      switch (num)
      {
        case 1:
          txtA.Text = "0";
          txtB.Text = "5";
          txtEpsilon.Text = "0.001";
          txtFunction.Text = "x*x - 4*x + 3";
          break;
        case 2:
          txtA.Text = "-5";
          txtB.Text = "5";
          txtEpsilon.Text = "0.0001";
          txtFunction.Text = "sin(x) + x*x/10";
          break;
        case 3:
          txtA.Text = "-2";
          txtB.Text = "3";
          txtEpsilon.Text = "0.001";
          txtFunction.Text = "x^4 - 3*x^3 + 2";
          break;
      }
    }

    // ================== Отрисовка графика ==================
    private void GraphPanel_Paint(object sender, PaintEventArgs e)
    {
      var g = e.Graphics;
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.Clear(Color.White);

      int W = graphPanel.Width;
      int H = graphPanel.Height;

      // Отступы
      int padLeft = 60, padRight = 30, padTop = 30, padBottom = 50;
      int plotW = W - padLeft - padRight;
      int plotH = H - padTop - padBottom;

      if (plotW < 50 || plotH < 50) return;

      // Если данных нет — показываем пустой график
      if (_lastFunc == null || _lastResult == null)
      {
        using (var font = new Font("Segoe UI", 11))
        using (var brush = new SolidBrush(Color.Gray))
        {
          g.DrawString("Введите данные и нажмите «Рассчитать»",
              font, brush, padLeft + 20, padTop + 20);
        }
        DrawAxes(g, padLeft, padTop, plotW, plotH, 0, 1, 0, 1);
        return;
      }

      // Определяем диапазон по X
      double xMin = _lastA;
      double xMax = _lastB;

      // Считаем значения функции на 200 точках
      int samples = 400;
      var points = new List<PointF>();
      double yMin = double.MaxValue;
      double yMax = double.MinValue;

      for (int i = 0; i <= samples; i++)
      {
        double x = xMin + (xMax - xMin) * i / samples;
        double y;
        try { y = _lastFunc(x); }
        catch { y = double.NaN; }

        if (!double.IsNaN(y) && !double.IsInfinity(y))
        {
          if (y < yMin) yMin = y;
          if (y > yMax) yMax = y;
        }
      }

      // Если yMin == yMax, расширяем
      if (Math.Abs(yMax - yMin) < 1e-9)
      {
        yMin -= 1;
        yMax += 1;
      }

      // Добавляем 10% отступа по Y
      double yRange = yMax - yMin;
      yMin -= yRange * 0.1;
      yMax += yRange * 0.1;

      // Функция перевода x в пиксели
      Func<double, float> toPx = x =>
          (float)(padLeft + (x - xMin) / (xMax - xMin) * plotW);

      Func<double, float> toPy = y =>
          (float)(padTop + plotH - (y - yMin) / (yMax - yMin) * plotH);

      // Рисуем оси и сетку
      DrawAxes(g, padLeft, padTop, plotW, plotH,
          xMin, xMax, yMin, yMax);

      // Рисуем функцию
      using (var pen = new Pen(Color.SteelBlue, 2))
      {
        PointF? prev = null;

        for (int i = 0; i <= samples; i++)
        {
          double x = xMin + (xMax - xMin) * i / samples;
          double y;
          try { y = _lastFunc(x); }
          catch { y = double.NaN; }

          if (double.IsNaN(y) || double.IsInfinity(y))
          {
            prev = null;
            continue;
          }

          var pt = new PointF(toPx(x), toPy(y));
          if (prev.HasValue)
            g.DrawLine(pen, prev.Value, pt);
          prev = pt;
        }
      }

      // Рисуем точку минимума
      float px = toPx(_lastResult.XMin);
      float py = toPy(_lastResult.FMin);

      using (var brush = new SolidBrush(Color.Red))
        g.FillEllipse(brush, px - 6, py - 6, 12, 12);

      using (var pen = new Pen(Color.Red, 2))
        g.DrawEllipse(pen, px - 8, py - 8, 16, 16);

      // Подпись
      using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
      using (var brush = new SolidBrush(Color.DarkRed))
      {
        string text = $"({_lastResult.XMin:F4}; {_lastResult.FMin:F4})";
        g.DrawString(text, font, brush, px + 12, py - 20);
      }
    }

    // ================== Вспомогательный метод: оси и сетка ==================
    private void DrawAxes(Graphics g, int padLeft, int padTop, int plotW, int plotH,
        double xMin, double xMax, double yMin, double yMax)
    {
      using (var gridPen = new Pen(Color.LightGray, 1))
      using (var axisPen = new Pen(Color.Black, 1.5f))
      using (var font = new Font("Segoe UI", 8))
      using (var brush = new SolidBrush(Color.Black))
      {
        // Вертикальная сетка (10 делений)
        for (int i = 0; i <= 10; i++)
        {
          float x = padLeft + plotW * i / 10f;
          g.DrawLine(gridPen, x, padTop, x, padTop + plotH);

          double xVal = xMin + (xMax - xMin) * i / 10.0;
          string label = xVal.ToString("F2");
          var size = g.MeasureString(label, font);
          g.DrawString(label, font, brush,
              x - size.Width / 2, padTop + plotH + 5);
        }

        // Горизонтальная сетка
        for (int i = 0; i <= 10; i++)
        {
          float y = padTop + plotH * i / 10f;
          g.DrawLine(gridPen, padLeft, y, padLeft + plotW, y);

          double yVal = yMax - (yMax - yMin) * i / 10.0;
          string label = yVal.ToString("F2");
          var size = g.MeasureString(label, font);
          g.DrawString(label, font, brush,
              padLeft - size.Width - 5, y - size.Height / 2);
        }

        // Ось X (основная)
        g.DrawLine(axisPen, padLeft, padTop, padLeft, padTop + plotH);
        // Ось Y
        g.DrawLine(axisPen, padLeft, padTop + plotH, padLeft + plotW, padTop + plotH);

        // Подписи осей
        using (var bFont = new Font("Segoe UI", 9, FontStyle.Bold))
        {
          g.DrawString("x", bFont, brush,
              padLeft + plotW + 5, padTop + plotH - 10);
          g.DrawString("y", bFont, brush, padLeft + 5, padTop - 20);
        }
      }
    }

    // ================== Парсинг double (с защитой от ведущих нулей) ==================
    private bool TryParseDouble(string text, out double value)
    {
      value = 0;
      text = text?.Trim().Replace(',', '.') ?? "";

      if (string.IsNullOrEmpty(text))
        return false;

      // Убираем знак для проверки
      string check = text;
      if (check.StartsWith("-") || check.StartsWith("+"))
        check = check.Substring(1);

      // Запрет ведущих нулей: "0045", "007", "00", "00.5"
      // Разрешаем: "0", "0.5", ".5"
      if (check.Length > 1 && check[0] == '0' && check[1] != '.')
        return false;

      if (!double.TryParse(text,
          NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        return false;

      if (double.IsNaN(value) || double.IsInfinity(value))
        return false;

      return true;
    }
  }
}