
// Adapted from: https://bitbucket.org/nxt/csv-toolkit

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Utilities.Csv {
	public enum InvalidTextMode {
		/// <summary>
		/// Ignore extra characters that appear after a quoted column (e.g. "data" 1;"")
		/// </summary>
		Ignore,
		/// <summary>
		/// Throw an exception when extra characters appear after a quoted column
		/// </summary>
		Throw
	}

	public class CsvReaderOptions : CsvOptions {

		/// <summary>
		/// Disable the support for quoted fields. 
		/// </summary>
		public bool DisableQuotedFields { get; set; }
		/// <summary>
		/// Keep the whitespace directly preceding or following a field separator
		/// </summary>
		public bool KeepWhiteSpaceAroundSeparators { get; set; }

		/// <summary>
		/// What should we do when invalid text is encountered
		/// </summary>
		public InvalidTextMode InvalidTextAction { get; set; }


		/// <summary>
		/// Use excels method of double quoting quotes to escape them,
		/// and treat "\" as a character to be escaped. 
		/// </summary>
		public bool ExcelQuoted { get; set; }

		public CsvReaderOptions() {
			DisableQuotedFields = false;
			InvalidTextAction = InvalidTextMode.Throw;
			ExcelQuoted = true;
		}

	}
}
