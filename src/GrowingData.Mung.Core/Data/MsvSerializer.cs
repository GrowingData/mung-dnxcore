using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Mung.Core {
	public class MsvSerializer {


		public static string Write(object o) {

			if (o is DateTime) {
				return ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss");

			}
			if (o == null || o == DBNull.Value) {
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
				return val;
			}

			var unknownType = true;

			if (type == DbType.Boolean) {
				var intValue = -1;
				if (int.TryParse(val, out intValue)) {
					if (intValue == 0) return false;
					if (intValue == 1) return true;
				}
				unknownType = false;
			}

			if (type == DbType.DateTime) {
				var dateTime = DateTime.MinValue;
				if (DateTime.TryParse(val, out dateTime)) {
					if (dateTime == DateTime.MinValue) {
						return DBNull.Value;
					}
					return dateTime;
				}
				unknownType = false;
			}

			if (type == DbType.Single) {
				var f = float.NaN;
				if (float.TryParse(val, out f)) {
					return f;
				}
				unknownType = false;
			}

			if (type == DbType.Double) {
				var d = double.NaN;
				if (double.TryParse(val, out d)) {
					return d;
				}
				unknownType = false;
			}

			if (type == DbType.Decimal) {
				var d = decimal.Zero;
				if (decimal.TryParse(val, out d)) {
					return d;
				}
				unknownType = false;
			}

			if (type == DbType.Byte) {
				byte b = 0;
				if (byte.TryParse(val, out b)) {
					return b;
				}
				unknownType = false;
			}

			if (type == DbType.Int16) {
				short int16 = -1;
				if (short.TryParse(val, out int16)) {
					return int16;
				}
				unknownType = false;
			}
			if (type == DbType.Int32) {
				int int32 = -1;
				if (int.TryParse(val, out int32)) {
					return int32;
				}
				unknownType = false;
			}
			if (type == DbType.Int64) {
				long int64 = -1;
				if (long.TryParse(val, out int64)) {
					return int64;
				}
				unknownType = false;
			}
			if (type == DbType.Guid) {
				Guid guid = Guid.Empty;
				if (Guid.TryParse(val, out guid)) {
					return guid;
				}
				unknownType = false;
			}

			if (unknownType) {
				throw new InvalidOperationException($"Unable to read value '{val}', as type {type} is unknown.");
			}

			throw new FormatException(InvalidFormatErrorMessage(type, val, state));

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