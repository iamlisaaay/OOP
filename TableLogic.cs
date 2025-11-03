using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace MauiApp1.Logic
{
    public class TableCell
    {
        public string Expression { get; set; } = "";
        public string Value { get; set; } = "";
        public TableCell(string expr = "") { Expression = expr; Value = expr; }
    }

    public class TableLogic
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public TableCell[,] Cells { get; private set; }

        public TableLogic(int rows = 0, int cols = 0)
        {
            Resize(rows, cols);
        }

        public void Resize(int newRows, int newCols)
        {
            var newCells = new TableCell[newRows, newCols];
            for (int r = 0; r < newRows; r++)
                for (int c = 0; c < newCols; c++)
                    newCells[r, c] = (r < Rows && c < Columns && Cells != null) ? Cells[r, c] ?? new TableCell() : new TableCell();

            Cells = newCells;
            Rows = newRows;
            Columns = newCols;
        }

        public string GetCellName(int row, int col)
        {
            char colChar = (char)('A' + col);
            return $"{colChar}{row + 1}";
        }

        public string ReplaceCellReferences(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return expr;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    string cellName = GetCellName(r, c);
                    string value = Cells[r, c]?.Value ?? "0";
                    expr = Regex.Replace(expr, $@"\b{Regex.Escape(cellName)}\b", value);
                }
            }
            return expr;
        }

        public string EvaluateExpression(string expr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expr)) return "";

                expr = ReplaceCellReferences(expr).Trim();
                string lowered = expr.ToLower();

                // логічні операції
                if (lowered.StartsWith("not "))
                {
                    var inner = expr.Substring(4);
                    string val = EvaluateExpression(inner);
                    return (val == "0" || val == "" || val == "ERR") ? "1" : "0";
                }

                if (lowered.Contains(" or ") || lowered.Contains(" and "))
                {
                    if (lowered.Contains(" or "))
                    {
                        var parts = expr.Split(new[] { " or " }, StringSplitOptions.None);
                        foreach (var p in parts)
                        {
                            string val = EvaluateExpression(p);
                            if (val != "0" && val != "ERR" && val != "") return "1";
                        }
                        return "0";
                    }
                    else
                    {
                        var parts = expr.Split(new[] { " and " }, StringSplitOptions.None);
                        foreach (var p in parts)
                        {
                            string val = EvaluateExpression(p);
                            if (val == "0" || val == "ERR" || val == "") return "0";
                        }
                        return "1";
                    }
                }

                // арифметика
                string prepared = expr
                    .Replace("eqv", "=")
                    .Replace(">=", "≥")
                    .Replace("<=", "≤")
                    .Replace("=", "==")
                    .Replace("≥", ">=")
                    .Replace("≤", "<=")
                    .Replace("==", "=");

                var dt = new DataTable();
                dt.Columns.Add("Eval", typeof(object), prepared);
                var row = dt.NewRow();
                dt.Rows.Add(row);
                var result = row["Eval"];

                if (result is bool b) return b ? "1" : "0";
                return result?.ToString() ?? "";
            }
            catch
            {
                return "ERR";
            }
        }

        public void RecalculateAll()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                {
                    var cell = Cells[r, c];
                    if (cell == null) continue;
                    cell.Value = EvaluateExpression(cell.Expression);
                }
        }
    }
}
