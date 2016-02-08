using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {


	public class QueryEventProcessor : EventProcessor {

		private Dictionary<string, List<Query>> _queries;

		public QueryEventProcessor() : base("QueryEventProcessor") {
			ReloadQueries();
		}
		public void ReloadQueries() {
			_queries = new Dictionary<string, List<Query>>();
			var queryList = Query.List(-1)
				.Where(q => !string.IsNullOrEmpty(q.EventType));

			var grouped = queryList.GroupBy(x => x.EventType);

			foreach (var group in grouped) {
				_queries[group.Key] = group.ToList();
			}
		}

		protected override void ProcessEvent(MungServerEvent mungEvent) {

			Console.WriteLine($"QueryProcessor.ProcessEvent({mungEvent.Type})");
			if (_queries.ContainsKey(mungEvent.Type)) {
				var toFire = _queries[mungEvent.Type];
				foreach (var fire in toFire) {

					Console.WriteLine($"QueryProcessor.ExecuteQuery({mungEvent.Type}, {fire.QueryId})");
					try {
						MungQuery.Execute(fire.Code, null, null);
					} catch (Exception ex) {
						Console.WriteLine($"Unable to process query (queryId: {fire.QueryId}):\r\n{ex.Message}\t\n----------\r\n{fire.Code}\r\n");

						MungApp.Current.ProcessInternalEvent("event_processor_query_error", new {
							query_id = fire.QueryId,
							code = fire.Code,
							message = ex.Message,
							stack = ex.StackTrace
						});

					}
				}
			}


		}



	}
}
