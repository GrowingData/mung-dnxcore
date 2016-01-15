"use strict";

var MUNG = MUNG || {};

MUNG.Graph = {
	startTime: new Date(),
	loadCallbacks: [],
	load: function (initializeGraph) {
		var scriptBlock = document.currentScript;
		var callback = {
			fn: function (scriptBlock) {
				var graph = $(scriptBlock).parents(".graph").first();

				initializeGraph(graph);
			},
			scriptBlock: scriptBlock
		};
		loadCallbacks.push(callback);

	},
	fire: function () {
		var d = new Date();
		var elapsed = d.getTime() - this.startTime.getTime();
		console.log("Executing graph scripts " + elapsed + "ms")

		for (var i = 0; i < this.loadCallbacks.length; i++) {
			var graphCallback = this.loadCallbacks[i];
			graphCallback.fn(graphCallback.scriptBlock);
		}
	}
};
