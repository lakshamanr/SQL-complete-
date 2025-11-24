using System.Collections.Generic;
using System.Linq;

namespace SSMSSQLComplete.Core.Schema
{
    public class DatabaseMetadata
    {
        public string DatabaseName { get; set; }
        public List<TableMetadata> Tables { get; set; }
        public List<TableMetadata> Views { get; set; }
        public List<ForeignKeyMetadata> ForeignKeys { get; set; }
        public List<string> StoredProcedures { get; set; }
        public List<string> Functions { get; set; }

        public DatabaseMetadata()
        {
            Tables = new List<TableMetadata>();
            Views = new List<TableMetadata>();
            ForeignKeys = new List<ForeignKeyMetadata>();
            StoredProcedures = new List<string>();
            Functions = new List<string>();
        }

        public TableMetadata GetTable(string name)
        {
            return Tables.FirstOrDefault(t =>
                t.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ColumnMetadata> GetColumns(string tableName)
        {
            var table = GetTable(tableName);
            return table?.Columns ?? Enumerable.Empty<ColumnMetadata>();
        }
    }
}
