using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SortingApp.Algorithms;
using SortingApp.Models;
using SortingApp.Services;
using SortingApp.Visualization;

namespace SortingApp.Forms
{
  public class MainForm : Form
  {
    // ================== Компоненты UI ==================
    private MenuStrip menuStrip;
    private DataGridView dataGridViewInput;
    private DataGridView dataGridViewResults;
    private Panel visualizationPanel;
    private Panel algorithmPanel;
    private CheckBox chkBubble, chkShaker, chkInsertion, chkQuick, chkBogo;
    private RadioButton rbAscending, rbDescending;
    private TextBox txtBogoLimit;
    private CheckBox chkDetailedVisualization;
    private CheckBox chkIntegerOnly;
    private CheckBox chkAnimate;
    private TrackBar trkAnimationSpeed;
    private Label lblAnimationSpeed;
    private Label lblFastest;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ProgressBar progressBar;

    // ================== Данные ==================
    private List<double> currentData = new List<double>();
    private List<ISortAlgorithm> algorithms;
    private SortVisualizer visualizer;

    // ================== Конструктор ==================
    public MainForm()
    {
      InitializeUI();
      InitializeAlgorithms();
      visualizer = new SortVisualizer(visualizationPanel);
    }

    private void InitializeAlgorithms()
    {
      algorithms = new List<ISortAlgorithm>
            {
                new BubbleSort(),
                new ShakerSort(),
                new InsertionSort(),
                new QuickSort(),
                new BogoSort()
            };
    }

