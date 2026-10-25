using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using SortingApp.Numeric;

namespace SortingApp.Forms
{
  public class GoldenSectionForm : Form
  {
    // ================== Компоненты ==================
    private MenuStrip menuStrip;
    private TextBox txtA, txtB, txtEpsilon, txtFunction;
    private RadioButton rbMin, rbMax;
    private Label lblResult;
    private Panel graphPanel;

    // Данные
    private Func<double, double> _lastFunc;
    private double _lastA, _lastB;
    private GoldenResult _lastResult;
    private int _currentStep = -1;

    // Критические точки и корни
    private List<CriticalPoint> _criticalPoints = new List<CriticalPoint>();
    private List<RootPoint> _roots = new List<RootPoint>();
    private bool _showRoots = true;

    // Управление графиком
    private bool _isDragging = false;
    private Point _dragStart;
    private float _offsetX = 0, _offsetY = 0;
    private float _zoom = 1.0f;

    // ================== Конструктор ==================
    public GoldenSectionForm()
    {
      InitializeUI();
    }

    // ================== UI ==================
    private void InitializeUI()
    {
      Text = "Метод золотого сечения";
      Width = 1400;
      Height = 800;
      StartPosition = FormStartPosition.CenterScreen;
      MinimumSize = new Size(1100, 700);
      Font = new Font("Segoe UI", 9);
      DoubleBuffered = true;

      // ---------- MenuStrip ----------
      menuStrip = new MenuStrip();

      var fileMenu = new ToolStripMenuItem("Файл");
      fileMenu.DropDownItems.Add("Сохранить график...", null, SaveGraph);
      fileMenu.DropDownItems.Add("Экспорт корней...", null, ExportRoots);
      fileMenu.DropDownItems.Add(new ToolStripSeparator());
      fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

      var actionMenu = new ToolStripMenuItem("Действия");
      actionMenu.DropDownItems.Add("Рассчитать", null, Calculate);
      actionMenu.DropDownItems.Add("Очистить", null, ClearAll);
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Пошаговый просмотр...", null, ShowSteps); // НОВОЕ
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("🔵 Найти все корни (нули)", null, FindAllRoots);
      actionMenu.DropDownItems.Add("Скрыть/показать корни", null, (s, e) =>
      {
        _showRoots = !_showRoots;
        graphPanel.Invalidate();
      });
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Пример 1: x² - 4x + 3 (min)", null, (s, e) => SetExample(1));
      actionMenu.DropDownItems.Add("Пример 2: -x² + 4x - 3 (max)", null, (s, e) => SetExample(2));
      actionMenu.DropDownItems.Add("Пример 3: |x - 2| (корень)", null, (s, e) => SetExample(3));

      var viewMenu = new ToolStripMenuItem("Вид");
      viewMenu.DropDownItems.Add("Сбросить вид (ПКМ)", null, (s, e) => ResetView());

      var helpMenu = new ToolStripMenuItem("Справка");
      helpMenu.DropDownItems.Add("О методе", null, (s, e) =>
          MessageBox.Show(
              "МЕТОД ЗОЛОТОГО СЕЧЕНИЯ\n\n" +
              "Ищет экстремум (min/max) унимодальной функции.\n\n" +
              "Алгоритм:\n" +
              "1. x1 = b - φ(b - a)\n" +
              "2. x2 = a + φ(b - a)\n" +
              "3. Если f(x1) < f(x2) → b = x2, иначе a = x1\n" +
              "4. Повторять, пока (b - a) > ε\n\n" +
              "φ = 0.6180339887 (золотое сечение)\n\n" +
              "МОДУЛЬ:\n" +
              "Можно писать |x - 2| — парсер поймёт.\n" +
              "Поддерживается: + - * / ^ ( ) | |\n" +
              "sin cos tan exp log sqrt abs pi e x",
              "О методе", MessageBoxButtons.OK, MessageBoxIcon.Information));

      menuStrip.Items.Add(fileMenu);
      menuStrip.Items.Add(actionMenu);
      menuStrip.Items.Add(viewMenu);
      menuStrip.Items.Add(helpMenu);
      MainMenuStrip = menuStrip;
      Controls.Add(menuStrip);

      // ---------- Левая панель ввода ----------
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

      AddInputField(inputPanel, "Левая граница a:", ref y, out txtA, "0");
      AddInputField(inputPanel, "Правая граница b:", ref y, out txtB, "5");
      AddInputField(inputPanel, "Точность ε:", ref y, out txtEpsilon, "0.001");

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
        Text = "x*x - 4*x + 3",
        Font = new Font("Consolas", 10)
      };
      inputPanel.Controls.Add(txtFunction);
      y += 35;

