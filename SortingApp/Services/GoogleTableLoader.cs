using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace SortingApp.Services
{
  public static class GoogleTableLoader
  {
    /// <summary>
    /// Загружает данные из опубликованной Google-таблицы в CSV.
    /// Ссылка должна быть в формате:
    /// https://docs.google.com/spreadsheets/d/{ID}/export?format=csv
    /// </summary>
    public static async Task<List<double>> LoadAsync(string url)
    {
      var result = new List<double>();

      using (var client = new HttpClient())
      {
        string csv = await client.GetStringAsync(url);
        var lines = csv.Split(new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
          var parts = line.Split(new[] { ',', ';', '\t' },
              StringSplitOptions.RemoveEmptyEntries);

          foreach (var part in parts)
          {
            string cleaned = part.Trim().Replace("\"", "");
            if (double.TryParse(
                cleaned,
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