using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;

using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Connection {

		public static Connection Mung {
			get {
				return new Connection() {
					ConnectionId = -1,
					ConnectionString = null,
					Name = "Mung"
				};
			}
		}
		public int ConnectionId;
		public int? ConnectionTypeId;
		public string Name;
		public string ConnectionString;

		public DateTime CreatedAt;
		public DateTime UpdatedAt;

		public ConnectionType Type {
			get {
				return ConnectionType.ConnectionTypes.FirstOrDefault(x => x.ConnectionTypeId == ConnectionTypeId);
			}
		}

		public DbConnection GetConnection() {
			if (ConnectionId == -1) {
				return DatabaseContext.Db.Warehouse();
			}

			var connectionType = Type;
			if (connectionType.Name == "SQL Server") {
				var cn = new SqlConnection(ConnectionString);
				cn.Open();
				return cn;
			}

			//if (connectionType.Name == "PostgreSQL") {
			//	var cn = new NpgsqlConnection(ConnectionString);
			//	cn.Open();
			//	return cn;
			//}

			throw new Exception("Unable to find connection provider with type: " + connectionType.Name);

		}


		/// <summary>
		/// Get a connection
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static Connection Get(int connectionId) {
			if (connectionId == -1) {
				return Mung;
			}
			using (var cn = DatabaseContext.Db.Metadata()) {
				var connection = cn.ExecuteAnonymousSql<Connection>(
						@"SELECT * FROM mung.Connection WHERE ConnectionId = @ConnectionId",
						 new { ConnectionId = connectionId }
					)
					.FirstOrDefault();
				return connection;
			}
		}

		/// <summary>
		/// List all connections
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static List<Connection> List() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var connections = cn.ExecuteAnonymousSql<Connection>(@"SELECT * FROM mung.Connection", null);
				connections.Add(Mung);

				return connections;
			}
		}

		/// <summary>
		/// Save or Create a Connection reference
		/// </summary>
		/// <returns></returns>
		public bool Save() {
			using (var cn = DatabaseContext.Db.Metadata()) {

				if (ConnectionId == -1) {
					var sql = @"
	INSERT INTO mung.Connection(ConnectionTypeId, Name, ConnectionString)
		SELECT @ConnectionTypeId, @Name, @ConnectionString";
					cn.ExecuteSql(sql, new {
						Name = Name,
						ConnectionString = ConnectionString,
						UpdatedAt = DateTime.UtcNow,
						CreatedAt = DateTime.UtcNow
					});

					return true;

				} else {

					var sql = @"
	UPDATE mung.Connection
		SET Name=@Name, ConnectionString=@ConnectionString, UpdatedAt=@UpdatedAt
		WHERE GraphId = @GraphId";
					cn.ExecuteSql(sql, new {
						Name = Name,
						ConnectionString = ConnectionString,
						UpdatedAt = DateTime.UtcNow
					});
					return true;
				}
			}
		}


		/// <summary>
		/// Delete a connection from the list
		/// </summary>
		/// <returns></returns>
		public bool Delete() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var sql = @"
	DELETE FROM mung.Connection
		WHERE ConnectionId = @ConnectionId";
				cn.ExecuteSql(sql, new {
					ConnectionId = ConnectionId
				});

				return true;
			}
		}
	}

}
