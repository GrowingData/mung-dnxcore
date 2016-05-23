
// Adapted from: https://bitbucket.org/nxt/csv-toolkit

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowingData.Utilities.Csv {
	public enum EndOfLineStyle {
		Lf,
		CrLf,
		Mixed
	}

	public abstract class CsvOptions {
		/// <summary>
		/// Use the first line as column headers (defaults to true)
		/// </summary>
		public bool FirstLineContainsHeaders { get; set; }
		/// <summary>
		/// The character to use as a field separator. Defaults to ;
		/// </summary>
		public char SeparatorChar { get; set; }
		/// <summary>
		/// The character used to quote fields 
		/// </summary>
		public char QuoteChar { get; set; }
		/// <summary>
		/// The end of line styles that are accepted
		/// </summary>
		public EndOfLineStyle EndOfLineStyle { get; set; }

		public CsvOptions() {
			FirstLineContainsHeaders = true;
			QuoteChar = '"';
			SeparatorChar = ';';
			EndOfLineStyle = EndOfLineStyle.Mixed;
		}
	}
}