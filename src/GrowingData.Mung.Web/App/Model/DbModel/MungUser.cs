using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Mung;
using GrowingData.Utilities.DnxCore;
using GrowingData.Utilities.Database;

namespace GrowingData.Mung.Web.Models {
	public class Munger {
		private static bool _mungersAtStartupChecked = false;
		private static int _mungersAtStartup = -1;

		public int MungerId { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }

		public bool IsAdmin { get; set; }

		public string InvitationCode { get; set; }

		public string PasswordHash { get; set; }
		public string PasswordSalt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime SeenAt { get; set; }




		public static bool HasMungers {
			get {
				if (!_mungersAtStartupChecked) {
					using (var cn = DatabaseContext.Db.Mung()) {
						var sql = @"SELECT COUNT(*) FROM munger";
						_mungersAtStartup = (int)cn.SelectList<long>(sql, null).FirstOrDefault();
						_mungersAtStartupChecked = true;
					}
				}
				return _mungersAtStartup > 1;


			}
		}


		public static Munger Get(string email) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM munger WHERE email = @Email";
				var munger = cn.SelectAnonymous<Munger>(sql, new { Email = email })
					.FirstOrDefault();

				return munger;
			}
		}

		public static Munger Get(int mungerId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM munger WHERE munger_id = @MungerId";
				var munger = cn.SelectAnonymous<Munger>(sql, new { MungerId = mungerId })
					.FirstOrDefault();
				return munger;
			}
		}

		public bool Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM munger WHERE munger_id = @MungerId ";
				cn.ExecuteNonQuery(sql, this);
			}
			return true;
		}


		public Munger Save() {
			if (MungerId <= 0) {
				return Insert();
			} else {
				return Update();
			}
		}

		/// <summary>
		/// Insert the current Munger into the database and return the new MungerId
		/// </summary>
		/// <returns></returns>
		public Munger Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"	
					INSERT INTO munger (name, email, password_hash, password_salt, is_admin) 
                        VALUES (@Name, @Email, @PasswordHash, @PasswordSalt, @IsAdmin)
						RETURNING munger_id";

				MungerId = cn.SelectList<int>(sql, this).FirstOrDefault();

				_mungersAtStartupChecked = false;

				return this;
			}
		}

		public Munger Update() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"	
					UPDATE munger 
						SET 
							name = @Name, 
							email = @Email, 
							password_hash = @PasswordHash, 
							password_salt = @PasswordSalt, 
							is_admin = @IsAdmin,
							seen_at = @SeenAt
						WHERE munger_id = @MungerId";
				cn.ExecuteNonQuery(sql, this);
				return this;

			}
		}
	}
}