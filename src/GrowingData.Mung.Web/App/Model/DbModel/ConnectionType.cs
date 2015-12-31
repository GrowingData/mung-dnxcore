using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class ConnectionType {
		public int ConnectionTypeId;
		public string Name;

		private static List<ConnectionType> _connectionTypes;

		public static List<ConnectionType> ConnectionTypes {
			get {
				if (_connectionTypes == null) {
					using (var cn = DatabaseContext.Db.Mung()) {
						_connectionTypes = cn.ExecuteAnonymousSql<ConnectionType>("SELECT * FROM ConnectionType", null);

					}
				}
				return _connectionTypes;
			}

		}
	}
}