using Csv2SqlInsert;
using System.Data;
using System.Text.Json;

//Read input argument
var configAsString = File.ReadAllText("./config.json");
TablesCollection config = JsonSerializer.Deserialize<TablesCollection>(configAsString)!;
string columnSeparator = config.ColumnSeparator;
string outputSql = "BEGIN TRAN\r\n";
int tableCount = 1;

var filter = new List<string>()
{
    //"Poids"
};

foreach (var tableEntity in config.Tables.Where(m => filter.Count == 0 || filter.Contains(m.Name)))
{
    Console.WriteLine($"{DateTime.Now} Reading {tableEntity.Name}.csv ...");
    string filePathCsv = Path.Combine(config.Folder, $"{tableEntity.Name}.csv");
    string tableSql = "BEGIN TRAN\r\n";

    //confert csv to DataTable
    var content = File.ReadAllText(filePathCsv);
    var table = new DataTable();
    bool isEspacedString = false;
    bool isHeader = true;
    string value = "";
    DataRow dataRow = null;
    int columnIndex = 0;
    int carIndex = 0;
    while (carIndex < content.Length)
    {
        var car1 = content[carIndex].ToString();
        var car2 = carIndex < content.Length - 1 ? content[carIndex + 1].ToString() : "";
        if (car1 == "\"")
        {
            if (isEspacedString)
            {
                if (isHeader)
                {
                    table.Columns.Add(new DataColumn(value));
                }
                else
                {
                    dataRow[columnIndex] = value;
                }
            }
            else
            {
                value = "";
            }
            isEspacedString = !isEspacedString;
        }
        else
        {
            if (car1 == "\r" && car2 == "\n" || carIndex == content.Length - 1)
            {
                if (isEspacedString)
                {
                    value += " ";
                }
                else
                {
                    if (carIndex == content.Length - 1 && car1 != columnSeparator)
                    {
                        value += car1;
                    }
                    if (isHeader)
                    {
                        table.Columns.Add(new DataColumn(value));
                        isHeader = false;
                    }
                    else
                    {
                        dataRow[columnIndex] = value;
                        table.Rows.Add(dataRow);
                    }
                    columnIndex = 0;
                    value = "";
                    dataRow = table.NewRow();
                }
                carIndex++;
            }
            else if (car1 == columnSeparator)
            {
                if (isEspacedString)
                {
                    value += " ";
                }
                else
                {
                    if (isHeader)
                    {
                        table.Columns.Add(new DataColumn(value));
                    }
                    else
                    {
                        dataRow[columnIndex] = value;
                    }
                    columnIndex++;
                    value = "";
                }
            }
            else
            {
                value += car1;
            }
        }
        carIndex++;
    }

    Console.WriteLine($"{DateTime.Now} Generating SQL INSERT for  {tableEntity.Name} ({tableCount}/{config.Tables.Count()})...");
    if (tableEntity.HasIdentity)
    {
        outputSql += $"SET IDENTITY_INSERT [{tableEntity.Name}] ON; \r\n";
        tableSql += $"SET IDENTITY_INSERT [{tableEntity.Name}] ON; \r\n";
    }
    decimal nbRows = table.Rows.Count;
    decimal count = 0;
    List<decimal> ratios = new List<decimal>() { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
    foreach (DataRow dr in table.Rows)
    {
        decimal ratio = count * 100 / nbRows;
        foreach (var r in ratios.OrderByDescending(m => m))
        {
            if (ratio > 0 && ratio % r == 0)
            {
                Console.WriteLine($"{DateTime.Now} {r}%");
                break;
            }
        }
        outputSql += DataRow2Sql(dr, tableEntity, table);
        tableSql += DataRow2Sql(dr, tableEntity, table);
        count++;
    }
    if (tableEntity.HasIdentity)
    {
        outputSql += $"SET IDENTITY_INSERT [{tableEntity.Name}] OFF; \r\n";
        tableSql += $"SET IDENTITY_INSERT [{tableEntity.Name}] OFF; \r\n";
    }
    //File.WriteAllText($"debug_{tableEntity.Name}.sql", tableSql);
    File.WriteAllText($"script.sql", outputSql);
    tableCount++;
}
outputSql += "COMMIT\r\n";

File.WriteAllText($"script.sql", outputSql);
Console.WriteLine($"{DateTime.Now} Done");

static string DataRow2Sql(DataRow dr, TableEntity tableEntity, DataTable table)
{
    string columsList = string.Join(",", tableEntity.Columns.Where(m => !m.Ignored).Select(m => $"[{m.Name}]"));
    string valuesList = "";
    foreach (var columnConfig in tableEntity.Columns.Where(m => !m.Ignored))
    {
        var dc = table.Columns[columnConfig.Name];
        var value = dr[dc];
        if (valuesList != "") { valuesList += ", "; }
        valuesList += Column2Sql(value, columnConfig);
    }
    return $"INSERT INTO [{tableEntity.Name}] ({columsList}) VALUES ({valuesList}); \r\n";
}

static string Column2Sql(object value, ColumnEntity columnEntity)
{
    if ((value ?? "").ToString().Trim().ToUpper() == "NULL") { return "NULL"; }

    string valueAsString = "";
    switch (columnEntity.ColumnType)
    {
        case ColumnTypeEnum.INT:
            {
                if ((value ?? "").ToString().Trim().ToUpper() == "") { return "NULL"; }
                valueAsString = $"{value}";
                break;
            }
        case ColumnTypeEnum.VARCHAR:
            {
                valueAsString = $"'{value.ToString().Replace("'", "''")}'";
                break;
            }
        case ColumnTypeEnum.DATETIME:
            {
                if ((value ?? "").ToString().Trim().ToUpper() == "") { return "NULL"; }
                valueAsString = $"'{(value ?? "").ToString().Substring(0, "yyyy-mm-ddThh:mi:ss.mmm".Length)}'";
                valueAsString = $"CONVERT(DATETIME, {valueAsString}, 126)";
                break;
            }
        case ColumnTypeEnum.BIT:
            {
                if ((value ?? "").ToString().Trim().ToUpper() == "") { return "NULL"; }
                valueAsString = ((value ?? "").ToString().Trim().ToUpper() == "TRUE") ? "1" : "0";
                break;
            }
        case ColumnTypeEnum.DECIMAL:
            {
                if ((value ?? "").ToString().Trim().ToUpper() == "") { return "NULL"; }
                valueAsString = (value ?? "").ToString().Replace(",", ".");
                break;
            }
        default:
            {
                break;
            }
    }

    return $"{valueAsString}";
}