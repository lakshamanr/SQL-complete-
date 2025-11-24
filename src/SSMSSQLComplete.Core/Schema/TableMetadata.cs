using System.Collections.Generic;

namespace SSMSSQLComplete.Core.Schema
{
    public class TableMetadata
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Type { get; set; } // BASE TABLE, VIEW
        public List<ColumnMetadata> Columns { get; set; }
        public string Description { get; set; }

        public TableMetadata()
        {
            Columns = new List<ColumnMetadata>();
            Schema = "dbo";
        }

        public string FullName => $"{Schema}.{Name}";

        public override string ToString()
        {
            return FullName;
        }
    }
}
