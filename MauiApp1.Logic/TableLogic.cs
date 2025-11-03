namespace MauiApp1.Logic;

public class TableLogic
{
    private readonly TableCell[,] tableCells;
    private int rows;
    private int columns;

    public TableLogic(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        tableCells = new TableCell[rows, columns];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                tableCells[r, c] = new TableCell();
    }

    public void SetExpression(int row, int col, string expr)
    {
        tableCells[row, col].Expression = expr;
        tableCells[row, col].Value = EvaluateExpression(expr);
    }

    public string GetValue(int row, int col)
    {
        return tableCells[row, col].Value;
    }

    private string EvaluateExpression(string expr)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(expr)) return "";

            // простий варіант: тільки числа та +, -, *, /
            var dt = new System.Data.DataTable();
            var result = dt.Compute(expr, "");
            return result.ToString();
        }
        catch
        {
            return "ERR";
        }
    }
}

public class TableCell
{
    public string Expression { get; set; }
    public string Value { get; set; }

    public TableCell(string expr = "")
    {
        Expression = expr;
        Value = expr;
    }
}
