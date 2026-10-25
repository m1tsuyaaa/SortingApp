using System;
using System.Drawing;
using System.Windows.Forms;
using SortingApp.Numeric;

namespace SortingApp.Forms
{
  public class StepsForm : Form
  {
    private GoldenResult _result;
    private bool _isMax;

    private int _currentStep = 0;
    private Timer _animationTimer;
    private bool _isPlaying = false;

    private Label lblInfo;
    private TrackBar trkIteration;
    private Button btnFirst, btnPrev, btnPlay, btnNext, btnFinal;

    public StepsForm(GoldenResult result, bool isMax)
    {
      _result = result;
      _isMax = isMax;

      InitializeUI();
      SetupAnimation();

      _currentStep = _result.Steps.Count - 1;
      trkIteration.Maximum = _result.Steps.Count - 1;
      trkIteration.Value = _currentStep;
      UpdateDisplay();
    }

    private void InitializeUI()
    {
      Text = "Пошаговый просмотр (Золотое сечение)";
      Width = 850;
      Height = 500;
      StartPosition = FormStartPosition.CenterParent;
      Font = new Font("Segoe UI", 10);
      BackColor = Color.FromArgb(245, 247, 250);

      var lblTitle = new Label
      {
        Text = "ПОШАГОВЫЙ ПРОСМОТР",
        Left = 20,
        Top = 20,
        Width = 800,
        Height = 30,
        Font = new Font("Segoe UI", 14, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 51, 102)
      };
      Controls.Add(lblTitle);

      btnFirst = new Button { Text = "◀◀", Left = 20, Top = 60, Width = 60, Height = 35 };
      btnFirst.Click += (s, e) => GotoStep(0);
      Controls.Add(btnFirst);

      btnPrev = new Button { Text = "◀", Left = 90, Top = 60, Width = 60, Height = 35 };
      btnPrev.Click += (s, e) => StepBackward();
      Controls.Add(btnPrev);

      btnPlay = new Button
      {
        Text = "▶ Играть",
        Left = 160,
        Top = 60,
        Width = 100,
        Height = 35,
        BackColor = Color.FromArgb(0, 120, 215),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat
      };
      btnPlay.FlatAppearance.BorderSize = 0;
      btnPlay.Click += (s, e) => TogglePlay();
      Controls.Add(btnPlay);

      btnNext = new Button { Text = "▶", Left = 270, Top = 60, Width = 60, Height = 35 };
      btnNext.Click += (s, e) => StepForward();
      Controls.Add(btnNext);

      btnFinal = new Button { Text = "▶▶", Left = 340, Top = 60, Width = 60, Height = 35 };
      btnFinal.Click += (s, e) => GotoFinal();
      Controls.Add(btnFinal);

      trkIteration = new TrackBar { Left = 20, Top = 110, Width = 780, Height = 45, TickFrequency = 1 };
      trkIteration.ValueChanged += (s, e) => { _currentStep = trkIteration.Value; UpdateDisplay(); };
      Controls.Add(trkIteration);

      lblInfo = new Label
      {
        Left = 20,
        Top = 170,
        Width = 790,
        Height = 260,
        Font = new Font("Consolas", 12),
        BackColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        Padding = new Padding(15)
      };
      Controls.Add(lblInfo);
    }

    private void SetupAnimation()
    {
      _animationTimer = new Timer();
      _animationTimer.Interval = 400;
      _animationTimer.Tick += (s, e) =>
      {
        if (_currentStep < _result.Steps.Count - 1) { _currentStep++; trkIteration.Value = _currentStep; }
        else StopPlay();
      };
    }

    private void TogglePlay() { if (_isPlaying) StopPlay(); else StartPlay(); }

    private void StartPlay()
    {
      if (_currentStep >= _result.Steps.Count - 1) _currentStep = 0;
      _isPlaying = true; btnPlay.Text = "⏸ Пауза"; btnPlay.BackColor = Color.FromArgb(200, 80, 80);
      _animationTimer.Start();
    }

    private void StopPlay()
    {
      _isPlaying = false; btnPlay.Text = "▶ Играть"; btnPlay.BackColor = Color.FromArgb(0, 120, 215);
      _animationTimer.Stop();
    }

    private void StepBackward() { if (_currentStep > 0) { _currentStep--; trkIteration.Value = _currentStep; } }
    private void StepForward() { if (_currentStep < _result.Steps.Count - 1) { _currentStep++; trkIteration.Value = _currentStep; } }
    private void GotoStep(int step) { _currentStep = Math.Max(0, Math.Min(step, _result.Steps.Count - 1)); trkIteration.Value = _currentStep; }
    private void GotoFinal() { _currentStep = _result.Steps.Count - 1; trkIteration.Value = _currentStep; }

    private void UpdateDisplay()
    {
      if (_result == null) return;
      var step = _result.Steps[_currentStep];
      string typeStr = _isMax ? "МАКСИМУМ" : "МИНИМУМ";

      if (_currentStep == _result.Steps.Count - 1)
      {
        lblInfo.Text =
            $"╔═══ ФИНАЛ ({typeStr}) ═══╗\n" +
            $"║ x* = {_result.XExtremum:F6}\n" +
            $"║ f(x*) = {_result.FExtremum:F6}\n" +
            $"║ Итераций: {_result.Iterations}\n" +
            $"║ Точность: {_result.FinalInterval:E3}\n" +
            $"╚══════════════════════════╝";
      }
      else
      {
        string comparison = (_isMax ? step.F1 > step.F2 : step.F1 < step.F2) ? "f(x1) выгоднее → b = x2" : "f(x2) выгоднее → a = x1";
        lblInfo.Text =
            $"Итерация {step.Number}\n" +
            $"───────────────────────────\n" +
            $"a  = {step.A:F6}\n" +
            $"b  = {step.B:F6}\n" +
            $"x1 = {step.X1:F6}   f(x1) = {step.F1:F6}\n" +
            $"x2 = {step.X2:F6}   f(x2) = {step.F2:F6}\n" +
            $"───────────────────────────\n" +
            $"Длина: {step.Interval:E3}\n" +
            $"Решение: {comparison}";
      }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == Keys.Space) { TogglePlay(); return true; }
      if (keyData == Keys.Left) { StepBackward(); return true; }
      if (keyData == Keys.Right) { StepForward(); return true; }
      if (keyData == Keys.Home) { GotoStep(0); return true; }
      if (keyData == Keys.End) { GotoFinal(); return true; }
      return base.ProcessCmdKey(ref msg, keyData);
    }
  }
}