using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using SortingApp.Numeric;

namespace SortingApp.Forms
{
  public class NewtonForm : Form
  {
    private MenuStrip menuStrip;
    private TextBox txtX0, txtEpsilon, txtFunction;
    private RadioButton rbMin, rbMax, rbRoot;
    private Label lblResult;
    private Panel graphPanel;

    private Func<double, double> _lastFunc;
    private double _lastX0;
    private NewtonResult _lastResult;
    private int _currentStep = -1;

    private bool _isDragging = false;
    private Point _dragStart;
    private float _offsetX = 0, _offsetY = 0;
    private float _zoom = 1.0f;

    public NewtonForm()
    {
      InitializeUI();
    }

    private void InitializeUI()
    {
      Text = "Метод Ньютона";
      Width = 1400;
      Height = 800;
      StartPosition = FormStartPosition.CenterScreen;
      MinimumSize = new Size(1100, 700);
      Font = new Font("Segoe UI", 9);
      DoubleBuffered = true;

      // ---------- MenuStrip ----------
      menuStrip = new MenuStrip();

      var fileMenu = new ToolStripMenuItem("Файл");
      fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

      var actionMenu = new ToolStripMenuItem("Действия");
      actionMenu.DropDownItems.Add("Рассчитать", null, Calculate);
      actionMenu.DropDownItems.Add("Очистить", null, ClearAll);
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Пошаговый просмотр...", null, ShowSteps);
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Пример 1: x³ - 3x² + 2 (min/max)", null, (s, e) => SetExample(1));
      actionMenu.DropDownItems.Add("Пример 2: x² - 4 (корень)", null, (s, e) => SetExample(2));

      var viewMenu = new ToolStripMenuItem("Вид");
      viewMenu.DropDownItems.Add("Сбросить вид (ПКМ)", null, (s, e) => ResetView());

      var helpMenu = new ToolStripMenuItem("Справка");
      helpMenu.DropDownItems.Add("О методе", null, (s, e) =>
          MessageBox.Show(
              "МЕТОД НЬЮТОНА (касательных)\n\n" +
              "Поиск экстремума:\n" +
              "x_{n+1} = x_n - f'(x_n) / f''(x_n)\n\n" +
              "Поиск корня:\n" +
              "x_{n+1} = x_n - f(x_n) / f'(x_n)\n\n" +
              "Производные вычисляются численно.\n" +
              "Метод быстро сходится, но требует\n" +
              "хорошего начального приближения x0.\n\n" +
              "Поддерживается: + - * / ^ ( ) | |\n" +
              "sin cos tan exp log sqrt abs pi e x",
              "О методе", MessageBoxButtons.OK, MessageBoxIcon.Information));

      menuStrip.Items.Add(fileMenu);
      menuStrip.Items.Add(actionMenu);
      menuStrip.Items.Add(viewMenu);
      menuStrip.Items.Add(helpMenu);
      MainMenuStrip = menuStrip;
      Controls.Add(menuStrip);

      // ---------- Левая панель ----------
      var inputPanel = new Panel
      {
        Left = 10,
        Top = 30,
        Width = 320,
        Height = 720,
        BorderStyle = BorderStyle.FixedSingle,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
      };

      int y = 15;
      var lblTitle = new Label
      {
        Text = "ПАРАМЕТРЫ ЗАДАЧИ",
        Left = 10,
        Top = y,
        Width = 300,
        Height = 22,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 51, 102)
      };
      inputPanel.Controls.Add(lblTitle);
      y += 35;

      AddInputField(inputPanel, "Начальное x₀:", ref y, out txtX0, "1.5");
      AddInputField(inputPanel, "Точность ε:", ref y, out txtEpsilon, "0.0001");

      y += 5;
      var lblFunc = new Label
      {
        Text = "Функция f(x):",
        Left = 10,
        Top = y,
        Width = 300,
        Font = new Font("Segoe UI", 9, FontStyle.Bold)
      };
      inputPanel.Controls.Add(lblFunc);
      y += 22;

      txtFunction = new TextBox
      {
        Left = 10,
        Top = y,
        Width = 295,
        Text = "x^3 - 3*x^2 + 2",
        Font = new Font("Consolas", 10)
      };
      inputPanel.Controls.Add(txtFunction);
      y += 35;

      // Радио-кнопки
      rbMin = new RadioButton { Text = "Минимум", Left = 10, Top = y, Width = 100, Checked = true };
      inputPanel.Controls.Add(rbMin);

