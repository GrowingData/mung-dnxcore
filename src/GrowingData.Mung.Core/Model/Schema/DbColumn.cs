using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace GrowingData.Mung.Core {
	public class DbColumn
	 {
		public string ColumnName;
		public MungType ColumnType;


		public DbColumn(string name, MungType type) {
			ColumnName = name.ToLower();
			ColumnType = type;
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