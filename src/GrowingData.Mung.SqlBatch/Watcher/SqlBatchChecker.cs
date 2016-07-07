using System;
using System.IO;
using System.Data.SqlClient;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GrowingData.Utilities.Database;
using GrowingData.Utilities.Csv;

namespace GrowingData.Mung.SqlBatch {
	public static class SqlBatchChecker {

		private const string _lockFilename = "sql-batch.lock";

		/// <summary>
		/// Create a file letting the system know that `Check` is currently running
		/// so as to guarantee that only a single process is running at one time.
		/// </summary>
		/// <param name="dataPath"></param>
		/// <returns></returns>
		private static bool CreateLock(string dataPath) {

			File.AppendAllText(Path.Combine(dataPath, "sql-batch-check.log"), "Start: " + DateTime.UtcNow.ToString() + "\r\n");
			var lockFilePath = Path.Combine(dataPath, _lockFilename);

			// Make sure we aren't already running...
			if (File.Exists(lockFilePath)) {
				File.AppendAllText(Path.Combine(dataPath, "sql-batch-check.log"), "End (skipped due to lock): " + DateTime.UtcNow.ToString() + "\r\n");
				return false;
			}

			File.WriteAllText(lockFilePath, DateTime.Now.ToString());
			return true;

		}

		private static void ResetLock(string dataPath) {
			var lockFilePath = Path.Combine(dataPath, _lockFilename);
			bool deleteSuccess = false;

			while (!deleteSuccess) {
				try {

					if (File.Exists(lockFilePath)) {
						File.Delete(lockFilePath);
					}
					deleteSuccess = true;
				} catch (Exception ex) {
					// Exception will be that we can't access the file because its being used by 
					// another process, so wait for that process to go away and try again
					Thread.Sleep(500);
				}
			}
		}

		public static void CleanUpOldFiles(string dataPath, Func<NpgsqlConnection> fnConnection) {
			try {
				Console.WriteLine("SqlBatchChecker ->  Cleaning up old files");
				File.AppendAllText(Path.Combine(dataPath, "cleanup-running.log"), "Start: " + DateTime.UtcNow.ToString());

				ResetLock(dataPath);

				foreach (var file in Directory.EnumerateFiles(dataPath, "loaded-*")) {
					File.Delete(file);
				}

				Check("active-", dataPath, fnConnection);
				Check("failed-", dataPath, fnConnection);
				File.AppendAllText(Path.Combine(dataPath, "cleanup-running.log"), "Done: " + DateTime.UtcNow.ToString());

			} catch (Exception ex) {
				var exceptionLogPath = Path.Combine(dataPath, "cleanup.log");
				var errorDetails = string.Format("{0}	{1}: {2}\r\n{3}",
					DateTime.Now.ToString(),
					ex.Message,
					"Cleanup",
					ex.StackTrace
				);

				File.AppendAllText(exceptionLogPath, errorDetails);

			}
		}



		public static void Check(string dataPath, Func<NpgsqlConnection> fnConnection) {
			Check("complete-", dataPath, fnConnection);
		}

		/// <summary>
		/// Checks for files marked with the "prefix" given, and attempts to load them
		/// into the database given by the connection "fnConnection".  If an error occurrs
		/// duirng loading, the file will either be left alone of
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="dataPath"></param>
		/// <param name="fnConnection"></param>
		/// <param name="moveWithError"></param>
		public static void Check(string prefix, string dataPath, Func<NpgsqlConnection> fnConnection) {

			try {
				if (!CreateLock(dataPath)) {
					return;
				}

				foreach (var file in Directory.EnumerateFiles(dataPath, prefix + "*")) {
					try {
						var loadedFileName = file.Replace("\\" + prefix, "\\loaded-");
						if (File.Exists(loadedFileName)) {
							// If we have already loaded it, just delete the old file
							File.Delete(file);
							Console.WriteLine("Skipped loading {0}, as it already has a loaded file", file);
							continue;
						}

						Console.WriteLine("Loading {0}...", file);

						var info = new FileInfo(file);
						var table = info.Name.Split('.').First()
							.Replace("complete-", "")
							.Replace("active-", "")
							.Replace("failed-", "");

						var sqlInsert = new PostgresqlBulkInserter(fnConnection);
						sqlInsert.Execute("mung", table, file);

						File.Move(file, file.Replace("\\" + prefix, "\\loaded-"));


						Console.WriteLine(" Done.");
					} catch (Exception ex) {
						var exceptionLogPath = Path.Combine(dataPath, "sql-batch-exceptions.log");

						var errorDetails = string.Format("{0}	{1}: {2}\r\n{3}",
							DateTime.Now.ToString(),
							ex.Message,
							file,
							ex.StackTrace
						);

						File.AppendAllText(exceptionLogPath, errorDetails);

						File.Move(file, file.Replace("\\" + prefix, "\\failed-"));


					}
				}

				File.AppendAllText(Path.Combine(dataPath, "sql-batch-check.log"), "Finished: " + DateTime.UtcNow.ToString() + "\r\n");
				
			} catch (Exception ex) {
				var exceptionLogPath = Path.Combine(dataPath, "sql-batch-exceptions.log");
				var errorDetails = string.Format("{0}	{1}: {2}\r\n{3}",
					DateTime.Now.ToString(),
					ex.Message,
					"Big error",
					ex.StackTrace
				);

				File.AppendAllText(exceptionLogPath, errorDetails);



			} finally {
				ResetLock(dataPath);
			}
		}

	}
}
