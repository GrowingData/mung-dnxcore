using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace GrowingData.Utilities.Database {
	public static class DataReaderExtensions {


		public static List<SqlRow> SelectRows(this IDataReader reader) {

			List<string> columnNames = Enumerable.Range(0, reader.FieldCount)
					.Select(x => reader.GetName(x))
					.ToList();

			var rows = new List<SqlRow>();
			while (reader.Read()) {
				var row = new SqlRow();
				for (var i = 0; i < columnNames.Count; i++) {
					row[columnNames[i]] = reader[i];
				}
				rows.Add(row);
			}

			return rows;


		}
		public static Dictionary<string, string> GetColumnNameKeys(DbDataReader r) {
			var columnNames = new Dictionary<string, string>();
			for (var i = 0; i < r.FieldCount; i++) {
				var key = DataReaderExtensions.StandardiseName(r.GetName(i));
				columnNames[key] = r.GetName(i);
			}
			return columnNames;
		}

		/// <summary>
		/// Binds the current row in the reader to a new object of Type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <returns></returns>
		public static T ReflectResult<T>(DbDataReader r) where T : new() {

			var type = typeof(T);
			var properties = ReflectPropertyKeys(type);
			var fields = ReflectFieldKeys(type);

			Dictionary<string, string> columnNames = null;

			var obj = new T();
			if (columnNames == null) {
				columnNames = DataReaderExtensions.GetColumnNameKeys(r);
			}

			BindProperties(r, properties, columnNames, obj);
			BindFields(r, fields, columnNames, obj);

			return obj;
		}

		public static string StandardiseName(string name) {
			return name.ToLower().Replace("_", "");
		}


		public static Dictionary<string, FieldInfo> ReflectFieldKeys(Type type) {
			return type.GetFields().ToDictionary(x => DataReaderExtensions.StandardiseName(x.Name));
		}

		public static Dictionary<string, PropertyInfo> ReflectPropertyKeys(Type type) {
			return type.GetProperties().ToDictionary(x => DataReaderExtensions.StandardiseName(x.Name));
		}

		public static void BindFields<T>(DbDataReader r,
		Dictionary<string, FieldInfo> fields,
		Dictionary<string, string> columnNames,
		T obj) where T : new() {
			foreach (var p in fields) {
				if (columnNames.ContainsKey(p.Key)) {
					var columnName = columnNames[p.Key];
					if (r[columnName] != DBNull.Value) {
						p.Value.SetValue(obj, r[columnName]);
					} else {
						if (p.Value.FieldType.GetTypeInfo().IsClass) {
							p.Value.SetValue(obj, null);
						}
						// Nullable value like "int?"
						if (Nullable.GetUnderlyingType(p.Value.FieldType) != null) {
							p.Value.SetValue(obj, null);
						}

					}
				}
			}
		}

		public static void BindProperties<T>(DbDataReader r,
			Dictionary<string, PropertyInfo> properties,
			Dictionary<string, string> columnNames,
			T obj) where T : new() {
			foreach (var p in properties) {
				if (columnNames.ContainsKey(p.Key)) {
					var columnName = columnNames[p.Key];
					if (r[columnName] != DBNull.Value) {
						p.Value.SetValue(obj, r[columnName]);
					} else {
						if (p.Value.PropertyType.GetTypeInfo().IsClass) {
							p.Value.SetValue(obj, null);
						}
						// Nullable value like "int?"
						if (Nullable.GetUnderlyingType(p.Value.PropertyType) != null) {
							p.Value.SetValue(obj, null);

						}
					}
				}
			}
		}
	}
}
