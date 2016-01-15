"use strict";

var MUNG = MUNG || {};

MUNG.Sql = {

	execute: function (sql, connection, callback, errorCallback) {

		$.ajax({
			url: "/api/sql/mung",
			data: { sql: sql, connection: connection },
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
};
