using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv2SqlInsert;

public class TablesCollection
{
    public string Folder { get; set; }
    public string ColumnSeparator { get; set; }
    public IEnumerable<TableEntity> Tables { get; set; }
}
