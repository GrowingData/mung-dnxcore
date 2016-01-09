using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Linq;
using System.Text;
using GrowingData.Mung.Core;

namespace GrowingData.Mung.SqlBatch {
	public class PostgresqlBulkInserter : BulkInserter {

		protected Func<NpgsqlConnection> _getConnection;
		//private static Dictionary<MungType, PostgresqlType> _lookup = PostgresqlDbTypeConverter.Types.ToDictionary(k => k.MungType);

		public PostgresqlBulkInserter(string schema, string filename, Func<NpgsqlConnection> cn)
			: base(schema, filename) {

			_getConnection = cn;

			// Make sure that the schema exists
			CreateSchemaIfRequired(schema);
		}

		public override DbTable GetDbSchema() {
			string sql = @"
					SELECT table_schema, table_name, column_name, data_type
					FROM information_schema.columns C
					WHERE table_schema NOT IN ('information_schema', 'pg_catalog')
					AND		table_schema = @Schema
					AND		table_name = @Table
					ORDER BY table_schema, ordinal_position
				";
			using (var cn = _getConnection()) {
				using (var cmd = new NpgsqlCommand(sql, cn)) {
					cmd.Parameters.AddWithValue("Schema", _schema);
					cmd.Parameters.AddWithValue("Table", _table);
					using (var reader = cmd.ExecuteReader()) {
						bool hasData = false;
						var tbl = new DbTable(_table, _schema);
						while (reader.Read()) {
							hasData = true;
							var columnName = reader["column_name"] as string;
							var sqlType = reader["data_type"] as string;

							tbl.Columns.Add(new DbColumn(columnName, DbTypeConverter.Postgresql.GetTypeFromInformationSchema(sqlType)));

						}
						if (!hasData) {
							return null;
						}

						return tbl;
					}
				}
			}
		}

		public override bool BulkInsert(DbTable table, MsvReader reader) {
			using (var cn = _getConnection()) {
				// Make a little cache of the pgTypes
				var pgTypes = table.Columns
					.Select(x => PostgresqlDbTypeConverter.Types.FirstOrDefault(p => p.MungType == x.ColumnType).PostgresqlDbType).ToList();

				using (var writer = cn.BeginBinaryImport(CopyCommand(table))) {

					while (reader.Read()) {

						writer.StartRow();
						for (var i = 0; i < table.Columns.Count; i++) {
							var col = table.Columns[i];

							writer.Write(reader[col.ColumnName], pgTypes[i]);

						}


					}


				}

			}

			return true;
		}

		public static string CopyCommand(DbTable table) {
			var columns = string.Join(",\n\t", table.Columns.Select(x => x.ColumnName));
			return $"COPY \"{table.SchemaName}\".\"{table.TableName}\"(\n\t{columns}\n) FROM STDIN BINARY";
		}


		public override bool CreateTable(DbTable tbl) {

			StringBuilder ddl = new System.Text.StringBuilder();
			ddl.Append($"CREATE TABLE \"{tbl.SchemaName}\".\"{tbl.TableName}\" (\r\n");
			
			foreach (var c in tbl.Columns) {
				var pgType = PostgresqlDbTypeConverter.Types.FirstOrDefault(x => x.MungType == c.ColumnType);

				if (c.ColumnName == tbl.TableName + "_id") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL PRIMARY KEY,\n");
					continue;
				}
				if (c.ColumnName == tbl.TableName + "_at") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == tbl.TableName + "_source") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == tbl.TableName + "_app") {
					ddl.Append($"	\"{c.ColumnName}\" {pgType.CreateColumnDefinition} NOT NULL,\n");
					continue;
				}

				ddl.Append($"	{c.ColumnName} {pgType.CreateColumnDefinition} NULL,\n");

			}
			// Remove the trailing ","
			ddl.Length -= 2;

			ddl.Append("\n)");

			return ExecuteCommand(ddl.ToString());
		}

		public bool ExecuteCommand(string sql) {
			using (var cn = _getConnection()) {
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
					var pgType = PostgresqlDbTypeConverter.Types.FirstOrDefault(t => t.MungType == c.ColumnType);
					string ddl = $"ALTER TABLE \"{Schema}\".\"{fromTbl.TableName}\" ADD \"{c.ColumnName}\" {pgType.CreateColumnDefinition} NULL";
					ExecuteCommand(ddl);
				} else {
					if (c.ColumnType != existing.ColumnType) {
						var newType = MungType.ExpandType(existing.ColumnType, c.ColumnType);
						var pgType = PostgresqlDbTypeConverter.Types.FirstOrDefault(t => t.MungType == newType);
						string ddl = $"ALTER TABLE \"{Schema}\".\"{fromTbl.TableName}\" ALTER COLUMN \"{c.ColumnName}\" TYPE {pgType.CreateColumnDefinition}";

						ExecuteCommand(ddl);

					}
				}
			}

			return true;
		}

	}
}