    // ================== Построение интерфейса ==================
    private void InitializeUI()
    {
      Text = "Сортировка данных — WinForms";
      Width = 1280;
      Height = 800;
      StartPosition = FormStartPosition.CenterScreen;
      MinimumSize = new Size(1000, 600);
      Font = new Font("Segoe UI", 9);

      // ---------- MenuStrip ----------
      menuStrip = new MenuStrip();

      var fileMenu = new ToolStripMenuItem("Файл");
      fileMenu.DropDownItems.Add("Загрузить из Excel...", null, LoadFromExcel);
      fileMenu.DropDownItems.Add("Загрузить из Google Table...", null, LoadFromGoogle);
      fileMenu.DropDownItems.Add(new ToolStripSeparator());
      fileMenu.DropDownItems.Add("Сохранить результаты...", null, SaveResults);
      fileMenu.DropDownItems.Add(new ToolStripSeparator());
      fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

      var actionMenu = new ToolStripMenuItem("Действия");
      actionMenu.DropDownItems.Add("Сгенерировать данные...", null, GenerateData);
      actionMenu.DropDownItems.Add("Рассчитать", null, RunSorting);
      actionMenu.DropDownItems.Add("Очистить", null, ClearAll);
      actionMenu.DropDownItems.Add(new ToolStripSeparator());
      actionMenu.DropDownItems.Add("Краш-тест (51234)", null, RunStressTest);

      var settingsMenu = new ToolStripMenuItem("Настройки");
      settingsMenu.DropDownItems.Add("Переключить режим визуализации", null,
          (s, e) => chkDetailedVisualization.Checked = !chkDetailedVisualization.Checked);

      var helpMenu = new ToolStripMenuItem("Справка");
      helpMenu.DropDownItems.Add("О программе", null, (s, e) =>
          MessageBox.Show(
              "Приложение сортировки данных\nWinForms, C#\n5 алгоритмов",
              "О программе",
              MessageBoxButtons.OK,
              MessageBoxIcon.Information));

      menuStrip.Items.Add(fileMenu);
      menuStrip.Items.Add(actionMenu);
      menuStrip.Items.Add(settingsMenu);
      menuStrip.Items.Add(helpMenu);
      MainMenuStrip = menuStrip;
      Controls.Add(menuStrip);

      // ---------- Левая панель: ввод ----------
      var leftPanel = new Panel
      {
        Left = 10,
        Top = 30,
        Width = 400,
        Height = 380,
        Anchor = AnchorStyles.Top | AnchorStyles.Left
      };

      var lblInput = new Label
      {
        Text = "Входные данные:",
        Left = 0,
        Top = 0,
        Width = 200,
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
      };

      dataGridViewInput = new DataGridView
      {
        Left = 0,
        Top = 25,
        Width = 400,
        Height = 350,
        AllowUserToAddRows = true,
        AllowUserToDeleteRows = true,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
      };
      dataGridViewInput.Columns.Add("Value", "Значение");
      dataGridViewInput.CellValidating += DataGridViewInput_CellValidating;

      leftPanel.Controls.Add(lblInput);
      leftPanel.Controls.Add(dataGridViewInput);
      Controls.Add(leftPanel);

      // ---------- Панель алгоритмов и настроек ----------
      algorithmPanel = new Panel
      {
        Left = 430,
        Top = 30,
        Width = 250,
        Height = 380,
        BorderStyle = BorderStyle.FixedSingle,
        Anchor = AnchorStyles.Top | AnchorStyles.Left,
        AutoScroll = true
      };

      int y = 10;

      var lblAlg = new Label
      {
        Text = "Алгоритмы:",
        Left = 10,
        Top = y,
        Width = 200,
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
      };
      algorithmPanel.Controls.Add(lblAlg);
      y += 30;

      chkBubble = CreateAlgorithmCheckBox("Пузырьковая", ref y);
      chkShaker = CreateAlgorithmCheckBox("Шейкерная", ref y);
      chkInsertion = CreateAlgorithmCheckBox("Вставками", ref y);
      chkQuick = CreateAlgorithmCheckBox("Быстрая", ref y);
      chkBogo = CreateAlgorithmCheckBox("BOGO", ref y);

      chkBubble.Checked = true;
      chkQuick.Checked = true;

      y += 10;
      var lblDir = new Label
      {
        Text = "Направление:",
        Left = 10,
        Top = y,
        Width = 200,
        Font = new Font("Segoe UI", 9, FontStyle.Bold)
      };
      algorithmPanel.Controls.Add(lblDir);
      y += 22;

      rbAscending = new RadioButton
      {
        Text = "По возрастанию",
        Left = 10,
        Top = y,
        Width = 200,
        Checked = true
      };
      algorithmPanel.Controls.Add(rbAscending);
      y += 22;

      rbDescending = new RadioButton
      {
        Text = "По убыванию",
        Left = 10,
        Top = y,
        Width = 200
      };
      algorithmPanel.Controls.Add(rbDescending);
      y += 25;

      var lblBogo = new Label
      {
        Text = "Лимит итераций BOGO:",
        Left = 10,
        Top = y,
        Width = 200
      };
      algorithmPanel.Controls.Add(lblBogo);
      y += 22;

      txtBogoLimit = new TextBox
      {
        Left = 10,
        Top = y,
        Width = 220,
        Text = "10000"
      };
      algorithmPanel.Controls.Add(txtBogoLimit);
      y += 30;

      chkIntegerOnly = new CheckBox
      {
        Text = "Только целые числа",
        Left = 10,
        Top = y,
        Width = 220,
        Checked = false
      };
      algorithmPanel.Controls.Add(chkIntegerOnly);
      y += 25;

      chkDetailedVisualization = new CheckBox
      {
        Text = "Детальная визуализация",
        Left = 10,
        Top = y,
        Width = 220,
        Checked = true
      };
      algorithmPanel.Controls.Add(chkDetailedVisualization);
      y += 25;

      chkAnimate = new CheckBox
      {
        Text = "Анимировать сортировку",
        Left = 10,
        Top = y,
        Width = 220,
        Checked = false
      };
      chkAnimate.CheckedChanged += (s, e) =>
      {
        trkAnimationSpeed.Enabled = chkAnimate.Checked;
        lblAnimationSpeed.Enabled = chkAnimate.Checked;
      };
      algorithmPanel.Controls.Add(chkAnimate);
      y += 25;

      lblAnimationSpeed = new Label
      {
        Text = "Скорость: 5 (медленно ↔ быстро)",
        Left = 10,
        Top = y,
        Width = 220,
        Enabled = false
      };
      algorithmPanel.Controls.Add(lblAnimationSpeed);
      y += 20;

      trkAnimationSpeed = new TrackBar
      {
        Left = 10,
        Top = y,
        Width = 220,
        Minimum = 1,
        Maximum = 10,
        Value = 5,
        TickFrequency = 1,
        Enabled = false
      };
      trkAnimationSpeed.ValueChanged += (s, e) =>
      {
        lblAnimationSpeed.Text =
            $"Скорость: {trkAnimationSpeed.Value} (медленно ↔ быстро)";
      };
      algorithmPanel.Controls.Add(trkAnimationSpeed);

      Controls.Add(algorithmPanel);

      // ---------- Панель визуализации ----------
      visualizationPanel = new Panel
      {
        Left = 690,
        Top = 30,
        Width = 560,
        Height = 380,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White,
        Anchor = AnchorStyles.Top | AnchorStyles.Right,
        AutoScroll = false
      };
      Controls.Add(visualizationPanel);

      // ---------- Результаты ----------
      var lblResults = new Label
      {
        Text = "Результаты сортировки:",
        Left = 10,
        Top = 420,
        Width = 300,
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
      };
      Controls.Add(lblResults);

      dataGridViewResults = new DataGridView
      {
        Left = 10,
        Top = 445,
        Width = 900,
        Height = 220,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
      };
      dataGridViewResults.Columns.Add("Algorithm", "Алгоритм");
      dataGridViewResults.Columns.Add("Time", "Время (мс)");
      dataGridViewResults.Columns.Add("Iterations", "Итераций");
      dataGridViewResults.Columns.Add("Count", "Элементов");
      dataGridViewResults.Columns.Add("Status", "Статус");
      Controls.Add(dataGridViewResults);

      lblFastest = new Label
      {
        Text = "⚡ Самый быстрый: —",
        Left = 920,
        Top = 445,
        Width = 330,
        Height = 40,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        ForeColor = Color.DarkGreen,
        Anchor = AnchorStyles.Top | AnchorStyles.Right
      };
      Controls.Add(lblFastest);

      progressBar = new ProgressBar
      {
        Left = 920,
        Top = 500,
        Width = 330,
        Height = 25,
        Anchor = AnchorStyles.Top | AnchorStyles.Right
      };
      Controls.Add(progressBar);

      // ---------- StatusStrip ----------
      statusStrip = new StatusStrip();
      statusLabel = new ToolStripStatusLabel("Готово");
      statusStrip.Items.Add(statusLabel);
      Controls.Add(statusStrip);
    }

