
// Adapted from: https://bitbucket.org/nxt/csv-toolkit

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GrowingData.Utilities.Csv {
	public class LineReader : IDisposable {
		private enum State {
			ProcessingText1,
			AfterProcessingText,
			InQuotedString,
			IgnoreWhitespace
		}
		private readonly TextReader _reader;
		private readonly bool _dispose;
		private readonly CsvReaderOptions _opts;
		private int _lineNumber = 0;
		private int _columnNumber = 0;

		private bool _isReading = false;
		private bool _isEOF = false;

		private long _newLineCount = 0;

		public int LineNumber { get { return _lineNumber; } }
		public bool IsReading { get { return _isReading; } }
		public bool IsEOF { get { return _isEOF; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input">The stream to read</param>
		/// <param name="dispose"></param>
		public LineReader(CsvReaderOptions options, TextReader reader, bool dispose = false) {
			_opts = options;
			this._reader = reader;
			this._dispose = dispose;
		}

		public LineReader(CsvReaderOptions options, Stream input, Encoding encoding, bool dispose = false) {
			_opts = options;
			this._dispose = dispose;
			this._reader = new StreamReader(input, encoding);
		}

		public LineReader(CsvReaderOptions options, FileInfo file, Encoding encoding) {
			_opts = options;
			var input = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
			_dispose = true;
			_reader = new StreamReader(input, encoding);
		}
		public void Dispose() {
			if (_dispose)
				_reader.Dispose();
		}

		public IEnumerable<string[]> GetLines() {
			_isReading = true;
			var sb = new StringBuilder();
			while (PeekChar() != -1) {
				_lineNumber++;
				_columnNumber = 0;
				var res = ConsumeLine(sb).ToArray();
				if (res.Length > 0)
					yield return res;
			}
			_isReading = false;
			_isEOF = true;
		}


		private int ReadChar() {
			_columnNumber++;
			var c = _reader.Read();
			if (c == (int)'\n') {
				_newLineCount++;
			}
			return c;
		}
		private int PeekChar() {
			return _reader.Peek();
		}

		private IEnumerable<string> ConsumeLine(StringBuilder sb) {

			var trailingWhitespace = new StringBuilder();
			var quoteChar = _opts.QuoteChar;
			var fieldSeparatorChar = _opts.SeparatorChar;
			var eolStyle = _opts.EndOfLineStyle;
			var allowQuotedFields = !_opts.DisableQuotedFields;
			var removeWhiteSpaceAroundSeparators = !_opts.KeepWhiteSpaceAroundSeparators;
			sb.Length = 0;
			var running = true;
			var initialState = removeWhiteSpaceAroundSeparators ? State.IgnoreWhitespace : State.ProcessingText1;
			var state = initialState;
			char lastChar = 'x';
			do {
				//read header
				int cc = ReadChar();
				if (cc == -1) {
					running = false;
					if (sb.Length > 0)
						yield return sb.ToString();
					sb.Length = 0;
					yield break;
				}
				var c = (char)cc;

				if (state == State.IgnoreWhitespace && Char.IsWhiteSpace(c) && c != '\r' && c != '\n')
					continue;

				if (allowQuotedFields && c == quoteChar && state != State.AfterProcessingText) {
					if (state == State.InQuotedString) {
						// If we are in a quoted string, then end the quoted string if we get the 
						// quote character. Unless its prefixed with a "\" or quotechar to escape it (provided
						// that the field doesn't look like '""',)
						//if (PeekChar() == (int)quoteChar || lastChar=='\\') {
						//	ReadChar();
						
						if (lastChar == '\\' && !_opts.ExcelQuoted) {
							sb.Append(c);
						} else {
							// Also allow double quote char ("") to escape a quote char, as long as its not the first 
							// character
							if (_opts.ExcelQuoted) {
								// Except where a field is  """Something that should be quoted"".
								if (PeekChar() == (int)quoteChar) {
									// Is it a double quote, or is it the end of the field?

									// The quote
									ReadChar();

									if (PeekChar() == (int)fieldSeparatorChar && sb.Length == 1) {
										state = State.AfterProcessingText;
									} else {
										sb.Append("\\" + c);
										continue;
									}
								}
							}

							// Keep the '"' quote characters because I want them so I can use the JSON serializer
							sb.Append(quoteChar);
							state = State.AfterProcessingText;
						}
					} else {
						//switch mode
						// Keep the '"' quote characters because I want them so I can use the JSON serializer
						sb.Append(quoteChar);
						state = State.InQuotedString;
					}
				} else if (state == State.InQuotedString) {
					if (_opts.ExcelQuoted && c == '\\') {
						sb.Append('\\');
					}
					sb.Append(c);
				} else if (c == fieldSeparatorChar) {
					string res = sb.ToString();
					sb.Length = 0;
					state = initialState;
					yield return res;
				} else if (eolStyle == EndOfLineStyle.Mixed && (c == '\r' || c == '\n')) {
					if (c == '\r' && PeekChar() == (int)'\n')
						c = (char)ReadChar();
					//end
					string res = sb.ToString();
					sb.Length = 0;
					running = false;
					yield return res;
				} else if (eolStyle == EndOfLineStyle.CrLf && c == '\r' && PeekChar() == '\n') {
					c = (char)ReadChar();
					//end
					string res = sb.ToString();
					sb.Length = 0;
					running = false;
					yield return res;
				} else if (eolStyle == EndOfLineStyle.Lf && c == '\n') {
					string res = sb.ToString();
					sb.Length = 0;
					running = false;
					yield return res;
				} else if (state == State.AfterProcessingText) {
					if (Char.IsWhiteSpace(c) || _opts.InvalidTextAction == InvalidTextMode.Ignore)
						continue;
					var msg = string.Format("Unexpected character: '{0}' on line {1}, column: {2}. Expected: '{3}'", c, _lineNumber, _columnNumber, fieldSeparatorChar);
					// read untill end of line to keep reset the state
					var tmp = ReadChar();
					while (tmp != -1 && tmp != '\n')
						tmp = ReadChar();
					throw new InvalidDataException(msg);
				} else {
					if (removeWhiteSpaceAroundSeparators && Char.IsWhiteSpace(c))
						trailingWhitespace.Append(c);
					else {
						if (state == State.IgnoreWhitespace)
							state = State.ProcessingText1;

						if (trailingWhitespace.Length > 0) {
							sb.Append(trailingWhitespace.ToString());
							trailingWhitespace.Length = 0;
						}
						sb.Append(c);
					}
				}
				lastChar = c;
			} while (running);
		}
	}
}
