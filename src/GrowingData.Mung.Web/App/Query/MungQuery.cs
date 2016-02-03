using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Mung.Web.Models;


namespace GrowingData.Mung.Web {
	public class MungQuery {

		public static MungQuery Execute(string query, Dictionary<string, object> parameters, Action<DbDataReader> eachRow) {
			var q = new MungQuery(query);
			q.Execute(parameters, eachRow);
			return q;

		}

		private string _query;
		private string _rewrittenQuery;
		private string[] _lines;

		private Connection _connection;
		private Connection _destinationConnection;
		private string _destinationSchema;
		private string _destinationTable;

		private List<Connection> _connections;
		//private Task _execution;

		private bool _isComplete = false;
		public bool IsComplete { get { return _isComplete; } }

		public MungQuery(string query) {
			_query = query;
			ParseQuery();
		}

		//public void BeginExecute(Dictionary<string, object> parameters) {
		//	_execution = Task.Run(() => Execute(parameters));
		//}

		public void Execute(Dictionary<string, object> parameters, Action<DbDataReader> eachRow) {
			_isComplete = false;
			if (_destinationConnection != null && _destinationSchema != null && _destinationTable != null) {
				_connection.BulkInsertTo(_rewrittenQuery,
					_destinationConnection, _destinationSchema, _destinationTable,
					parameters, eachRow);


				_isComplete = true;
			} else {
				_connection.Execute(_rewrittenQuery, parameters, eachRow);
				_isComplete = true;
			}
		}

		private void ParseQuery() {
			var newQuery = new StringBuilder();

			_connections = Connection.List();
			var lines = _query.Split('\n');

			for (var i = 0; i < lines.Length; i++) {
				var line = lines[i].Trim();
				var addToQuery = !ParseInput(line) && !ParseOutput(line);
				if (addToQuery) {
					newQuery.AppendLine(line);
				}
			}


			if (_connection == null) {
				throw new Exception(
@"Please specify a connection to run this query on with a line:
	INPUT <connection_name>;

Command at the start of the query.");
			}
			_rewrittenQuery = newQuery.ToString();

		}

		private bool ParseInput(string line) {
			if (line.ToLower().StartsWith("input ")) {
				var connectionName = line.Split(' ').Last().Replace(";", "");
				_connection = _connections.FirstOrDefault(c => c.Name.ToLower() == connectionName.ToLower());

				if (_connection == null) {
					throw new Exception($"Unable to find the source connection named '{connectionName}'.");
				}

				return true;
			}
			return false;
		}

		private bool ParseOutput(string line) {
			if (line.ToLower().StartsWith("output ")) {
				var copyToDestination = line.Substring(7)
					.Replace(";", "")
					.Split('.')
					.Select(x => x.Trim())
					.ToArray();
				if (copyToDestination.Length != 3) {
					throw new Exception(
@"If specifying OUTPUT please ensure that the destination contains 3 parts:
	OUTPUT <destination_connection>.<destination_schema>.<destination_table>;

Command at the start of the query.");
				}
				_destinationConnection = _connections.FirstOrDefault(c => c.Name.ToLower() == copyToDestination[0]);

				if (_destinationConnection == null) {
					throw new Exception($"Unable to find the destination connection named '{copyToDestination[0]}'.");
				}
				_destinationSchema = copyToDestination[1];
				_destinationTable = copyToDestination[2];

				return true;
			}
			return false;


		}
	}
}
