"use strict";

var MUNG = MUNG || {};

MUNG.Utilities = {

	convertToUrl: function (title) {
		return title
        .toLowerCase()
        .replace(/[^\w ]+/g,'')
        .replace(/ +/g,'-')
		;
	}

};
