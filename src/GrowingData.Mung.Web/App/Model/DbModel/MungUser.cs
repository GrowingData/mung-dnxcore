using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Mung;
using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class MungUser {
		private static bool _mungersAtStartupChecked = false;
		private static int _mungersAtStartup = -1;

		public int MungUserId { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }

		public bool IsAdmin { get; set; }

		public string InvitationCode { get; set; }

		public string PasswordHash { get; set; }
		public string PasswordSalt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime LastSeenAt { get; set; }




		public static bool HasMungers {
			get {
				if (!_mungersAtStartupChecked) {
					using (var cn = DatabaseContext.Db.Mung()) {
						_mungersAtStartup = (int) cn.DumpList<long>("SELECT COUNT(*) FROM MungUser", null).FirstOrDefault();
						_mungersAtStartupChecked = true;
					}
				}
				return _mungersAtStartup > 1;


			}
		}


		public static MungUser Get(string email) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var munger = cn.ExecuteAnonymousSql<MungUser>(
						@"SELECT * FROM MungUser WHERE Email = @Email",
						 new { Email = email }
					)
					.FirstOrDefault();
				return munger;
			}
		}

		public static MungUser Get(int MungUserId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var munger = cn.ExecuteAnonymousSql<MungUser>(
						@"SELECT * FROM MungUser WHERE MungUserId = @MungUserId",
						 new { MungUserId = MungUserId }
					)
					.FirstOrDefault();
				return munger;
			}
		}

		public bool Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				cn.ExecuteSql(@"DELETE FROM MungUser WHERE MungUserId = @MungUserId ", this);
			}
			return true;
		}


		/// <summary>
		/// Insert the current Munger into the database and return the new MungUserId
		/// </summary>
		/// <returns></returns>
		public int Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var dbUser = cn.ExecuteAnonymousSql<MungUser>(
					@"	INSERT INTO MungUser (Name, Email, PasswordHash, PasswordSalt, IsAdmin) 
                            VALUES (@Name, @Email, @PasswordHash, @PasswordSalt, @IsAdmin);

						SELECT * FROM MungUser WHERE MungUserId = lastval();",
					this).FirstOrDefault();

				MungUserId = dbUser.MungUserId;

				_mungersAtStartupChecked = false;

				return MungUserId;
			}
		}

		public bool Update() {
			using (var cn = DatabaseContext.Db.Mung()) {
				cn.ExecuteSql(@"
						UPDATE MungUser 
							SET 
								Name = @Name, 
								Email = @Email, 
								PasswordHash = @PasswordHash, 
								PasswordSalt = @PasswordSalt, 
								IsAdmin = @IsAdmin,
								LastSeenAt = @LastSeenAt
						WHERE MungUserId = @MungUserId",
					this
				);
				return true;

			}
		}
	}
}