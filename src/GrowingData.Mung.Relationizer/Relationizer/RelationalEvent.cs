using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Mung.Core;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Relationizer {

	public class RelationalEvent {
		public const int KeyLength = 12;
		public static string GetKey() {
			return RandomString.Get(KeyLength);
		}

		public string Name;

		public string Id;
		public string ParentId;
		
		public string ParentType;


		public string Source;
		public int AppId;
		public DateTime LogTime;

		private List<DbColumn> _schema;
		private Dictionary<string, object> _values;

		public Dictionary<string, object> Values { get { return _values; } }

		public IEnumerable<DbColumn> Schema { get { return _schema; } }

		public void AddField(string name, MungType type, object value) {
			var field = new DbColumn(name, type);
			_schema.Add(field);
			_values[field.ColumnName] = value;

		}

		public RelationalEvent(MungServerEvent evt) {
			_schema = new List<DbColumn>();
			_values = new Dictionary<string, object>();

			Source = evt.Source;
			LogTime = evt.LogTime;
			AppId = evt.AppId;
		}

		public override string ToString() {
			return string.Format("{0}: {1}->{2} | {3}",
				Name,
				Id,
				ParentId,
				string.Join(",", _values.Select(s => string.Format("\"{0}\"", s)))
			);
		}

	}


}
