using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace GrowingData.Mung.Core {
	public class PostgresqlBulkInserter : DbBulkInserter {

		protected Func<NpgsqlConnection> _connectionFactory;

		public PostgresqlBulkInserter(Func<DbConnection> targetConnection, string targetSchema, string targetTable)
			: base(targetConnection, targetSchema, targetTable) {

			_connectionFactory = () => {
				return targetConnection() as NpgsqlConnection;
			};


			// Make sure that the schema exists
			CreateSchemaIfRequired(targetSchema);
		}

		public override DbTable GetDbSchema() {
			var schemaReader = new PostgresqlDbSchemaReader(_connectionFactory);
			return schemaReader.GetTables(_targetSchema, _targetTable).FirstOrDefault();

		}

		public override bool BulkInsert(DbTable table, MsvReader reader) {
			using (var cn = _connectionFactory()) {
				// Make a little cache of the pgTypes
				var pgTypes = table.Columns
					.Select(x => PostgresqlDbTypeConverter.Get(x.MungType).PostgresqlDbType)
					.ToList();

				// Not all the columns in the table may be present in the actual reader, so
				// we insert null if they are missing.  
				HashSet<string> actualColumns = new HashSet<string>(reader.Columns.Select(c => c.ColumnName));
				using (var writer = cn.BeginBinaryImport(CopyCommand(table))) {
					while (reader.Read()) {
						writer.StartRow();
						for (var i = 0; i < table.Columns.Count; i++) {
							var col = table.Columns[i];
							if (actualColumns.Contains(col.ColumnName)) {
								writer.Write(reader[col.ColumnName], pgTypes[i]);
							} else {
								writer.WriteNull();
							}
						}
					}
				}
			}
			return true;
		}

		public override bool BulkInsert(DbDataReader reader, Action<DbDataReader> eachRow) {
			// Read the first row to get the information on the schema

			if (reader.Read()) {
				var table = new DbTable(_targetTable, _targetSchema);
				var pgTypes = new List<NpgsqlDbType>();
				for (var i = 0; i < reader.FieldCount; i++) {
					var col = new DbColumn(reader.GetName(i), reader.GetFieldType(i));
					table.Columns.Add(col);

					var type = PostgresqlDbTypeConverter.Get(col.MungType);

					if (type == null) {
						throw new Exception($"Unable to load Postgres type for type: {col.MungType.Code}, Column: {col.ColumnName}");
					}
					pgTypes.Add(type.PostgresqlDbType);
				}

				//Make sure that the table exists, and has the right schema
				var oldSchema = GetDbSchema();
				if (oldSchema == null) {
					CreateTable(table);
				} else {
					ModifySchema(oldSchema, table);
				}

				using (var cn = _connectionFactory()) {
					using (var writer = cn.BeginBinaryImport(CopyCommand(table))) {
						do {

							writer.StartRow();
							for (var i = 0; i < table.Columns.Count; i++) {
								var col = table.Columns[i];
								var val = reader[col.ColumnName];
								if (val == DBNull.Value) {
									writer.WriteNull();
								} else {
									writer.Write(reader[col.ColumnName], pgTypes[i]);
								}
							}
							if (eachRow != null) {
								eachRow(reader);
							}

						} while (reader.Read());
					}
				}

			}
			return true;
		}


		public static string CopyCommand(DbTable table) {
			var columns = string.Join(",\n\t", table.Columns.Select(x => $"\"{x.ColumnName}\""));

			return $"COPY \"{table.SchemaName}\".\"{table.TableName}\"(\n\t{columns}\n) FROM STDIN BINARY";
		}


		public override bool CreateTable(DbTable tbl) {

			StringBuilder ddl = new System.Text.StringBuilder();
			ddl.Append($"CREATE TABLE \"{tbl.SchemaName}\".\"{tbl.TableName}\" (\r\n");

			foreach (var c in tbl.Columns) {
				var pgType = PostgresqlDbTypeConverter.Get(c.MungType);

				if (c.ColumnName == "_id_") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL PRIMARY KEY,\n");
					continue;
				}
				if (c.ColumnName == "_at_") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == "_source_") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == "_app_") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL,\n");
					continue;
				}

				ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NULL,\n");

			}
			// Remove the trailing ","
			ddl.Length -= 2;

			ddl.Append("\n);");

			// Create a BRIN index on it for fast range queries (since thats what we do most of)
			ddl.Append($"CREATE INDEX \"idx_{tbl.TableName}\" ON \"{tbl.SchemaName}\".\"{tbl.TableName}\" USING BRIN (\"_at_\");");


			return ExecuteCommand(ddl.ToString());
		}

		public bool ExecuteCommand(string sql) {
			using (var cn = _connectionFactory()) {
				using (var cmd = new NpgsqlCommand(sql, cn)) {
					cmd.ExecuteNonQuery();
				}
			}
			return true;

		}

		public bool SameColumn(DbColumn a, DbColumn b) {
			return a.ColumnName.ToLowerInvariant() == b.ColumnName.ToLowerInvariant();

		}

		public bool CreateSchemaIfRequired(string schemaName) {
			var cmd = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
			ExecuteCommand(cmd);
			return true;
		}


		public override bool ModifySchema(DbTable fromTbl, DbTable toTbl) {

			foreach (var c in toTbl.Columns) {
				var existing = fromTbl
					.Columns
					.FirstOrDefault(x => SameColumn(x, c));

				// Add the new column...
				if (existing == null) {
					fromTbl.Columns.Add(c);
					var pgType = PostgresqlDbTypeConverter.Get(c.MungType);
					string ddl = $"ALTER TABLE \"{Schema}\".\"{fromTbl.TableName}\" ADD \"{c.ColumnName}\" {pgType.CreateColumnDefinition} NULL";
					ExecuteCommand(ddl);
				} else {
					if (c.MungType != existing.MungType) {
						var newType = MungType.ExpandType(existing.MungType, c.MungType);
						var pgType = PostgresqlDbTypeConverter.Get(newType);
						string ddl = $"ALTER TABLE \"{Schema}\".\"{fromTbl.TableName}\" ALTER COLUMN \"{c.ColumnName}\" TYPE {pgType.CreateColumnDefinition}";

						ExecuteCommand(ddl);

					}
				}
			}

			return true;
		}

	}
}