      rbMax = new RadioButton { Text = "Максимум", Left = 115, Top = y, Width = 100 };
      inputPanel.Controls.Add(rbMax);

      rbRoot = new RadioButton { Text = "Корень", Left = 220, Top = y, Width = 80 };
      inputPanel.Controls.Add(rbRoot);
      y += 30;

      var lblHint = new Label
      {
        Text = "Поддерживается:\n" +
                 "+ - * / ^ ( ) | |\n" +
                 "sin cos tan exp log sqrt abs\n" +
                 "pi, e, x",
        Left = 10,
        Top = y,
        Width = 300,
        Height = 75,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 8, FontStyle.Italic)
      };
      inputPanel.Controls.Add(lblHint);
      y += 85;

      var resultPanel = new Panel
      {
        Left = 10,
        Top = y,
        Width = 295,
        Height = 200,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.FromArgb(245, 250, 255)
      };

      var lblResTitle = new Label
      {
        Text = "РЕЗУЛЬТАТ",
        Left = 10,
        Top = 8,
        Width = 270,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 51, 102)
      };
      resultPanel.Controls.Add(lblResTitle);

      lblResult = new Label
      {
        Text = "—",
        Left = 10,
        Top = 30,
        Width = 275,
        Height = 160,
        Font = new Font("Consolas", 9),
        ForeColor = Color.FromArgb(0, 100, 0)
      };
      resultPanel.Controls.Add(lblResult);

      inputPanel.Controls.Add(resultPanel);
      Controls.Add(inputPanel);

      // ---------- График ----------
      graphPanel = new Panel
      {
        Left = 340,
        Top = 30,
        Width = 1030,
        Height = 720,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
        Cursor = Cursors.SizeAll
      };
      graphPanel.Paint += GraphPanel_Paint;
      graphPanel.MouseDown += GraphPanel_MouseDown;
      graphPanel.MouseMove += GraphPanel_MouseMove;
      graphPanel.MouseUp += GraphPanel_MouseUp;
      graphPanel.MouseWheel += GraphPanel_MouseWheel;
      graphPanel.DoubleBuffered(true);
      Controls.Add(graphPanel);
    }

    private void AddInputField(Panel parent, string label, ref int y,
        out TextBox textBox, string defaultValue)
    {
      var lbl = new Label { Text = label, Left = 10, Top = y, Width = 140 };
      parent.Controls.Add(lbl);

      textBox = new TextBox
      {
        Left = 155,
        Top = y - 3,
        Width = 150,
        Text = defaultValue,
        Font = new Font("Consolas", 9)
      };
      parent.Controls.Add(textBox);
      y += 35;
    }

    // ================== Расчёт ==================
    private void Calculate(object sender, EventArgs e)
    {
      if (!TryParseDouble(txtX0.Text, out double x0)) { ShowError("x₀ — не число!"); return; }
      if (!TryParseDouble(txtEpsilon.Text, out double eps)) { ShowError("ε — не число!"); return; }
      if (eps <= 0) { ShowError("Точность ε должна быть > 0!"); return; }

      FunctionParser parser;
      try
      {
        parser = new FunctionParser(txtFunction.Text);
        parser.Evaluate(x0);
      }
      catch (Exception ex) { ShowError($"Ошибка в формуле: {ex.Message}"); return; }

      Func<double, double> func = x => parser.Evaluate(x);

      try
      {
        _lastFunc = func;
        _lastX0 = x0;

        if (rbRoot.Checked)
          _lastResult = NewtonMethod.FindRoot(func, x0, eps);
        else if (rbMax.Checked)
          _lastResult = NewtonMethod.FindExtremum(func, x0, eps, true);
        else
          _lastResult = NewtonMethod.FindExtremum(func, x0, eps, false);

        _currentStep = _lastResult.Steps.Count - 1;

        _offsetX = 0;
        _offsetY = 0;
        _zoom = 1.0f;

        string modeStr = _lastResult.IsRoot ? "КОРЕНЬ" : (_lastResult.IsMaximum ? "МАКСИМУМ" : "МИНИМУМ");

        lblResult.Text =
            $"РЕЗУЛЬТАТ ({modeStr}):\n" +
            $"x* = {_lastResult.XResult:F8}\n" +
            $"f(x*) = {_lastResult.FResult:F8}\n" +
            $"Итераций: {_lastResult.Iterations}";

        graphPanel.Invalidate();
        Text = "Метод Ньютона — Расчёт завершён";
      }
      catch (Exception ex)
      {
        ShowError($"Ошибка расчёта:\n{ex.Message}");
      }
    }

    private void ShowSteps(object sender, EventArgs e)
    {
      if (_lastResult == null || _lastResult.Steps.Count == 0)
      {
        MessageBox.Show("Сначала выполните расчёт.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var stepsForm = new StepsFormNewton(_lastResult))
      {
        stepsForm.ShowDialog(this);
      }
    }

    private void ClearAll(object sender, EventArgs e)
    {
      if (MessageBox.Show("Очистить все данные?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

      txtX0.Text = "1.5"; txtEpsilon.Text = "0.0001"; txtFunction.Text = "x^3 - 3*x^2 + 2";
      lblResult.Text = "—";
      _lastFunc = null;
      _lastResult = null;
      _currentStep = -1;
      _offsetX = 0; _offsetY = 0; _zoom = 1.0f;

      graphPanel.Invalidate();
      Text = "Метод Ньютона";
    }

    private void SetExample(int num)
    {
      switch (num)
      {
        case 1:
          txtX0.Text = "1.5"; txtEpsilon.Text = "0.0001"; txtFunction.Text = "x^3 - 3*x^2 + 2"; rbMin.Checked = true; break;
        case 2:
          txtX0.Text = "1"; txtEpsilon.Text = "0.0001"; txtFunction.Text = "x^2 - 4"; rbRoot.Checked = true; break;
      }
      Calculate(null, EventArgs.Empty);
    }

    private void ResetView() { _offsetX = 0; _offsetY = 0; _zoom = 1.0f; graphPanel.Invalidate(); }

    private void GraphPanel_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left) { _isDragging = true; _dragStart = e.Location; graphPanel.Cursor = Cursors.Hand; }
      else if (e.Button == MouseButtons.Right) ResetView();
    }

    private void GraphPanel_MouseMove(object sender, MouseEventArgs e)
    {
      if (!_isDragging) return;
      _offsetX += e.X - _dragStart.X;
      _offsetY += e.Y - _dragStart.Y;
      _dragStart = e.Location;
      graphPanel.Invalidate();
    }

    private void GraphPanel_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left) { _isDragging = false; graphPanel.Cursor = Cursors.SizeAll; }
    }

    private void GraphPanel_MouseWheel(object sender, MouseEventArgs e)
    {
      float oldZoom = _zoom;
      if (e.Delta > 0) _zoom *= 1.15f;
      else _zoom /= 1.15f;
      if (_zoom > 50) _zoom = 50;
      if (_zoom < 0.05f) _zoom = 0.05f;
      float factor = _zoom / oldZoom;
      _offsetX = (_offsetX - e.X) * factor + e.X;
      _offsetY = (_offsetY - e.Y) * factor + e.Y;
      graphPanel.Invalidate();
    }

    private void GraphPanel_Paint(object sender, PaintEventArgs e)
    {
      var g = e.Graphics;
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.Clear(Color.White);

      int W = graphPanel.Width, H = graphPanel.Height;
      int padLeft = 70, padRight = 40, padTop = 40, padBottom = 60;
      int plotW = W - padLeft - padRight;
      int plotH = H - padTop - padBottom;

      if (plotW < 50 || plotH < 50) return;

      if (_lastFunc == null || _lastResult == null)
      {
        using (var font = new Font("Segoe UI", 12))
        using (var brush = new SolidBrush(Color.Gray))
        {
          var text = "Введите параметры и нажмите «Рассчитать»";
          var size = g.MeasureString(text, font);
          g.DrawString(text, font, brush, (W - size.Width) / 2, (H - size.Height) / 2);
        }
        return;
      }

      double xMin = _lastX0 - 2, xMax = _lastX0 + 2;
      int samples = 600;
      double yMin = double.MaxValue, yMax = double.MinValue;

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

      if (Math.Abs(yMax - yMin) < 1e-9) { yMin -= 1; yMax += 1; }
      double yRange = yMax - yMin;
      yMin -= yRange * 0.1;
      yMax += yRange * 0.1;

      double xRange = xMax - xMin;
      double centerX = (xMin + xMax) / 2 + _offsetX / (plotW * _zoom) * xRange;
      double centerY = (yMin + yMax) / 2 - _offsetY / (plotH * _zoom) * (yMax - yMin);
      double halfX = xRange / (2 * _zoom);
      double halfY = (yMax - yMin) / (2 * _zoom);

      double viewXMin = centerX - halfX;
      double viewXMax = centerX + halfX;
      double viewYMin = centerY - halfY;
      double viewYMax = centerY + halfY;

      Func<double, float> toPx = x => (float)(padLeft + (x - viewXMin) / (viewXMax - viewXMin) * plotW);
      Func<double, float> toPy = y => (float)(padTop + plotH - (y - viewYMin) / (viewYMax - viewYMin) * plotH);

      using (var gridPen = new Pen(Color.FromArgb(230, 230, 230), 1))
      {
        for (int i = 0; i <= 10; i++)
        {
          float x = padLeft + plotW * i / 10f;
          g.DrawLine(gridPen, x, padTop, x, padTop + plotH);
          float y = padTop + plotH * i / 10f;
          g.DrawLine(gridPen, padLeft, y, padLeft + plotW, y);
        }
      }

      using (var axisPen = new Pen(Color.Black, 2))
      {
        if (viewYMin <= 0 && viewYMax >= 0)
          g.DrawLine(axisPen, padLeft, toPy(0), padLeft + plotW, toPy(0));
        if (viewXMin <= 0 && viewXMax >= 0)
          g.DrawLine(axisPen, toPx(0), padTop, toPx(0), padTop + plotH);
      }

      using (var pen = new Pen(Color.FromArgb(70, 130, 200), 2.5f))
      {
        PointF? prev = null;
        for (int i = 0; i <= samples; i++)
        {
          double x = viewXMin + (viewXMax - viewXMin) * i / samples;
          double y;
          try { y = _lastFunc(x); }
          catch { y = double.NaN; }

          if (double.IsNaN(y) || double.IsInfinity(y)) { prev = null; continue; }
          float px = toPx(x), py = toPy(y);
          if (py < padTop - 200 || py > padTop + plotH + 200) { prev = null; continue; }

          var pt = new PointF(px, py);
          if (prev.HasValue) g.DrawLine(pen, prev.Value, pt);
          prev = pt;
        }
      }

      // Точка результата
      float pxE = toPx(_lastResult.XResult);
      float pyE = toPy(_lastResult.FResult);
      Color pointColor = _lastResult.IsRoot ? Color.Green : (_lastResult.IsMaximum ? Color.Blue : Color.Red);

      using (var glow = new SolidBrush(Color.FromArgb(60, pointColor)))
        g.FillEllipse(glow, pxE - 15, pyE - 15, 30, 30);

      using (var brush = new SolidBrush(pointColor))
        g.FillEllipse(brush, pxE - 8, pyE - 8, 16, 16);

      using (var pen = new Pen(Color.White, 2))
        g.DrawEllipse(pen, pxE - 8, pyE - 8, 16, 16);

      using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
      using (var brush = new SolidBrush(_lastResult.IsRoot ? Color.DarkGreen : (_lastResult.IsMaximum ? Color.DarkBlue : Color.DarkRed)))
      using (var bgBrush = new SolidBrush(Color.FromArgb(220, Color.White)))
      {
        string label = _lastResult.IsRoot ? "ROOT" : (_lastResult.IsMaximum ? "MAX" : "MIN");
        string text = $"{label} ({_lastResult.XResult:F4}; {_lastResult.FResult:F4})";
        var size = g.MeasureString(text, font);
        g.FillRectangle(bgBrush, pxE + 15, pyE - 25, size.Width + 6, size.Height + 2);
        g.DrawString(text, font, brush, pxE + 18, pyE - 24);
      }
    }

    private bool TryParseDouble(string text, out double value)
    {
      value = 0;
      text = text?.Trim().Replace(',', '.') ?? "";
      if (string.IsNullOrEmpty(text)) return false;
      string check = text;
      if (check.StartsWith("-") || check.StartsWith("+")) check = check.Substring(1);
      if (check.Length > 1 && check[0] == '0' && check[1] != '.') return false;
      if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return false;
      if (double.IsNaN(value) || double.IsInfinity(value)) return false;
      return true;
    }

    private void ShowError(string text)
    {
      MessageBox.Show(text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == (Keys.Control | Keys.R)) { Calculate(null, EventArgs.Empty); return true; }
      return base.ProcessCmdKey(ref msg, keyData);
    }
  }
}