using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv2SqlInsert;

public enum ColumnTypeEnum
{
    INT = 1,
    VARCHAR = 2,
    DATETIME = 3,
    BIT = 4,
    DECIMAL = 5,
    VARBINARY = 6
}
