
function mungGraphModel(data, dashboard) {
	if (data == null) throw "mungGraphModel: data is null";
	if (dashboard == null) throw "mungGraphModel: dashboard is null";


	var self = this;
	self.data = data;


	this.events = new MUNG.EventManager();


	this.saved = function (fn) {
		self.events.bind("saved", fn);
		return this;
	}

	

	this.update = function (newViewData) {
		self.data = newViewData;
	}

	this.save = function () {
		
	}


	return this;
}
