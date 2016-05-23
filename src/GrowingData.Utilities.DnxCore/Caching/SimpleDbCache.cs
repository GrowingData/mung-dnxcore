using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Common;

using System.Diagnostics;
using GrowingData.Utilities.Database;

using Newtonsoft.Json;

namespace GrowingData.Utilities {

	public class SimpleDbCache<K, T> where T : class, new() {


		public int Timeout = 100;
		private object _loadSync = new object();

		private bool _loaded = false;
		private bool _loading = false;

		private string _sql;

		private Func<DbConnection> _fnConnection;
		private Func<T, K> _fnKeySelector;


		private List<T> _list = null;
		private Dictionary<K, T> _map = null;

		public bool IsLoaded { get { return _loaded; } }
		public bool IsLoading { get { return _loading; } }

		public SimpleDbCache(Func<DbConnection> fnConnection, Func<T, K> keySelector, string sql) {
			_fnConnection = fnConnection;
			_fnKeySelector = keySelector;
			_sql = sql;
			Load();
		}

		public string Json {
			get {
				return JsonConvert.SerializeObject(_map);
			}
		}

		private void Initialize() {
			Task.Run(() => {
				// Add some jitter to stop things all happening at once.
				//Thread.Sleep((int)(new Random().NextDouble() * 1000));
				Load();
			});
		}

		public List<T> List {
			get {

				if (!_loaded) {
					Load();
					//throw new InvalidOperationException("Cache not loaded, please check .IsLoaded field before accessing .Cached");
				}
				return _list;

			}
		}

		public T this[K key] {
			get {
				T obj;

				if (!_loaded) {
					Load();
				}
				if (_map.TryGetValue(key, out obj)) {
					return obj;
				}
				return null;
			}

		}

		public bool WaitReady() {
			int count = 0;
			while (!_loaded && count < Timeout) {
				Thread.Sleep(1);
				count++;
			}
			return count < Timeout;
		}

		public void Load() {
			try {
				if (_loaded) {
					return;
				}

				lock (_loadSync) {
					// If you are loaded, then dont do it again
					if (_loaded) {
						return;
					}

					_loading = true;

					using (var cn = _fnConnection()) {
						_list = cn.SelectAnonymous<T>(_sql, null);
						_map = _list.ToDictionary(_fnKeySelector);

					}

					_loaded = true;
				}
			} catch (Exception ex) {
				Debug.WriteLine("Unable to load cache for {0}, {1}\r\n{2}", typeof(T), ex.Message, ex.StackTrace);

			} finally {
				_loading = false;
			}
		}
	}
}
