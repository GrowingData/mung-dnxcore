﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Mung;
using GrowingData.Utilities.DnxCore;

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
		public DateTime LastSeenAt { get; set; }




		public static bool HasMungers {
			get {
				if (!_mungersAtStartupChecked) {
					using (var cn = DatabaseContext.Db.Metadata()) {
						_mungersAtStartup = cn.DumpList<int>("SELECT COUNT(*) FROM mung.Munger", null).FirstOrDefault();
						_mungersAtStartupChecked = true;
					}
				}
				return _mungersAtStartup > 0;


			}
		}


		public static Munger Get(string email) {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var munger = cn.ExecuteAnonymousSql<Munger>(
						@"SELECT * FROM mung.Munger WHERE Email = @Email",
						 new { Email = email }
					)
					.FirstOrDefault();
				return munger;
			}
		}

		public static Munger Get(int mungerId) {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var munger = cn.ExecuteAnonymousSql<Munger>(
						@"SELECT * FROM mung.Munger WHERE MungerId = @MungerId",
						 new { MungerId = mungerId }
					)
					.FirstOrDefault();
				return munger;
			}
		}

		public bool Delete() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				cn.ExecuteSql(@"DELETE FROM mung.Munger WHERE MungerId = @MungerId ", this);
			}
			return true;
		}


		/// <summary>
		/// Insert the current Munger into the database and return the new MungerId
		/// </summary>
		/// <returns></returns>
		public int Insert() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var dbUser = cn.ExecuteAnonymousSql<Munger>(
					@"	INSERT INTO mung.Munger (Name, Email, PasswordHash, PasswordSalt, IsAdmin) 
                            VALUES (@Name, @Email, @PasswordHash, @PasswordSalt, @IsAdmin) 

						SELECT * FROM mung.Munger WHERE MungerId = SCOPE_IDENTITY()",
					this).FirstOrDefault();

				MungerId = dbUser.MungerId;

				_mungersAtStartupChecked = false;

				return MungerId;
			}
		}

		public bool Update() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				cn.ExecuteSql(@"
						UPDATE mung.Munger 
							SET 
								Name = @Name, 
								Email = @Email, 
								PasswordHash = @PasswordHash, 
								PasswordSalt = @PasswordSalt, 
								IsAdmin = @IsAdmin,
								LastSeenAt = @LastSeenAt
						WHERE MungerId = @MungerId",
					this
				);
				return true;

			}
		}
	}
}