using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Schema
{
    public interface ISchemaProvider
    {
        /// <summary>
        /// Fetches database metadata from the connection
        /// </summary>
        Task<DatabaseMetadata> FetchMetadataAsync(string connectionString, string databaseName);

        /// <summary>
        /// Tests if the connection is valid
        /// </summary>
        Task<bool> TestConnectionAsync(string connectionString);
    }
}
