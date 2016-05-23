using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GrowingData.Utilities.Database {
	public static class DbConnectionExtensions {

		public static int DEFAULT_TIMEOUT = 0;
		public static char DEFAULT_PARAMETER_PREFIX = '@';


		public static List<string> SqlParameterNames(string sql) {
			return SqlParameterNames(sql, DEFAULT_PARAMETER_PREFIX);
		}

		public static List<string> SqlParameterNames(string sql, char parameterPrefix) {

			var parameters = new HashSet<string>();
			var inVariable = false;
			var buffer = new StringBuilder();
			for (var i = 0; i < sql.Length; i++) {
				var c = sql[i];
				if (inVariable) {
					if (char.IsLetterOrDigit(c) || c == '_') {
						buffer.Append(c);
					} else {
						var p = buffer.ToString();
						if (p.Length > 0 && !parameters.Contains(p)) {
							parameters.Add(p);
						}

						buffer.Length = 0;
					}
				} else {
					if (c == parameterPrefix) {
						inVariable = true;
					}
				}
			}
			if (buffer.Length > 0) {
				var p = buffer.ToString();
				if (!parameters.Contains(p)) {
					parameters.Add(p);
				}
			}

			return parameters.ToList();
		}

		/// <summary>
		/// Parse the SQL looking for parameters, then create parameters for those 
		/// variables if the supplied object has a matching Field or Property
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		private static void BindParameters(DbCommand cmd, string sql, object ps) {
			if (ps != null) {


				var type = ps.GetType();

				var properties = type.GetProperties();
				var fields = type.GetFields();

				var sqlParameters = SqlParameterNames(sql);

				foreach (var p in properties) {
					if (sqlParameters.Contains(p.Name)) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + p.Name, p.GetValue(ps)));

					}
				}
				foreach (var f in fields) {
					if (sqlParameters.Contains(f.Name)) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + f.Name, f.GetValue(ps)));
					}
				}
			}
		}


		public static DbParameter GetParameter(DbCommand cmd, string name, object val) {
			DbParameter p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = val == null ? (object)DBNull.Value : val;
			return p;
		}

		/// <summary>
		/// Creates a command, using the specified Conenction, with the specified SQL and with
		/// all SQL variables (@Param) bound to fields from the supplied object 
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static DbCommand CreateCommand(this DbConnection cn, string sql, object ps) {
			var cmd = cn.CreateCommand();

			cmd.CommandText = sql;
			cmd.CommandTimeout = DEFAULT_TIMEOUT;

			BindParameters(cmd, sql, ps);

			return cmd;
		}



		/// <summary>
		/// Use refelection to bind columns to the type of object specified
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static List<T> SelectAnonymous<T>(this DbConnection cn, string sql, object ps) where T : new() {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				var type = typeof(T);

				var properties = type.GetProperties().ToDictionary(x => x.Name);
				var fields = type.GetFields().ToDictionary(x => x.Name);

				using (var r = cmd.ExecuteReader()) {
					return ReflectResults<T>(r);
				}
			}
		}


		public static DbDataReader ExecuteReader<T>(this DbConnection cn, string sql, object ps) where T : new() {
			using (var cmd = cn.CreateCommand(sql, ps)) {

				using (var r = cmd.ExecuteReader()) {
					return r;
				}
			}
		}

		public static List<T> ReflectResults<T>(DbDataReader r) where T : new() {

			List<T> results = new List<T>();
			while (r.Read()) {
				var obj = r.ReflectResult<T>();
				results.Add(obj);
			}

			return results;
		}



		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				return cmd.ExecuteNonQuery();
			}
		}


		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery(this DbConnection cn, DbTransaction txn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				cmd.Transaction = txn;
				return cmd.ExecuteNonQuery();
			}
		}

		//public static async Task<int> ExecuteSqlAsync(this DbConnection cn, string sql, object ps) {
		//	using (var cmd = cn.CreateCommand()) {
		//		cmd.CommandText = sql;
		//		cmd.CommandTimeout = DEFAULT_TIMEOUT;
		//		if (ps != null) {
		//			foreach (var p in ps.GetType().GetProperties()) {
		//				cmd.Parameters.Add(GetParameter(cmd, "@" + p.Name, p.GetValue(ps)));
		//			}
		//		}
		//		return await cmd.ExecuteNonQueryAsync();
		//	}
		//}




		public static void SelectForEach(this DbConnection cn, string sql, object ps, Action<DbDataReader> fn) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						fn(reader);
					}
				}
			}
		}


		public static IEnumerable<TResult> SelectForEach<TResult>(this DbConnection cn, string sql, object ps, Func<DbDataReader, TResult> fn) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						yield return fn(reader);
					}
				}
			}
		}


		/// <summary>
		/// Yields a dictionary for each row that is returned from the query.
		/// The Dictionary is the same object for each row, with its values changed.
		/// e.g. if you try to call .ToList() on the enumeration all the rows will have
		/// the same values as the last row.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static IEnumerable<DbRow> SelectRows(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					var rowData = new DbRow();
					while (reader.Read()) {
						for (var i = 0; i < reader.FieldCount; i++) {
							rowData[reader.GetName(i)] = reader[i];
						}
						yield return rowData;
					}
				}
			}
		}
		/// <summary>
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static List<DbRow> SelectRowsList(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				var rows = new List<DbRow>();
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						var rowData = new DbRow();
						for (var i = 0; i < reader.FieldCount; i++) {
							rowData[reader.GetName(i)] = reader[i];
						}
						rows.Add(rowData);
					}
				}
				return rows;
			}
		}


		public static List<T> SelectList<T>(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					var list = new List<T>();
					while (reader.Read()) {
						if (reader[0] != DBNull.Value) {
							list.Add((T)reader[0]);
						}
					}
					return list;
				}
			}
		}

		public static DbDataReader ExecuteReader(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				return cmd.ExecuteReader();
			}
		}

		public static int ExecuteTSV(this DbConnection cn, string sql, object ps, StreamWriter writer) {
			StringBuilder output = new StringBuilder();

			int rowCount = 0;
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					bool isFirst = true;
					while (reader.Read()) {

						if (isFirst) {

							List<string> names = new List<string>();

							for (var i = 0; i < reader.FieldCount; i++) {
								names.Add(reader.GetName(i));
							}
							writer.WriteLine(string.Join("\t", names));

							isFirst = false;
						}

						var rowData = Enumerable.Range(0, reader.FieldCount).Select(i => Serialize(reader[i]));

						writer.WriteLine(string.Join("\t", rowData));

						rowCount++;

						if (rowCount % 1000 == 0) {
							writer.Flush();
							System.Diagnostics.Debug.WriteLine(string.Format("Wrote {0} rows", rowCount));
						}
					}
				}
			}


			return rowCount;
		}


		public static string Serialize(object o) {
			if (o is DateTime) {
				return ((DateTime)o).ToString("yyyy-MM-dd HH':'mm':'ss");

			}
			if (o == DBNull.Value) {
				return "NULL";
			}

			if (o is string) {

				// Strings are escaped 
				return "\"" + Escape(o.ToString()) + "\"";

			}
			return o.ToString();

		}
		private static string Escape(string unescaped) {
			return unescaped
				.Replace("\\", "\\" + "\\")     // '\' -> '\\'
				.Replace("\"", "\\" + "\"");        // '"' -> '""'
		}


		public static string ExecuteJsonRows(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					// Field names
					List<string> columnNames =
						Enumerable.Range(0, reader.FieldCount)
							.Select(x => reader.GetName(x))
							.ToList();
					List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
					while (reader.Read()) {
						Dictionary<string, string> rowData = new Dictionary<string, string>();
						for (var i = 0; i < reader.FieldCount; i++) {
							if (reader[i].GetType() == typeof(DateTime)) {
								// Use ISO time
								rowData[columnNames[i]] = ((DateTime)reader[i]).ToString("s");
							} else {
								rowData[columnNames[i]] = reader[i].ToString();
							}
						}
						data.Add(rowData);
					}
					return JsonConvert.SerializeObject(new { ColumnNames = columnNames, Rows = data });
				}
			}
		}


		//public static DataTable ExecuteDataTable(this DbConnection cn, string sql, object ps) {
		//	using (var cmd = cn.CreateCommand(sql, ps)) {
		//		using (var reader = cmd.ExecuteReader()) {
		//			// Field names
		//			var table = new DataTable();
		//			table.Load(reader);
		//			return table;
		//		}
		//	}
		//}


	}
}
