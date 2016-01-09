
(function ($) {
	$.fn.mungGraphEditor = function (dashboard) {
		if (dashboard == null) throw "mungGraphEditor: dashboard is null";

		var self = this;


		self.graphModel = null;

		self.defaultTemplate = "Table";
		self.connectionSelector = self.find(".select-connection");
		self.selectTemplate = self.find(".select-template");
		self.graphTemplates = $(".graph-templates");

		self.editorHtml = self.find(".editor-html");
		self.editorSql = self.find(".editor-sql");
		self.editorJs = self.find(".editor-js");

		self.modal({
			backdrop: 'static',
			keyboard: false,
			show: false
		});

		self.btnSave = self.find(".btn-save").click(function () {
			self.graphModel.update(self.readUi());
			self.graphModel.save(function () {
				alert("Saved");
			});
		});

		self.btnClose = self.find(".btn-close").click(function () {
			self.modal('hide');
		});

		function unescape(code) {
			return code.replace(/&gt;/g, ">").replace(/&lt;/g, "<");
		}

		self.btnLoadTemplate = self.find(".load-template").click(function () {
			var templateName = self.find(".select-template").val();
			self.bindTemplate(templateName);
		});

		self.bindTemplate = function (templateName) {
			var template = self.graphTemplates.find(".graph-template." + templateName);

			if (template) {
				// Now bind the defaults everything...
				self.editorHtml.editor.code(unescape(template.find(".default-html").html()));
				self.editorSql.editor.code(unescape(template.find(".default-sql").html()));
				self.editorJs.editor.code(unescape(template.find(".default-js").html()));

				self.find(".graph-setting").css("display", "none");
				self.find(".html-row").css("display", "none");
				self.find(".js-row").css("display", "none");

				var templateInner = template.find(".graph-template-inner");

				if (templateInner.hasClass("show-sql")) {
					self.find(".graph-setting-sql").css("display", "block");
				}
				if (templateInner.hasClass("show-html")) {
					self.find(".graph-setting-html").css("display", "block");
				}
				if (templateInner.hasClass("show-js")) {
					self.find(".graph-setting-js").css("display", "block");
				}

				if (templateInner.data("connection-id")) {
					self.connectionSelector.val(templateInner.data("connection-id"));
				}
			}

		}

		// Actually bind codeMirror after the modal has appeared to 
		// so that CodeMirror displays code without you needing to click on it
		self.on('shown.bs.modal', function () {
			self.editorHtml.editor.code(self.graphModel.data.Html);
			self.editorSql.editor.code(self.graphModel.data.Sql);
			self.editorJs.editor.code(self.graphModel.data.Js);
		});


		self.btnPreview = self.find(".btn-preview").click(function () {
			self.find(".graph.preview").empty();

			var previewModel = new mungGraphModel(self.readUi(), dashboard);

			var graph = $(".graph.template")
				.clone()
				.removeClass("template")
				.appendTo(self.find(".graph.preview"))
				.mungGraph(previewModel, dashboard);

		});



		this.create = function () {
			self.graphModel = new mungGraphModel({
				GraphId: -1,
				Html: $("." + self.defaultTemplate + " .default-html").html(),
				Sql: $("." + self.defaultTemplate + " .default-sql").text(),
				Js: $("." + self.defaultTemplate + " .default-js").text(),
				X: -1,
				Y: -1,
				ConnectionId: self.connectionSelector.val(),
				Width: 12,
				Height: 5,
			}, dashboard.dashboardModel);

			self.graphModel.saved(function () {
				document.location.reload();
				self.modal('hide');
			});

			self.find(".modal-title .edit").hide();
			self.find(".modal-title .add").show();

			self.modal('show');

		}

		this.edit = function (graphModel) {
			self.graphModel = graphModel;

			self.graphModel.saved(function () {
				document.location.reload();
				self.modal('hide');
			});

			self.find(".modal-title .edit").show();
			self.find(".modal-title .add").hide();

			self.find(".graph-title").val(graphModel.data.Title);
			self.connectionSelector.val(graphModel.data.ConnectionId);

			self.modal('show');

		}

		this.readUi = function () {
			return {
				GraphId: self.graphModel.data.GraphId,

				Title: self.find(".graph-title").val(),

				Html: self.editorHtml.editor.code(),
				Sql: self.editorSql.editor.code(),
				Js: self.editorJs.editor.code(),

				ConnectionId: self.connectionSelector.val(),
				X: self.graphModel.data.X,
				Y: self.graphModel.data.Y,
				Width: self.graphModel.data.Width,
				Height: self.graphModel.data.Height,
			}
		}



		function initializeEditors() {
			self.editorSql.mungEditorSql(dashboard, self);
			self.editorHtml.mungEditorSql(dashboard, self);
			self.editorJs.mungEditorSql(dashboard, self);
		}

		initializeEditors();
		return this;
	}

}(jQuery));
