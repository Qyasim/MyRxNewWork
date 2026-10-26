using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using Akelon.MyModule.Structures.Module;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using System.Globalization;
using System.IO;

namespace Akelon.MyModule.Isolated.AsposeFunctions
{
  public partial class IsolatedFunctions
  {
    public Stream GenerateCalendarPdf(int year, int month)
    {
        // Создаём новый PDF документ
        var document = new Document();
        var page = document.Pages.Add();
        
        // Настройка страницы (A4, альбомная или книжная)
        page.PageInfo.Margin = new MarginInfo(36, 36, 36, 36); // Отступы

        // 1. Заголовок (Месяц и Год)
        var title = new TextFragment($"{CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetMonthName(month)} {year}");
        title.TextState.Font = FontRepository.FindFont("Arial");
        title.TextState.FontSize = 24;
        title.TextState.FontStyle = FontStyles.Bold;
        title.HorizontalAlignment = HorizontalAlignment.Center;
        page.Paragraphs.Add(title);

        // Добавим немного места после заголовка
        page.Paragraphs.Add(new TextFragment(" "));

        // 2. Создаём таблицу для календаря
        var table = new Table
        {
            ColumnWidths = "70 70 70 70 70 70 70", // 7 колонок
            DefaultCellBorder = new BorderInfo(BorderSide.All, 0.5f, Color.Gray),
            DefaultCellPadding = new MarginInfo(5, 5, 5, 5),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // 3. Заголовки дней недели
        var headerRow = table.Rows.Add();
        string[] dayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
        for (int i = 0; i < 7; i++)
        {
            var cell = headerRow.Cells.Add();
            var text = new TextFragment(dayNames[i]);
            text.TextState.Font = FontRepository.FindFont("Arial");
            text.TextState.FontSize = 12;
            text.TextState.FontStyle = FontStyles.Bold;
            // Выходные дни (Сб, Вс) можно выделить цветом
            if (i >= 5) text.TextState.ForegroundColor = Color.Red;
            cell.Paragraphs.Add(text);
            cell.BackgroundColor = Color.LightGray;
            cell.Alignment = HorizontalAlignment.Center;
        }

        // 4. Заполнение днями
        var firstDayOfMonth = new DateTime(year, month, 1);
        // Находим понедельник первой недели
        int diff = ((int)firstDayOfMonth.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var calendarStart = firstDayOfMonth.AddDays(-diff);

        var currentDate = calendarStart;
        // Рисуем 6 недель (стандартный максимум для месяца)
        for (int week = 0; week < 6; week++)
        {
            var row = table.Rows.Add();
            for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
            {
                var cell = row.Cells.Add();
                cell.Alignment = HorizontalAlignment.Center;
                
                // Если день не принадлежит текущему месяцу, делаем его серым
                if (currentDate.Month != month)
                {
                    cell.BackgroundColor = Color.WhiteSmoke;
                    var grayText = new TextFragment(currentDate.Day.ToString());
                    grayText.TextState.ForegroundColor = Color.LightGray;
                    grayText.TextState.FontSize = 14;
                    cell.Paragraphs.Add(grayText);
                }
                else
                {
                    var dayText = new TextFragment(currentDate.Day.ToString());
                    dayText.TextState.FontSize = 14;
                    dayText.TextState.Font = FontRepository.FindFont("Arial");
                    
                    // Выходные дни
                    if (dayOfWeek >= 5) 
                    {
                        dayText.TextState.ForegroundColor = Color.Red;
                        cell.BackgroundColor = Color.MistyRose; // Легкий розовый фон
                    }
                    cell.Paragraphs.Add(dayText);
                }

                currentDate = currentDate.AddDays(1);
            }
        }

        page.Paragraphs.Add(table);

        // Сохраняем в поток
        var stream = new MemoryStream();
        document.Save(stream);
        stream.Position = 0; // Сбрасываем позицию для чтения
        return stream;
    }
  }
}