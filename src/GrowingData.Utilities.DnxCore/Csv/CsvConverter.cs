using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GrowingData.Utilities.Csv {
	public class CsvConverter {


		public static string Serialize(object o) {

			if (o == null || o == DBNull.Value) {
				return "NULL";
			}

			return Newtonsoft.Json.JsonConvert.SerializeObject(o);
		}

		private static string InvalidFormatErrorMessage(DbType expected, string value, ReaderState state) {
			if (state != null) {
				return $"Error reading line: {state.LineNumber}, field: {state.FieldNumber}. Expected a DbType.{expected}, got: '{value}'.";
			} else {
				return $"Error reading value: '{value}' as type DbType.{expected}.";
			}
		}

		public static object Read(string val, DbType type, ReaderState state) {
			if (IsDBNull(val)) {
				return DBNull.Value;
			}

			if (type == DbType.String) {
				if (val.StartsWith("\"") && val.EndsWith("\"")) {
					val = val.Substring(1, val.Length - 2);
				}
				return val;
			}
			if (type == DbType.DateTime) return JsonConvert.DeserializeObject<DateTime>(val);
			if (type == DbType.Boolean) return JsonConvert.DeserializeObject<bool>(val);
			if (type == DbType.Single) return JsonConvert.DeserializeObject<double>(val);
			if (type == DbType.Double) return JsonConvert.DeserializeObject<double>(val);
			if (type == DbType.Decimal) return JsonConvert.DeserializeObject<decimal>(val);
			if (type == DbType.Byte) return JsonConvert.DeserializeObject<byte>(val);
			if (type == DbType.Int16) return JsonConvert.DeserializeObject<short>(val);
			if (type == DbType.Int32) return JsonConvert.DeserializeObject<int>(val);
			if (type == DbType.Int64) return JsonConvert.DeserializeObject<long>(val);
			if (type == DbType.Guid) return JsonConvert.DeserializeObject<Guid>(val);


			throw new InvalidOperationException($"Unable to read value '{val}', as type {type} is unknown.");



		}
		public static bool IsDBNull(string val) {
			val = val.ToLower();

			return string.IsNullOrEmpty(val)
				|| val == "null"
				|| val == "nil"
				|| val == "DBNull.Value";
		}
	}
}