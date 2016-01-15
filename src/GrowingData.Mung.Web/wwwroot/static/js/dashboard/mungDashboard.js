
(function ($) {
	// «
	$.fn.mungDashboard = function (model) {
		if (model == null) throw "mungDashboard: model is null";

		var self = this;

		self.model = model;
		self.poller = MUNG.LongPoll();
		self.graphMap = {};

		//self.componentPopup = $("#edit-graph").mungGraphEditor(self);

		self.addComponentButton = $("#add-graph").click(function () {
			// Create a new graph, then send the user to the editor for
			// the Graph.
			var r = Math.random().toString().split(".")[1].substr(0, 6);
			var graphUrl = model.ResourceUrl + "/new-graph-" + r + ".html";

			$.ajax({
				url: "/api/file-system" + graphUrl,
				data: {
					"data": "<h1>A new graph</h1>"
				},
				method: "PUT",
				success: function (response) {
					if (response.Success) {
						document.location = "/mung?open="+response.ResourceUrl;
					}
				}
			});

		});

		this.editGraph = function (graph) {
			self.componentPopup.edit(graph.graphModel);
		}

		function bindGridStack() {
			self.graphHolder = self.find(".graph-holder")
				.gridstack({
					width: 12,
					always_show_resize_handle: /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent),
					resizable: {
						handles: 'e, se, s, sw, w'
					}
				});

			self.grid = self.graphHolder.data("gridstack");

			self.graphHolder.on('change', function (e, items) {
				for (var i = 0; i < items.length; i++) {
					var item = items[i];
					if (item._dirty) {
						var model = item.el.data("model");
						var ref = item.el.data("ref");

						ref.saveLayout(item.x, item.y, item.width, item.height);
						item._dirty = false;

						// Changing the size of them element might necessitate
						// a re-render (especially for d3 stuff)
						if (ref.render) {
							ref.render();
						}
					}
				}
			});

		}

		function bindGraphs() {
			self.graphMap = {};
			$(".graph").each(function () {
				var graphElem = $(this).mungGraph(self);
				self.graphMap[graphElem.model.GraphId] = graphElem;


				// Now bind the initilization script call to the element and dashboard
				for (var i = 0; i < MUNG.Loader.graphReadyCallbacks.length; i++) {
					var graphScriptReference = MUNG.Loader.graphReadyCallbacks[i];
					var scriptGraphId = $(graphScriptReference.elem).parents(".graph").data("graph-id");

					if (scriptGraphId == graphElem.model.GraphId) {
						graphScriptReference.callback(graphElem, self);
					}
				}
			})
		}




		function init() {
			bindGridStack();
			bindGraphs();
		}


		init();

		return this;
	}

}(jQuery));
