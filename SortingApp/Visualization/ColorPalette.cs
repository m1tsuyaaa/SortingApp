using System.Collections.Generic;
using System.Drawing;

namespace SortingApp.Visualization
{
  public static class ColorPalette
  {
    public static readonly List<Color> Colors = new List<Color>
        {
            Color.FromArgb(220, 53, 69),    // красный
            Color.FromArgb(0, 123, 255),    // синий
            Color.FromArgb(40, 167, 69),    // зелёный
            Color.FromArgb(255, 153, 0),    // оранжевый
            Color.FromArgb(111, 66, 193)    // фиолетовый
        };

    public static Color Get(int index)
    {
      return Colors[index % Colors.Count];
    }
  }
}