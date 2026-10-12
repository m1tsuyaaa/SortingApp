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
  public class MinimizationForm : Form
  {
    // ================== Компоненты ==================
    private MenuStrip menuStrip;

    private TextBox txtA, txtB, txtEpsilon, txtFunction;
    private Label lblResult;

    private Panel graphPanel;
    private bool _isDarkTheme = false;

    // Пошаговый просмотр
    private Panel iterationPanel;
    private TrackBar trkIteration;
    private Label lblIterationInfo;
    private Button btnPrev, btnNext, btnFirst, btnFinal, btnPlay;
    private Timer _animationTimer;
    private bool _isPlaying = false;

    // Данные
    private Func<double, double> _lastFunc;
    private double _lastA, _lastB;
    private DichotomyResult _lastResult;
    private int _currentStep = -1;
    private List<double> _detectedMinima = new List<double>();

    // Управление графиком
    private bool _isDragging = false;
    private Point _dragStart;
    private float _offsetX = 0, _offsetY = 0;
    private float _zoom = 1.0f;

    // ================== Конструктор ==================
    public MinimizationForm()
    {
      InitializeUI();
      SetupAnimation();
    }

    // ================== Построение UI ==================
    private void InitializeUI()
    {
      Text = "Поиск минимума методом дихотомии";
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
      fileMenu.DropDownItems.Add(new ToolStripSeparator());
      fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

      var actionMenu = new ToolStripMenuItem("Действия");
      actionMenu.DropDownItems.Add("Рассчитать", null, Calculate);
      actionMenu.DropDownItems.Add("Очистить", null, ClearAll);
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Пример 1: x² - 4x + 3", null, (s, e) => SetExample(1));
      actionMenu.DropDownItems.Add("Пример 2: sin(x) + x²/10", null, (s, e) => SetExample(2));
      actionMenu.DropDownItems.Add("Пример 3: x⁴ - 3x³ + 2", null, (s, e) => SetExample(3));
      actionMenu.DropDownItems.Add("Пример 4: (x-1)² + sin(2x)", null, (s, e) => SetExample(4));
      actionMenu.DropDownItems.Add("Пример 5: sin(x) [НЕ унимодальная]", null, (s, e) => SetExample(5));

      var viewMenu = new ToolStripMenuItem("Вид");
      viewMenu.DropDownItems.Add("Тёмная тема", null, (s, e) => ToggleTheme());
      viewMenu.DropDownItems.Add(new ToolStripSeparator());
      viewMenu.DropDownItems.Add("Сбросить вид (ПКМ)", null, (s, e) => ResetView());
      viewMenu.DropDownItems.Add("Приблизить", null, (s, e) => ZoomIn());
      viewMenu.DropDownItems.Add("Отдалить", null, (s, e) => ZoomOut());

      var helpMenu = new ToolStripMenuItem("Справка");
      helpMenu.DropDownItems.Add("О методе", null, (s, e) =>
          MessageBox.Show(
              "МЕТОД ПОЛОВИННОГО ДЕЛЕНИЯ (ДИХОТОМИИ)\n\n" +
              "Задача:\n" +
              "Найти локальный минимум f(x) на [a, b] с точностью ε.\n\n" +
              "Алгоритм:\n" +
              "1. c = (a + b) / 2 — середина отрезка\n" +
              "2. x1 = c - ε/4, x2 = c + ε/4\n" +
              "3. Если f(x1) < f(x2) → b = x2\n" +
              "   Иначе → a = x1\n" +
              "4. Повторять, пока (b - a) > ε\n\n" +
              "ВАЖНО:\n" +
              "Метод работает ТОЛЬКО для унимодальных\n" +
              "функций (с одним минимумом на отрезке).\n" +
              "Если на [a, b] несколько минимумов —\n" +
              "появится ошибка с их списком.\n\n" +
              "УПРАВЛЕНИЕ ГРАФИКОМ:\n" +
              "• ЛКМ + движение — сдвиг\n" +
              "• Колёсико — масштаб\n" +
              "• ПКМ — сброс вида\n\n" +
              "ПОШАГОВЫЙ ПРОСМОТР:\n" +
              "• Слайдер внизу слева\n" +
              "• Кнопки ◀◀ / ◀ / ▶ / ▶▶\n" +
              "• Кнопка ▶ Играть — автопроигрывание",
              "О методе",
              MessageBoxButtons.OK,
              MessageBoxIcon.Information));

      menuStrip.Items.Add(fileMenu);
      menuStrip.Items.Add(actionMenu);
      menuStrip.Items.Add(viewMenu);
      menuStrip.Items.Add(helpMenu);
      MainMenuStrip = menuStrip;
      Controls.Add(menuStrip);

      // ---------- Левая панель: ввод ----------
      var inputPanel = new Panel
      {
        Left = 10,
        Top = 30,
        Width = 320,
        Height = 340,
        BorderStyle = BorderStyle.FixedSingle,
        Anchor = AnchorStyles.Top | AnchorStyles.Left
      };

      var lblTitle = new Label
      {
        Text = "ПАРАМЕТРЫ ЗАДАЧИ",
        Left = 10,
        Top = 10,
        Width = 300,
        Height = 22,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 51, 102)
      };
      inputPanel.Controls.Add(lblTitle);

      AddInputField(inputPanel, "Левая граница a:", 45, out txtA, "0");
      AddInputField(inputPanel, "Правая граница b:", 80, out txtB, "5");
      AddInputField(inputPanel, "Точность ε:", 115, out txtEpsilon, "0.001");

      var lblFunc = new Label
      {
        Text = "Функция f(x):",
        Left = 10,
        Top = 150,
        Width = 300,
        Font = new Font("Segoe UI", 9, FontStyle.Bold)
      };
      inputPanel.Controls.Add(lblFunc);

      txtFunction = new TextBox
      {
        Left = 10,
        Top = 172,
        Width = 295,
        Text = "x*x - 4*x + 3",
        Font = new Font("Consolas", 10)
      };
      inputPanel.Controls.Add(txtFunction);

      var lblHint = new Label
      {
        Text = "Поддерживается: + - * / ^ ( )\n" +
                 "sin cos tan exp log sqrt abs\n" +
                 "pi, e, x\n\n" +
                 "⚠ Метод работает только для унимодальных",
        Left = 10,
        Top = 200,
        Width = 300,
        Height = 75,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 8, FontStyle.Italic)
      };
      inputPanel.Controls.Add(lblHint);

      lblResult = new Label
      {
        Text = "Результат: —",
        Left = 10,
        Top = 275,
        Width = 300,
        Height = 55,
        Font = new Font("Consolas", 9, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 100, 0)
      };
      inputPanel.Controls.Add(lblResult);

      Controls.Add(inputPanel);

      // ---------- Панель пошагового просмотра ----------
      iterationPanel = new Panel
      {
        Left = 10,
        Top = 380,
        Width = 320,
        Height = 330,
        BorderStyle = BorderStyle.FixedSingle,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
      };

      var lblIterTitle = new Label
      {
        Text = "ПОШАГОВЫЙ ПРОСМОТР",
        Left = 10,
        Top = 10,
        Width = 300,
        Height = 22,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 51, 102)
      };
      iterationPanel.Controls.Add(lblIterTitle);

      btnFirst = new Button
      {
        Text = "◀◀",
        Left = 10,
        Top = 40,
        Width = 55,
        Height = 32,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Enabled = false
      };
      btnFirst.Click += (s, e) => GotoStep(0);
      iterationPanel.Controls.Add(btnFirst);

      btnPrev = new Button
      {
        Text = "◀",
        Left = 70,
        Top = 40,
        Width = 55,
        Height = 32,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Enabled = false
      };
      btnPrev.Click += (s, e) => StepBackward();
      iterationPanel.Controls.Add(btnPrev);

      btnPlay = new Button
      {
        Text = "▶ Играть",
        Left = 130,
        Top = 40,
        Width = 80,
        Height = 32,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        BackColor = Color.FromArgb(0, 120, 215),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Enabled = false
      };
      btnPlay.FlatAppearance.BorderSize = 0;
      btnPlay.Click += (s, e) => TogglePlay();
      iterationPanel.Controls.Add(btnPlay);

      btnNext = new Button
      {
        Text = "▶",
        Left = 215,
        Top = 40,
        Width = 55,
        Height = 32,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Enabled = false
      };
      btnNext.Click += (s, e) => StepForward();
      iterationPanel.Controls.Add(btnNext);

      btnFinal = new Button
      {
        Text = "▶▶",
        Left = 275,
        Top = 40,
        Width = 35,
        Height = 32,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Enabled = false
      };
      btnFinal.Click += (s, e) => GotoFinal();
      iterationPanel.Controls.Add(btnFinal);

      lblIterationInfo = new Label
      {
        Text = "Итерация: —",
        Left = 10,
        Top = 80,
        Width = 300,
        Height = 180,
        Font = new Font("Consolas", 9),
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.FromArgb(245, 245, 250),
        Padding = new Padding(5)
      };
      iterationPanel.Controls.Add(lblIterationInfo);

      var lblSlider = new Label
      {
        Text = "Итерация:",
        Left = 10,
        Top = 268,
        Width = 60
      };
      iterationPanel.Controls.Add(lblSlider);

      trkIteration = new TrackBar
      {
        Left = 70,
        Top = 262,
        Width = 240,
        Minimum = 0,
        Maximum = 0,
        TickFrequency = 1,
        Enabled = false
      };
      trkIteration.ValueChanged += (s, e) =>
      {
        if (_lastResult == null) return;
        _currentStep = trkIteration.Value;
        UpdateIterationDisplay();
      };
      iterationPanel.Controls.Add(trkIteration);

      Controls.Add(iterationPanel);

      // ---------- График ----------
      graphPanel = new Panel
      {
        Left = 340,
        Top = 30,
        Width = 1030,
        Height = 680,
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

    private void AddInputField(Panel parent, string label, int top, out TextBox textBox, string defaultValue)
    {
      var lbl = new Label
      {
        Text = label,
        Left = 10,
        Top = top,
        Width = 140
      };
      parent.Controls.Add(lbl);

      textBox = new TextBox
      {
        Left = 155,
        Top = top - 3,
        Width = 150,
        Text = defaultValue,
        Font = new Font("Consolas", 9)
      };
      parent.Controls.Add(textBox);
    }

    // ================== Анимация ==================
    private void SetupAnimation()
    {
      _animationTimer = new Timer();
      _animationTimer.Interval = 500;
      _animationTimer.Tick += (s, e) =>
      {
        if (_lastResult == null) { StopPlay(); return; }

        if (_currentStep < _lastResult.Steps.Count - 1)
        {
          _currentStep++;
          trkIteration.Value = _currentStep;
        }
        else
        {
          StopPlay();
        }
      };
    }

    private void TogglePlay()
    {
      if (_isPlaying) StopPlay();
      else StartPlay();
    }

    private void StartPlay()
    {
      if (_lastResult == null) return;

      if (_currentStep >= _lastResult.Steps.Count - 1)
        _currentStep = 0;

      _isPlaying = true;
      btnPlay.Text = "⏸ Пауза";
      btnPlay.BackColor = Color.FromArgb(200, 80, 80);
      _animationTimer.Start();
    }

    private void StopPlay()
    {
      _isPlaying = false;
      btnPlay.Text = "▶ Играть";
      btnPlay.BackColor = Color.FromArgb(0, 120, 215);
      _animationTimer.Stop();
    }

    // ================== Расчёт ==================
    private void Calculate(object sender, EventArgs e)
    {
      // Очищаем список найденных минимумов
      _detectedMinima.Clear();

      if (!TryParseDouble(txtA.Text, out double a))
      {
        ShowError("Параметр 'a' — не число!");
        return;
      }
      if (!TryParseDouble(txtB.Text, out double b))
      {
        ShowError("Параметр 'b' — не число!");
        return;
      }
      if (a >= b)
      {
        ShowError("Должно быть a < b!");
        return;
      }
      if (!TryParseDouble(txtEpsilon.Text, out double eps))
      {
        ShowError("Параметр 'ε' — не число!");
        return;
      }
      if (eps <= 0)
      {
        ShowError("Точность ε должна быть > 0!");
        return;
      }
      if (eps > (b - a))
      {
        ShowError("Точность ε должна быть меньше (b - a)!");
        return;
      }

      FunctionParser parser;
      try
      {
        parser = new FunctionParser(txtFunction.Text);
        double test = parser.Evaluate((a + b) / 2);
        if (double.IsNaN(test) || double.IsInfinity(test))
          throw new Exception("Функция не определена на отрезке");
      }
      catch (Exception ex)
      {
        ShowError($"Ошибка в формуле: {ex.Message}");
        return;
      }

      Func<double, double> func = x => parser.Evaluate(x);

      try
      {
        StopPlay();

        _lastFunc = func;
        _lastA = a;
        _lastB = b;
        _lastResult = DichotomyMethod.FindMinimum(func, a, b, eps);

        _offsetX = 0;
        _offsetY = 0;
        _zoom = 1.0f;

        trkIteration.Minimum = 0;
        trkIteration.Maximum = _lastResult.Steps.Count - 1;
        trkIteration.Value = _lastResult.Steps.Count - 1;
        trkIteration.Enabled = true;
        _currentStep = _lastResult.Steps.Count - 1;

        btnFirst.Enabled = true;
        btnPrev.Enabled = true;
        btnNext.Enabled = true;
        btnFinal.Enabled = true;
        btnPlay.Enabled = true;

        UpdateIterationDisplay();
        graphPanel.Invalidate();

        statusUpdate($"Расчёт завершён. Итераций: {_lastResult.Iterations}");
      }
      catch (NotUnimodalException ex)
      {
        // ============================================================
        // КРАСИВОЕ СООБЩЕНИЕ О НЕУНИМОДАЛЬНОСТИ
        // ============================================================
        string minimaList = "";
        int showCount = Math.Min(ex.LocalMinima.Count, 15);
        for (int i = 0; i < showCount; i++)
        {
          minimaList += $"  • x{i + 1} ≈ {ex.LocalMinima[i]:F4}\n";
        }
        if (ex.LocalMinima.Count > showCount)
        {
          minimaList += $"  ... и ещё {ex.LocalMinima.Count - showCount}\n";
        }

        var result = MessageBox.Show(
            $"❌ ОШИБКА: ФУНКЦИЯ НЕ ЯВЛЯЕТСЯ УНИМОДАЛЬНОЙ!\n\n" +
            $"На интервале [{a}; {b}] найдено " +
            $"{ex.LocalMinimaCount} локальных минимумов:\n\n" +
            $"{minimaList}\n" +
            $"⚠ Метод половинного деления работает ТОЛЬКО\n" +
            $"для функций с ОДНИМ минимумом на отрезке.\n\n" +
            $"Что делать:\n" +
            $"1. Сузьте интервал [a, b] так, чтобы на нём был\n" +
            $"   только ОДИН локальный минимум\n" +
            $"2. Или используйте другой метод оптимизации\n" +
            $"   (метод Монте-Карло, сканирование, золотое сечение)\n\n" +
            $"Хотите увидеть найденные минимумы на графике?",

            "Функция не унимодальна",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error);

        if (result == DialogResult.Yes)
        {
          _lastFunc = func;
          _lastA = a;
          _lastB = b;
          _detectedMinima = new List<double>(ex.LocalMinima);

          // Заглушка для _lastResult, чтобы отрисовка работала
          _lastResult = new DichotomyResult
          {
            XMin = ex.LocalMinima.Count > 0 ? ex.LocalMinima[0] : (a + b) / 2,
            FMin = ex.LocalMinima.Count > 0 ? func(ex.LocalMinima[0]) : func((a + b) / 2),
            Iterations = 0,
            FinalInterval = b - a,
            Steps = new List<DichotomyIteration>
                        {
                            new DichotomyIteration
                            {
                                Number = 0,
                                A = a, B = b,
                                C = (a + b) / 2,
                                X1 = a, X2 = b,
                                F1 = func(a), F2 = func(b)
                            }
                        }
          };
          _currentStep = 0;

          // Сброс UI
          trkIteration.Minimum = 0;
          trkIteration.Maximum = 0;
          trkIteration.Value = 0;
          trkIteration.Enabled = false;
          btnFirst.Enabled = false;
          btnPrev.Enabled = false;
          btnNext.Enabled = false;
          btnFinal.Enabled = false;
          btnPlay.Enabled = false;

          // Сброс вида
          _offsetX = 0;
          _offsetY = 0;
          _zoom = 1.0f;

          UpdateIterationDisplay();
          graphPanel.Invalidate();

          statusUpdate($"⚠ Обнаружено {ex.LocalMinimaCount} минимумов на отрезке");
        }
        else
        {
          // Пользователь отказался смотреть — очищаем
          _detectedMinima.Clear();
          _lastFunc = null;
          _lastResult = null;
          _currentStep = -1;

          trkIteration.Minimum = 0;
          trkIteration.Maximum = 0;
          trkIteration.Value = 0;
          trkIteration.Enabled = false;
          btnFirst.Enabled = false;
          btnPrev.Enabled = false;
          btnNext.Enabled = false;
          btnFinal.Enabled = false;
          btnPlay.Enabled = false;

          lblResult.Text = "Результат: —";
          lblIterationInfo.Text = "Итерация: —";

          graphPanel.Invalidate();
        }
      }
      catch (Exception ex)
      {
        ShowError($"Ошибка расчёта:\n{ex.Message}");
      }
    }

    private void statusUpdate(string text)
    {
      Text = $"Поиск минимума методом дихотомии — {text}";
    }

    private void ShowError(string text)
    {
      MessageBox.Show(text, "Ошибка",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    // ================== Пошаговый просмотр ==================
    private void StepBackward()
    {
      if (_lastResult == null) return;
      if (_currentStep > 0)
      {
        _currentStep--;
        trkIteration.Value = _currentStep;
      }
    }

    private void StepForward()
    {
      if (_lastResult == null) return;
      if (_currentStep < _lastResult.Steps.Count - 1)
      {
        _currentStep++;
        trkIteration.Value = _currentStep;
      }
    }

    private void GotoStep(int step)
    {
      if (_lastResult == null) return;
      _currentStep = Math.Max(0, Math.Min(step, _lastResult.Steps.Count - 1));
      trkIteration.Value = _currentStep;
    }

    private void GotoFinal()
    {
      if (_lastResult == null) return;
      _currentStep = _lastResult.Steps.Count - 1;
      trkIteration.Value = _currentStep;
    }

    private void UpdateIterationDisplay()
    {
      if (_lastResult == null) return;

      // Особый случай: функция немонотонна
      if (_detectedMinima != null && _detectedMinima.Count > 1)
      {
        lblIterationInfo.Text =
            $"⚠ ФУНКЦИЯ НЕМОНОТОННА\n" +
            $"───────────────────\n" +
            $"На отрезке найдено\n" +
            $"{_detectedMinima.Count} локальных минимумов.\n\n" +
            $"Метод дихотомии НЕ ПРИМЕНИМ.\n\n" +
            $"Сузьте интервал [a, b]\n" +
            $"до одного минимума.";

        lblResult.Text =
            $"❌ Метод не применим:\n" +
            $"Найдено {_detectedMinima.Count} минимумов";
        return;
      }

      var step = _lastResult.Steps[
          Math.Max(0, Math.Min(_currentStep, _lastResult.Steps.Count - 1))];

      if (_currentStep == _lastResult.Steps.Count - 1)
      {
        lblIterationInfo.Text =
            $"╔═══ ФИНАЛ ═══╗\n" +
            $"║ x* = {_lastResult.XMin:F6}\n" +
            $"║ f(x*) = {_lastResult.FMin:F6}\n" +
            $"║ Итераций: {_lastResult.Iterations}\n" +
            $"║ Точность: {_lastResult.FinalInterval:E3}\n" +
            $"╚══════════════╝";

        lblResult.Text =
            $"✓ РЕЗУЛЬТАТ:\n" +
            $"x* = {_lastResult.XMin:F6}\n" +
            $"f(x*) = {_lastResult.FMin:F6}\n" +
            $"Итераций: {_lastResult.Iterations}\n" +
            $"Точность: {_lastResult.FinalInterval:E3}";
      }
      else
      {
        string comparison = step.F1 < step.F2
            ? "f(x1) < f(x2) → b = x2"
            : "f(x2) ≤ f(x1) → a = x1";

        lblIterationInfo.Text =
            $"Итерация {step.Number}\n" +
            $"───────────────────\n" +
            $"a  = {step.A:F6}\n" +
            $"b  = {step.B:F6}\n" +
            $"c  = {step.C:F6}\n" +
            $"x1 = {step.X1:F6}\n" +
            $"x2 = {step.X2:F6}\n" +
            $"f(x1) = {step.F1:F6}\n" +
            $"f(x2) = {step.F2:F6}\n" +
            $"Длина: {step.Interval:E3}\n" +
            $"───────────────────\n" +
            $"{comparison}";

        lblResult.Text =
            $"Результат: —\n" +
            $"(текущая итерация: {step.Number})";
      }

      graphPanel.Invalidate();
    }

    // ================== Меню: Очистить ==================
    private void ClearAll(object sender, EventArgs e)
    {
      if (MessageBox.Show("Очистить все данные?", "Подтверждение",
          MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        return;

      StopPlay();

      txtA.Text = "0";
      txtB.Text = "5";
      txtEpsilon.Text = "0.001";
      txtFunction.Text = "x*x - 4*x + 3";
      lblResult.Text = "Результат: —";
      lblIterationInfo.Text = "Итерация: —";
      _lastFunc = null;
      _lastResult = null;
      _currentStep = -1;
      _detectedMinima.Clear();
      _offsetX = 0;
      _offsetY = 0;
      _zoom = 1.0f;

      trkIteration.Minimum = 0;
      trkIteration.Maximum = 0;
      trkIteration.Value = 0;
      trkIteration.Enabled = false;
      btnFirst.Enabled = false;
      btnPrev.Enabled = false;
      btnNext.Enabled = false;
      btnFinal.Enabled = false;
      btnPlay.Enabled = false;

      graphPanel.Invalidate();
    }

    // ================== Примеры ==================
    private void SetExample(int num)
    {
      switch (num)
      {
        case 1:
          txtA.Text = "0"; txtB.Text = "5";
          txtEpsilon.Text = "0.001";
          txtFunction.Text = "x*x - 4*x + 3";
          break;
        case 2:
          txtA.Text = "-5"; txtB.Text = "5";
          txtEpsilon.Text = "0.0001";
          txtFunction.Text = "sin(x) + x*x/10";
          break;
        case 3:
          txtA.Text = "-2"; txtB.Text = "3";
          txtEpsilon.Text = "0.001";
          txtFunction.Text = "x^4 - 3*x^3 + 2";
          break;
        case 4:
          txtA.Text = "-2"; txtB.Text = "3";
          txtEpsilon.Text = "0.0001";
          txtFunction.Text = "(x-1)^2 + sin(2*x)";
          break;
        case 5:
          // НЕунимодальная функция — вызовет ошибку
          txtA.Text = "0"; txtB.Text = "1000";
          txtEpsilon.Text = "0.001";
          txtFunction.Text = "sin(x)";
          break;
      }
      Calculate(null, EventArgs.Empty);
    }

    // ================== Сохранение графика ==================
    private void SaveGraph(object sender, EventArgs e)
    {
      using (var sfd = new SaveFileDialog())
      {
        sfd.Filter = "PNG|*.png";
        sfd.FileName = "dichotomy_graph";
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
          var bmp = new Bitmap(graphPanel.Width, graphPanel.Height);
          graphPanel.DrawToBitmap(bmp, new Rectangle(0, 0,
              graphPanel.Width, graphPanel.Height));
          bmp.Save(sfd.FileName, ImageFormat.Png);
          MessageBox.Show("График сохранён!", "Готово",
              MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка сохранения:\n{ex.Message}",
              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    // ================== Тема ==================
    private void ToggleTheme()
    {
      _isDarkTheme = !_isDarkTheme;
      graphPanel.BackColor = _isDarkTheme
          ? Color.FromArgb(30, 30, 40)
          : Color.White;
      graphPanel.Invalidate();
    }

    // ================== Управление видом ==================
    private void ResetView()
    {
      _offsetX = 0;
      _offsetY = 0;
      _zoom = 1.0f;
      graphPanel.Invalidate();
    }

    private void ZoomIn()
    {
      _zoom *= 1.25f;
      if (_zoom > 50) _zoom = 50;
      graphPanel.Invalidate();
    }

    private void ZoomOut()
    {
      _zoom /= 1.25f;
      if (_zoom < 0.05f) _zoom = 0.05f;
      graphPanel.Invalidate();
    }

    // ================== События мыши ==================
    private void GraphPanel_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        _isDragging = true;
        _dragStart = e.Location;
        graphPanel.Cursor = Cursors.Hand;
      }
      else if (e.Button == MouseButtons.Right)
      {
        ResetView();
      }
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
      if (e.Button == MouseButtons.Left)
      {
        _isDragging = false;
        graphPanel.Cursor = Cursors.SizeAll;
      }
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

      Color bgColor = _isDarkTheme ? Color.FromArgb(30, 30, 40) : Color.White;
      Color fgColor = _isDarkTheme ? Color.White : Color.Black;
      Color gridColor = _isDarkTheme ? Color.FromArgb(60, 60, 80) : Color.FromArgb(230, 230, 230);
      Color axisColor = _isDarkTheme ? Color.White : Color.Black;

      g.Clear(bgColor);

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
          g.DrawString(text, font, brush,
              (W - size.Width) / 2, (H - size.Height) / 2);
        }
        return;
      }

      // Текущая итерация
      int safeStep = Math.Max(0, Math.Min(_currentStep, _lastResult.Steps.Count - 1));
      var currentStep = _lastResult.Steps[safeStep];

      // Диапазоны
      double xMin = _lastA;
      double xMax = _lastB;

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

      if (Math.Abs(yMax - yMin) < 1e-9)
      {
        yMin -= 1;
        yMax += 1;
      }

      double yRange = yMax - yMin;
      yMin -= yRange * 0.1;
      yMax += yRange * 0.1;

      // Zoom и offset
      double xRange = xMax - xMin;
      double centerX = (xMin + xMax) / 2 + _offsetX / (plotW * _zoom) * xRange;
      double centerY = (yMin + yMax) / 2 - _offsetY / (plotH * _zoom) * (yMax - yMin);

      double halfX = xRange / (2 * _zoom);
      double halfY = (yMax - yMin) / (2 * _zoom);

      double viewXMin = centerX - halfX;
      double viewXMax = centerX + halfX;
      double viewYMin = centerY - halfY;
      double viewYMax = centerY + halfY;

      Func<double, float> toPx = x =>
          (float)(padLeft + (x - viewXMin) / (viewXMax - viewXMin) * plotW);
      Func<double, float> toPy = y =>
          (float)(padTop + plotH - (y - viewYMin) / (viewYMax - viewYMin) * plotH);

      // === Сетка ===
      using (var gridPen = new Pen(gridColor, 1))
      {
        for (int i = 0; i <= 10; i++)
        {
          float x = padLeft + plotW * i / 10f;
          g.DrawLine(gridPen, x, padTop, x, padTop + plotH);

          float y = padTop + plotH * i / 10f;
          g.DrawLine(gridPen, padLeft, y, padLeft + plotW, y);
        }
      }

      // === Оси X и Y ===
      using (var axisPen = new Pen(axisColor, 2))
      using (var arrowPen = new Pen(axisColor, 2))
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

      // === Подписи осей ===
      using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
      using (var brush = new SolidBrush(fgColor))
      {
        if (viewYMin <= 0 && viewYMax >= 0)
        {
          float y0 = toPy(0);
          g.DrawString("x", font, brush, padLeft + plotW + 5, y0 - 18);
        }

        if (viewXMin <= 0 && viewXMax >= 0)
        {
          float x0 = toPx(0);
          g.DrawString("y", font, brush, x0 + 8, padTop - 20);
        }

        if (viewXMin <= 0 && viewXMax >= 0 && viewYMin <= 0 && viewYMax >= 0)
        {
          float x0 = toPx(0);
          float y0 = toPy(0);
          g.DrawString("0", font, brush, x0 - 15, y0 + 3);
        }
      }

      // === Деления и подписи ===
      using (var font = new Font("Segoe UI", 8))
      using (var brush = new SolidBrush(fgColor))
      {
        for (int i = 0; i <= 10; i++)
        {
          float x = padLeft + plotW * i / 10f;
          double xVal = viewXMin + (viewXMax - viewXMin) * i / 10.0;
          string label = FormatNumber(xVal);
          var size = g.MeasureString(label, font);
          g.DrawString(label, font, brush,
              x - size.Width / 2, padTop + plotH + 5);

          float y = padTop + plotH * i / 10f;
          double yVal = viewYMax - (viewYMax - viewYMin) * i / 10.0;
          string yLabel = FormatNumber(yVal);
          var ySize = g.MeasureString(yLabel, font);
          g.DrawString(yLabel, font, brush,
              padLeft - ySize.Width - 5, y - ySize.Height / 2);
        }
      }

      // === Текущий отрезок [a, b] ===
      float ax = toPx(currentStep.A);
      float bx = toPx(currentStep.B);
      using (var brush = new SolidBrush(Color.FromArgb(40, Color.DodgerBlue)))
      {
        g.FillRectangle(brush, ax, padTop, bx - ax, plotH);
      }
      using (var pen = new Pen(Color.DodgerBlue, 2) { DashStyle = DashStyle.Dash })
      {
        g.DrawLine(pen, ax, padTop, ax, padTop + plotH);
        g.DrawLine(pen, bx, padTop, bx, padTop + plotH);
      }

      // === Функция ===
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

          float px = toPx(x);
          float py = toPy(y);

          if (py < padTop - 200 || py > padTop + plotH + 200)
          {
            prev = null;
            continue;
          }

          var pt = new PointF(px, py);
          if (prev.HasValue) g.DrawLine(pen, prev.Value, pt);
          prev = pt;
        }
      }

      // ============================================================
      // === ВСЕ НАЙДЕННЫЕ МИНИМУМЫ (если функция немонотонна) ===
      // ============================================================
      if (_detectedMinima != null && _detectedMinima.Count > 1)
      {
        using (var pen = new Pen(Color.Orange, 2))
        using (var brush = new SolidBrush(Color.Orange))
        {
          for (int i = 0; i < _detectedMinima.Count; i++)
          {
            double mx = _detectedMinima[i];
            double my;
            try { my = _lastFunc(mx); }
            catch { continue; }

            float px = toPx(mx);
            float py = toPy(my);

            if (py < padTop - 50 || py > padTop + plotH + 50) continue;

            g.FillEllipse(brush, px - 4, py - 4, 8, 8);
            g.DrawEllipse(pen, px - 5, py - 5, 10, 10);

            using (var font = new Font("Segoe UI", 7, FontStyle.Bold))
            using (var brushTxt = new SolidBrush(Color.DarkOrange))
            {
              g.DrawString($"{i + 1}", font, brushTxt, px - 3, py - 18);
            }
          }
        }

        // Информация сверху
        using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
        using (var brush = new SolidBrush(Color.Red))
        using (var bgBrush = new SolidBrush(Color.FromArgb(240, Color.White)))
        {
          string text = $"⚠ Найдено {_detectedMinima.Count} минимумов — метод неприменим";
          var size = g.MeasureString(text, font);
          g.FillRectangle(bgBrush, padLeft + 10, padTop + 10,
              size.Width + 12, size.Height + 8);
          using (var borderPen = new Pen(Color.Red, 2))
            g.DrawRectangle(borderPen, padLeft + 10, padTop + 10,
                size.Width + 12, size.Height + 8);
          g.DrawString(text, font, brush, padLeft + 16, padTop + 14);
        }
      }
      // ============================================================
      // === ОДИНОЧНЫЙ МИНИМУМ (если функция унимодальна) ===
      // ============================================================
      else
      {
        // Точки x1 и x2 (только на промежуточных итерациях)
        if (_currentStep < _lastResult.Steps.Count - 1)
        {
          float cx = toPx(currentStep.C);
          using (var pen = new Pen(Color.Orange, 1.5f) { DashStyle = DashStyle.Dot })
          {
            g.DrawLine(pen, cx, padTop, cx, padTop + plotH);
          }

          using (var brush = new SolidBrush(Color.Orange))
          {
            g.FillEllipse(brush, toPx(currentStep.X1) - 5,
                toPy(currentStep.F1) - 5, 10, 10);
            g.FillEllipse(brush, toPx(currentStep.X2) - 5,
                toPy(currentStep.F2) - 5, 10, 10);
          }

          using (var pen = new Pen(Color.DarkOrange, 2))
          {
            g.DrawEllipse(pen, toPx(currentStep.X1) - 6,
                toPy(currentStep.F1) - 6, 12, 12);
            g.DrawEllipse(pen, toPx(currentStep.X2) - 6,
                toPy(currentStep.F2) - 6, 12, 12);
          }

          using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
          using (var brush = new SolidBrush(Color.DarkOrange))
          {
            g.DrawString("x1", font, brush,
                toPx(currentStep.X1) - 8, toPy(currentStep.F1) + 8);
            g.DrawString("x2", font, brush,
                toPx(currentStep.X2) + 3, toPy(currentStep.F2) + 8);
          }
        }

        // Точка минимума
        float pxMin = toPx(_lastResult.XMin);
        float pyMin = toPy(_lastResult.FMin);

        using (var glow = new SolidBrush(Color.FromArgb(60, Color.Red)))
          g.FillEllipse(glow, pxMin - 15, pyMin - 15, 30, 30);

        using (var brush = new SolidBrush(Color.Red))
          g.FillEllipse(brush, pxMin - 8, pyMin - 8, 16, 16);

        using (var pen = new Pen(Color.White, 2))
          g.DrawEllipse(pen, pxMin - 8, pyMin - 8, 16, 16);

        using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
        using (var brush = new SolidBrush(Color.DarkRed))
        using (var bgBrush = new SolidBrush(Color.FromArgb(220, Color.White)))
        {
          string text = $"({_lastResult.XMin:F4}; {_lastResult.FMin:F4})";
          var size = g.MeasureString(text, font);
          g.FillRectangle(bgBrush, pxMin + 15, pyMin - 25,
              size.Width + 6, size.Height + 2);
          g.DrawString(text, font, brush, pxMin + 18, pyMin - 24);
        }
      }

      // === Информация внизу ===
      using (var font = new Font("Segoe UI", 9))
      using (var brush = new SolidBrush(fgColor))
      {
        string info = $"Zoom: {_zoom:F2}x  |  ЛКМ — сдвиг  |  Колёсико — масштаб  |  ПКМ — сброс  |  " +
                      $"Итерация: {_currentStep + 1}/{_lastResult.Steps.Count}";
        g.DrawString(info, font, brush, padLeft, H - 25);
      }
    }

    private string FormatNumber(double value)
    {
      if (Math.Abs(value) < 1e-10) return "0";
      if (Math.Abs(value) >= 1000 || Math.Abs(value) < 0.01)
        return value.ToString("E1");
      return value.ToString("F2");
    }

    // ================== Утилиты ==================
    private bool TryParseDouble(string text, out double value)
    {
      value = 0;
      text = text?.Trim().Replace(',', '.') ?? "";
      if (string.IsNullOrEmpty(text)) return false;

      string check = text;
      if (check.StartsWith("-") || check.StartsWith("+"))
        check = check.Substring(1);

      if (check.Length > 1 && check[0] == '0' && check[1] != '.')
        return false;

      if (!double.TryParse(text,
          NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        return false;

      if (double.IsNaN(value) || double.IsInfinity(value))
        return false;

      return true;
    }

    // ================== Горячие клавиши ==================
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == (Keys.Control | Keys.R))
      {
        Calculate(null, EventArgs.Empty);
        return true;
      }
      if (keyData == Keys.Space)
      {
        if (btnPlay.Enabled) TogglePlay();
        return true;
      }
      if (keyData == Keys.Left)
      {
        if (btnPrev.Enabled) StepBackward();
        return true;
      }
      if (keyData == Keys.Right)
      {
        if (btnNext.Enabled) StepForward();
        return true;
      }
      if (keyData == Keys.Home)
      {
        if (btnFirst.Enabled) GotoStep(0);
        return true;
      }
      if (keyData == Keys.End)
      {
        if (btnFinal.Enabled) GotoFinal();
        return true;
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }
  }

  // Расширение для DoubleBuffered
  public static class ControlExtensions
  {
    public static void DoubleBuffered(this Control control, bool enabled)
    {
      var prop = typeof(Control).GetProperty("DoubleBuffered",
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.NonPublic);
      prop?.SetValue(control, enabled, null);
    }
  }
}