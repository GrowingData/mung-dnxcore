using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GrowingData.Utilities.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GrowingData.Mung.Web.Models {
	public class Query : IBracketsEditable {
		public const string DirectiveEventName = "@MUNG_EVENT_NAME";
		public const string DirectiveEventFilter = "@MUNG_EVENT_FILTER";

		public int QueryId;

		public string Name;
		public string Path;
		public string Code;

		public int CreatedByMunger;
		public int UpdatedByMunger;

		public DateTime CreatedAt;
		public DateTime UpdatedAt;

		public string EncodedName { get { return WebUtility.UrlEncode(Name); } }

		public string ResourceUrl { get { return $"/{MungFileSystem.QueryRootUrlPart}/{EncodedName}"; } }

		public string EventType {
			get {
				var lines = Code.Split('\n').Select(x => x.Trim());
				var eventTypeLine = lines.FirstOrDefault(x => x.StartsWith(DirectiveEventName));

				if (eventTypeLine != null) {
					return eventTypeLine.Replace(DirectiveEventName, "").Trim();
				}
				return null;
			}
		}

		public JToken Filter {
			get {
				var lines = Code.Split('\n').Select(x => x.Trim());
				var filterLine = lines.FirstOrDefault(x => x.StartsWith(DirectiveEventFilter));

				if (filterLine != null) {
					return JToken.Parse(filterLine.Replace(DirectiveEventFilter, ""));
				}

				return null;
			}
		}

		public static Query Get(string name) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM query WHERE name = @Name OR name = @DecodedName";
				var query = cn.SelectAnonymous<Query>(sql, new { Name = name, DecodedName = WebUtility.UrlDecode(name) })
					.FirstOrDefault();

				return query;
			}
		}
		public static Query Get(int queryId) {
			using (var cn = DatabaseContext.Db.Mung()) {

				var sql = @"SELECT * FROM query WHERE query_id = @QueryId";
				var query = cn.SelectAnonymous<Query>(sql, new { QueryId = queryId })
					.FirstOrDefault();

				return query;
			}
		}
		public static List<Query> List(int mungerId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM query";
				var queries = cn.SelectAnonymous<Query>(sql, null);
				return queries;
			}
		}

		public Query Save() {
			Query q = null;
			if (QueryId <= 0) {
				q = Insert();
			} else {
				q = Update();
			}
			MungApp.Current.QueryEventProcessor.ReloadQueries();
			return q;
		}

		public Query Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					INSERT INTO query (name, path, code, created_by_munger, updated_by_munger)
						VALUES (@Name, @Path, @Code, @CreatedByMunger, @CreatedByMunger)
						RETURNING query_id
					";

				QueryId = cn.SelectList<int>(sql, this).FirstOrDefault();
				return this;
			}
		}

		public Query Update() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					UPDATE query 
						SET 
							name = @Name, 
							path = @Path, 
							code = @Code,
							updated_by_munger = @CreatedByMunger,
							updated_at = @UpdatedAt
						WHERE query_id = @QueryId
					";
				cn.ExecuteNonQuery(sql, this);

				return this;
			}
		}

		public void Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM query WHERE query_id = @QueryId";
				cn.ExecuteNonQuery(sql, this);
			}
			MungApp.Current.QueryEventProcessor.ReloadQueries();
		}
	}
}