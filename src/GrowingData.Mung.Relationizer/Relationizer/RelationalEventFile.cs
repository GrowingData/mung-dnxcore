using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using GrowingData.Utilities.DnxCore;
using GrowingData.Mung.Core;

namespace GrowingData.Mung.Relationizer {

	/// <summary>
	/// Holds the actual reference to a file which a RelationalEventWriter
	/// will write to.
	/// </summary>
	public class RelationalEventFile : IDisposable {
		public const string FilePrefixActive = "active";
		public const string FilePrefixComplete = "complete";

		public static List<DbColumn> DefaultColumns = new List<DbColumn>() {
			{new DbColumn("_id_", MungType.Get(typeof(string))) },
			{new DbColumn("_parentId_",  MungType.Get(typeof(string))) },
			{new DbColumn("_at_",  MungType.Get(typeof(DateTime))) },
			{new DbColumn("_source_",  MungType.Get(typeof(string))) },
			{new DbColumn("_appId_",  MungType.Get(typeof(int))) },
		};

		private object _locker = new object();

		private StreamWriter _currentStream;
		private MsvWriter _writer;

		private string _basePath;
		private string _time;
		private string _eventName;
		private string _parentEvent;
		private string _schemaHash;

		private SortedList<string, DbColumn> _schema;

		public string SchemaHash { get { return _schemaHash; } }
		public string TimeString { get { return _time; } }


		public string GetFilepath(string type) {
			return Path.Combine(_basePath, GetFilename(type));
		}


		public string GetFilename(string type) {
			var fileName = string.Format("{0}-{1}.{2}-{3}.msv",
				type,
				_eventName,
				_time,
				_schemaHash);

			return fileName;
		}

		public RelationalEventFile(string basePath, string time, string eventName, string parentEvent, SortedList<string, DbColumn> schema) {
			_basePath = basePath;
			_time = time;
			_eventName = eventName;
			_parentEvent = parentEvent;
			_schema = new SortedList<string, DbColumn>(schema);

			_schemaHash = GetSchemaHash(schema);

			Open();
		}

		private void Open() {
			lock (_locker) {
				var fileStream = File.Open(GetFilepath(FilePrefixActive), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				_currentStream = new StreamWriter(fileStream);

				_writer = new MsvWriter(_currentStream);

				WriteHeader(_currentStream, _schema);
			}
		}

		public void WriteRow(RelationalEvent evt) {
			WriteRow(_currentStream, _schema, evt);
		}




		public void Dispose() {
			if (_currentStream != null) {
				_currentStream.Dispose();

				File.Move(GetFilepath(FilePrefixActive), GetFilepath(FilePrefixComplete));
			}
		}


		public void WriteHeader(StreamWriter writer, SortedList<string, DbColumn> schema) {
			var columns = new List<DbColumn>();
			columns.Add(new DbColumn("_id_", MungType.Get(typeof(string))));
			if (_parentEvent != null) {
				columns.Add(new DbColumn("_pid_", MungType.Get(typeof(string))));
			}

			columns.Add(new DbColumn("_at_", MungType.Get(typeof(DateTime))));
			columns.Add(new DbColumn("_source_", MungType.Get(typeof(string))));
			columns.Add(new DbColumn("_app_", MungType.Get(typeof(string))));


			foreach (var c in schema.Values) {
				columns.Add(c);
			}

			_writer.WriteHeader(columns);
		}

		public void WriteRow(StreamWriter writer, SortedList<string, DbColumn> schema, RelationalEvent evt) {
			lock (_locker) {
				var values = new Dictionary<string, object>();
				values.Add("_id_", evt.Id);
				if (evt.ParentType != null) {
					values.Add("_pid_", evt.ParentId);

				}
				values.Add("_at_", evt.LogTime);
				values.Add("_source_", evt.Source);
				values.Add("_app_", evt.AppId);

				foreach (var kv in evt.Values) {
					values.Add(kv.Key, kv.Value);
				}

				_writer.WriteRow(values);
			}
		}

		public static string GetSchemaHash(SortedList<string, DbColumn> schema) {
			var schemaString = string.Join("|", schema.Values.Select(x => string.Format("{0}:{1}", x.ColumnName, x.MungType)));
			return schemaString.HashStringMD5();
		}


	}

}
