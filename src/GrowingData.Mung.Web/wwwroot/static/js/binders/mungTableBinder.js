
(function ($) {
	// «
	$.fn.mungTableBinder = function (data, formatMap) {
		if (data == null) throw "mungTableBinder: data is null";
		if (data.ColumnNames == null) throw "mungTableBinder: data.ColumnNames is missing";
		if (data.Rows == null) throw "mungTableBinder: data.Rows is missing";

		var self = this;
		var gsItem = self.parents(".grid-stack-item");

		var gsHeight = gsItem.height();
		var headerHeight = gsItem.find(".graph-header").outerHeight();
		var footerHeight = gsItem.find(".graph-footer").outerHeight();

		var availableHeight = gsHeight - (headerHeight + footerHeight);

		var holder = $("<div>")
			.addClass("mung-table-holder graph-inner-scroll")
			.css("height", availableHeight + "px")
			.appendTo(self)

		this.bindTable = function () {
			holder.empty();

			var table = $("<table>").addClass("mung-table table").appendTo(holder);

			var tr = $("<tr>").appendTo(table);
			_.each(data.ColumnNames, function (column) {
				$("<th>").text(column).appendTo(tr);
			});
			_.each(data.Rows, function (row) {
				var tr = $("<tr>").appendTo(table);
				_.each(data.ColumnNames, function (column) {
					var val = row[column];
					if (formatMap && formatMap[column]) {
						val = formatMap[column](val);
					}
					$("<td>").text(val).appendTo(tr);
				});
			});




		}

		this.bindTable();

		return this;
	}

}(jQuery));
