namespace SSMSSQLComplete.Core.Schema
{
    public class ForeignKeyMetadata
    {
        public string Name { get; set; }
        public string FromTable { get; set; }
        public string FromColumn { get; set; }
        public string ToTable { get; set; }
        public string ToColumn { get; set; }
        public string Schema { get; set; }

        public ForeignKeyMetadata()
        {
            Schema = "dbo";
        }

        public override string ToString()
        {
            return $"{Name}: {FromTable}.{FromColumn} -> {ToTable}.{ToColumn}";
        }
    }
}
