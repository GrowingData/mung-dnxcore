
(function ($) {
	$.fn.tickerUsers = function (connection, icons) {

		var self = this;
		var gooroos = {};

		function init() {
			var eventTypes = self.data("event-types").split(',');
			connection.registerCallback(eventTypes, function (evt) {
				var gid = evt.Data.gooroo_id;
				// We have a user
				var userNode = gooroos[gid];
				if (userNode) {
					userNode.prependTo(self.find("ul.user-list"));
				} else {
					userNode = self.find(".user-template")
						.clone()
						.addClass("gooroo-" + gid)
						.removeClass("template")
						.removeClass("user-template")
						.prependTo(self.find("ul.user-list"));



					var img = "https://gooroo.io/static/img/unknown.png";
					if (evt.Data.Photo != "/static/img/unknown.png") {
						img = evt.Data.Photo;
					}
					if (gid == -1) {
						userNode.find(".user-name").text("Anonymous");
						userNode.find(".user-photo").css("background-image", "url(https://gooroo.io/static/img/unknown.png)");
					} else {
						userNode.find(".user-name").text(evt.Data.FirstName + " " + evt.Data.LastName);
						userNode.find(".user-photo").css("background-image", "url(" + img + ")");

					}

					gooroos[gid] = userNode;
				}
				

				var iconsHolder = self.find(".gooroo-" + gid + " .mini-icons");
				for (var i = 0; i < icons.length; i++) {
					if (icons[i].checkFilter(evt)) {
						console.log("yo")
						iconsHolder.prepend(
							$("<li>").append(
								icons[i].find("i.fa").clone()
							)
						);
					}
				}



			});

		}

		init();

	}

}(jQuery));

