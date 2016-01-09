using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using GrowingData.Mung.Core;

namespace GrowingData.Mung.Relationizer {
	/// <summary>
	/// Manages schema changes over time, ensuring that everything is
	/// always written in a consistent manner, even if the objects schema
	/// is somewhat different to the actual schema (e.g. more columns)
	/// </summary>
	public class RelationalEventWriter : IDisposable {
		private object _syncRoot = new object();
		private string _eventName;
		private string _parentEvent;
		private string _basePath;
		private string _lastTimeString;

		private SortedList<string, DbColumn> _schema;

		// For the same event type, we might actually have multiple
		// different schemas (e.g. the event is being logged with different params),
		// so we need to track them independently
		private Dictionary<string, RelationalEventFile> _schemaFiles;

		private int _rowsWritten = 0;

		public RelationalEventWriter(string basePath, string eventName, string parentEvent) {
			_eventName = eventName;
			_parentEvent = parentEvent;
			_schemaFiles = new Dictionary<string, RelationalEventFile>();
			_basePath = basePath;
		}


		/// <summary>
		/// Get the file name for the current moment in time, plus the schema version
		/// </summary>
		/// <returns></returns>
		private string GetCurrentTimeString() {
			//var minutes = Math.Floor(DateTime.UtcNow.Minute / 10d) * 10;
			var minutes = DateTime.UtcNow.Minute;

			return string.Format("{0}.{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd.HH"),
				minutes
			);
		}

		public void Write(RelationalEvent evt) {
			lock (_syncRoot) {
				if (_schema == null) {
					_schema = new SortedList<string, DbColumn>();
					foreach (var s in evt.Schema) {
						_schema.Add(s.ColumnName, s);
					}


				} else {
					// Check to see if anything has changed...
					foreach (var s in evt.Schema) {
						if (_schema.ContainsKey(s.ColumnName)) {
							if (_schema[s.ColumnName].ColumnType != s.ColumnType) {
								var oldType = _schema[s.ColumnName].ColumnType;
								var newType = s.ColumnType;



								// Try to expand the type without losing fidelity
								_schema[s.ColumnName].ColumnType = MungType.ExpandType(oldType, newType);
							}
						} else {
							// There is a new field
							_schema.Add(s.ColumnName, s);
						}
					}
				}

				// Its a new time block, so dispose of all the active files
				if (_lastTimeString == null || _lastTimeString != GetCurrentTimeString()) {
					var keys = _schemaFiles.Keys.ToList();

					foreach (var k in keys) {
						_schemaFiles[k].Dispose();
						_schemaFiles.Remove(k);
					}

					_lastTimeString = GetCurrentTimeString();
				}

				var schemaHash = RelationalEventFile.GetSchemaHash(_schema);

				if (!_schemaFiles.ContainsKey(schemaHash)) {
					_schemaFiles[schemaHash] = new RelationalEventFile(_basePath, GetCurrentTimeString(), _eventName, _parentEvent, _schema);
				}

				_schemaFiles[schemaHash].WriteRow(evt);
			}
		}


		public void Dispose() {
			foreach (var k in _schemaFiles.Keys) {
				_schemaFiles[k].Dispose();
				_schemaFiles.Remove(k);
			}
		}
	}

}
