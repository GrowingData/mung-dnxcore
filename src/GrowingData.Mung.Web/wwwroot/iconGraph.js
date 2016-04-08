
(function ($) {
	$.fn.iconGraph = function (connection) {

		var self = this;

		var now = new Date();
		var dateStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
		var dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);

		function getChartData(steps) {
			var points = [];
			for (var i = 0; i < steps; i++) {
				points.push(0);
			}
			return points;
		}

		function getTimeBucket(points, logTimeStr) {
			var steps = points.length;
			var logTime = logTimeStr instanceof Date ? logTimeStr : Date.parse(logTimeStr)

			var totalMs = dateEnd - dateStart;
			var msPerStep = totalMs / steps;
			var timeMs = logTime - dateStart;

			var timeBucket = Math.floor(timeMs / msPerStep);
			return timeBucket;
		}
		function addChartPoint(points, logTimeStr) {
			var steps = points.length;
			var logTime = Date.parse(logTimeStr);

			var totalMs = dateEnd - dateStart;
			var msPerStep = totalMs / steps;
			var timeMs = logTime - dateStart;

			var timeBucket = getTimeBucket(points, logTimeStr);

			points[timeBucket] += 1;
		}

		function renderChart(l, points) {
			var chartHolder = l.find(".chart-background");
			var height = chartHolder.height() - 2;

			var max = 1, min = 0;
			for (var i = 0; i < points.length; i++) {
				if (points[i] > max) max = points[i];
			}

			var bars = chartHolder.children();
			var color = l.parents(".map-grouping").first().data("color").toString();
			for (var i = 0; i < points.length; i++) {
				if (bars.length < i) {
					var bar = $("<i>");
					bindValueToBar(bar, height, (points[i] / max));
					chartHolder.append(bar);
				} else {
					bindValueToBar($(bars[i]), height, (points[i] / max));
				}
			}
			// Mark the current time bar as active
			var bars = chartHolder.children();
			var bucket = getTimeBucket(points, new Date());

			$(bars[bucket]).addClass("active");

			chartHolder.find("i").css({ "background": convertHex(color, 0.6) });
			chartHolder.find("i.active").css({ "background": convertHex(color, 1) });

		}
		function bindValueToBar(bar, height, pc) {
			bar.css({
				"height": 1 + (pc * height) + "px",
				"margin-top": (1 - pc) * height + "px"
			});
			return bar;
		}


		function init() {

			self.data("chart", getChartData(30));

			var eventTypes = self.data("event-types").split(',');

			self.append(
				$("<div>").addClass("counter").data("count", 0).text(0)
			);
			self.append(
				$("<div>").addClass("chart-background")
			);

			if (eventTypes) {
				connection.registerCallback(eventTypes, function (evt) {
					var filterFns = self.data("filter");
					if (filterFns != null) {
						for (var k in filterFns) {
							if (!evt.Data[k]) {
								//console.log("Filter failed: '" + k + "'is unknown in " + JSON.stringify(evt.Data));
								return;
							}


							if (!filterFns[k](evt.Data[k])) {
								//console.log("Filter failed", k, evt.Data[k], filterFns[k]);
								return;
							}
						}
					}
					var counter = self.find(".counter");
					var count = parseInt(counter.data("count")) + 1;
					counter.data("count", count);
					counter.text(count);

					var points = self.data("chart");
					addChartPoint(points, evt.LogTime);

					renderChart(self, points);

					if (evt.RealTime) {
						self.addClass("flash");
						setTimeout(function () { self.removeClass("flash"); }, 200);
					}

				});
			}

		}

		init();

	}

}(jQuery));

