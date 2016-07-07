using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace GrowingData.Utilities.Database {
	public class SqlColumn {
		public string ColumnName;
		public MungType MungType;

		public SqlColumn(string name, Type type) {
			ColumnName = name.ToLower();
			MungType = MungType.Get(type);
		}

		public SqlColumn(string name, MungType type) {
			ColumnName = name.ToLower();
			MungType = type;
		}

		public int CompareTo(object obj) {
			var other = obj as SqlColumn;
			return this.ColumnName.CompareTo(other.ColumnName);
		}

		public override int GetHashCode() {
			return ColumnName.GetHashCode();
		}
	}
}