using System.Collections.Generic;
using System.Linq;

namespace SSMSSQLComplete.Core.Parsing
{
    public static class TSqlKeywords
    {
        private static readonly HashSet<string> _keywords = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            // DML Keywords
            "SELECT", "INSERT", "UPDATE", "DELETE", "MERGE",
            "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "OUTER", "CROSS", "FULL",
            "ON", "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "IS", "NULL",
            "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC",
            "DISTINCT", "ALL", "TOP", "OFFSET", "FETCH", "NEXT", "ROWS", "ONLY",
            "AS", "INTO", "VALUES", "SET",

            // DDL Keywords
            "CREATE", "ALTER", "DROP", "TRUNCATE",
            "TABLE", "VIEW", "INDEX", "PROCEDURE", "FUNCTION", "TRIGGER",
            "DATABASE", "SCHEMA",
            "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "UNIQUE", "CHECK", "DEFAULT",
            "CONSTRAINT", "CLUSTERED", "NONCLUSTERED",

            // Data Types
            "INT", "INTEGER", "BIGINT", "SMALLINT", "TINYINT",
            "DECIMAL", "NUMERIC", "FLOAT", "REAL", "MONEY", "SMALLMONEY",
            "VARCHAR", "CHAR", "NVARCHAR", "NCHAR", "TEXT", "NTEXT",
            "DATE", "TIME", "DATETIME", "DATETIME2", "SMALLDATETIME", "DATETIMEOFFSET",
            "BIT", "BINARY", "VARBINARY", "IMAGE",
            "XML", "UNIQUEIDENTIFIER",

            // Functions
            "COUNT", "SUM", "AVG", "MIN", "MAX",
            "CAST", "CONVERT", "COALESCE", "ISNULL", "NULLIF",
            "CASE", "WHEN", "THEN", "ELSE", "END",
            "EXISTS", "ROW_NUMBER", "RANK", "DENSE_RANK", "OVER", "PARTITION",

            // Other Keywords
            "BEGIN", "END", "IF", "ELSE", "WHILE", "BREAK", "CONTINUE", "RETURN",
            "GO", "USE", "EXEC", "EXECUTE", "PRINT", "DECLARE", "SET",
            "WITH", "CTE", "UNION", "INTERSECT", "EXCEPT",
            "GRANT", "REVOKE", "DENY",
            "TRANSACTION", "COMMIT", "ROLLBACK", "SAVEPOINT",
            "TRY", "CATCH", "THROW", "RAISERROR"
        };

        public static bool IsKeyword(string word)
        {
            return _keywords.Contains(word);
        }

        public static IEnumerable<string> AllKeywords => _keywords.OrderBy(k => k);

        public static IEnumerable<string> ClauseKeywords => new[]
        {
            "SELECT", "FROM", "WHERE", "GROUP BY", "HAVING", "ORDER BY",
            "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "OUTER JOIN", "CROSS JOIN"
        };
    }
}
