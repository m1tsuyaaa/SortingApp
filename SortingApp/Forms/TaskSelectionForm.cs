using System;
using System.Drawing;
using System.Windows.Forms;

namespace SortingApp.Forms
{
  public class TaskSelectionForm : Form
  {
    public TaskSelectionForm()
    {
      InitializeUI();
    }

    private void InitializeUI()
    {
      Text = "Выбор задачи";
      Width = 700;
      Height = 700;
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      Font = new Font("Segoe UI", 10);
      BackColor = Color.FromArgb(245, 247, 250);

      // Заголовок
      var lblTitle = new Label
      {
        Text = "ВЫБЕРИТЕ ЗАДАЧУ",
        Left = 0,
        Top = 30,
        Width = ClientSize.Width,
        Height = 40,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 16, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 51, 102)
      };
      Controls.Add(lblTitle);

      var lblSubtitle = new Label
      {
        Text = "Практическая работа — численные методы и алгоритмы",
        Left = 0,
        Top = 70,
        Width = ClientSize.Width,
        Height = 25,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 10, FontStyle.Italic),
        ForeColor = Color.Gray
      };
      Controls.Add(lblSubtitle);

      // Карточка 1 — Сортировка
      var card1 = CreateTaskCard(
          "📊  Задача 1: Сортировка массивов",
          "Пять алгоритмов сортировки:\n" +
          "Пузырьковая, Шейкерная, Вставками,\n" +
          "Быстрая, BOGO\n\n" +
          "• Ввод вручную / Excel / Google Table\n" +
          "• Генерация данных\n" +
          "• Визуализация и анимация\n" +
          "• Замер времени",
          Color.FromArgb(70, 130, 200),
          130,
          (s, e) => OpenTask(new MainForm()));

      Controls.Add(card1);

      // Карточка 2 — Дихотомия
      var card2 = CreateTaskCard(
          "📐  Задача 2: Поиск минимума",
          "Метод половинного деления (дихотомии)\n" +
          "для поиска локального минимума f(x)\n" +
          "на отрезке [a, b] с точностью ε\n\n" +
          "• Парсер формул\n" +
          "• График с осями\n" +
          "• Пошаговый просмотр\n" +
          "• Проверка унимодальности",
          Color.FromArgb(70, 180, 120),
          300,
          (s, e) => OpenTask(new MinimizationForm()));

      Controls.Add(card2);

      // Карточка 3 — Золотое сечение
      var card3 = CreateTaskCard(
          "🏆 Задача 3: Золотое сечение",
          "Метод золотого сечения для поиска\n" +
          "экстремума (min/max) функции f(x)\n\n" +
          "• Поиск минимума и максимума\n" +
          "• Поддержка модуля |x|\n" +
          "• Поиск корней функции\n" +
          "• Красивый график с осями",
          Color.FromArgb(200, 140, 50),
          470,
          (s, e) => OpenTask(new GoldenSectionForm()));

      Controls.Add(card3);

      // Нижняя подпись
      var lblFooter = new Label
      {
        Text = "© 2026 · WinForms · C# · Численные методы",
        Left = 0,
        Top = ClientSize.Height - 30,
        Width = ClientSize.Width,
        Height = 25,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 8),
        ForeColor = Color.Gray
      };
      Controls.Add(lblFooter);
    }

    private Panel CreateTaskCard(string title, string description,
        Color accentColor, int top, EventHandler onClickAction)
    {
      var card = new Panel
      {
        Left = 40,
        Top = top,
        Width = ClientSize.Width - 100,
        Height = 150,
        BackColor = Color.White,
        Cursor = Cursors.Hand,
        BorderStyle = BorderStyle.FixedSingle
      };

      // Цветная полоска слева
      var accent = new Panel
      {
        Left = 0,
        Top = 0,
        Width = 6,
        Height = card.Height,
        BackColor = accentColor
      };
      card.Controls.Add(accent);

      // Заголовок
      var lblTitle = new Label
      {
        Text = title,
        Left = 20,
        Top = 15,
        Width = card.Width - 40,
        Height = 30,
        Font = new Font("Segoe UI", 13, FontStyle.Bold),
        ForeColor = Color.FromArgb(30, 30, 40)
      };
      card.Controls.Add(lblTitle);

      // Описание
      var lblDesc = new Label
      {
        Text = description,
        Left = 20,
        Top = 50,
        Width = card.Width - 60,
        Height = 90,
        Font = new Font("Segoe UI", 9),
        ForeColor = Color.FromArgb(80, 80, 90)
      };
      card.Controls.Add(lblDesc);

      // Стрелка справа
      var lblArrow = new Label
      {
        Text = "→",
        Left = card.Width - 45,
        Top = 55,
        Width = 40,
        Height = 40,
        Font = new Font("Segoe UI", 22, FontStyle.Bold),
        ForeColor = accentColor,
        TextAlign = ContentAlignment.MiddleCenter
      };
      card.Controls.Add(lblArrow);

      // ✅ Обработчик на саму карточку
      card.Click += onClickAction;

      // ✅ Обработчик на все дочерние элементы
      foreach (Control c in card.Controls)
      {
        c.Click += onClickAction;
        c.Cursor = Cursors.Hand;

        c.MouseEnter += (s, e) =>
            card.BackColor = Color.FromArgb(245, 250, 255);
        c.MouseLeave += (s, e) =>
            card.BackColor = Color.White;
      }

      // Hover для самой карточки
      card.MouseEnter += (s, e) =>
          card.BackColor = Color.FromArgb(245, 250, 255);
      card.MouseLeave += (s, e) =>
          card.BackColor = Color.White;

      return card;
    }

    private void OpenTask(Form taskForm)
    {
      Hide();
      taskForm.FormClosed += (s, e) => Show();
      taskForm.Show();
    }
  }
}