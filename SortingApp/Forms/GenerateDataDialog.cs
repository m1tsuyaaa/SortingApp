using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SortingApp.Services;

namespace SortingApp.Forms
{
  public class GenerateDataDialog : Form
  {
    public List<double> GeneratedData { get; private set; }

    private NumericUpDown nudCount;
    private NumericUpDown nudMin;
    private NumericUpDown nudMax;
    private CheckBox chkFractional;

    public GenerateDataDialog()
    {
      Text = "Генерация данных";
      Size = new System.Drawing.Size(340, 260);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      StartPosition = FormStartPosition.CenterParent;
      MaximizeBox = false;
      MinimizeBox = false;

      var lblCount = new Label
      {
        Text = "Количество элементов:",
        Left = 15,
        Top = 20,
        Width = 180
      };

      nudCount = new NumericUpDown
      {
        Left = 200,
        Top = 18,
        Width = 100,
        Minimum = 1,
        Maximum = 100000,
        Value = 50
      };

      var lblMin = new Label
      {
        Text = "Минимум:",
        Left = 15,
        Top = 55,
        Width = 180
      };

      nudMin = new NumericUpDown
      {
        Left = 200,
        Top = 53,
        Width = 100,
        Minimum = -1000000,
        Maximum = 1000000,
        DecimalPlaces = 2,
        Value = -100
      };

      var lblMax = new Label
      {
        Text = "Максимум:",
        Left = 15,
        Top = 90,
        Width = 180
      };

      nudMax = new NumericUpDown
      {
        Left = 200,
        Top = 88,
        Width = 100,
        Minimum = -1000000,
        Maximum = 1000000,
        DecimalPlaces = 2,
        Value = 100
      };

      chkFractional = new CheckBox
      {
        Text = "Разрешить дробные числа",
        Left = 15,
        Top = 125,
        Width = 250,
        Checked = true
      };
      chkFractional.CheckedChanged += (s, e) =>
      {
        // Если только целые — ограничиваем точность
        if (!chkFractional.Checked)
        {
          nudMin.DecimalPlaces = 0;
          nudMax.DecimalPlaces = 0;
          nudMin.Value = Math.Round(nudMin.Value);
          nudMax.Value = Math.Round(nudMax.Value);
        }
        else
        {
          nudMin.DecimalPlaces = 2;
          nudMax.DecimalPlaces = 2;
        }
      };

      var btnOk = new Button
      {
        Text = "OK",
        Left = 130,
        Top = 165,
        Width = 80,
        DialogResult = DialogResult.OK
      };

      btnOk.Click += (s, e) =>
      {
        if (nudMin.Value >= nudMax.Value)
        {
          MessageBox.Show(
              "Минимум должен быть меньше максимума!",
              "Ошибка",
              MessageBoxButtons.OK,
              MessageBoxIcon.Warning);
          DialogResult = DialogResult.None;
          return;
        }

        GeneratedData = DataGenerator.Generate(
            (int)nudCount.Value,
            (double)nudMin.Value,
            (double)nudMax.Value,
            chkFractional.Checked);
      };

      var btnCancel = new Button
      {
        Text = "Отмена",
        Left = 220,
        Top = 165,
        Width = 80,
        DialogResult = DialogResult.Cancel
      };

      Controls.Add(lblCount);
      Controls.Add(nudCount);
      Controls.Add(lblMin);
      Controls.Add(nudMin);
      Controls.Add(lblMax);
      Controls.Add(nudMax);
      Controls.Add(chkFractional);
      Controls.Add(btnOk);
      Controls.Add(btnCancel);

      AcceptButton = btnOk;
      CancelButton = btnCancel;
    }
  }
}