using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv2SqlInsert;

public class TableEntity
{
    public string Name { get; set; }
    public bool HasIdentity { get; set; }
    public IEnumerable<ColumnEntity> Columns { get; set; }
}
