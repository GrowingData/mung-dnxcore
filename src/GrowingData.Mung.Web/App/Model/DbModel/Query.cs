using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Query : IBracketsEditable {
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

		public static Query Get(string name) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM query WHERE name = @Name OR name = @DecodedName";
				var query = cn.ExecuteAnonymousSql<Query>(sql, new { Name = name, DecodedName = WebUtility.UrlDecode(name) })
					.FirstOrDefault();

				return query;
			}
		}
		public static Query Get(int queryId) {
			using (var cn = DatabaseContext.Db.Mung()) {

				var sql = @"SELECT * FROM query WHERE query_id = @QueryId";
				var query = cn.ExecuteAnonymousSql<Query>(sql, new { QueryId = queryId })
					.FirstOrDefault();

				return query;
			}
		}
		public static List<Query> List(int mungerId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM query";
				var queries = cn.ExecuteAnonymousSql<Query>(sql, null);
				return queries;
			}
		}

		public Query Save() {
			if (QueryId <= 0) {
				return Insert();
			} else {
				return Update();
			}
		}

		public Query Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					INSERT INTO query (name, path, code, created_by_munger, updated_by_munger)
						VALUES (@Name, @Path, @Code, @CreatedByMunger, @CreatedByMunger)
						RETURNING query_id
					";

				QueryId = cn.DumpList<int>(sql, this).FirstOrDefault();
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
				cn.ExecuteSql(sql, this);

				return this;
			}
		}

		public void Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM query WHERE query_id = @QueryId";
				cn.ExecuteSql(sql, this);
			}
		}
	}
}