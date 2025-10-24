using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;

namespace MauiApp1;

public class TableCell
{
    public string Expression { get; set; }  
    public string Value { get; set; }      
    public TableCell(string expression = "")
    {
        Expression = expression;
        Value = expression;
    }
}

public partial class TablePage : ContentPage
{
    private int rows;
    private int columns;
    private TableCell[,] tableCells;
    private Entry[,] tableEntries; 
    private bool showValues = false;
    public TablePage(bool createNew = true)
    {
        InitializeComponent();

        if (createNew)
        {
            rows = 0;
            columns = 0;
            PageTitle.Text = "Нова таблиця";
        }
        else
        {
            PageTitle.Text = "Відкрита таблиця";
            _ = OpenFileAsync();
        }
    }

    private void OnAddRowClicked(object sender, EventArgs e)
    {
        ResizeArrays(rows + 1, columns);
        TableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int c = 0; c < columns; c++)
        {
            tableCells[rows, c] = new TableCell();
            var entry = CreateEntry(rows, c);
            TableGrid.Add(entry, c, rows);
            tableEntries[rows, c] = entry;
        }

        rows++;
    }

    private void OnAddColumnClicked(object sender, EventArgs e)
    {
        ResizeArrays(rows, columns + 1);
        TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        for (int r = 0; r < rows; r++)
        {
            tableCells[r, columns] = new TableCell();
            var entry = CreateEntry(r, columns);
            TableGrid.Add(entry, columns, r);
            tableEntries[r, columns] = entry;
        }

        columns++;
    }

    private void ResizeArrays(int newRows, int newCols)
    {
        var newCells = new TableCell[newRows, newCols];
        var newEntries = new Entry[newRows, newCols];

        if (tableCells != null)
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    newCells[r, c] = tableCells[r, c];
                    newEntries[r, c] = tableEntries[r, c];
                }
        }

        tableCells = newCells;
        tableEntries = newEntries;
        rows = newRows > rows ? rows : newRows;
        columns = newCols > columns ? columns : newCols;
    }
   private void OnViewToggled(object sender, ToggledEventArgs e)
{
    showValues = e.Value;

    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < columns; c++)
        {
            var entry = tableEntries[r, c];
            var cell = tableCells[r, c];
            if (entry == null || cell == null) continue;

            entry.TextChanged -= Entry_TextChanged;

            if (showValues)
                entry.Text = cell.Value; 
            else
                entry.Text = cell.Expression; 

            entry.TextChanged += Entry_TextChanged;
        }
    }
}


    // Обробник вводу користувача
    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (tableEntries[r, c] == entry)
                    {
                        tableCells[r, c].Expression = entry.Text;
                  
                        tableCells[r, c].Value = EvaluateExpression(entry.Text);
                        break;
                    }
                }
            }
        }
        RecalculateAll();
    }
    private void RecalculateAll()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var cell = tableCells[r, c];
                if (cell == null) continue;

                string expr = cell.Expression;
                cell.Value = EvaluateExpression(expr);

  
            }
        }
    }

    private string EvaluateExpression(string expr)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(expr))
                return "";

            expr = ReplaceCellReferences(expr).Trim();

            // логічні оператори 
            string lowered = expr.ToLower();

            // підтримка not
            if (lowered.StartsWith("not "))
            {
                var inner = lowered.Substring(4);
                string val = EvaluateExpression(inner);
                return (val == "0" || val == "" || val == "ERR") ? "1" : "0";
            }

            //  підтримка and / or
            if (lowered.Contains(" or ") || lowered.Contains(" and "))
            {
          
                string[] parts;
                if (lowered.Contains(" or "))
                {
                    parts = lowered.Split(new[] { " or " }, StringSplitOptions.None);
                    foreach (string p in parts)
                    {
                        string val = EvaluateExpression(p);
                        if (val != "0" && val != "ERR" && val != "")
                            return "1";
                    }
                    return "0";
                }
                else if (lowered.Contains(" and "))
                {
                    parts = lowered.Split(new[] { " and " }, StringSplitOptions.None);
                    foreach (string p in parts)
                    {
                        string val = EvaluateExpression(p);
                        if (val == "0" || val == "ERR" || val == "")
                            return "0";
                    }
                    return "1";
                }
            }

            //обчислюємо через DataTable
            expr = expr
                .Replace("eqv", "=")
                .Replace(">=", "≥")
                .Replace("<=", "≤")
                .Replace("=", "==")
                .Replace("≥", ">=")
                .Replace("≤", "<=")
                .Replace("==", "=");

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Eval", typeof(object), expr);
            var row = dt.NewRow();
            dt.Rows.Add(row);
            var result = row["Eval"];

            if (result is bool b)
                return b ? "1" : "0";

            return result.ToString();
        }
        catch
        {
            return "ERR";
        }
    }




    private Entry CreateEntry(int r, int c)
    {
        var entry = new Entry
        {
            Text = tableCells[r, c]?.Expression ?? "",
            TextColor = Colors.Black,           
            BackgroundColor = Colors.White,     
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = 1
        };
        entry.TextChanged += Entry_TextChanged;
        return entry;
    }
 

    private string GetCellName(int row, int col)
    {
        char colChar = (char)('A' + col);
        return $"{colChar}{row + 1}";
    }

    private void LoadTableFromText(string text)
    {
        TableGrid.RowDefinitions.Clear();
        TableGrid.ColumnDefinitions.Clear();
        TableGrid.Children.Clear();

        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return;

        rows = lines.Length;
        columns = lines[0].Split(',').Length;

        tableCells = new TableCell[rows, columns];
        tableEntries = new Entry[rows, columns];

        for (int c = 0; c < columns; c++)
            TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        for (int r = 0; r < rows; r++)
        {
            TableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cells = lines[r].Split(',');

            for (int c = 0; c < columns; c++)
            {
                string expr = c < cells.Length ? cells[c] : "";
                tableCells[r, c] = new TableCell(expr);

                var entry = CreateEntry(r, c);
                TableGrid.Add(entry, c, r);
                tableEntries[r, c] = entry;
            }
        }
    }

    private async Task OpenFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть текстовий файл з таблицею",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".txt" } }
                })
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                LoadTableFromText(content);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося відкрити файл: {ex.Message}", "OK");
        }
    }

    
    private void OnCalculateClicked(object sender, EventArgs e)
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
            {
                try
                {
                    string expr = tableCells[r, c].Expression;
                    string replaced = ReplaceCellReferences(expr);
                    var dt = new System.Data.DataTable();
                    tableCells[r, c].Value = dt.Compute(replaced, "").ToString();
                    tableEntries[r, c].Text = tableCells[r, c].Value;
                }
                catch
                {
                    tableCells[r, c].Value = "ERR";
                    tableEntries[r, c].Text = "ERR";
                }
            }
    }

    private string ReplaceCellReferences(string expr)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                string cellName = GetCellName(r, c);
                string value = tableCells[r, c].Value ?? "0";
               
                expr = System.Text.RegularExpressions.Regex.Replace(
                    expr,
                    $@"\b{cellName}\b",
                    value
                );
            }
        }
        return expr;
    }
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await SaveFileAsync();
    }

    private async Task SaveFileAsync()
    {
        try
        {
            
            var sb = new StringBuilder();
            for (int r = 0; r < rows; r++)
            {
                var rowValues = new List<string>();
                for (int c = 0; c < columns; c++)
                {
                    string expr = tableCells[r, c]?.Expression ?? "";
                   
                    rowValues.Add(expr.Replace(",", ";"));
                }
                sb.AppendLine(string.Join(",", rowValues));
            }

            string textToSave = sb.ToString();

      
            string fileName = await DisplayPromptAsync("Збереження", "Введіть назву файлу (без розширення):",
                                                       "OK", "Відміна", "таблиця");

            if (string.IsNullOrWhiteSpace(fileName))
                return;

      
            if (!fileName.EndsWith(".txt"))
                fileName += ".txt";

            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            File.WriteAllText(filePath, textToSave, Encoding.UTF8);

            await DisplayAlert("✅ Успіх", $"Файл збережено:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Помилка", $"Не вдалося зберегти файл:\n{ex.Message}", "OK");
        }
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }



}
