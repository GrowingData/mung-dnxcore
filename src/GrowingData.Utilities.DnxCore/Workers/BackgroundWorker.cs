using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace GrowingData.Utilities.DnxCore {

	static class TaskExtensions {
		public static void Forget(this Task task) {
		}
	}

	public static class BackgroundWorker {
		public static CancellationTokenSource Start(TimeSpan interval, Action action) {
			var token = new CancellationTokenSource();

			WorkAsync(action, new TimeSpan(0, 0, 0), interval, token.Token).Forget();

			return token;
		}

		public static CancellationTokenSource Start(TimeSpan dueTime, TimeSpan interval, Action action) {
			var token = new CancellationTokenSource();

			WorkAsync(action, dueTime, interval, token.Token).Forget();

			return token;
		}

		public static async Task WorkAsync(Action action, TimeSpan dueTime, TimeSpan interval, CancellationToken token) {
			// Initial wait time before we begin the periodic loop.
			if (dueTime > TimeSpan.Zero)
				await Task.Delay(dueTime, token);

			// Repeat this loop until cancelled.
			while (!token.IsCancellationRequested) {
				action();

				// Wait to repeat again.
				if (interval > TimeSpan.Zero)
					await Task.Delay(interval, token);
			}
		}

	}
}
