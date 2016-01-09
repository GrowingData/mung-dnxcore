using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Connection {
		private const string MungSqlEventsConnectionName = "Mung Events (SQL)";
		private const string MungRealtimeEventsConnectionName = "Mung Events (Real Time)";


		public int ConnectionId;
		public int? ProviderId;
		public string Name;
		public string ConnectionString;

		public int CreatedByMunger;
		public int UpdatedByMunger;
		public DateTime CreatedAt;
		public DateTime UpdatedAt;


		public static void InitializeConnection() {
			var connections = List();
			var mungConnection = connections.FirstOrDefault(x => x.Name == MungSqlEventsConnectionName);

			string eventsConnectionString = null;
			using (var events = DatabaseContext.Db.Events()) {
				eventsConnectionString = events.ConnectionString;
			}
			if (mungConnection == null) {
				mungConnection = new Connection() {
					ConnectionId = -1,
					Name = MungSqlEventsConnectionName,
					ConnectionString = eventsConnectionString,
					ProviderId = Provider.Providers.FirstOrDefault(x => x.Name == "PostgreSQL").ProviderId,
					CreatedByMunger=-1,
					UpdatedByMunger=-1
				};

				mungConnection.Insert();
				return;
			}

			// Update the connection to use the system connection string
			if (mungConnection.ConnectionString != eventsConnectionString) {
				mungConnection.ConnectionString = eventsConnectionString;
				mungConnection.Update();
			}

			// Make sure that there is a Mung Real Time connection too;
			var mungRT = connections.FirstOrDefault(x => x.Name == MungRealtimeEventsConnectionName);
			if (mungRT == null) {
				mungConnection = new Connection() {
					ConnectionId = -2,
					Name = MungRealtimeEventsConnectionName,
					ConnectionString = "/api/firehose/poll",
					ProviderId = Provider.Providers.FirstOrDefault(x => x.Name == "Real Time").ProviderId,
					CreatedByMunger = -1,
					UpdatedByMunger = -1
				};
				mungConnection.Insert();
			}

		}

		public Provider Provider {
			get {
				return Provider.Providers.FirstOrDefault(x => x.ProviderId == ProviderId);
			}
		}

		public DbConnection GetConnection() {
			if (ConnectionId == -1) {
				return DatabaseContext.Db.Events();
			}

			var provider = Provider;
			if (provider.Name == "SQL Server") {
				var cn = new SqlConnection(ConnectionString);
				cn.Open();
				return cn;
			}

			if (provider.Name == "PostgreSQL") {
				var cn = new NpgsqlConnection(ConnectionString);
				cn.Open();
				return cn;
			}

			throw new Exception("Unable to find connection provider with type: " + provider.Name);

		}


		/// <summary>
		/// Get a connection
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static Connection Get(int connectionId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var connection = cn.ExecuteAnonymousSql<Connection>(@"
					SELECT * FROM connection WHERE connection_id = @connection_id",
					new { connection_id = connectionId }
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
			using (var cn = DatabaseContext.Db.Mung()) {
				var connections = cn.ExecuteAnonymousSql<Connection>(
					@"SELECT * FROM connection",
					null
				);
				return connections;
			}
		}

		/// <summary>
		/// Save or Create a Connection reference
		/// </summary>
		/// <returns></returns>
		public Connection Save() {
			using (var cn = DatabaseContext.Db.Mung()) {
				UpdatedAt = DateTime.UtcNow;
				if (ConnectionId <= 0) {
					return Insert();
				} else {
					return Update();
				}
			}
		}

		public Connection Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				CreatedAt = DateTime.UtcNow;

				var sql = @"
						INSERT INTO connection(provider_id, name, connection_string, created_by_munger, updated_by_munger)
							VALUES (@ProviderId, @Name, @ConnectionString, @CreatedByMunger, @UpdatedByMunger)
							RETURNING connection_id";

				ConnectionId = cn.ExecuteSql(sql, this);
				return this;

			}
		}
		public Connection Update() {
			using (var cn = DatabaseContext.Db.Mung()) {

				var sql = @"
						UPDATE Connection
							SET name = @Name, 
								connection_string = @ConnectionString, 
								updated_at = @UpdatedAt
							WHERE connection_id = @ConnectionId";
				cn.ExecuteSql(sql, this);
				return this;
			}
		}

		/// <summary>
		/// Delete a connection from the list
		/// </summary>
		/// <returns></returns>
		public bool Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM Connection WHERE ConnectionId = @ConnectionId";
				cn.ExecuteSql(sql, this);

				return true;
			}
		}
	}

}
