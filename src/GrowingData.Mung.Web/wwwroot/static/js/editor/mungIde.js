
(function ($) {
	$.fn.mungIde = function (dashboard, graph) {
		if (dashboard == null) throw "mungIde: dashboard is null";

		var self = this;
		self.textTitle = self.find("#graph-title");

		// Bind the model
		if (graph != null) {
			self.graphModel = new mungGraphModel(graph, dashboard);
		} else {
			self.graphModel = new mungGraphModel({
				GraphId: -1,
				Html: $("." + self.defaultTemplate + " .default-html").html(),
				X: -1,
				Y: -1,
				Width: 12,
				Height: 5,
			}, dashboard);
		}

		self.codeHolder = self.find(".ide-code");
		self.editorHtml = self.find(".editor-html");
		
		self.textarea = self.find(".graph-html").css("height", self.codeHolder.height() + "px");

		function unescape(code) {
			return code.replace(/&gt;/g, ">").replace(/&lt;/g, "<");
		}


		//self.btnPreview = self.find(".btn-preview").click(function () {
		//	self.find(".graph.preview").empty();

		//	var previewModel = new mungGraphModel(self.readUi(), dashboard);

		//	var graph = $(".graph.template")
		//		.clone()
		//		.removeClass("template")
		//		.appendTo(self.find(".graph.preview"))
		//		.mungGraph(previewModel, dashboard);

		//});


		this.readUi = function () {
			return {
				GraphId: self.graphModel.data.GraphId,
				Html: self.editorHtml.editor.code(),
				X: self.graphModel.data.X,
				Y: self.graphModel.data.Y,
				Width: self.graphModel.data.Width,
				Height: self.graphModel.data.Height,
				Title: self.textTitle.val()
			}
		}

		self.save = function () {
			self.graphModel.update(self.readUi());
			self.graphModel.save(function () {
				alert("Saved");
			});
		};


		function initializeEditors() {
			self.editorHtml.mungEditorHtml(dashboard, self);
			self.editorHtml.editor.code(self.graphModel.data.Html);


			self.graphModel.saved(function () {
				document.location = dashboard.Url;
			});


		}

		initializeEditors();
		return this;
	}

}(jQuery));
