

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

	var emailTemplateHolder;
	var iframe;

	function updateFocusedEditor(focusedEditor) {
		if (editor) {
			inputField.removeEventListener("input", inputChangedHandler, true);
		}

		if (focusedEditor) {

			editor = focusedEditor;

			if (editor.getFile().name.endsWith(".cshtml")) {
				editor.on("change", onChangeHandler);
				updatePreview(editor);
			}
		}
	};



	function onChangeHandler(event) {
		updatePreview(event.target);

		var code = event.target.document.getText();

	};

	function updatePreview(ed) {
		var code = ed.document.getText();

		$.ajax({
			url: "/notifications/preview",
			method: "POST",
			data: { template: code },
			success: function (r) {
				iframe.contents().find('body').html(r);
				updateContentHeight();
			},
			error: function (r) {
				iframe.contents().find('body').html(r);
				//iframe.contents().html(r.responseText);
				//var errorDiv = $("<div>").addClass("email-template error-message").text(r.Message).appendTo(emailTemplateHolder);
				//var stackPre = $("<pre>").addClass("email-template error-stack").text(r.Stack).appendTo(emailTemplateHolder);


			}
		});
	}
	function updateContentHeight() {
		var innerHeight = iframe.contents().find('body').height();
		var maxHeight = $(window).height() * 0.3;

		if (innerHeight < maxHeight) {
			iframe.css("height", innerHeight + "px");
		} else {
			iframe.css("height", maxHeight + "px");
		}
		Resizer.show(emailTemplateHolder);

	}

	function initMungTemplateRender() {
		emailTemplateHolder = $("<div>")
			.css("border-top", "solid 2px #2d2e30")
			.attr("id", "email-preview-inner");

		iframe = $("<iframe>").css( {
			"border": "none",
			"width": "100%",
			"margin": "0",
			"padding": "0"
		}).appendTo(emailTemplateHolder);

		iframe.load(function () {
		})


		AppInit.htmlReady(function () {
			WorkspaceManager.createBottomPanel("email-preview", emailTemplateHolder, 200);

		});

		AppInit.appReady(function () {

			EditorManager.on("activeEditorChange", function (event, focusedEditor) {
				updateFocusedEditor(focusedEditor);
			});

		});
	}


	(function () {
		initMungTemplateRender();
	}());
});
