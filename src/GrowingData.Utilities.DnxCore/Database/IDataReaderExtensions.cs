using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace GrowingData.Utilities.Database {
	public static class DataReaderExtensions {


		public static List<DbRow> SelectRows(this DbDataReader reader) {

			List<string> columnNames = Enumerable.Range(0, reader.FieldCount)
					.Select(x => reader.GetName(x))
					.ToList();

			var rows = new List<DbRow>();
			while (reader.Read()) {
				var row = new DbRow();
				for (var i = 0; i < columnNames.Count; i++) {
					row[columnNames[i]] = reader[i];
				}
				rows.Add(row);
			}

			return rows;


		}
		private static string KeyFromColumnName(string columnName) {
			return columnName.ToLower().Replace("_", "");
		}
		private static string KeyFromProperty(string propertyName) {
			return propertyName.ToLower().Replace("_", "");
		}


		/// <summary>
		/// Binds the current row in the reader to a new object of Type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <returns></returns>
		public static T ReflectResult<T>(this DbDataReader r) where T : new() {


			var type = typeof(T);

			var properties = type.GetProperties().ToDictionary(x => x.Name);
			var fields = type.GetFields().ToDictionary(x => x.Name);
			
			var obj = new T();

			var columnNames = new Dictionary<string, string>();
			for (var i = 0; i < r.FieldCount; i++) {
				var name = r.GetName(i);
				var key = KeyFromColumnName(name);

				columnNames[key] = name;
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
			return obj;
		}

	}

}
