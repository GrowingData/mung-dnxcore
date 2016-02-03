"use strict";

var MUNG = MUNG || {};

MUNG.Query = new function () {
	var self = this;

	function getRandomString() {
		return Math.random().toString().split('.')[1];
	}

	function iFrameAjax(sql, format) {

		var iframe = $("<iframe>").css("display", "none").appendTo($("body"));
		iframe.ready(function () {
			iframe.contents().find("body").append(
					$("<form>")
						.addClass(callbackPrefix)
						.attr("method", "POST")
						.attr("action", "/api/query")
						.append($("<input>").attr("type", "hidden").attr("name", "sql").val(sql))
						.append($("<input>").attr("type", "hidden").attr("name", "format").val(format))
			);
			iframe.contents().find("." + callbackPrefix).submit();
		});
	}
	
	self.execute = function (sql, parameters, callback, errorCallback) {
		$.ajax({
			url: "/api/query",
			data: { 
				sql: sql, 
				parameters: parameters, 
				format: "json"
			},
			method: "POST",
			success: function (r) {
				if (r.ErrorMessage) {
					if (errorCallback) {
						errorCallback(r);
					}
				} else {
					callback(r);
				}
			},
			error: function (r) {

			}
		});
	}

	self.stream = function (sql, rowCallback, completeCallback, errorCallback) {
		var r = getRandomString();
		var format = "jsonp_row_" + r;
		MUNG.Callbacks.bind(callbackPrefix, rowCallback, completeCallback);
		iFrameAjax(sql, format);
	}

	self.streamRowCount = function (sql, rowCallback, completeCallback, errorCallback) {
		var r = getRandomString();
		var format = "jsonp_count_" + r;
		MUNG.Callbacks.bind(callbackPrefix, rowCallback, completeCallback);
		iFrameAjax(sql, format);
	}

}();

MUNG.Callbacks = new function () {
	var self = this;
	var callbacks = {};

	this.bind = function (r, row, complete) {
		callbacks[r] = {
			row: row,
			complete: complete
		};
	}

	this.get = function (r) {
		return callbacks[r];
	}

}();
