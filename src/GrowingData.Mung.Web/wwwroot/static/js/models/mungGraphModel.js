﻿
function mungGraphModel(data, dashboard) {
	if (data == null) throw "mungGraphModel: data is null";
	if (dashboard == null) throw "mungGraphModel: dashboard is null";


	var self = this;
	self.data = data;


	this.events = new MUNG.EventManager();


	this.saved = function (fn) {
		self.events.bind("saved", fn);
		return this;
	}

	this.setDimensions = function (x, y, width, height, connectionId) {

		self.data.X = x;
		self.data.Y = y;
		self.data.Width = width;
		self.data.Height = height;
		self.data.ConnectionId = connectionId;

		self.save();
	}

	this.update = function (newViewData) {
		self.data = newViewData;
	}

	this.save = function () {
		$.ajax({
			url: "/api/dashboard/graph",
			data: {
				"url": dashboard.Url,
				"graphJson": JSON.stringify(self.data)
			},
			method: "POST",
			success: function (response) {
				self.events.fire("saved", response, self)
			},
			error: function (a, b, c, d) {
				console.error("Error: mungGraphEditor.save", { a: a, b: b, c: c, d: d });
			}
		});
	}


	return this;
}