    private CheckBox CreateAlgorithmCheckBox(string text, ref int y)
    {
      var cb = new CheckBox
      {
        Text = text,
        Left = 10,
        Top = y,
        Width = 220
      };
      algorithmPanel.Controls.Add(cb);
      y += 28;
      return cb;
    }

    // ================== Валидация ввода ==================
    private void DataGridViewInput_CellValidating(
        object sender, DataGridViewCellValidatingEventArgs e)
    {
      if (e.ColumnIndex != 0) return;
      if (e.FormattedValue == null) return;

      string value = e.FormattedValue.ToString();
      if (string.IsNullOrWhiteSpace(value)) return;

      if (!ValidationService.TryParseDouble(value, out double parsed))
      {
        e.Cancel = true;
        MessageBox.Show(
            $"Некорректное значение: '{value}'. Введите число.",
            "Ошибка ввода",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return;
      }

      if (chkIntegerOnly != null && chkIntegerOnly.Checked &&
          !ValidationService.IsInteger(parsed))
      {
        e.Cancel = true;
        MessageBox.Show(
            $"В режиме «Только целые числа» нельзя вводить '{value}'.\n" +
            "Введите целое число или снимите галочку.",
            "Ошибка ввода",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
      }
    }

    // ================== Меню: генерация ==================
    private void GenerateData(object sender, EventArgs e)
    {
      using (var dlg = new GenerateDataDialog())
      {
        if (dlg.ShowDialog(this) == DialogResult.OK &&
            dlg.GeneratedData != null)
        {
          var data = dlg.GeneratedData;

          if (chkIntegerOnly != null && chkIntegerOnly.Checked)
            data = data.Select(v => Math.Round(v)).ToList();

          currentData = data;
          LoadDataToGrid(currentData);
          statusLabel.Text = $"Сгенерировано: {currentData.Count} элементов";
        }
      }
    }

    // ================== Меню: загрузка из Excel ==================
    private void LoadFromExcel(object sender, EventArgs e)
    {
      using (var ofd = new OpenFileDialog())
      {
        ofd.Filter = "Excel Files|*.xlsx;*.xls";
        ofd.Title = "Выберите Excel-файл";

        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
          var data = ExcelLoader.Load(ofd.FileName);

          if (data.Count == 0)
          {
            MessageBox.Show("В файле не найдено числовых данных.",
                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
          }

          if (chkIntegerOnly != null && chkIntegerOnly.Checked)
            data = data.Select(v => Math.Round(v)).ToList();

          currentData = data;
          LoadDataToGrid(currentData);
          statusLabel.Text = $"Загружено из Excel: {data.Count} элементов";
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка загрузки Excel:\n{ex.Message}",
              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    // ================== Меню: загрузка из Google ==================
    private async void LoadFromGoogle(object sender, EventArgs e)
    {
      using (var dlg = new GoogleTableDialog())
      {
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
          statusLabel.Text = "Загрузка из Google Table...";
          Cursor = Cursors.WaitCursor;

          var data = await GoogleTableLoader.LoadAsync(dlg.Url);

          if (data.Count == 0)
          {
            MessageBox.Show("В таблице не найдено числовых данных.",
                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            statusLabel.Text = "Готово";
            return;
          }

          if (chkIntegerOnly != null && chkIntegerOnly.Checked)
            data = data.Select(v => Math.Round(v)).ToList();

          currentData = data;
          LoadDataToGrid(currentData);
          statusLabel.Text = $"Загружено из Google: {data.Count} элементов";
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка загрузки Google Table:\n{ex.Message}",
              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
          statusLabel.Text = "Ошибка загрузки";
        }
        finally
        {
          Cursor = Cursors.Default;
        }
      }
    }

    // ================== Меню: сохранение ==================
    private void SaveResults(object sender, EventArgs e)
    {
      if (dataGridViewResults.Rows.Count == 0)
      {
        MessageBox.Show("Нет результатов для сохранения.",
            "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var sfd = new SaveFileDialog())
      {
        sfd.Filter = "CSV|*.csv|Text|*.txt";
        sfd.FileName = "sort_results";

        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
          using (var writer = new System.IO.StreamWriter(
              sfd.FileName, false, System.Text.Encoding.UTF8))
          {
            var headers = new List<string>();
            foreach (DataGridViewColumn col in dataGridViewResults.Columns)
              headers.Add(col.HeaderText);
            writer.WriteLine(string.Join(";", headers));

            foreach (DataGridViewRow row in dataGridViewResults.Rows)
            {
              if (row.IsNewRow) continue;
              var cells = new List<string>();
              foreach (DataGridViewCell cell in row.Cells)
                cells.Add(cell.Value?.ToString() ?? "");
              writer.WriteLine(string.Join(";", cells));
            }
          }

          statusLabel.Text = $"Сохранено: {sfd.FileName}";
          MessageBox.Show("Результаты успешно сохранены!",
              "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка сохранения:\n{ex.Message}",
              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    // ================== Меню: очистка ==================
    private void ClearAll(object sender, EventArgs e)
    {
      var result = MessageBox.Show(
          "Очистить все данные и результаты?",
          "Подтверждение",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question);

      if (result != DialogResult.Yes) return;

      currentData.Clear();
      dataGridViewInput.Rows.Clear();
      dataGridViewResults.Rows.Clear();
      visualizationPanel.Controls.Clear();
      lblFastest.Text = "⚡ Самый быстрый: —";
      progressBar.Value = 0;
      statusLabel.Text = "Очищено";
    }

    // ================== Меню: краш-тест ==================
    private void RunStressTest(object sender, EventArgs e)
    {
      currentData = DataGenerator.Generate(51234, -1000, 1000, true);
      LoadDataToGrid(currentData);
      chkDetailedVisualization.Checked = false;
      chkAnimate.Checked = false;

      statusLabel.Text = "Краш-тест: 51234 элемента...";
      RunSorting(sender, e);
    }

    // ============================================================
    //  ГЛАВНАЯ ЛОГИКА: последовательная сортировка,
    //  затем параллельная анимация всех алгоритмов
    // ============================================================
    private async void RunSorting(object sender, EventArgs e)
    {
      // 1. Получить данные
      if (currentData == null || currentData.Count == 0)
        currentData = ReadDataFromGrid();

      if (currentData.Count == 0)
      {
        MessageBox.Show("Нет данных для сортировки!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (chkIntegerOnly.Checked)
      {
        currentData = currentData.Select(v => Math.Round(v)).ToList();
        LoadDataToGrid(currentData);
      }

      // 2. Выбранные алгоритмы
      var selected = GetSelectedAlgorithms();
      if (selected.Count == 0)
      {
        MessageBox.Show("Выберите хотя бы один алгоритм!", "Ошибка",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      bool ascending = rbAscending.Checked;

      // 3. Лимит BOGO
      int bogoLimit = 10000;
      if (!string.IsNullOrWhiteSpace(txtBogoLimit.Text))
      {
        if (!int.TryParse(txtBogoLimit.Text, out bogoLimit) || bogoLimit <= 0)
        {
          MessageBox.Show("Лимит итераций BOGO должен быть > 0!",
              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }
      }

      // 4. Визуализатор
      if (visualizer == null)
        visualizer = new SortVisualizer(visualizationPanel);

      bool animate = chkAnimate.Checked;

      if (animate && currentData.Count > 100)
      {
        MessageBox.Show(
            "Анимация поддерживается только для массивов ≤ 100 элементов.",
            "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        animate = false;
      }

      // 5. Блокировка UI
      ToggleUIEnabled(false);
      dataGridViewResults.Rows.Clear();
      visualizationPanel.Controls.Clear();
      progressBar.Maximum = selected.Count;
      progressBar.Value = 0;

      var results = new List<SortResult>();
      var datasets = new List<List<double>>();
      var names = new List<string>();
      var colors = new List<Color>();
      var allSnapshots = new List<List<List<double>>>();

      // ============================================================
      // ЭТАП 1: последовательно выполняем все сортировки
      // ============================================================
      for (int i = 0; i < selected.Count; i++)
      {
        var algorithm = selected[i];

        statusLabel.Text = $"Выполняется: {algorithm.Name}...";
        Application.DoEvents();

        var localAlg = algorithm;
        var localData = new List<double>(currentData);
        var localAsc = ascending;
        var localLimit = bogoLimit;
        var localAnimate = animate;
        var localColor = ColorPalette.Get(i);

        List<List<double>> snapshots = null;
        if (localAnimate)
        {
          snapshots = new List<List<double>>();
          snapshots.Add(new List<double>(localData));
        }

        var taskResult = await Task.Run(() =>
        {
          int iterCount = 0;
          Action<List<double>, int> onIter = (snap, it) =>
          {
            iterCount = it;
            if (localAnimate && snapshots != null)
            {
              snapshots.Add(new List<double>(snap));
            }
          };

          var swLocal = System.Diagnostics.Stopwatch.StartNew();
          List<double> sorted = null;
          Exception captured = null;

          try
          {
            int maxIter = localAlg is BogoSort ? localLimit : 0;
            sorted = localAlg.Sort(
                localData, localAsc, onIter, maxIter);
          }
          catch (Exception ex)
          {
            captured = ex;
          }

          swLocal.Stop();

          var sr = new SortResult
          {
            AlgorithmName = localAlg.Name,
            ElapsedMilliseconds = swLocal.Elapsed.TotalMilliseconds,
            Iterations = iterCount,
            ElementCount = localData.Count,
            Success = captured == null && sorted != null,
            ErrorMessage = captured?.Message
          };

          return Tuple.Create(sorted, sr, snapshots);
        });

        List<double> finalData = taskResult.Item1;
        SortResult result = taskResult.Item2;
        List<List<double>> snaps = taskResult.Item3;

        results.Add(result);
        allSnapshots.Add(snaps);

        if (finalData != null && finalData.Count > 0)
        {
          datasets.Add(finalData);
          names.Add(algorithm.Name);
          colors.Add(localColor);
        }

        AddResultRow(result);
        progressBar.Value = i + 1;
        Application.DoEvents();
      }

      // ============================================================
      // ЭТАП 2: визуализация
      // ============================================================
      if (animate && allSnapshots.Count > 0 && names.Count > 0)
      {
        await PlayParallelAnimation(allSnapshots, names, colors, datasets);
      }
      else
      {
        if (datasets.Count > 0)
        {
          try
          {
            visualizer.DrawMultiple(datasets, names, colors);
          }
          catch (Exception ex)
          {
            MessageBox.Show(
                $"Ошибка визуализации:\n{ex.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
      }

      // 6. Самый быстрый
      var successResults = results.Where(r => r.Success).ToList();
      if (successResults.Count > 0)
      {
        var fastest = successResults
            .OrderBy(r => r.ElapsedMilliseconds).First();

        lblFastest.Text =
            $"⚡ Самый быстрый: {fastest.AlgorithmName} " +
            $"({fastest.ElapsedMilliseconds:F2} мс)";
      }
      else
      {
        lblFastest.Text = "⚡ Самый быстрый: —";
      }

      ToggleUIEnabled(true);
      statusLabel.Text =
          $"Готово. Отсортировано: {results.Count} алгоритмов " +
          $"({currentData.Count} элементов)";
    }

    // ============================================================
    //  ПАРАЛЛЕЛЬНАЯ АНИМАЦИЯ: все алгоритмы одновременно
    // ============================================================
    private async Task PlayParallelAnimation(
        List<List<List<double>>> allSnapshots,
        List<string> names,
        List<Color> colors,
        List<List<double>> finalDatasets)
    {
      int n = allSnapshots.Count;
      if (n == 0 || names.Count == 0) return;

      // Фильтруем только те алгоритмы, у которых есть снапшоты
      var validIndices = new List<int>();
      for (int i = 0; i < n; i++)
      {
        if (i < names.Count &&
            allSnapshots[i] != null &&
            allSnapshots[i].Count > 0)
        {
          validIndices.Add(i);
        }
      }

      if (validIndices.Count == 0) return;

      int m = validIndices.Count;

      var vSnapshots = new List<List<List<double>>>(m);
      var vNames = new List<string>(m);
      var vColors = new List<Color>(m);

      for (int k = 0; k < m; k++)
      {
        int idx = validIndices[k];
        vSnapshots.Add(allSnapshots[idx]);
        vNames.Add(names[idx]);
        vColors.Add(colors[idx]);
      }

      // Максимум кадров среди всех
      int maxFrames = 0;
      for (int i = 0; i < m; i++)
      {
        if (vSnapshots[i].Count > maxFrames)
          maxFrames = vSnapshots[i].Count;
      }

      if (maxFrames == 0) return;

      var currentFrames = new List<List<double>>(m);
      var currentIndices = new List<int>(m);
      var totalFrames = new List<int>(m);
      var finished = new List<bool>(m);

      for (int i = 0; i < m; i++)
      {
        currentFrames.Add(new List<double>(vSnapshots[i][0]));
        currentIndices.Add(0);
        totalFrames.Add(vSnapshots[i].Count);
        finished.Add(false);
      }

      // Общее число кадров анимации
      int globalTotalSteps = 500;
      if (maxFrames < 500) globalTotalSteps = maxFrames;

      int baseDelay = 205 - trkAnimationSpeed.Value * 20;
      if (baseDelay < 5) baseDelay = 5;

      for (int step = 0; step < globalTotalSteps; step++)
      {
        double t = globalTotalSteps > 1
            ? (double)step / (globalTotalSteps - 1)
            : 1.0;

        bool allDone = true;

        for (int i = 0; i < m; i++)
        {
          int total = totalFrames[i];
          if (total == 0) continue;

          int frame = (int)(t * (total - 1));
          if (frame >= total) frame = total - 1;
          if (frame < 0) frame = 0;

          currentIndices[i] = frame;
          currentFrames[i] = new List<double>(vSnapshots[i][frame]);

          if (frame >= total - 1)
            finished[i] = true;
          else
            allDone = false;
        }

        visualizer.DrawAnimationFrame(
            currentFrames, vNames,
            currentIndices, totalFrames,
            vColors, finished);

        statusLabel.Text =
            $"Анимация: кадр {step + 1}/{globalTotalSteps}";

        await Task.Delay(baseDelay);

        if (allDone) break;
      }

      // Финальный кадр — точные результаты
      for (int k = 0; k < m; k++)
      {
        int idx = validIndices[k];
        finished[k] = true;

        if (finalDatasets != null && finalDatasets.Count > k)
          currentFrames[k] = new List<double>(finalDatasets[k]);
        else if (vSnapshots[k].Count > 0)
          currentFrames[k] = new List<double>(
              vSnapshots[k][vSnapshots[k].Count - 1]);

        currentIndices[k] = totalFrames[k];
      }

      visualizer.DrawAnimationFrame(
          currentFrames, vNames,
          currentIndices, totalFrames,
          vColors, finished);

      await Task.Delay(500);
    }

    // ================== Вспомогательные методы ==================
    private List<double> ReadDataFromGrid()
    {
      var result = new List<double>();

      foreach (DataGridViewRow row in dataGridViewInput.Rows)
      {
        if (row.IsNewRow) continue;
        var cell = row.Cells[0].Value;
        if (cell == null) continue;

        string text = cell.ToString();
        if (string.IsNullOrWhiteSpace(text)) continue;

        if (ValidationService.TryParseDouble(text, out double value))
          result.Add(value);
      }

      return result;
    }

    private void LoadDataToGrid(List<double> data)
    {
      dataGridViewInput.Rows.Clear();

      if (data.Count > 1000)
      {
        int show = Math.Min(1000, data.Count);
        for (int i = 0; i < show; i++)
          dataGridViewInput.Rows.Add(data[i]);

        statusLabel.Text = $"Показано {show} из {data.Count} элементов";
      }
      else
      {
        foreach (var v in data)
          dataGridViewInput.Rows.Add(v);
      }
    }

    private List<ISortAlgorithm> GetSelectedAlgorithms()
    {
      var selected = new List<ISortAlgorithm>();

      if (chkBubble.Checked) selected.Add(algorithms[0]);
      if (chkShaker.Checked) selected.Add(algorithms[1]);
      if (chkInsertion.Checked) selected.Add(algorithms[2]);
      if (chkQuick.Checked) selected.Add(algorithms[3]);
      if (chkBogo.Checked) selected.Add(algorithms[4]);

      return selected;
    }

    private void AddResultRow(SortResult result)
    {
      string status = result.Success ? "OK" : "Ошибка";

      int index = dataGridViewResults.Rows.Add(
          result.AlgorithmName,
          result.ElapsedMilliseconds.ToString("F2"),
          result.Iterations.ToString(),
          result.ElementCount.ToString(),
          status);

      if (!result.Success)
      {
        dataGridViewResults.Rows[index].DefaultCellStyle.BackColor =
            Color.MistyRose;
      }
    }

    private void ToggleUIEnabled(bool enabled)
    {
      menuStrip.Enabled = enabled;
      dataGridViewInput.Enabled = enabled;
      algorithmPanel.Enabled = enabled;
      Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
    }

    // ================== Горячие клавиши ==================
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == (Keys.Control | Keys.R))
      {
        RunSorting(null, EventArgs.Empty);
        return true;
      }
      if (keyData == (Keys.Control | Keys.G))
      {
        GenerateData(null, EventArgs.Empty);
        return true;
      }
      if (keyData == (Keys.Control | Keys.Delete))
      {
        ClearAll(null, EventArgs.Empty);
        return true;
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }
  }
}