
(function ($) {
	$.fn.mungGraph = function (dashboard) {
		if (dashboard == null) throw "mungGraph: dashboard is null";

		var self = this;

		self.dashboard = dashboard;
		self.data("ref", this);
		self.model = self.data("model");

		self.content = self.find(".graph-content");
		self.contentInner = self.find(".graph-content-inner");
		self.error = self.find(".graph-error");
		self.title = self.find(".graph-title");

		self.edit = self.find(".edit-graph");
		self.status = self.find(".status");

		this.saveLayout = function (x, y, width, height) {
			self.model.X = x;
			self.model.Y = y;
			self.model.Width = width;
			self.model.Height = height;
			console.log("Saving layout for " + self.model.Title + ": " + [x, y, width, height].join(","));
			self.setStatus("Saving layout...");
			$.ajax({
				url: "/api/file-system/graph/size",
				data: {
					"path": "/Dashboards" + dashboard.model.Url + "/" + self.model.Name,
					"x": x,
					"y": y,
					"width": width,
					"height": height,
				},
				method: "POST",
				success: function (response) {
					//self.events.fire("saved", response, self)
					self.setStatus("Saved layout");
				},
				error: function (a, b, c, d) {
					console.error("Error: mungGraph.saveLayout", { a: a, b: b, c: c, d: d });
				}
			});
		}
		this.setStatus = function (text) {
			self.status.text(text);
		}



		function init() {
			self.setStatus("Initializing...");
			self.edit.click(function () {
				var url = dashboard.model.Url;
				document.location = "/mung/?open=/Dashboards" + url + "/" + self.model.Name + ".html";

			});
			//self.dashboard.grid.add_widget(self, self.model.X, self.model.Y, self.model.Width, self.model.Height);
			self.setStatus("Ready.");
		}

		init();

		return this;
	}

}(jQuery));
