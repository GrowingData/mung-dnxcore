using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using GrowingData.Mung.Core;
using Newtonsoft.Json;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {


	public class JsonPostgresProcessor : EventProcessor {
		private const string _schemaName = "mung";

		private Func<NpgsqlConnection> _getConnection;

		//private NpgsqlConnection _connection;
		//private NpgsqlBinaryImporter _importer;

		public JsonPostgresProcessor(Func<NpgsqlConnection> connection) : base("JsonPostgresProcessor") {
			_getConnection = connection;

			CheckTable();

		}

		//private void SetupImport() {
		//	_connection = _getConnection();
		//	_importer = _connection.BeginBinaryImport(CopyCommand());

		//}

		private void CheckTable() {
			ExecuteCommand($"CREATE SCHEMA IF NOT EXISTS {_schemaName}");

			var tableDefinition = $"CREATE TABLE IF NOT EXISTS {_schemaName}.all(" + @"
                at timestamp with time zone NOT NULL,
				event_type text NOT NULL,
				source text NOT NULL,
				app_id int NOT NULL,
				json_data json NOT NULL
			)";
			ExecuteCommand(tableDefinition);
		}

		public static string CopyCommand() {
			return $"COPY {_schemaName}.all(at, event_type, source, app_id, json_data) FROM STDIN BINARY";
		}


		protected override void ProcessEvent(MungServerEvent mungEvent) {

			using (var cn = _getConnection()) {
				using (var importer = cn.BeginBinaryImport(CopyCommand())) {
					importer.StartRow();
					importer.Write(mungEvent.LogTime, NpgsqlDbType.TimestampTZ);
					importer.Write(mungEvent.Type, NpgsqlDbType.Text);
					importer.Write(mungEvent.Source, NpgsqlDbType.Text);
					importer.Write(mungEvent.AppId, NpgsqlDbType.Integer);
					importer.Write(JsonConvert.SerializeObject(mungEvent.Data), NpgsqlDbType.Json);

				}
			}
		}

		public bool ExecuteCommand(string sql) {
			using (var cn = _getConnection()) {
				using (var cmd = new NpgsqlCommand(sql, cn)) {
					cmd.ExecuteNonQuery();
				}
			}
			return true;

		}



	}
}
