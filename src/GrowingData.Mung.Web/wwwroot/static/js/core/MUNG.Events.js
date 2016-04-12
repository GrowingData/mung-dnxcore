
var MUNG = MUNG || {};

MUNG.Events = function (settings) {
	var self = this;

	this.currentRequest = null;
	this.watchers = [];
	this.allEventTypes = {};
	this.lastSince = settings.since ? settings.since.toISOString() : null;
	this.apiHost = settings.apiHost;
	this.appKey = settings.appKey;
	this.accessToken = settings.accessToken;

	this.hasStarted = false;

	this.registerCallback = function (eventTypes, fn) {
		self.watchers.push({
			eventTypes: eventTypes,
			fn: fn
		});

		for (var i = 0; i < eventTypes.length; i++) {
			self.allEventTypes[eventTypes[i]] = true;
		}
		if (!self.hasStarted) {
			self.hasStarted = true;
			startRequest();
		}
	}


	function startRequest() {
		var eventTypes = [];
		for (var k in self.allEventTypes) {
			eventTypes.push(k);
		}

		var url = "/ext/v1/query/events?eventTypes=" + eventTypes.join(",");
		if (self.lastSince) {
			url += "&since=" + self.lastSince
		}

		if (self.apiHost) {
			url = self.apiHost + url;
		}

		self.currentRequest = $.ajax({
			url: url,
			method: "POST",
			data: {
				appKey: self.appKey,
				accessToken: self.accessToken,
			},
			success: function (r) {
				if (r.Success) {
					for (var i = 0; i < r.Events.length; i++) {
						r.Events[i].RealTime = r.RealTime;
						fireEventCallback(r.Events[i])
						self.lastSince = r.Events[i].LogTime;
					}

				}
				startRequest();
			},
			error: function (r) {
				console.log(r);
				//setTimeout(startRequest, 5000);
				//startRequest();
			}
		});

	}

	function fireEventCallback(event) {
		// Work out which watchers need to be made aware of this
		for (var i = 0; i < self.watchers.length; i++) {
			var w = self.watchers[i];
			if (w.eventTypes.indexOf(event.Type) != -1 || w.eventTypes.indexOf("*") != -1) {
				// This watcher is watching this event type
				w.fn(event);
			}
		}
	}

	function abortRequest() {
		if (self.currentRequest != null) {
			console.log("Aborting request");
			self.currentRequest.abort();
		}
	}

	function init() {
		if (!self.apiToken) {
			// try to a token
			var url = "/ext/v1/auth/access_token";
			if (self.apiHost) {
				url = self.apiHost + url;
			}

			var result = $.ajax({
				url: url,
				async: false,
				success: function (r) {
					self.accessToken = r.accessToken;
				}
			});
		}
	}


	init();
	return self;
}