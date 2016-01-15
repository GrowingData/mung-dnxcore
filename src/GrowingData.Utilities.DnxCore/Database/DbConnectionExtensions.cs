﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Data.Common;
using Newtonsoft.Json;

namespace GrowingData.Utilities.DnxCore {
	public static class DbConnectionExtensions {
		public static int DEFAULT_TIMEOUT = 0;
		public static char DEFAULT_PARAMETER_PREFIX = '@';

		#region "Parameter handling"

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

		public static DbParameter GetParameter(DbCommand cmd, string name, object val) {
			DbParameter p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = val == null ? (object)DBNull.Value : val;
			return p;
		}


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

		#endregion

		#region Command handling

		private static DbCommand CreateCommand(this DbConnection cn, string sql, object ps) {
			var cmd = cn.CreateCommand();

			cmd.CommandText = sql;
			cmd.CommandTimeout = DEFAULT_TIMEOUT;

			BindParameters(cmd, sql, ps);

			return cmd;
		}


		public static List<T> ExecuteAnonymousSql<T>(this DbConnection cn, string sql, object ps) where T : new() {

			using (var cmd = cn.CreateCommand(sql, ps)) {

				var type = typeof(T);


				using (var r = cmd.ExecuteReader()) {
					return ReflectResults<T>(r);
				}
			}
		}
		public static DbDataReader ExecuteSql<T>(this DbConnection cn, string sql, object ps) where T : new() {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var r = cmd.ExecuteReader()) {
					return r;
				}
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
		public static int ExecuteSql(this DbConnection cn, string sql, object ps) {
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
		public static int ExecuteSql(this DbConnection cn, DbTransaction txn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				cmd.Transaction = txn;

				return cmd.ExecuteNonQuery();
			}
		}


		public static void ExecuteRow(this DbConnection cn, string sql, object ps, Action<DbDataReader> eachRow) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						eachRow(reader);
					}
				}
			}
		}

		#endregion

		private static string KeyFromColumnName(string columnName) {
			return columnName.ToLower().Replace("_", "");
		}
		private static string KeyFromProperty(string propertyName) {
			return propertyName.ToLower().Replace("_", "");
		}

		#region Result handling

		/// <summary>
		/// Uses reflection to bind any fields or properties of the supplied object
		/// with any matching column names.  Matches are case insensitive.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <returns></returns>
		public static List<T> ReflectResults<T>(DbDataReader r) where T : new() {

			var type = typeof(T);

			var properties = type.GetProperties().ToDictionary(x => x.Name);
			var fields = type.GetFields().ToDictionary(x => x.Name);

			Dictionary<string, string> columnNames = null;
			List<T> results = new List<T>();
			while (r.Read()) {
				var obj = new T();

				if (columnNames == null) {
					columnNames = new Dictionary<string, string>();
					for (var i = 0; i < r.FieldCount; i++) {
						var name = r.GetName(i);
						var key = KeyFromColumnName(name);

						columnNames[key] = name;
					}
				}

				foreach (var p in properties) {
					var pKey = KeyFromProperty(p.Key);
					if (columnNames.ContainsKey(pKey)) {
						var columnName = columnNames[pKey];
						if (r[columnName] != DBNull.Value) {
							p.Value.SetValue(obj, r[columnName]);
						}
					}
				}

				foreach (var p in fields) {
					var pKey = KeyFromProperty(p.Key);
					if (columnNames.ContainsKey(pKey)) {
						var columnName = columnNames[pKey];
						if (r[columnName] != DBNull.Value) {
							p.Value.SetValue(obj, r[columnName]);
						}
					}
				}
				results.Add(obj);
			}

			return results;
		}

		#endregion

		#region Dumpers

		public static List<T> DumpList<T>(this DbConnection cn, string sql, object ps) {
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


		public static DbDataReader DumpReader(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				cmd.CommandText = sql;
				cmd.CommandTimeout = DEFAULT_TIMEOUT;

				BindParameters(cmd, sql, ps);

				return cmd.ExecuteReader();
			}
		}


		public static string DumpTSVFormatted(this DbConnection cn, string sql, object ps) {
			StringBuilder output = new StringBuilder();

			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					bool isFirst = true;
					int rowCount = 0;
					while (reader.Read()) {

						if (isFirst) {
							//MungLog.Log.LogEvent("MungedDataWriter.Write", "Retreiving...");
							// Recycle the same array so we're not constantly allocating

							List<string> names = new List<string>();

							for (var i = 0; i < reader.FieldCount; i++) {
								names.Add(reader.GetName(i));
							}
							var namesLine = string.Join("\t", names);
							string underline = new String('-', namesLine.Length + (names.Count * 3));

							output.AppendLine(underline);
							output.AppendLine(namesLine);
							output.AppendLine(underline);

							isFirst = false;
						}
						for (var i = 0; i < reader.FieldCount; i++) {
							output.AppendFormat("{0}\t", Serialize(reader[i]));
						}
						output.Append("\n");


						rowCount++;

					}
				}
			}


			return output.ToString();
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


		public static string DumpJsonRows(this DbConnection cn, string sql, object ps) {
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

		#endregion


	}
}
