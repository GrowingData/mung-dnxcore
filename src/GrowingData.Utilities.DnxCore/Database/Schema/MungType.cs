using System;
using System.Data;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Utilities.Database {
	public class MungTypeUnknownException : Exception {
		public MungTypeUnknownException(string message) : base(message) { }
	}
	public enum MungTypeCode {
		Unknown,
		String,
		Bool,
		Double,
		DateTime,
		Decimal,
		Integer,
		Guid
	}
	public class MungType : IEqualityComparer<MungType>, IComparable<MungType>, IComparable {


		public readonly Type DotNetType;
		public readonly DbType DatabaseType;
		public readonly MungTypeCode Code;

		public override string ToString() {
			return Code.ToString();
		}

		public static MungType Parse(string codeString) {
			MungTypeCode typeCode = MungTypeCode.Unknown;
			if (Enum.TryParse(codeString, out typeCode)) {
				return Get(typeCode);
			}
			throw new Exception($"Unable to parse MungType from {codeString}");
		}

		public bool Equals(MungType x, MungType y) {
			return x.Code == y.Code;
		}

		public int GetHashCode(MungType obj) {
			return obj.Code.GetHashCode();
		}

		public int CompareTo(object obj) {
			return Code.CompareTo((obj as MungType).Code);
		}

		public int CompareTo(MungType other) {
			return Code.CompareTo(other.Code);
		}


		private MungType(MungTypeCode code, Type dotNetType, DbType dbType) {
			Code = code;
			DotNetType = dotNetType;
			DatabaseType = dbType;
		}

		public static MungType Get(MungTypeCode mungCode) {
			var mt = ValidTypes.FirstOrDefault(t => t.Code == mungCode);
			if (mt == null) {
				throw new MungTypeUnknownException($"Unable to convert MungTypeCode from {mungCode} to MungType (type is unknown).");
			}
			return mt;
		}

		public static MungType Get(Type dotNetType) {
			var mt = ValidTypes.FirstOrDefault(t => t.DotNetType == dotNetType);
			if (mt == null) {
				throw new MungTypeUnknownException($"Unable to convert dotNetType from {dotNetType} to MungType (type is unknown).");
			}
			return mt;
		}
		public static MungType Get(string headerString) {
			DbType type;
			if (Enum.TryParse<DbType>(headerString, out type)) {
				var mt = ValidTypes.FirstOrDefault(t => t.DatabaseType == type);

				if (mt == null) {
					throw new MungTypeUnknownException($"MungType Header: {headerString} is unknown.");
				}
				return mt;
			}
			throw new MungTypeUnknownException($"MungType Header: Unable to parse {headerString} as a DbType.");

		}

		public static List<MungType> ValidTypes = new List<MungType>() {
			new MungType(MungTypeCode.String, typeof(string), DbType.String),
			new MungType(MungTypeCode.Bool, typeof(bool), DbType.Boolean ),
			new MungType(MungTypeCode.DateTime, typeof(DateTime), DbType.DateTime ),
			new MungType(MungTypeCode.Double, typeof(double), DbType.Double ),
			new MungType(MungTypeCode.Decimal, typeof(decimal), DbType.Decimal ),

			new MungType(MungTypeCode.Double, typeof(float), DbType.Single ),
			new MungType(MungTypeCode.Integer, typeof(long), DbType.Int64 ),
			new MungType(MungTypeCode.Integer, typeof(byte), DbType.Byte ),
			new MungType(MungTypeCode.Integer, typeof(short), DbType.Int32 ),
			new MungType(MungTypeCode.Integer, typeof(int), DbType.Int32 ),
			new MungType(MungTypeCode.Guid, typeof(Guid), DbType.Guid )
		};


		public static MungType ExpandType(MungType oldType, MungType newType) {
			// Same as the old one
			if (oldType.Code == newType.Code) {
				return newType;
			}

			// If it was string, stay with string 
			if (oldType.Code == MungTypeCode.String) {
				return oldType;
			}


			// Int64 -> Double
			if (EitherTypeIs(oldType, newType, MungTypeCode.Integer) && EitherTypeIs(oldType, newType, MungTypeCode.Double)) {
				return Get(MungTypeCode.Double);
			}

			// Int64 -> Decimal
			if (EitherTypeIs(oldType, newType, MungTypeCode.Integer) && EitherTypeIs(oldType, newType, MungTypeCode.Decimal)) {
				return Get(MungTypeCode.Decimal);
			}

			// Int64 -> Double
			if (EitherTypeIs(oldType, newType, MungTypeCode.Integer) && EitherTypeIs(oldType, newType, MungTypeCode.Double)) {
				return Get(MungTypeCode.Double);
			}

			// Decimal -> Double
			if (EitherTypeIs(oldType, newType, MungTypeCode.Decimal) && EitherTypeIs(oldType, newType, MungTypeCode.Double)) {
				return Get(MungTypeCode.Double);
			}



			return Get(MungTypeCode.String);

		}
		private static bool EitherTypeIs(MungType oldType, MungType newType, MungTypeCode code) {
			return oldType.Code == code || newType.Code == code;
		}



		public static MungType Get(JTokenType type) {
			Type t = null;
			if (type == JTokenType.String) {
				t = typeof(string);
			}

			if (type == JTokenType.Float) {
				t = typeof(double);
			}

			if (type == JTokenType.Integer) {
				t = typeof(long);
			}

			if (type == JTokenType.Date) {
				t = typeof(DateTime);
			}

			if (type == JTokenType.Boolean) {
				t = typeof(bool);
			}

			if (type == JTokenType.TimeSpan) {
				t = typeof(TimeSpan);
			}
			if (t == null) {
				throw new TypeLoadException($"MungType.Get(JTokenType)  => Unknown type: {type}");

			}
			return MungType.Get(t);
		}

	}


}