using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowingData.Utilities {
	public class NeedlemanWunschAlignment {
		public int Score { get; set; }
		public string Path { get; set; }
		public string One { get; set; }
		public string Two { get; set; }

		public string A { get; set; }
		public string B { get; set; }
		public new string ToString() {
			var s = string.Format("Score: {0}\r\nA: {1}\r\nB: {2}", Score, One, Two);
			return s;
		}

	}


	public static class NeedlemanWunsch {
		// A rad algorithm for calculating alignments, which 
		// enables the comparison of similarity of strings by looking
		// at the distance between them in terms of edits.


		const string DONE = @"¤";
		const string DIAG = @"\";
		const string UP = @"|";
		const string LEFT = @"-";

		public static int DistanceNeedlemanWunsch(this string me, string other) {
			return DistanceNeedlemanWunschAlignment(me, other).Score;
		}

		public static NeedlemanWunschAlignment DistanceNeedlemanWunschAlignment(this string xs, string ys) {

			int GAP_PENALTY = -1;
			int MISMATCH_PENALTY = -2;
			int MATCH = 0;
			const string GAP = @"-";
			//int m = -2;

			int xLength = xs.Length;
			int yLength = ys.Length;

			var dpTable = new int[xLength + 1, yLength + 1];		// dynamic programming buttom up memory table
			var traceTable = new string[xLength + 1, yLength + 1];	// trace back path

			// Initialize arrays
			for (int i = 0; i < xLength + 1; i++) {
				dpTable[i, 0] = i * GAP_PENALTY;
			}

			for (int j = 0; j < yLength + 1; j++) {
				dpTable[0, j] = j * GAP_PENALTY;
			}
			traceTable[0, 0] = DONE;

			for (int i = 1; i < xLength + 1; i++) {
				traceTable[i, 0] = UP;
			}

			for (int j = 1; j < yLength + 1; j++) {
				traceTable[0, j] = LEFT;
			}
			// calc
			for (int i = 1; i < xLength + 1; i++) {
				for (int j = 1; j < yLength + 1; j++) {
					var alpha = MISMATCH_PENALTY;
					if (xs[i - 1] == ys[j - 1]) {
						alpha = MATCH;
					}

					//var alpha = Alpha(xs.ElementAt(i - 1).ToString(), ys.ElementAt(j - 1).ToString());
					var diag = alpha + dpTable[i - 1, j - 1];
					var up = GAP_PENALTY + dpTable[i - 1, j];
					var left = GAP_PENALTY + dpTable[i, j - 1];
					var max = Max(diag, up, left);
					dpTable[i, j] = max;

					if (max == diag)
						traceTable[i, j] = DIAG;
					else if (max == up)
						traceTable[i, j] = UP;
					else
						traceTable[i, j] = LEFT;
				}
			}

			var traceBack = ParseTraceBack(traceTable, xLength + 1, yLength + 1);

			var sb = new StringBuilder();
			string first, second;

			if (xs.Length != ys.Length) {
				string s;
				if (xs.Length > ys.Length) {
					s = ys;
					first = xs;
				} else {
					s = xs;
					first = ys;
				}


				int i = 0;
				foreach (var trace in traceBack) {
					if (trace.ToString() == DIAG)
						sb.Append(s.ElementAt(i++).ToString());
					else
						sb.Append(GAP);
				}

				second = sb.ToString();
			} else {
				first = xs;
				second = ys;
			}
			var sequence = new NeedlemanWunschAlignment() { Score = dpTable[xLength, yLength], Path = traceBack, One = first, Two = second, A = xs, B = ys };
			return sequence;
		}

		static string ParseTraceBack(string[,] T, int I, int J) {
			var sb = new StringBuilder();
			int i = I - 1;
			int j = J - 1;
			string path = T[i, j];
			while (path != DONE) {
				sb.Append(path);
				if (path == DIAG) {
					i--;
					j--;
				} else if (path == UP)
					i--;
				else if (path == LEFT)
					j--;

				path = T[i, j];
			}
			//return sb.ToString().Reverse().ToString();
			return ReverseString(sb.ToString());
		}

		static string ReverseString(string s) {
			char[] arr = s.ToCharArray();
			Array.Reverse(arr);
			return new string(arr);
		}
		static int Max(int a, int b, int c) {
			if (a >= b && a >= c)
				return a;
			if (b >= a && b >= c)
				return b;
			return c;
		}

	}
}
