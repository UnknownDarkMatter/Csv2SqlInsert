using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv2SqlInsert;

public class ColumnEntity
{
    public string Name { get; set; }
    public bool Ignored { get; set; }
    public ColumnTypeEnum ColumnType { get; set; }
}
