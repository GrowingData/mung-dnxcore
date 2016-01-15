using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrowingData.Mung.Client {
	public class TimeStampedFile : IDisposable {
		private object _locker;

		private string _name;
		private string _basePath;
		private string _timeFormatString;

		private FileStream _openFile;
		private StreamWriter _writer;

		private string _filePath;


		public TimeStampedFile(string name, string basePath, string timeFormatString) {
			_name = name;
			_basePath = basePath;
			_filePath = Path.Combine(basePath, GetFilename(name, timeFormatString));

			_openFile = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			_writer = new StreamWriter(_openFile);
		}

		public TimeStampedFile(string name, string basePath)
			: this(name, basePath, "yyyy-MM-dd.HH.mm") {
		}

		public string GetFilename(string type, string timeFormatString) {

			var fileName = string.Format("{0}-{1}.log",
				_name,
				DateTime.UtcNow.ToString(timeFormatString)
			);

			return fileName;
		}

		public long WriteLine(string line) {
			lock (_locker) {
				var startPos = _openFile.Position;
				_openFile.WriteByte((byte)'0');
				_openFile.WriteByte((byte)'\t');
				_writer.WriteLine(line);
				// Try to make sure that its been flushed to the disk
				_writer.Flush();
				_openFile.Flush(true);

				return startPos;
			}
		}

		public void SetComplete(long position) {
			lock (_locker) {
				_openFile.Seek(position, SeekOrigin.Begin);
				_openFile.WriteByte((byte)'1');
				_openFile.Seek(0, SeekOrigin.End);
				
				_writer.Flush();
				_openFile.Flush(true);
			}
		}

		public void Dispose() {
			_openFile.Dispose();
			_writer.WriteLine();
		}
	}
}
