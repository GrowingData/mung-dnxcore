

define(function (require, exports, module) {
	"use strict";

	// Load dependent modules
	var AppInit = brackets.getModule("utils/AppInit"),
        CodeHintManager = brackets.getModule("editor/CodeHintManager"),
		CodeMirror = brackets.getModule("thirdparty/CodeMirror2/lib/codemirror"),
        TokenUtils = brackets.getModule("utils/TokenUtils");

	var schema = null;

	var tablePrecedingKeywords = {
		"from": true,
		"join": true,
		"update": true,
		"into": true,

	}

	var columnPrecedingKeywords = {
		"select": true,
		"where": true,
		"by": true,
		"on": true,
		"between": true,
		"and": true,
		"or": true,
		"=": true,
		">": true,
		"<": true,
		"+": true,
		"-": true,
		"/": true,
		"*": true,
		">=": true,
		"=<": true,
		"(": true,
	}
	MUNG.Connections.load(function (connections) {
		schema = connections;
	});

	var tableAliases = {};
	var visibleTables = {};

	function buildAlias(tableName) {
		// Try the first letter...
		var firstLetter = tableName[0].toUpperCase();
		if (!(firstLetter in tableAliases)) {
			return firstLetter;
		}
		var uscore = null;
		var camel = null;

		var uppers = [];
		var underscores = [];
		var underScore = false, hasLower = false;
		var currentCode = null;
		for (var i = 0; i < tableName.length; i++) {
			currentCode = tableName.charCodeAt(i);
			if (currentCode >= 65 && currentCode <= 90) {
				uppers.push(tableName[i])
			}
			if (currentCode >= 97 && currentCode <= 122) {
				hasLower = true;
			}
			if (underScore) {
				underscores.push(tableName[i].toUpperCase());
				underScore = false;
			}
			if (tableName[i] == 'self') {
				underScore = true;
			}
		}
		// "gooroo_skill" => "GS"
		if (underscores.length > 0) {
			uscore = tableName[0].toUpperCase() + underscores.join("")
			if (!(uscore in tableAliases)) {
				return uscore;
			}
		}

		// "GoorooSkill" => "GS"
		if (uppers.length > 0 && hasLower) {
			camel = uppers.join("").toUpperCase();
			if (!(camel in tableAliases)) {
				return camel;
			}
		}

		// Just keep on adding numbers until we end up with something
		// assuming we have less than 1000 tables.
		for (var i = 2; i < 1000; i++) {
			if (!((firstLetter + i) in tableAliases)) {
				return firstLetter + i;
			}
			if (camel != null && !((camel + i) in tableAliases)) {
				return camel + i;
			}
			if (uscore != null && !((uscore + i) in tableAliases)) {
				return uscore + i;
			}
		}
	};


	function TableHints() {
		this.exclusion = null;

		this.updateExclusion = function () {
		};

		this.hasHints = function (editor, implicitChar) {
			this.editor = editor;
			var pos = editor.getCursorPos();

			// Is the previous token something that preceeds a table name reference?
			var ctx = TokenUtils.getInitialContext(editor._codeMirror, pos);

			var lowerToken = "";
			while (true) {
				var prev = TokenUtils.movePrevToken(ctx);
				if (!prev) {
					return false;
				}
				var tokenStr = ctx.token.string;
				console.log("TableHints.hasHints: prevToken = " + tokenStr);
				if (tokenStr) {
					lowerToken = tokenStr.toLowerCase().trim();
					if (lowerToken.length == 0) {
						continue;
					}
					if (tablePrecedingKeywords[lowerToken]) {
						return true;
					} else {
						return false;
					}
				}
			}
		};

		this.getHints = function (implicitChar) {
			var query, result;

			var pos = this.editor.getCursorPos();
			var ctx = TokenUtils.getInitialContext(this.editor._codeMirror, pos);
			//var prev = TokenUtils.movePrevToken(ctx);
			if (ctx.token.string == null || ctx.token.string.length == 0) {
				return null;
			}
			visibleTables = {};

			console.log("TableHints.getHints: query == " + ctx.token.string);
			var lowerToken = ctx.token.string.toLowerCase();

			var matchingTables = [];
			for (var k in schema) {
				var cn = schema[k];
				for (var i in cn.tables) {
					var tbl = cn.tables[i];
					if (tbl.TableName.toLowerCase().indexOf(lowerToken) >= 0
						|| tbl.SchemaName.toLowerCase().indexOf(lowerToken) >= 0) {

						var qualifiedName = "\"" + tbl.SchemaName + "\"" + "." + "\"" + tbl.TableName + "\""
						var alias = buildAlias(tbl.TableName);

						var completion = qualifiedName + " " + alias;

						matchingTables.push(completion);

						visibleTables[completion] = { table: tbl, alias: alias, qualified: qualifiedName };
					}
				}
			}
			if (matchingTables.length == 0) {
				return null;
			}

			return {
				hints: matchingTables,
				match: query,
				selectInitial: true,
				handleWideResults: false
			};
		};

		this.insertHint = function (completion) {
			console.log("TableHints.insertHint: " + completion);

			var tbl = visibleTables[completion];

			var pos = this.editor.getCursorPos();
			var ctx = TokenUtils.getInitialContext(this.editor._codeMirror, pos);
			//var prev = TokenUtils.movePrevToken(ctx);
			if (ctx.token.string == null || ctx.token.string.length == 0) {
				return false;
			}
			this.editor.document.replaceRange(completion,
				{ ch: ctx.token.start, line: pos.line },
				{ ch: ctx.token.end, line: pos.line }
			);

			tableAliases[tbl.alias] = tbl;

			return true;
		};
	}

	function ColumnHints() {
		this.cachedHints = null;
		this.exclusion = "";

		this.updateExclusion = function () {
		};

		this.hasHints = function (editor, implicitChar) {
			this.editor = editor;
			var pos = editor.getCursorPos();

			// Is the previous token something that preceeds a table name reference?
			var ctx = TokenUtils.getInitialContext(editor._codeMirror, pos);

			var lowerToken = "";
			while (true) {
				var prev = TokenUtils.movePrevToken(ctx);
				if (!prev) {
					return false;
				}
				var tokenStr = ctx.token.string;
				console.log("ColumnHints.hasHints: prevToken = " + tokenStr);
				if (tokenStr == ",") {
					return true;
				}
				if (tokenStr) {
					lowerToken = tokenStr.toLowerCase().trim();
					if (lowerToken.length == 0) {
						continue;
					}
					if (columnPrecedingKeywords[lowerToken]) {
						console.log("hasHints")
						return true;
					} else {
						console.log("noHints")
						return false;
					}
				}
			}
		};

		this.getHints = function (implicitChar) {
			var query, result;

			var pos = this.editor.getCursorPos();
			var ctx = TokenUtils.getInitialContext(this.editor._codeMirror, pos);
			console.log("ColumnHints.getHints: query == " + ctx.token.string);
			//var prev = TokenUtils.movePrevToken(ctx);
			if (ctx.token.string == null || ctx.token.string.length == 0) {
				return null;
			}
			visibleTables = {};

			var lowerToken = ctx.token.string.toLowerCase();

			var matchingColumns = [];
			for (var k in tableAliases) {
				var ta = tableAliases[k];
				var tbl = ta.table;
				// Looking through all the table alias'
				for (var i in tbl.Columns) {
					var col = tbl.Columns[i];

					if (col.ColumnName.toLowerCase().indexOf(lowerToken)) {
						var alias = ta.alias;

						matchingColumns.push(alias + "." + col.ColumnName);
					}
				}
			}
			if (matchingColumns.length == 0) {
				return null;
			}

			return {
				hints: matchingColumns,
				match: query,
				selectInitial: true,
				handleWideResults: false
			};
		};

		this.insertHint = function (completion) {
			console.log("ColumnHints.insertHint: " + completion);

			var pos = this.editor.getCursorPos();
			var ctx = TokenUtils.getInitialContext(this.editor._codeMirror, pos);
			//var prev = TokenUtils.movePrevToken(ctx);
			if (ctx.token.string == null || ctx.token.string.length == 0) {
				return false;
			}
			this.editor.document.replaceRange(completion,
				{ ch: ctx.token.start, line: pos.line },
				{ ch: ctx.token.end, line: pos.line }
			);


			return true;
		};
	}


	AppInit.appReady(function () {
		// Parse JSON files
		//tags = JSON.parse(HTMLTags);
		//attributes = JSON.parse(HTMLAttributes);

		// Register code hint providers
		var tableHints = new TableHints();
		var columnHints = new ColumnHints();

		CodeHintManager.registerHintProvider(tableHints, ["sql"], 0);
		CodeHintManager.registerHintProvider(columnHints, ["sql"], 0);

		// For unit testing
		//exports.tagHintProvider = tagHints;
		//exports.attrHintProvider = attrHints;
	});
});