      // Радио-кнопки
      rbMin = new RadioButton
      {
        Text = "Найти минимум",
        Left = 10,
        Top = y,
        Width = 150,
        Checked = true
      };
      inputPanel.Controls.Add(rbMin);

      rbMax = new RadioButton
      {
        Text = "Найти максимум",
        Left = 165,
        Top = y,
        Width = 150
      };
      inputPanel.Controls.Add(rbMax);
      y += 30;

      var lblHint = new Label
      {
        Text = "Поддерживается:\n" +
                 "+ - * / ^ ( ) | |\n" +
                 "sin cos tan exp log sqrt abs\n" +
                 "pi, e, x\n\n" +
                 "Пример модуля: |x - 2|",
        Left = 10,
        Top = y,
        Width = 300,
        Height = 95,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 8, FontStyle.Italic)
      };
      inputPanel.Controls.Add(lblHint);
      y += 105;

      // Панель результата
      var resultPanel = new Panel
      {
        Left = 10,
        Top = y,
        Width = 295,
        Height = 180,
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
        Height = 140,
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
      _criticalPoints.Clear();
      _roots.Clear();

      if (!TryParseDouble(txtA.Text, out double a)) { ShowError("Параметр 'a' — не число!"); return; }
      if (!TryParseDouble(txtB.Text, out double b)) { ShowError("Параметр 'b' — не число!"); return; }
      if (a >= b) { ShowError("Должно быть a < b!"); return; }
      if (!TryParseDouble(txtEpsilon.Text, out double eps)) { ShowError("Параметр 'ε' — не число!"); return; }
      if (eps <= 0) { ShowError("Точность ε должна быть > 0!"); return; }

      FunctionParser parser;
      try
      {
        parser = new FunctionParser(txtFunction.Text);
        double test = parser.Evaluate((a + b) / 2);
        if (double.IsNaN(test) || double.IsInfinity(test))
          throw new Exception("Функция не определена на отрезке");
      }
      catch (Exception ex) { ShowError($"Ошибка в формуле: {ex.Message}"); return; }

      Func<double, double> func = x => parser.Evaluate(x);
      bool findMax = rbMax.Checked;

      try
      {
        _lastFunc = func;
        _lastA = a;
        _lastB = b;
        _lastResult = GoldenSectionMethod.FindExtremum(func, a, b, eps, findMax);
        _currentStep = _lastResult.Steps.Count - 1;

        _offsetX = 0;
        _offsetY = 0;
        _zoom = 1.0f;

        lblResult.Text =
            $"РЕЗУЛЬТАТ ({(_lastResult.IsMaximum ? "МАКСИМУМ" : "МИНИМУМ")}):\n" +
            $"x* = {_lastResult.XExtremum:F6}\n" +
            $"f(x*) = {_lastResult.FExtremum:F6}\n" +
            $"Итераций: {_lastResult.Iterations}\n" +
            $"Точность: {_lastResult.FinalInterval:E3}";

        graphPanel.Invalidate();
        this.Text = "Метод золотого сечения — Расчёт завершён";
      }
      catch (NotUnimodalException ex)
      {
        var result = MessageBox.Show(
            $"❌ ОШИБКА: ФУНКЦИЯ НЕ УНИМОДАЛЬНА!\n\n" +
            ex.Message + "\n\n" +
            $"Хотите увидеть все критические точки на графике?",
            "Функция не унимодальна", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

        if (result == DialogResult.Yes)
        {
          _lastFunc = func;
          _lastA = a;
          _lastB = b;
          _criticalPoints = ex.AllCriticalPoints != null ? new List<CriticalPoint>(ex.AllCriticalPoints) : new List<CriticalPoint>();

          _lastResult = new GoldenResult
          {
            XExtremum = (a + b) / 2,
            FExtremum = func((a + b) / 2),
            Iterations = 0,
            FinalInterval = b - a,
            Steps = new List<GoldenIteration>
                        {
                            new GoldenIteration { Number = 0, A = a, B = b, X1 = a, X2 = b, F1 = func(a), F2 = func(b) }
                        }
          };
          _currentStep = 0;

          lblResult.Text = $"⚠ Функция немонотонна.\nНайдено: {ex.AllCriticalPoints?.Count ?? 0} критических точек.";
          graphPanel.Invalidate();
        }
      }
      catch (Exception ex)
      {
        ShowError($"Ошибка расчёта:\n{ex.Message}");
      }
    }

    // ================== Поиск корней ==================
    private void FindAllRoots(object sender, EventArgs e)
    {
      _roots.Clear();
      if (!TryParseDouble(txtA.Text, out double a) || !TryParseDouble(txtB.Text, out double b)) { ShowError("Некорректные границы a и b!"); return; }
      if (a >= b) { ShowError("Должно быть a < b!"); return; }

      FunctionParser parser;
      try { parser = new FunctionParser(txtFunction.Text); }
      catch (Exception ex) { ShowError($"Ошибка в формуле: {ex.Message}"); return; }

      Func<double, double> func = x => parser.Evaluate(x);

      try
      {
        _lastFunc = func;
        _lastA = a;
        _lastB = b;
        _roots = RootFinder.FindRoots(func, a, b, 1e-8, 2000);

        if (_lastResult == null)
        {
          _lastResult = new GoldenResult
          {
            XExtremum = (a + b) / 2,
            FExtremum = func((a + b) / 2),
            Iterations = 0,
            FinalInterval = b - a,
            Steps = new List<GoldenIteration>
                        {
                            new GoldenIteration { Number = 0, A = a, B = b, X1 = a, X2 = b, F1 = func(a), F2 = func(b) }
                        }
          };
          _currentStep = 0;
        }

        int negToPos = 0, posToNeg = 0;
        foreach (var r in _roots)
        {
          if (r.Direction > 0) negToPos++;
          else if (r.Direction < 0) posToNeg++;
        }

        lblResult.Text =
            $"КОРНИ ФУНКЦИИ\n" +
            $"Найдено: {_roots.Count}\n" +
            $"↗ (- → +): {negToPos}\n" +
            $"↘ (+ → -): {posToNeg}";

        int showMax = Math.Min(_roots.Count, 6);
        for (int i = 0; i < showMax; i++)
        {
          string arrow = _roots[i].Direction > 0 ? "↗" : "↘";
          lblResult.Text += $"\n{arrow} x{i + 1} = {_roots[i].X:F4}";
        }
        if (_roots.Count > showMax) lblResult.Text += $"\n... и ещё {_roots.Count - showMax}";

        graphPanel.Invalidate();
        this.Text = "Метод золотого сечения — Корни найдены";
      }
      catch (Exception ex) { ShowError($"Ошибка поиска корней:\n{ex.Message}"); }
    }

    // ================== Пошаговый просмотр ==================
    private void ShowSteps(object sender, EventArgs e)
    {
      if (_lastResult == null || _lastResult.Steps == null || _lastResult.Steps.Count == 0)
      {
        MessageBox.Show("Сначала выполните расчёт.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var stepsForm = new StepsForm(_lastResult, _lastResult.IsMaximum))
      {
        stepsForm.ShowDialog(this);
      }
    }

    // ================== Очистка ==================
    private void ClearAll(object sender, EventArgs e)
    {
      if (MessageBox.Show("Очистить все данные?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

      txtA.Text = "0"; txtB.Text = "5"; txtEpsilon.Text = "0.001";
      txtFunction.Text = "x*x - 4*x + 3";
      lblResult.Text = "—";
      _lastFunc = null;
      _lastResult = null;
      _currentStep = -1;
      _criticalPoints.Clear();
      _roots.Clear();
      _offsetX = 0; _offsetY = 0; _zoom = 1.0f;

      graphPanel.Invalidate();
    }

    // ================== Примеры ==================
    private void SetExample(int num)
    {
      switch (num)
      {
        case 1:
          txtA.Text = "0"; txtB.Text = "5"; txtEpsilon.Text = "0.001"; txtFunction.Text = "x*x - 4*x + 3"; rbMin.Checked = true; break;
        case 2:
          txtA.Text = "0"; txtB.Text = "5"; txtEpsilon.Text = "0.001"; txtFunction.Text = "-x*x + 4*x - 3"; rbMax.Checked = true; break;
        case 3:
          txtA.Text = "0"; txtB.Text = "5"; txtEpsilon.Text = "0.001"; txtFunction.Text = "|x - 2|"; rbMin.Checked = true; break;
      }
      Calculate(null, EventArgs.Empty);
    }

    // ================== Сохранение ==================
    private void SaveGraph(object sender, EventArgs e)
    {
      using (var sfd = new SaveFileDialog())
      {
        sfd.Filter = "PNG|*.png";
        sfd.FileName = "golden_section_graph";
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
          var bmp = new Bitmap(graphPanel.Width, graphPanel.Height);
          graphPanel.DrawToBitmap(bmp, new Rectangle(0, 0, graphPanel.Width, graphPanel.Height));
          bmp.Save(sfd.FileName, ImageFormat.Png);
          MessageBox.Show("График сохранён!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
      }
    }

    private void ExportRoots(object sender, EventArgs e)
    {
      if (_roots == null || _roots.Count == 0)
      {
        MessageBox.Show("Нет корней для экспорта.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var sfd = new SaveFileDialog())
      {
        sfd.Filter = "CSV|*.csv";
        sfd.FileName = "roots";

        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
          using (var writer = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
          {
            writer.WriteLine("№;X;F(X);Направление");
            for (int i = 0; i < _roots.Count; i++)
            {
              var r = _roots[i];
              string dir = r.Direction > 0 ? "(- → +)" : r.Direction < 0 ? "(+ → -)" : "= 0";
              writer.WriteLine($"{i + 1};{r.X:F6};{r.F:F6};{dir}");
            }
          }
          MessageBox.Show("Корни сохранены!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
      }
    }

    // ================== Управление видом ==================
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

    // ================== Отрисовка ==================
    private void GraphPanel_Paint(object sender, PaintEventArgs e)
    {
      var g = e.Graphics;
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

      g.Clear(Color.White);

      int W = graphPanel.Width;
      int H = graphPanel.Height;
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

      var currentStep = _lastResult.Steps[0];
      double xMin = _lastA, xMax = _lastB;
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

      // Сетка
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

      // Оси
      using (var axisPen = new Pen(Color.Black, 2))
      using (var arrowPen = new Pen(Color.Black, 2))
      {
        if (viewYMin <= 0 && viewYMax >= 0)
        {
          float y0 = toPy(0);
          g.DrawLine(axisPen, padLeft, y0, padLeft + plotW, y0);
          g.DrawLine(arrowPen, padLeft + plotW - 12, y0 - 6, padLeft + plotW, y0);
          g.DrawLine(arrowPen, padLeft + plotW - 12, y0 + 6, padLeft + plotW, y0);
        }
        if (viewXMin <= 0 && viewXMax >= 0)
        {
          float x0 = toPx(0);
          g.DrawLine(axisPen, x0, padTop, x0, padTop + plotH);
          g.DrawLine(arrowPen, x0 - 6, padTop + 12, x0, padTop);
          g.DrawLine(arrowPen, x0 + 6, padTop + 12, x0, padTop);
        }
      }

      // Функция
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

      // Точки x1, x2
      bool isFinal = (_lastResult.Steps.Count <= 1);
      if (!isFinal && _currentStep < _lastResult.Steps.Count - 1)
      {
        using (var brush = new SolidBrush(Color.Orange))
        {
          g.FillEllipse(brush, toPx(currentStep.X1) - 4, toPy(currentStep.F1) - 4, 8, 8);
          g.FillEllipse(brush, toPx(currentStep.X2) - 4, toPy(currentStep.F2) - 4, 8, 8);
        }
      }

      // Точка экстремума
      float pxE = toPx(_lastResult.XExtremum);
      float pyE = toPy(_lastResult.FExtremum);
      Color pointColor = _lastResult.IsMaximum ? Color.Blue : Color.Red;

      using (var glow = new SolidBrush(Color.FromArgb(60, pointColor)))
        g.FillEllipse(glow, pxE - 15, pyE - 15, 30, 30);

      using (var brush = new SolidBrush(pointColor))
        g.FillEllipse(brush, pxE - 8, pyE - 8, 16, 16);

      using (var pen = new Pen(Color.White, 2))
        g.DrawEllipse(pen, pxE - 8, pyE - 8, 16, 16);

      using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
      using (var brush = new SolidBrush(_lastResult.IsMaximum ? Color.DarkBlue : Color.DarkRed))
      using (var bgBrush = new SolidBrush(Color.FromArgb(220, Color.White)))
      {
        string label = _lastResult.IsMaximum ? "MAX" : "MIN";
        string text = $"{label} ({_lastResult.XExtremum:F4}; {_lastResult.FExtremum:F4})";
        var size = g.MeasureString(text, font);
        g.FillRectangle(bgBrush, pxE + 15, pyE - 25, size.Width + 6, size.Height + 2);
        g.DrawString(text, font, brush, pxE + 18, pyE - 24);
      }

      // Корни
      if (_showRoots && _roots != null && _roots.Count > 0)
      {
        for (int i = 0; i < _roots.Count; i++)
        {
          var root = _roots[i];
          float px = toPx(root.X);
          float py = toPy(0);
          Color rootColor = root.Direction > 0 ? Color.FromArgb(0, 180, 80) : Color.FromArgb(220, 0, 120);

          using (var brush = new SolidBrush(rootColor))
            g.FillEllipse(brush, px - 6, py - 6, 12, 12);

          using (var pen = new Pen(Color.White, 2))
            g.DrawEllipse(pen, px - 6, py - 6, 12, 12);

          using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
          using (var brush = new SolidBrush(rootColor))
          {
            string arrow = root.Direction > 0 ? "↗" : "↘";
            g.DrawString($"{arrow}x{i + 1}", font, brush, px - 12, py + 10);
          }
        }
      }
    }

    // ================== Утилиты ==================
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