using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GrowingData.Mung.Client {

	internal class PersistentFileHandle : IDisposable {
		private string _fileName;
		private int _referenceCount = 0;
		private object _fileLock = new object();
		private Stream _fileStream;
		private StreamWriter _fileWriter;

		public int ReferenceCount { get { return _referenceCount; } }

		internal PersistentFileHandle(string fileName) {
			_fileName = fileName;
			_referenceCount = 0;

			_fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			_fileWriter = new StreamWriter(_fileStream);

		}

		public static string GetFilename(string name) {

			var fileName = string.Format("{0}-{1}.log",
				name,
				DateTime.UtcNow.ToString("yyyy-MM-dd.HH")
			);

			return fileName;
		}


		public void WriteEvent(PersistentQueueEvent evt) {
			var json = JsonConvert.SerializeObject(evt);
			lock (_fileLock) {
				_referenceCount++;

				_fileStream.Seek(0, SeekOrigin.End);
				evt.FilePosition = _fileStream.Position;

				// Write the start of the line
				_fileStream.WriteByte((byte)'0');
				_fileStream.WriteByte((byte)'\t');

				_fileWriter.WriteLine(json);

			}
		}

		public void SetEventComplete(PersistentQueueEvent evt) {
			lock (_fileLock) {
				_fileStream.Seek(evt.FilePosition, SeekOrigin.Begin);
				_fileStream.WriteByte((byte)'1');
				_referenceCount--;
			}
		}


		public void Dispose() {
			_fileWriter.Dispose();
			_fileStream.Dispose();

		}
	}
}

