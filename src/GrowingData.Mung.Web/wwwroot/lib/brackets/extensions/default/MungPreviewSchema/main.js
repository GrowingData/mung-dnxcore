

/*jslint vars: true, plusplus: true, devel: true, nomen: true, indent: 4, maxerr: 50 */
/*global define, brackets, window */

define(function (require, exports, module) {
	"use strict";

	var editor = null,
		inputField = null,
		inProgress = false;


	var EditorManager = brackets.getModule("editor/EditorManager");
	var WorkspaceManager = brackets.getModule("view/WorkspaceManager");
	var AppInit = brackets.getModule("utils/AppInit");
	var Resizer = brackets.getModule("utils/Resizer");
	var DocumentManager = brackets.getModule("document/DocumentManager");

	var connections = [];


	var $schemaPanel = $("<div>")
			.css({
				position: "absolute",
				width: "250px",
				top: 10 + $("#titlebar").height() + "px",
				bottom: "0",
				"overflow": "auto",
				"overflow-x": "auto",
				"overflow-y": "auto",
				"border-left": "solid 2px #2d2e30",
			})
			.attr("id", "sql-schema-panel")
			.addClass("outline-main quiet-scrollbars jstree panel")
			.appendTo($(".main-view"));

	var $schemaListHolder = $("<div>")
		.addClass("jstree-brackets")
		.append(
			$("<div>")
				.addClass("working-set-header")
				.text("Connections")
		)
		.appendTo($schemaPanel)

	var $schemaRoot = $("<ul>")
		.addClass("jstree-no-dots jstree-no-icons")
		.appendTo($schemaListHolder)

	function updateFocusedEditor(focusedEditor) {
		if (editor) {
			editor.off("change", onChangeHandler);
		}

		if (focusedEditor) {

			editor = focusedEditor;

			if (editor.getFile().name.endsWith(".sql")) {
				editor.on("change", onChangeHandler);
				updatePreview(editor);
				$schemaPanel.show();
			} else {
				$schemaPanel.hide();
			}
		}
	};



	function onChangeHandler(event) {
		updatePreview(event.target);
		var code = event.target.document.getText();
	};

	function updatePreview(ed) {
		var code = ed.document.getText();
		//$queryResultsHolder.empty();
	}

	function updateConnections() {
		$schemaRoot.empty();

		MUNG.Connections.load(function (connections) {
			console.log("Got connections!")
			for (var k in connections) {
				var connection = connections[k];
				var connectionRoot = $("<li>")
					.addClass("connection jstree-closed")
					.append($("<ins>").addClass("jstree-icon"))
					.append(
						$("<a>")
							.append($("<ins>").addClass("jstree-icon"))
							.append($("<a>").text(connection.Name))
							.click(function () { toggleOpen(this); })
					)
					.appendTo($schemaRoot);

				var tablesUl = $("<ul>")
					.addClass("tables jstree-brackets jstree-no-dots jstree-no-icons")
					.appendTo(connectionRoot);

				var tables = connection.tables;

				for (var i = 0; i < tables.length; i++) {
					bindTable(tables[i], tablesUl)
				}
			}
		});
	}



	function bindTable(table, tablesUl) {
		// Bind the table...
		var tableRoot = $("<li>")
			.addClass("table jstree-closed")
			.append($("<ins>").addClass("jstree-icon"))
			.append(
				$("<a>")
					.append($("<ins>").addClass("jstree-icon"))
					.append($("<a>").text(table.SchemaName + "." + table.TableName))
					.click(function () { toggleOpen(this); })
			)
			.appendTo(tablesUl);

		var columnsUl = $("<ul>")
			.addClass("columns jstree-brackets jstree-no-dots jstree-no-icons")
			.appendTo(tableRoot);

		for (var i = 0; i < table.Columns.length; i++) {
			var column = table.Columns[i];
			$("<li>")
			.addClass("column")
			.append($("<span>").text(column.ColumnName))
			.appendTo(columnsUl);
		}

	}

	function toggleOpen(a) {
		var li = $(a).parents("li").first();
		if (li.is(".jstree-closed")) {
			li.removeClass("jstree-closed").addClass("jstree-open");
		} else {
			li.removeClass("jstree-open").addClass("jstree-closed");
		}
	}

	function updateContentHeight() {
		var innerHeight = $queryPanel.height();
		var maxHeight = $(window).height() * 0.3;

		if (innerHeight < maxHeight) {
			$queryPanel.css("height", innerHeight + "px");
		} else {
			$queryPanel.css("height", maxHeight + "px");
		}
		$schemaPanel.css("bottom", $queryPanel.height() + 28 + "px");
		Resizer.show($queryPanel);

	}

	function initializePreviewSchema() {
		AppInit.htmlReady(function () {
			var toolbarPx = $("#main-toolbar:visible").width() || 0;
			$schemaPanel.css("right", toolbarPx + "px");
			Resizer.makeResizable($schemaPanel, "horz", "left", 250);
		});

		AppInit.appReady(function () {
			EditorManager.on("activeEditorChange", function (event, focusedEditor) {
				updateFocusedEditor(focusedEditor);
			});
			updateConnections();
		});
	}


	(function () {
		initializePreviewSchema();
	}());
});
