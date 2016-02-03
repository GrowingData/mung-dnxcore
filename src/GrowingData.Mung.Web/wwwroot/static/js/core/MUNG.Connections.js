"use strict";

var MUNG = MUNG || {};

function ConnectionSchema() {
	var self = this;

	var connectionsListed = false;
	var schemasQueried = 0;
	var schemasQuerying = 0;

	var _connections = {};

	var events = new MUNG.EventManager();

	this.load = function (cb) {
		if (checkAllLoaded()) {
			console.log("MUNG.Connections: fire immediate");
			cb(_connections);
		} else {
			events.bind("load", cb);
		}
	}

	function listConnections() {
		connectionsListed = false;
		$.ajax({
			url: "/api/connection/list",
			method: "GET",
			success: function (r) {
				if (r.Success) {
					for (var i = 0; i < r.Connections.length; i++) {
						var name = r.Connections[i].Name;

						_connections[name] = r.Connections[i];
						_connections[name].tables = null;

						listSchema(name);
					}
					connectionsListed = true;
				}
			},
			error: function (r) {
			}
		});
	};

	function listSchema(connectionName) {
		var cn = _connections[connectionName];

		$.ajax({
			url: "/api/connection/" + connectionName + "/schema",
			method: "GET",
			success: function (r) {
				if (r.Success) {
					cn.tables = r.Tables;
				}

				checkFireLoaded();
			},
			error: function (r) {
			}
		});
	}

	function checkFireLoaded() {
		if (checkAllLoaded()) {
			events.fire("load", _connections);
			console.log("MUNG.Connections: fireLoad");
		}
	}

	function checkAllLoaded() {
		if (!connectionsListed) {
			return false;
		}
		for (var k in _connections) {
			if (_connections[k].tables == null) {
				console.log(k + ": missing tables...")
				return false;
			}
			console.log(k + ": has tables!")
		}
		return true;
	}

	listConnections();

	return this;
}

MUNG.Connections = new ConnectionSchema();


