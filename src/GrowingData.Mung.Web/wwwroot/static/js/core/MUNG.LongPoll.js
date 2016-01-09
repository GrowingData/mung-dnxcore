
var MUNG = MUNG || {};

MUNG.LongPoll = function (settings) {
	var self = this;

	this.currentRequest = null;
	this.watchers = [];
	this.allEventTypes = {};

	this.registerCallback = function (eventTypes, fn) {
		abortRequest();

		self.watchers.push({
			eventTypes: eventTypes,
			fn: fn
		});

		for (var i = 0; i < eventTypes.length; i++) {
			self.allEventTypes[eventTypes[i]] = true;
		}

		startRequest();
	}


	function startRequest() {
		var eventTypes = []
		for (var k in self.allEventTypes) {
			eventTypes.push(k);
		}

		self.currentRequest = $.ajax({
			url: "/api/firehose/poll?eventTypes=" + eventTypes.join(","),
			method: "GET",
			success: function (r) {
				if (r.Success) {
					// Work out which watchers need to be made aware of this
					for (var i = 0; i < self.watchers.length; i++) {
						var w = self.watchers[i];
						if (w.eventTypes.indexOf(r.Event.Type) != -1 || w.eventTypes.indexOf("*") != -1) {
							// This watcher is watching this event type
							w.fn(r.Event);
						}
					}
				}

				startRequest();
			},
			error: function (r) {

				startRequest();
			}
		});

	}

	function abortRequest() {
		if (self.currentRequest != null) {
			console.log("Aborting request");
			self.currentRequest.abort();
		}
	}


	return self;
}