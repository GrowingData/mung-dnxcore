using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowingData.Utilities {
	public static class SmithWaterman {

		public static int DistanceSmithWaterman(this string me, string other) {
			var sw = new SmithWatermanAlignment(me, other);
			return sw.ComputeSmithWaterman();
		}
		public static SmithWatermanAlignment DistanceSmithWatermanAlignment(this string me, string other) {
			var sw = new SmithWatermanAlignment(me, other);
			return sw;
		}
	}

	public class SmithWatermanAlignment {
		private string one, two;
		private int[,] matrix;
		private int gap;
		private int match;
		private int o;
		private int l;
		private int e;

		public SmithWatermanAlignment(string one, string two) {
			this.one = one.ToLower();
			this.two = two.ToLower();

			// Define affine gap starting values
			this.match = 2;
			o = -2;
			l = 0;
			e = -1;

			// initialize matrix to 0
			matrix = new int[one.Length + 1, two.Length + 1];
			for (int i = 0; i < one.Length; i++)
				for (int j = 0; j < two.Length; j++)
					matrix[i, j] = 0;

		}

		// returns the alignment score
		public int ComputeSmithWaterman() {
			for (int i = 0; i < one.Length; i++) {
				for (int j = 0; j < two.Length; j++) {
					gap = o + (l - 1) * e;
					if (i != 0 && j != 0) {
						if (one[i] == two[j]) {
							// match
							// reset l
							l = 0;
							matrix[i, j] = Math.Max(0, Math.Max(
											matrix[i - 1, j - 1] + match, Math.Max(
															matrix[i - 1, j] + gap,
															matrix[i, j - 1] + gap)));
						} else {
							// gap
							l++;
							matrix[i, j] = Math.Max(0, Math.Max(
											matrix[i - 1, j - 1] + gap, Math.Max(
															matrix[i - 1, j] + gap,
															matrix[i, j - 1] + gap)));

						}
					}
				}
			}

			// find the highest value
			int longest = 0;
			int iL = 0, jL = 0;
			for (int i = 0; i < one.Length; i++) {
				for (int j = 0; j < two.Length; j++) {
					if (matrix[i, j] > longest) {
						longest = matrix[i, j];
						iL = i;
						jL = j;
					}
				}
			}

			// Backtrack to reconstruct the path
			int ii = iL;
			int jj = jL;
			Stack<String> actions = new Stack<String>();

			while (ii != 0 && jj != 0) {
				// diag case
				if (Math.Max(matrix[ii - 1, jj - 1],
								Math.Max(matrix[ii - 1, jj], matrix[ii, jj - 1])) == matrix[ii - 1, jj - 1]) {
					actions.Push("align");
					//Console.WriteLine("a");
					ii = ii - 1;
					jj = jj - 1;
					// left case
				} else if (Math.Max(matrix[ii - 1, jj - 1],
								Math.Max(matrix[ii - 1, jj], matrix[ii, jj - 1])) == matrix[ii, jj - 1]) {
					actions.Push("insert");
					//Console.WriteLine("i");
					jj = jj - 1;
					// up case
				} else {
					actions.Push("delete");
					//Console.WriteLine("d");
					ii = ii - 1;
				}
			}

			string alignOne = "";
			string alignTwo = "";
			string[] tmp = new string[actions.Count];
			actions.CopyTo(tmp, 0);

			Stack<string> backActions = new Stack<string>(tmp);
			for (int z = 0; z < one.Length; z++) {
				alignOne = alignOne + one[z];
				if (actions.Count > 0) {
					String curAction = actions.Pop();
					// Console.WriteLine(curAction);
					if (curAction.Equals("insert")) {
						alignOne = alignOne + "-";
						while (actions.Peek().Equals("insert")) {
							alignOne = alignOne + "-";
							actions.Pop();
						}
					}
				}
			}

			for (int z = 0; z < two.Length; z++) {
				alignTwo = alignTwo + two[z];
				if (backActions.Count > 0) {
					String curAction = backActions.Pop();
					if (curAction.Equals("delete")) {
						alignTwo = alignTwo + "-";
						while (backActions.Peek().Equals("delete")) {
							alignTwo = alignTwo + "-";
							backActions.Pop();
						}
					}
				}
			}

			// print alignment
			//Console.WriteLine(alignOne + "\n" + alignTwo);
			return longest;
		}

		public void printMatrix() {
			for (int i = 0; i < one.Length; i++) {
				if (i == 0) {
					for (int z = 0; z < two.Length; z++) {
						if (z == 0)
							Console.Write("   ");
						Console.Write(two[z] + "  ");

						if (z == two.Length - 1)
							Console.WriteLine();
					}
				}

				for (int j = 0; j < two.Length; j++) {
					if (j == 0) {
						Console.Write(one[i] + "  ");
					}
					Console.Write(matrix[i, j] + "  ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		//public static void main(String[] args) {
		//	// DNA sequence Test:
		//	SmithWaterman sw = new SmithWaterman("ACACACTA", "AGCACACA");
		//	Console.WriteLine("Alignment Score: " + sw.ComputeSmithWaterman());
		//	sw.printMatrix();

		//	sw = new SmithWaterman("gcgcgtgc", "gcaagtgca");
		//	Console.WriteLine("Alignment Score: " + sw.ComputeSmithWaterman());
		//	sw.printMatrix();
		//}
	}

}
