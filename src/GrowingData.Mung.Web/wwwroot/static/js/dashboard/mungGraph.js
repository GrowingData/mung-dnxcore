
(function ($) {
	$.fn.mungGraph = function (graphModel, dashboard) {
		if (graphModel == null) throw "mungGraph: graphModel is null";

		var self = this;

		self.dashboard = dashboard;
		self.graphModel = graphModel;
		self.data("model", graphModel);
		self.data("ref", this);

		self.content = self.find(".graph-content");
		self.contentInner = self.find(".graph-content-inner");
		self.error = self.find(".graph-error");
		self.title = self.find(".graph-title");

		self.edit = self.find(".edit-graph");
		self.status = self.find(".status");

		self.currentData = null;
		self.currentDataBinder = null;

		setStatus("Initializing...");

		function parseFunctionBody(functionString) {
			var firstBrace = functionString.indexOf("{") + 1;
			var lastBrace = functionString.lastIndexOf("}");


			return functionString.substring(firstBrace, lastBrace);

		}

		// Render method for drawing the current graph without doing a data
		// request (for resize events, etc).
		this.render = function () {
			setStatus("Rendering...");
			try {
				self.currentDataBinder(self.currentData, self);
				setStatus("Done");
			} catch (x) {
				self.error.text("Javascript data binding: " + x.toString()).show();
				setStatus("Binding error");
				console.log(x);
				return
			}

		}

		this.refresh = function () {
			setStatus("Refreshing...");
			self.contentInner.html(self.graphModel.data.Html);
			self.title.text(self.graphModel.data.Title);

			self.error.hide();


			// Firstly try to get the binding function
			var fn = null;
			try {
				fn = Function("data", "$component", parseFunctionBody(self.graphModel.data.Js));

			} catch (x) {
				self.error.text("Javascript parse error: " + x.toString()).show();
				setStatus("Javascript function parse error");
				console.log(x);
				return;
			}
			self.currentDataBinder = fn;
			$.ajax({
				url: "/api/sql/mung",
				data: { sql: self.graphModel.data.Sql },
				method: "POST",
				success: function (r) {
					if (r.ErrorMessage) {
						self.error.text("SQL Error: " + r.ErrorMessage).show();
						setStatus("Sql Error");
					} else {

						self.currentData = r;
						self.render();
					}
				},
				error: function (r) {

				}
			});
		}

		function setStatus(text) {
			self.status.text(text);

		}

		function init() {
			self.edit.click(function () {
				self.dashboard.editGraph(self);

			});

			if (graphModel.data.ConnectionId == 5) {
				// Realtime, so register this with the dashboardPoller
				var fn = null;
				try {
					fn = Function("data", "$component", parseFunctionBody(self.graphModel.data.Js));

				} catch (x) {
					self.error.text("Javascript parse error: " + x.toString()).show();
					setStatus("Javascript function parse error");
					console.log(x);
					return;
				}


				setStatus("Initializing...");
				self.contentInner.html(self.graphModel.data.Html);
				self.title.text(self.graphModel.data.Title);


				var graphSettings = self.find(".realtime-graph");
				if (graphSettings.length==0){
					setStatus("Realtime graphs require an element with class 'realtime-graph' and an attribute 'data-subscription-events' ");
					
				}
				var events = graphSettings.data("subscription-events").split(',');

				self.dashboard.poller.registerCallback(events, function (data) {
					fn(data, self);
				});

				setStatus("Ready.");
				return;
			}

			self.refresh();
		}

		init();

		return this;
	}

}(jQuery));
