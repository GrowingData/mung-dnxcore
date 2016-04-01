
var MUNG = MUNG || {};

MUNG.Events = function (settings) {
	var self = this;

	this.currentRequest = null;
	this.watchers = [];
	this.allEventTypes = {};
	this.lastSince = settings.since ? settings.since.toISOString() : null;
	this.apiHost = settings.host;

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
		var eventTypes = []
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
			method: "GET",
			success: function (r) {
				if (r.Success) {
					for (var i = 0; i < r.Events.length; i++) {
						r.Events[i].RealTime = r.RealTime;
						console.log("Events:", r.Events[i]);
						fireEventCallback(r.Events[i])

						self.lastSince = r.Events[i].LogTime;
					}
					
				}

				startRequest();
			},
			error: function (r) {

				startRequest();
			}
		});

	}

	function fireEventCallback(event){
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


	return self;
}