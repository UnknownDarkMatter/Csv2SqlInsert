using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv2SqlInsert
{
    public static class Utils
    {
        public static ColumnTypeEnum SqlTypeToTypeEnum(int sqlType)
        {
            //INT = 1,
            //VARCHAR = 2,
            //DATETIME = 3,
            //BIT = 4,
            //DECIMAL = 5,
            //VARBINARY = 6,
            switch (sqlType) {
                case 56:
                    {
                        return ColumnTypeEnum.INT;
                    }
                case 127:
                    {
                        return ColumnTypeEnum.INT;
                    }
                case 231:
                    {
                        return ColumnTypeEnum.VARCHAR;
                    }
                case 104:
                    {
                        return ColumnTypeEnum.BIT;
                    }
                case 42:
                    {
                        return ColumnTypeEnum.DATETIME;
                    }
                case 43:
                    {
                        return ColumnTypeEnum.DATETIME;
                    }
                case 165:
                    {
                        return ColumnTypeEnum.VARBINARY;
                    }
                case 106:
                    {
                        return ColumnTypeEnum.DECIMAL;
                    }
                default:
                    {
                        throw new NotImplementedException(sqlType.ToString());
                    }
            }

        }

        public static bool HasIdentity(string tableName, string[] dbcolumns)
        {
            int identityColIndex = 0;
            var cells = dbcolumns[0].Split(',');
            foreach (string cell in cells)
            {
                if (cell == "is_identity")
                {
                    break;
                }
                identityColIndex++;
            }

            bool firstLine = true;
            foreach (var line in dbcolumns)
            {
                if (firstLine) { firstLine = false; continue; }
                var cells2 = line.Split(',');
                string tableNameCurrent = cells2[0];
                if(tableNameCurrent == tableName)
                {
                    if (cells2[identityColIndex] == "True")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
