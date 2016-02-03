using System;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvToolkit;
using CsvToolkit.Read;
using System.Collections;

namespace GrowingData.Mung.Core {

	/// <summary>
	/// A munged data file is a 
	/// </summary>
	public class ReaderState {
		public int LineNumber;
		public int FieldNumber;
	}

	public class MsvReader : DbDataReader {

		private LineReader _lineReader;
		private IEnumerator<string[]> _cursor;


		private List<DbColumn> _columns;
		public List<DbColumn> Columns { get { return _columns; } }
		
		public int RowNumber { get { return _lineReader.LineNumber; } }


		public MsvReader(TextReader reader) {
			_lineReader = new LineReader(new CsvReaderOptions() {
				SeparatorChar = '\t',
				FirstLineContainsHeaders = true,
				InvalidTextAction = InvalidTextMode.Throw,
				QuoteChar = '\"'
			}, reader);

			_cursor = _lineReader.GetLines().GetEnumerator();

			// Read the first line, as we expect it to have headers
			_cursor.MoveNext();


			// The file format here is that the first row includes the 
			// columnHeaders in the format "<name>:<type>"
			var columnHeaders = _cursor.Current;

			var columns = new DbColumn[columnHeaders.Length];

			

			for (var i = 0; i < columnHeaders.Length; i++) {
				var header = columnHeaders[i];
				var splitHeader = header.Split(':');



				if (splitHeader.Length == 2) {
					var columnName = splitHeader[0];
					var typeHeader = splitHeader[1];

					var mungType = MungType.Parse(typeHeader);

					columns[i] = new DbColumn(columnName, mungType);
				}
			}
			_columns = columns.ToList();
		}
		public override bool NextResult() {
			return false;
		}

		public override bool Read() {
			return _cursor.MoveNext();
		}

		public override int Depth { get { return 1; } }
		public override int FieldCount { get { return _columns.Count; } }
		public override bool HasRows { get { return _lineReader.IsReading; } }
		public override bool IsClosed { get { return _lineReader.IsEOF; } }

		public override int RecordsAffected { get { return -1; } }

		public override object this[string name] {
			get {
				var ordinal = GetOrdinal(name);
				if (ordinal != -1) {
					return GetValue(ordinal);
				}

				throw new KeyNotFoundException($"Unable to find column with name {name}");
			}
		}

		public override object this[int ordinal] {
			get {
				return GetValue(ordinal);
			}
		}


		public override object GetValue(int fieldIndex) {
			var val = _cursor.Current[fieldIndex];
			var readerState = new ReaderState() {
				LineNumber = _lineReader.LineNumber,
				FieldNumber = fieldIndex
			};

			return MsvConverter.Read(val, _columns[fieldIndex].MungType.DatabaseType, readerState);

		}



		public override bool GetBoolean(int ordinal) {
			return (bool)GetValue(ordinal);
		}

		public override byte GetByte(int ordinal) {
			return (byte)GetValue(ordinal);
		}

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
			throw new NotImplementedException();
		}
		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
			throw new NotImplementedException();
		}


		public override char GetChar(int ordinal) {
			return (char)GetValue(ordinal);
		}

		public override string GetDataTypeName(int ordinal) {
			return _columns[ordinal].MungType.ToString();
		}

		public override DateTime GetDateTime(int ordinal) {
			return (DateTime)GetValue(ordinal);
		}

		public override decimal GetDecimal(int ordinal) {
			return (decimal)GetValue(ordinal);
		}

		public override double GetDouble(int ordinal) {
			return (double)GetValue(ordinal);
		}

		public override IEnumerator GetEnumerator() {
			throw new NotImplementedException();
		}

		public override Type GetFieldType(int ordinal) {
			throw new NotImplementedException();
		}

		public override float GetFloat(int ordinal) {
			return (float)GetValue(ordinal);
		}

		public override Guid GetGuid(int ordinal) {
			return (Guid)GetValue(ordinal);
		}

		public override short GetInt16(int ordinal) {
			return (short)GetValue(ordinal);
		}

		public override int GetInt32(int ordinal) {
			return (int)GetValue(ordinal);
		}

		public override long GetInt64(int ordinal) {
			return (long)GetValue(ordinal);
		}

		public override string GetName(int ordinal) {
			return _columns[ordinal].ColumnName;
		}

		public override int GetOrdinal(string name) {
			for (var i = 0; i < _columns.Count; i++) {
				if (_columns[i].ColumnName == name) {
					return i;
				}
			}
			return -1;
		}

		public override string GetString(int ordinal) {
			return (string)GetValue(ordinal);
		}

		public override int GetValues(object[] values) {
			if (values.Length != _columns.Count) {
				throw new InvalidOperationException($"Expected the values array to contain {_columns.Count} values, not {values.Length} as was received.");
			}

			for (var i = 0; i < _columns.Count; i++) {
				values[i] = GetValue(i);
			}
			return _columns.Count;
		}

		public override bool IsDBNull(int ordinal) {
			var val = _cursor.Current[ordinal].ToLower();
			return MsvConverter.IsDBNull(val);

		}

	}
}
