using System;
using System.Windows.Forms;

namespace SortingApp.Forms
{
  public class GoogleTableDialog : Form
  {
    public string Url { get; private set; }

    private TextBox txtUrl;

    public GoogleTableDialog()
    {
      Text = "Загрузка из Google Table";
      Size = new System.Drawing.Size(560, 180);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      StartPosition = FormStartPosition.CenterParent;
      MaximizeBox = false;
      MinimizeBox = false;

      var lbl = new Label
      {
        Text = "Вставьте ссылку на CSV-экспорт Google-таблицы:",
        Left = 15,
        Top = 15,
        Width = 500
      };

      txtUrl = new TextBox
      {
        Left = 15,
        Top = 45,
        Width = 510,
        Text = "https://docs.google.com/spreadsheets/d/{ID}/export?format=csv"
      };

      var btnOk = new Button
      {
        Text = "OK",
        Left = 350,
        Top = 90,
        Width = 80,
        DialogResult = DialogResult.OK
      };

      btnOk.Click += (s, e) =>
      {
        if (string.IsNullOrWhiteSpace(txtUrl.Text))
        {
          MessageBox.Show("Введите ссылку!", "Ошибка",
              MessageBoxButtons.OK, MessageBoxIcon.Warning);
          DialogResult = DialogResult.None;
          return;
        }
        Url = txtUrl.Text.Trim();
      };

      var btnCancel = new Button
      {
        Text = "Отмена",
        Left = 440,
        Top = 90,
        Width = 80,
        DialogResult = DialogResult.Cancel
      };

      Controls.Add(lbl);
      Controls.Add(txtUrl);
      Controls.Add(btnOk);
      Controls.Add(btnCancel);

      AcceptButton = btnOk;
      CancelButton = btnCancel;
    }
  }
}