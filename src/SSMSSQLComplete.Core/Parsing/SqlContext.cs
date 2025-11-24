using System.Collections.Generic;

namespace SSMSSQLComplete.Core.Parsing
{
    public class SqlContext
    {
        /// <summary>
        /// Current SQL clause (SELECT, FROM, WHERE, etc.)
        /// </summary>
        public string CurrentClause { get; set; }

        /// <summary>
        /// Tables referenced in the query
        /// </summary>
        public List<string> Tables { get; set; }

        /// <summary>
        /// Table aliases in the query
        /// </summary>
        public Dictionary<string, string> Aliases { get; set; }

        /// <summary>
        /// Current word being typed
        /// </summary>
        public string CurrentWord { get; set; }

        /// <summary>
        /// Previous token before current position
        /// </summary>
        public SqlToken PreviousToken { get; set; }

        /// <summary>
        /// Indicates if we're after a dot (table.column scenario)
        /// </summary>
        public bool AfterDot { get; set; }

        /// <summary>
        /// Table qualifier before the dot (if AfterDot is true)
        /// </summary>
        public string TableQualifier { get; set; }

        /// <summary>
        /// Nesting level of parentheses
        /// </summary>
        public int ParenthesisLevel { get; set; }

        /// <summary>
        /// Indicates if we're inside a subquery
        /// </summary>
        public bool InSubquery { get; set; }

        public SqlContext()
        {
            Tables = new List<string>();
            Aliases = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"Clause: {CurrentClause}, Tables: {string.Join(", ", Tables)}, AfterDot: {AfterDot}";
        }
    }
}
