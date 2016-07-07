using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Linq;
using System.Text;
using GrowingData.Utilities;
using GrowingData.Utilities.Csv;

namespace GrowingData.Utilities.Database {

	public class PostgresqlBulkInserter : DbBulkInserter {

		protected Func<NpgsqlConnection> _connectionFactory;

		public PostgresqlBulkInserter(Func<DbConnection> targetConnection)
			: base() {
			_connectionFactory = () => {
				return targetConnection() as NpgsqlConnection;
			};

			
		}

		public override SqlTable GetDbSchema(string schema, string table) {
			var schemaReader = new PostgresqlDbSchemaReader(_connectionFactory);
			return schemaReader.GetTables(schema, table).FirstOrDefault();

		}

		public override bool BulkInsert(string schemaName, string tableName, CsvReader reader) {
			using (var cn = _connectionFactory()) {

				var existingTable = GetDbSchema(schemaName, tableName);
				var readerTable = new SqlTable(tableName, schemaName);

				for (var i = 0; i < reader.Columns.Count; i++) {
					var column = reader.Columns[i];
					readerTable.Columns.Add(column);
				}

				if (existingTable == null) {
					CreateTable(readerTable);
				}

				var table = GetDbSchema(schemaName, tableName);
				// Make a little cache of the pgTypes
				var pgTypes = table.Columns
					.Select(x => PostgresqlTypeConverter.Get(x.MungType).PostgresqlDbType)
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
								var val = reader[col.ColumnName];
								if (val == DBNull.Value) {
									writer.WriteNull();
								} else {
									writer.Write(reader[col.ColumnName], pgTypes[i]);
								}
							} else {
								writer.WriteNull();
							}
						}
					}
				}
			}
			return true;
		}

		public override bool BulkInsert(string schemaName, string tableName, DbDataReader reader, Action<DbDataReader> callback) {
			// Read the first row to get the information on the schema

			if (reader.Read()) {

				var existingTable = GetDbSchema(schemaName, tableName);
				var readerTable = new SqlTable(tableName, schemaName);

				var pgTypes = new List<NpgsqlDbType>();
				for (var i = 0; i < reader.FieldCount; i++) {
					var columnName = reader.GetName(i);
					var columnType = reader.GetFieldType(i);

					var column = new SqlColumn(columnName, MungType.Get(columnType));
					readerTable.Columns.Add(column);


					var type = PostgresqlTypeConverter.Get(column.MungType);

					if (type == null) {
						throw new Exception($"Unable to load Postgres type for type: {column.MungType.Code}, Column: {column.ColumnName}");
					}
					pgTypes.Add(type.PostgresqlDbType);
				}
				if (existingTable == null) {
					CreateTable(readerTable);
				} else {
					ModifySchema(existingTable, readerTable);
				}

				var table = GetDbSchema(schemaName, tableName);



				var rowCount = 0L;
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
							if (callback != null) {
								callback(reader);
							}
							rowCount++;

						} while (reader.Read());
					}
				}

			}
			return true;
		}


		public static string CopyCommand(SqlTable table) {
			var columns = string.Join(",\n\t", table.Columns.Select(x => $"\"{x.ColumnName}\""));

			return $"COPY \"{table.SchemaName}\".\"{table.TableName}\"(\n\t{columns}\n) FROM STDIN BINARY";
		}


		public override bool CreateTable(SqlTable tbl) {

			StringBuilder ddl = new System.Text.StringBuilder();
			ddl.Append($"CREATE TABLE \"{tbl.SchemaName}\".\"{tbl.TableName}\" (\r\n");

			foreach (var c in tbl.Columns) {
				var pgType = PostgresqlTypeConverter.Get(c.MungType);

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

			if (tbl.Columns.FirstOrDefault(c => c.ColumnName == "_at_") != null) {
				// Create a BRIN index on it for fast range queries (since thats what we do most of)
				ddl.Append($"CREATE INDEX \"idx_{tbl.TableName}\" ON \"{tbl.SchemaName}\".\"{tbl.TableName}\" USING BRIN (\"_at_\");");


			}


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

		public bool SameColumn(SqlColumn a, SqlColumn b) {
			return a.ColumnName.ToLowerInvariant() == b.ColumnName.ToLowerInvariant();

		}

		public override bool CreateSchemaIfRequired(string schemaName) {
			var cmd = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
			ExecuteCommand(cmd);
			return true;
		}


		public override bool ModifySchema(SqlTable fromTbl, SqlTable toTbl) {

			foreach (var c in toTbl.Columns) {
				var existing = fromTbl
					.Columns
					.FirstOrDefault(x => SameColumn(x, c));

				// Add the new column...
				if (existing == null) {
					fromTbl.Columns.Add(c);
					var pgType = PostgresqlTypeConverter.Get(c.MungType);
					string ddl = $"ALTER TABLE \"{fromTbl.SchemaName}\".\"{fromTbl.TableName}\" ADD \"{c.ColumnName}\" {pgType.CreateColumnDefinition} NULL";
					ExecuteCommand(ddl);
				} else {
					if (c.MungType != existing.MungType) {
						var newType = MungType.ExpandType(existing.MungType, c.MungType);
						var pgType = PostgresqlTypeConverter.Get(newType);
						string ddl = $"ALTER TABLE \"{fromTbl.SchemaName}\".\"{fromTbl.TableName}\" ALTER COLUMN \"{c.ColumnName}\" TYPE {pgType.CreateColumnDefinition}";

						ExecuteCommand(ddl);

					}
				}
			}

			return true;
		}

	}
}
