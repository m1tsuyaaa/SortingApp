using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OfficeOpenXml;

namespace SortingApp.Services
{
  public static class ExcelLoader
  {
    public static List<double> Load(string filePath)
    {
      var result = new List<double>();

      var fileInfo = new FileInfo(filePath);
      using (var package = new ExcelPackage(fileInfo))
      {
        var worksheet = package.Workbook.Worksheets[1];
        if (worksheet?.Dimension == null)
          return result;

        int rows = worksheet.Dimension.End.Row;
        int cols = worksheet.Dimension.End.Column;

        for (int r = 1; r <= rows; r++)
        {
          for (int c = 1; c <= cols; c++)
          {
            var cell = worksheet.Cells[r, c].Value;
            if (cell == null) continue;

            if (double.TryParse(
                cell.ToString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double val))
            {
              result.Add(val);
            }
          }
        }
      }

      return result;
    }
  }
}