namespace SSMSSQLComplete.Core.Schema
{
    public class ColumnMetadata
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsIdentity { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public int OrdinalPosition { get; set; }

        public override string ToString()
        {
            return $"{Name} {DataType}{(IsNullable ? " NULL" : " NOT NULL")}";
        }
    }
}
