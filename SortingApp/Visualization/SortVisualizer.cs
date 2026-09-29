using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SortingApp.Visualization
{
    public class SortVisualizer
    {
        private readonly Panel _panel;

        public SortVisualizer(Panel panel)
        {
            _panel = panel;
        }

        // === ФИНАЛЬНЫЕ ДИАГРАММЫ (без анимации) ===
        public void DrawMultiple(
            List<List<double>> datasets,
            List<string> names,
            List<Color> colors = null)
        {
            _panel.Controls.Clear();
            _panel.AutoScroll = true;

            if (datasets == null || datasets.Count == 0)
                return;

            int blockHeight = _panel.Height / datasets.Count;
            if (blockHeight < 60) blockHeight = 60;

            for (int i = 0; i < datasets.Count; i++)
            {
                var picture = new PictureBox
                {
                    Height = blockHeight,
                    Width = _panel.Width - 25,
                    Top = i * blockHeight,
                    Left = 0,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var color = colors != null && i < colors.Count
                    ? colors[i]
                    : ColorPalette.Get(i);

                var bmp = new Bitmap(
                    picture.Width > 0 ? picture.Width : 400,
                    picture.Height > 0 ? picture.Height : 60);

                using (var g = Graphics.FromImage(bmp))
                {
                    DrawDataset(g, datasets[i], color, names[i]);
                }

                picture.Image = bmp;
                _panel.Controls.Add(picture);
            }
        }

        // === ОДИНОЧНАЯ ДИАГРАММА (для одиночной анимации) ===
        public void DrawSingle(
            List<double> data,
            Color color,
            string title = null)
        {
            _panel.Controls.Clear();
            _panel.AutoScroll = false;

            if (data == null || data.Count == 0)
                return;

            var picture = new PictureBox
            {
                Height = _panel.Height - 5,
                Width = _panel.Width - 5,
                Top = 0,
                Left = 0,
                BackColor = Color.White
            };

            var bmp = new Bitmap(
                picture.Width > 0 ? picture.Width : 400,
                picture.Height > 0 ? picture.Height : 100);

            using (var g = Graphics.FromImage(bmp))
            {
                DrawDataset(g, data, color, title ?? "");
            }

            picture.Image = bmp;
            _panel.Controls.Add(picture);
        }

        // === ПАРАЛЛЕЛЬНАЯ АНИМАЦИЯ (все алгоритмы одновременно) ===
        public void DrawAnimationFrame(
            List<List<double>> currentFrames,
            List<string> names,
            List<int> frameIndices,
            List<int> totalFrames,
            List<Color> colors,
            List<bool> finished)
        {
            _panel.Controls.Clear();
            _panel.AutoScroll = false;

            if (currentFrames == null || currentFrames.Count == 0)
                return;

            int n = currentFrames.Count;
            int blockHeight = _panel.Height / n;
            if (blockHeight < 50) blockHeight = 50;

            for (int i = 0; i < n; i++)
            {
                var picture = new PictureBox
                {
                    Height = blockHeight,
                    Width = _panel.Width - 5,
                    Top = i * blockHeight,
                    Left = 0,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var color = colors[i];
                var bmp = new Bitmap(
                    picture.Width > 0 ? picture.Width : 400,
                    picture.Height > 0 ? picture.Height : 60);

                string title = $"{names[i]} — кадр " +
                               $"{frameIndices[i] + 1}/{totalFrames[i]}";

                if (finished[i])
                    title = $"{names[i]} — финал ✅";

                using (var g = Graphics.FromImage(bmp))
                {
                    DrawDataset(g, currentFrames[i], color, title);
                }

                picture.Image = bmp;
                _panel.Controls.Add(picture);
            }
        }

        private void DrawDataset(
            Graphics g,
            List<double> data,
            Color color,
            string name)
        {
            int width = (int)g.VisibleClipBounds.Width;
            int height = (int)g.VisibleClipBounds.Height;

            g.Clear(Color.White);

            if (data == null || data.Count == 0) return;

            double min = data[0];
            double max = data[0];
            foreach (var v in data)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            double range = max - min;
            if (range == 0) range = 1;

            float barWidth = (float)width / data.Count;
            if (barWidth < 1) barWidth = 1;

            using (var brush = new SolidBrush(color))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    float barHeight = (float)((data[i] - min) / range
                        * (height - 25));

                    g.FillRectangle(
                        brush,
                        i * barWidth,
                        height - barHeight - 18,
                        barWidth > 1 ? barWidth - 0.5f : 1,
                        barHeight);
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString(name, font, textBrush, 5, 3);
                }
            }
        }
    }
}