using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Schema
{
    public class SchemaFetcher : ISchemaProvider
    {
        private const string TablesQuery = @"
            SELECT
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                t.TABLE_TYPE
            FROM INFORMATION_SCHEMA.TABLES t
            WHERE t.TABLE_TYPE IN ('BASE TABLE', 'VIEW')
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME";

        private const string ColumnsQuery = @"
            SELECT
                c.TABLE_SCHEMA,
                c.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                c.IS_NULLABLE,
                c.ORDINAL_POSITION,
                c.COLUMN_DEFAULT,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY,
                CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_FOREIGN_KEY,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA
                AND c.TABLE_NAME = pk.TABLE_NAME
                AND c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
            ) fk ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA
                AND c.TABLE_NAME = fk.TABLE_NAME
                AND c.COLUMN_NAME = fk.COLUMN_NAME
            ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION";

        private const string ForeignKeysQuery = @"
            SELECT
                fk.name AS FK_NAME,
                OBJECT_SCHEMA_NAME(fk.parent_object_id) AS SCHEMA_NAME,
                OBJECT_NAME(fk.parent_object_id) AS FROM_TABLE,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS FROM_COLUMN,
                OBJECT_NAME(fk.referenced_object_id) AS TO_TABLE,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS TO_COLUMN
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc
                ON fk.object_id = fkc.constraint_object_id
            ORDER BY FROM_TABLE, FK_NAME";

        private const string StoredProceduresQuery = @"
            SELECT
                SCHEMA_NAME(schema_id) + '.' + name AS PROC_NAME
            FROM sys.procedures
            WHERE is_ms_shipped = 0
            ORDER BY name";

        private const string FunctionsQuery = @"
            SELECT
                SCHEMA_NAME(schema_id) + '.' + name AS FUNC_NAME
            FROM sys.objects
            WHERE type IN ('FN', 'IF', 'TF')
            ORDER BY name";

        public async Task<DatabaseMetadata> FetchMetadataAsync(string connectionString, string databaseName)
        {
            var stopwatch = Stopwatch.StartNew();
            var metadata = new DatabaseMetadata { DatabaseName = databaseName };

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Fetch tables and views
                    var tablesDict = new Dictionary<string, TableMetadata>();
                    using (var command = new SqlCommand(TablesQuery, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var table = new TableMetadata
                            {
                                Schema = reader.GetString(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2)
                            };

                            var key = $"{table.Schema}.{table.Name}";
                            tablesDict[key] = table;

                            if (table.Type == "BASE TABLE")
                                metadata.Tables.Add(table);
                            else if (table.Type == "VIEW")
                                metadata.Views.Add(table);
                        }
                    }

                    // Fetch columns
                    using (var command = new SqlCommand(ColumnsQuery, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var schema = reader.GetString(0);
                            var tableName = reader.GetString(1);
                            var key = $"{schema}.{tableName}";

                            if (tablesDict.TryGetValue(key, out var table))
                            {
                                var column = new ColumnMetadata
                                {
                                    Name = reader.GetString(2),
                                    DataType = reader.GetString(3),
                                    MaxLength = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                    Precision = reader.IsDBNull(5) ? (int?)null : (int)reader.GetByte(5),
                                    Scale = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                    IsNullable = reader.GetString(7) == "YES",
                                    OrdinalPosition = reader.GetInt32(8),
                                    DefaultValue = reader.IsDBNull(9) ? null : reader.GetString(9),
                                    IsPrimaryKey = reader.GetInt32(10) == 1,
                                    IsForeignKey = reader.GetInt32(11) == 1,
                                    IsIdentity = reader.IsDBNull(12) ? false : reader.GetInt32(12) == 1
                                };

                                table.Columns.Add(column);
                            }
                        }
                    }

                    // Fetch foreign keys
                    using (var command = new SqlCommand(ForeignKeysQuery, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var fk = new ForeignKeyMetadata
                            {
                                Name = reader.GetString(0),
                                Schema = reader.GetString(1),
                                FromTable = reader.GetString(2),
                                FromColumn = reader.GetString(3),
                                ToTable = reader.GetString(4),
                                ToColumn = reader.GetString(5)
                            };

                            metadata.ForeignKeys.Add(fk);
                        }
                    }

                    // Fetch stored procedures
                    using (var command = new SqlCommand(StoredProceduresQuery, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            metadata.StoredProcedures.Add(reader.GetString(0));
                        }
                    }

                    // Fetch functions
                    using (var command = new SqlCommand(FunctionsQuery, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            metadata.Functions.Add(reader.GetString(0));
                        }
                    }
                }

                stopwatch.Stop();
                Infrastructure.Logger.Instance.Info(
                    $"Schema fetched: {metadata.Tables.Count} tables, {metadata.Views.Count} views in {stopwatch.ElapsedMilliseconds}ms");

                return metadata;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error fetching schema: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
