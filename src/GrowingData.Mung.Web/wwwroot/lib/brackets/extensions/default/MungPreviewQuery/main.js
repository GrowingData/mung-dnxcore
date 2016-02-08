

/*jslint vars: true, plusplus: true, devel: true, nomen: true, indent: 4, maxerr: 50 */
/*global define, brackets, window */

define(function (require, exports, module) {
	"use strict";
	var _editor = null;

	var EditorManager = brackets.getModule("editor/EditorManager");
	var WorkspaceManager = brackets.getModule("view/WorkspaceManager");
	var AppInit = brackets.getModule("utils/AppInit");
	var Resizer = brackets.getModule("utils/Resizer");
	var DocumentManager = brackets.getModule("document/DocumentManager");

	var $queryPanel = $("<div>")
			.css({
				"border-top": "solid 2px #2d2e30",
				"overflow": "auto",
				"overflow-x": "auto",
				"overflow-y": "auto",
			})
			.attr("id", "query-panel");


	var $toolbarHolder = $("<ul>")
		.addClass("toolbar")
		.appendTo($queryPanel);



	var $queryResultsHolder = $("<div>")
		.attr("id", "query-results-table")
		.appendTo($queryPanel);


	// Bind some buttons...
	var $executeQueryButton = $("<li>")
		.addClass("toolbar-button")
		.append($("<a>")
			.text("Run")
			.click(function () { updatePreview(); return false; })
		)
		.appendTo($toolbarHolder);


	function updateFocusedEditor(focusedEditor) {
		if (_editor) {
			_editor.off("change", onChangeHandler);
		}

		if (focusedEditor) {
			_editor = focusedEditor;
			if (_editor.getFile().name.endsWith(".sql")) {
				_editor.on("change", onChangeHandler);
				//updatePreview(_editor);
				$queryPanel.show();
			} else {
				$queryPanel.hide();
			}
		}
	};


	function onChangeHandler(event) {
		//updatePreview(event.target);
		//var code = event.target.document.getText();
	};

	function updatePreview() {
		var code = _editor.document.getText();
		$queryResultsHolder.empty();


		var table = $("<table>")
			.addClass("mung-table table")
			.appendTo($queryResultsHolder);

		var isFirst = true;
		MUNG.Query.stream(code, function (row) {
			if (isFirst) {
				var header = $("<tr>").appendTo(table);
				for (var k in row) {
					$("<th>").text(k).appendTo(header);
				}
				isFirst = false;
			}

			var tr = $("<tr>").appendTo(table);
			for (var k in row) {
				$("<td>").text(row[k]).appendTo(tr);
			}
		},
		function () {

		},
		function (err) {
			$queryResultsHolder.empty();

			$("<div>").addClass("error").text(err.Message).appendTo($queryResultsHolder);

		});

		//$.ajax({
		//	url: "/api/query",
		//	method: "POST",
		//	data: { sql: code },
		//	success: function (r) {
		//		if (r.ErrorMessage) {
		//			$queryResultsHolder.append($("<p>").addClass("error red-bg").html(r.ErrorMessage));
		//			updateContentHeight();

		//			return;
		//		}
		//		$queryResultsHolder.mungTableBinder(r);
		//		updateContentHeight();
		//	},

		//	error: function (r) {
		//	}
		//});
	};

	function updateContentHeight() {
		var innerHeight = $queryPanel.height();
		var maxHeight = $(window).height() * 0.3;

		if (innerHeight < maxHeight) {
			$queryPanel.css("height", innerHeight + "px");
		} else {
			$queryPanel.css("height", maxHeight + "px");
		}
		$("#sql-schema-panel").css("bottom", $queryPanel.height() + 28 + "px");
		Resizer.show($queryPanel);

	};

	function initializePreviewQuery() {
		AppInit.htmlReady(function () {
			WorkspaceManager.createBottomPanel("query-panel", $queryPanel, 200);

			$queryPanel.on("panelResizeUpdate", function () {
				$("#sql-schema-panel").css("bottom", $queryPanel.height() + 28 + "px");
			});
		});

		AppInit.appReady(function () {

			EditorManager.on("activeEditorChange", function (event, focusedEditor) {
				updateFocusedEditor(focusedEditor);
			});

		});
	};


	(function () {
		initializePreviewQuery();
	}());
});
