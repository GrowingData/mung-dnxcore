using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using GrowingData.Mung.Core;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Connection {
		private const string MungSqlEventsConnectionName = "mung_events";



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

			string eventsConnectionString = DatabaseContext.Db.EventsConnectionString;

			if (mungConnection == null) {
				mungConnection = new Connection() {
					ConnectionId = -1,
					Name = MungSqlEventsConnectionName,
					ConnectionString = eventsConnectionString,
					ProviderId = Provider.Providers.FirstOrDefault(x => x.Name == "PostgreSQL").ProviderId,
					CreatedByMunger = -1,
					UpdatedByMunger = -1
				};

				mungConnection.Insert();
				return;
			}

			// Update the connection to use the system connection string
			if (mungConnection.ConnectionString != eventsConnectionString) {
				mungConnection.ConnectionString = eventsConnectionString;
				mungConnection.Update();
			}
		}

		public Provider Provider {
			get {
				return Provider.Providers.FirstOrDefault(x => x.ProviderId == ProviderId);
			}
		}

		public List<DbTable> GetTables() {
			var reader = Provider.GetSchemaReader(GetConnection);
			return reader.GetTables();
		}
		public List<DbTable> GetTables(string schema) {
			var reader = Provider.GetSchemaReader(GetConnection);
			return reader.GetTables();

		}
		public DbTable GetTable(string schema, string table) {
			var reader = Provider.GetSchemaReader(GetConnection);
			return reader.GetTables().FirstOrDefault();
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
		/// Executes 'sourceCommand' against this connection, outputting the
		/// results to the targetConnection.
		/// the table given by 'targetSchema' and 'targetTable' in the current connection
		/// </summary>
		/// <param name="source"></param>
		/// <param name="command"></param>
		/// <param name="parameters"></param>
		/// <param name="desintationSchema"></param>
		/// <param name="desginationTable"></param>
		public void BulkInsertTo(string sourceCommand,
			Connection destinationConnection, string desintationSchema, string desginationTable,
			Dictionary<string, object> parameters, Action<DbDataReader> eachRow) {

			var inserter = destinationConnection.Provider.GetBulkInserter(destinationConnection.GetConnection, desintationSchema, desginationTable);

			using (var sourceCn = GetConnection()) {

				using (var cmd = sourceCn.CreateCommand(sourceCommand, parameters)) {
					using (var reader = cmd.ExecuteReader()) {
						inserter.BulkInsert(reader, eachRow);
					}

				}

			}

		}

		public void Execute(string sql, Dictionary<string, object> parameters, Action<DbDataReader> eachRow) {

			using (var sourceCn = GetConnection()) {
				using (var cmd = sourceCn.CreateCommand(sql, parameters)) {
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							eachRow(reader);
						}

					}
				}

			}
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
		/// Get a connection
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static Connection Get(string connectionName) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var connection = cn.ExecuteAnonymousSql<Connection>(@"
					SELECT * FROM connection WHERE name = @connectionName",
					new { connectionName = connectionName }
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
