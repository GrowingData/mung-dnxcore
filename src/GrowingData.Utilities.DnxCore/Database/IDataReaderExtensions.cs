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

		/// <summary>
		/// Binds the current row in the reader to a new object of Type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <returns></returns>
		public static T ReflectResult<T>(DbDataReader r) where T : new() {

			var type = typeof(T);
			var properties = type.GetProperties().ToDictionary(x => x.Name);
			var fields = type.GetFields().ToDictionary(x => x.Name);

			HashSet<string> columnNames = null;

			var obj = new T();
			if (columnNames == null) {
				columnNames = new HashSet<string>();
				for (var i = 0; i < r.FieldCount; i++) {
					columnNames.Add(r.GetName(i));
				}
			}

			foreach (var p in properties) {
				if (columnNames.Contains(p.Key)) {
					if (r[p.Key] != DBNull.Value) {
						p.Value.SetValue(obj, r[p.Key]);
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
			foreach (var p in fields) {
				if (columnNames.Contains(p.Key)) {
					if (r[p.Key] != DBNull.Value) {
						p.Value.SetValue(obj, r[p.Key]);
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

			return obj;
		}

	}
}
