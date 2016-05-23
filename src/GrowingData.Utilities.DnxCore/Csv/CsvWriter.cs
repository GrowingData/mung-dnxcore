using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using GrowingData.Utilities.Database;

namespace GrowingData.Utilities.Csv {
	public class CsvWriter : IDisposable {
		private static HashSet<Type> _validTypes = new HashSet<Type>(MungType.ValidTypes.Select(x => x.DotNetType));


		private TextWriter _writer;
		private List<DbColumn> _columns;
		public CsvWriter(TextWriter writer) {
			_writer = writer;
		}

		public void WriteHeader(IEnumerable<DbColumn> columns) {

			_columns = columns.ToList();
			_writer.Write(string.Join("\t", _columns.Select(c => $"{c.ColumnName}:{c.MungType}")));
			_writer.Write("\n");
		}

		/// <summary>
		/// Writes a row of data to the file, using the same order as specified
		/// in _columns.
		/// </summary>
		/// <param name="reader"></param>
		public void WriteRow(DbDataReader reader) {
			if (_columns == null) {
				throw new InvalidOperationException("Unable to Write a row until the header has been written");
			}

			for (var i = 0; i < _columns.Count; i++) {
				_writer.Write(CsvConverter.Serialize(reader[_columns[i].ColumnName]));
				_writer.Write("\t");
			}
			_writer.Write("\n");

			// Always flush to make sure that data is safe
			_writer.Flush();
		}


		public void WriteRow(Dictionary<string, object> reader) {
			if (_columns == null) {
				throw new InvalidOperationException("Unable to Write a row until the header has been written");
			}

			for (var i = 0; i < _columns.Count; i++) {
				if (reader.ContainsKey(_columns[i].ColumnName)) {
					_writer.Write(CsvConverter.Serialize(reader[_columns[i].ColumnName]));
					_writer.Write("\t");
				} else {
					_writer.Write("NULL\t");
				}
			}
			_writer.Write("\n");

			// Always flush to make sure that data is safe
			_writer.Flush();	
		}

		public void Dispose() {
			_writer.Flush();
		}

		public static void WriteException(Exception ex, TextWriter writer) {
			writer.Write("###ERROR###\r\n");
			writer.Write(ex.Message + "\r\n" + ex.StackTrace);
		}


		/// <summary>
		/// Reads data from the data set and writes it to the supplied stream.
		/// </summary>
		/// <param name="reader">Open reader that hasn't been read from yet</param>
		/// <param name="writer"></param>
		/// <returns>Number of rows written</returns>
		public static int Write(DbDataReader reader, TextWriter writer) {
			try {
				using (CsvWriter msv = new CsvWriter(writer)) {
					int rowCount = 0;
					while (reader.Read()) {

						if (rowCount == 0) {
							List<DbColumn> columns = new List<DbColumn>();


							List<string> names = new List<string>();
							List<DbType> types = new List<DbType>();
							List<string> headers = new List<string>();

							for (var i = 0; i < reader.FieldCount; i++) {

								var name = reader.GetName(i);
								var type = reader[i].GetType();

								if (!_validTypes.Contains(type)) {
									throw new TypeLoadException($"Unable to work with Type: {type}");
								}

								columns.Add(new DbColumn(name, MungType.Get(type)));
							}
							msv.WriteHeader(columns);
						}
						msv.WriteRow(reader);
						rowCount++;
					}
					return rowCount;
				}

			} catch (Exception ex) {
				//MungLog.LogException("MungedData.Write", ex);
				return -1;
			}
		}
		
	}
}
