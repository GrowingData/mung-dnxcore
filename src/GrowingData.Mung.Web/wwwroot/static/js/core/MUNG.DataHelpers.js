"use strict";

var MUNG = MUNG || {};

MUNG.DataHelpers = {

	formatCurrency: function (v) {
		return "$" + d3.format(',.2s')(v).replace("G", "B");
	},

	formatInt: function (v) {
		return d3.format(',.0f')(v)
	},
	formatFloat: function (v) {
		return d3.format(',.2r')(v)
	},
	formatDays: function (v) {
		return parseInt(v) + " days";
	},
	formatPercent: function (v) {
		return d3.format('%')(v);
	},

	format: function (unit) {
		if (unit == "$" || unit == "dollars") return MUNG.DataHelpers.formatCurrency;
		if (unit == "days") return MUNG.DataHelpers.formatDays;
		if (unit == "%" || unit == "percent") return MUNG.DataHelpers.formatPercent;
		if (unit == "i" || unit=="integer") return MUNG.DataHelpers.formatInt;
		if (unit == "f" || unit == "float2dp") return MUNG.DataHelpers.formatFloat;

		return MUNG.DataHelpers.formatInt;
	},

	formatDate_YYYMMDD: function(d){
		return d3.time.format("%Y-%m-%d")(new Date(d));
	},

	formatDateTime: function (d) {
		return d3.time.format("%Y-%m-%d %H:%M:%S")(new Date(d));
	},

	parseDate: function (input) {
		if (input.indexOf('T') > 0) {
			var dateInput = input.split('T')[0];
			var timeInput = input.split('T')[1];

			var dateParts = dateInput.split('-');
			var timeParts = timeInput.split(":");

			// Note: months are 0-based
			return new Date(dateParts[0], dateParts[1] - 1, dateParts[2], timeParts[0], timeParts[1], timeParts[2]); 
		}
		var parts = input.split('-');
		// new Date(year, month [, day [, hours[, minutes[, seconds[, ms]]]]])
		return new Date(parts[0], parts[1] - 1, parts[2]); // Note: months are 0-based
	},

	bindValue: function ($elem, value) {
		var d3unit = $elem.data("format-d3");
		if (d3unit) {
			$elem.text(d3.format(d3unit)(value));
			return
		}

		var unit = $elem.data("format-unit");
		if (unit){
			var formatter = MUNG.DataHelpers.format(unit);
			$elem.text(formatter(value));
			return
		}

		// Default, go for 2 dp.
		$elem.text(MUNG.DataHelpers.formatFloat(value));
	},

	getOrdinal: function (n) {
		var s = ["th", "st", "nd", "rd"],
			v = n % 100;
		return n + (s[(v - 20) % 10] || s[v] || s[0]);
	}

};
