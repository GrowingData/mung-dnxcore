using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace GrowingData.Utilities.Database {
	public class DbColumn {
		public string ColumnName;
		public MungType MungType;

		public DbColumn(string name, Type type) {
			ColumnName = name.ToLower();
			MungType = MungType.Get(type);
		}

		public DbColumn(string name, MungType type) {
			ColumnName = name.ToLower();
			MungType = type;
		}

		public int CompareTo(object obj) {
			var other = obj as DbColumn;
			return this.ColumnName.CompareTo(other.ColumnName);
		}

		public override int GetHashCode() {
			return ColumnName.GetHashCode();
		}
	}
}