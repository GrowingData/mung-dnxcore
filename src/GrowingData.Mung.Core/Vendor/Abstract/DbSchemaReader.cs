using System;
using System.Data;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System.Collections.Generic;
using Npgsql;
using GrowingData.Mung.Core;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Mung.Core {
	public abstract class DbSchemaReader {

		private Func<DbConnection> _connectionFactory;

		public abstract DbTypeConverter TypeConverter { get; }

		/// <summary>
		/// A query that returns the following columns:
		///		table_schema, table_name, column_name, data_type
		///	And the following parameters:
		///		@Schema, @Table
		///	
		/// Example (postgresql):
		///		SELECT table_schema, table_name, column_name, data_type
		///		FROM information_schema.columns C
		///		WHERE table_schema NOT IN ('information_schema', 'pg_catalog')
		///		AND (table_schema = @Schema OR @Schema IS NULL)
		///		AND (table_name = @Table OR @Table IS NULL)
		///		ORDER BY table_schema, ordinal_position
		/// 
		/// </summary>
		public abstract string SchemaSql { get; }

		public DbSchemaReader(Func<DbConnection> cn) {
			_connectionFactory = cn;
		}

		public List<DbTable> GetTables() {
			return GetTables(null, null);
		}

		public List<DbTable> GetTables(string schema) {
			return GetTables(schema, null);
		}

		public List<DbTable> GetTables(string schema, string table) {


			var tables = new Dictionary<string, DbTable>();
			using (var cn = _connectionFactory()) {
				using (var cmd = cn.CreateCommand()) {
					cmd.CommandText = SchemaSql;

					cmd.Parameters.Add(GetParameter(cmd, "Schema", schema));
					cmd.Parameters.Add(GetParameter(cmd, "Table", table));

					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							var tableName = (string)reader["table_name"];
							var tableSchema = (string)reader["table_schema"];
							var columnName = (string)reader["column_name"];
							var columnType = (string)reader["data_type"];

							DbTable tbl = null;

							if (!tables.ContainsKey(tableName)) {
								tbl = new DbTable(tableName, tableSchema);
								tables[tableName] = tbl;
							} else {
								tbl = tables[tableName];
							}
							var type = TypeConverter.GetTypeFromInformationSchema(columnType);
							if (type != null) {
								tbl.Columns.Add(new DbColumn(columnName, type));
							}
						}
					}
				}
			}
			return tables.Values.OrderBy(t => t.SchemaName).ThenBy(t => t.TableName).ToList();
		}

		public DbParameter GetParameter(DbCommand cmd, string name, object value) {
			var p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = value == null ? DBNull.Value : value;
			return p;
		}
	}

}