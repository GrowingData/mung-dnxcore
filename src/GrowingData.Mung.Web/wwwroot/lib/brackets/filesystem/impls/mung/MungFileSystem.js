/*
 * Copyright (c) 2013 Adobe Systems Incorporated. All rights reserved.
 *  
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *  
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *  
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 * 
 */


/*jslint vars: true, plusplus: true, devel: true, nomen: true, regexp: true, indent: 4, maxerr: 50 */
/*global define, window, PathUtils */

define(function (require, exports, module) {
	"use strict";

	var FileSystemError = require("filesystem/FileSystemError"),
        FileSystemStats = require("filesystem/FileSystemStats"),
        AjaxFileSystem = require("filesystem/impls/demo/AjaxFileSystem");


	// Brackets uses FileSystem to read from various internal paths that are not in the user's project storage. We
	// redirect core-extension access to a simple $.ajax() to read from the source code location we're running from,
	// and for now we ignore we possibility of user-installable extensions or persistent user preferences.
	var CORE_EXTENSIONS_PREFIX = PathUtils.directory(window.location.href) + "extensions/default/";
	//    var USER_EXTENSIONS_PREFIX = "/.brackets.user.extensions$/";
	//    var CONFIG_PREFIX = "/.$brackets.config$/";


	var fileSystem = null
	var fileSystemIndex = null

	var dashboardContent = {};
	var dashboardGraphs = {};

	function _dashboardUrl() {
		var url = document.location.pathname;
		var urlParts = url.split("/");
		var dashboardUrlParts = [];
		for (var i = 3; i < urlParts.length; i++) {
			dashboardUrlParts.push(urlParts[i]);
		}
		var dashboardUrl = "/" + dashboardUrlParts.join("/");
		return dashboardUrl;
	}

	function _buildIndexUrl(item) {
		fileSystemIndex[item.Url] = item;
		for (var i = 0; i < item.Children.length; i++) {
			_buildIndexUrl(item.Children[i]);
		}
	}
	function _buildIndexName(item, parentUrl) {

		var myUrl = "";
		if (parentUrl) {
			myUrl = parentUrl + item.Name;
			if (item.Type == "directory" && !myUrl.endsWith("/")){
				myUrl += "/";
			}
			fileSystemIndex[myUrl] = item;
		}

		for (var i = 0; i < item.Children.length; i++) {
			_buildIndexName(item.Children[i], myUrl || "/");
		}
	}

	function _getFileSystemItems(async) {
		//var dashboardUrl = _dashboardUrl();
		$.ajax({
			url: "/api/file-system/ls",
			method: "GET",
			success: function (response) {
				if (response.Success) {
					fileSystem = response.Root;
					fileSystemIndex = {};
					_buildIndexUrl(response.Root);
					_buildIndexName(response.Root);

				}
			},
			async: async
		});
	}
	_getFileSystemItems(false);


	function _fileFromPath(path) {
		var pathParts = path.split("/");

		return pathParts[pathParts.length - 1];
	}


	function _startsWith(path, prefix) {
		return (path.substr(0, prefix.length) === prefix);
	}

	function _stripTrailingSlash(path) {
		return path[path.length - 1] === "/" ? path.substr(0, path.length - 1) : path;
	}

	function _getDemoData(fullPath) {
		var prefix = "/";

		// IF you are not in our prefix, then return null;
		if (fullPath.substr(0, prefix.length) !== prefix) {
			return null;
		}
		var suffix = _stripTrailingSlash(fullPath.substr(prefix.length));
		if (!suffix) {
			return dashboardContent;
		}

		var segments = suffix.split("/");
		var dir = dashboardContent;
		var i;
		for (i = 0; i < segments.length; i++) {
			if (!dir) { return null; }
			dir = dir[segments[i]];
		}
		return dir;
	}

	function _makeStat(result) {
		var options = {
			isFile: result.Type == "file", //typeof demoData === "string",
			mtime: new Date(0),
			hash: 0
		};
		//if (options.isFile) {
		//	options.size = demoData.length;
		//}
		return new FileSystemStats(options);
	}
	function _nameFromPath(path) {
		var segments = _stripTrailingSlash(path).split("/");
		return segments[segments.length - 1];
	}


	function stat(path, callback) {
		if (_startsWith(path, CORE_EXTENSIONS_PREFIX)) {
			AjaxFileSystem.stat(path, callback);
			return;
		}

		var result = fileSystemIndex[path];
		if (!result) {
			callback(FileSystemError.NOT_FOUND);
			return;
		}
		callback(null, _makeStat(result));
	}

	function exists(path, callback) {
		console.log("exists: " + path);
		stat(path, function (err) {
			if (err) {
				callback(null, false);
			} else {
				callback(null, true);
			}
		});
	}

	function readdir(path, callback) {
		if (_startsWith(path, CORE_EXTENSIONS_PREFIX)) {
			callback("Directory listing unavailable: " + path);
			return;
		}

		console.log("readdir: " + path);

		// Here is a tricky thing, in that the "name" supplied
		// here isn't actually exactly the same as the "urlpart"
		// given in the url (since a name can have ' ', but the 
		// url will encode it as '+').  So we might need to look 
		// a litle harder to find the directory.

		var result = fileSystemIndex[path];
		if (!result) {
			callback(FileSystemError.NOT_FOUND);
			return;
		}


		var stats = [], names = [];
		for (var i = 0; i < result.Children.length; i++) {
			var child = result.Children[i]
			names.push(child.Name);
			stats.push(_makeStat(child));
		}
		callback(null, names, stats);
	}

	function mkdir(path, mode, callback) {
		console.log("mkdir: " + path);
		callback("Cannot modify folders on HTTP demo server");
	}


	function readFile(path, options, callback) {
		if (typeof options === "function") {
			callback = options;
		}

		if (_startsWith(path, CORE_EXTENSIONS_PREFIX)) {
			AjaxFileSystem.readFile(path, callback);
			return;
		}

		console.log("readFile: " + path);
		var result = fileSystemIndex[path];
		if (!result) {
			callback(FileSystemError.NOT_FOUND);
			return;
		}

		$.ajax({
			url: "/api/file-system" + result.Url,
			method: "GET",
			data: { url: path },
			success: function (response) {
				//if (!response.Success) {
				//	callback(FileSystemError.NOT_FOUND);
				//}

				callback(null, response, _makeStat(result));

			}
		});


		//var storeData = _getDemoData(path);
		//if (!storeData && storeData !== "") {
		//	callback(FileSystemError.NOT_FOUND);
		//} else if (typeof storeData !== "string") {
		//	callback(FileSystemError.INVALID_PARAMS);
		//} else {
		//	var name = _nameFromPath(path);
		//	callback(null, storeData, _makeStat(storeData[name]));
		//}
	}


	function rename(oldPath, newPath, callback) {
		console.log("rename: " + oldPath + " -> " + newPath);

		var result = fileSystemIndex[oldPath];
		if (!result) {
			callback(FileSystemError.NOT_FOUND);
			return;
		}

		$.ajax({
			url: "/api/file-system" + result.Url + "/rename",
			data: {
				"oldPath": oldPath,
				"newPath": newPath
			},
			method: "POST",
			success: function (response) {
				if (response.Success) {
					// Update our cache of file names
					_getFileSystemItems(false);
					callback(null);
				}
			}
		});

	}
	function writeFile(path, data, options, callback) {
		console.log("writeFile: " + path);

		var result = fileSystemIndex[path];
		if (!result) {
			callback(FileSystemError.NOT_FOUND);
			return;
		}

		// Now lets save the graph, not as hard as it sounds
		$.ajax({
			url: "/api/file-system" + result.Url,
			data: {
				"data": data
			},
			method: "PUT",
			success: function (response) {
				if (response.Success) {
					// Update our cache of file names
					_getFileSystemItems(false);
					callback(null, _makeStat(data), response.IsNew);
				}
			}
		});
	}
	function moveToTrash(path, callback) {
		console.log("moveToTrash: " + path);

		var result = fileSystemIndex[path];
		if (!result) {
			callback(FileSystemError.NOT_FOUND);
			return;
		}

		// Now lets save the graph, not as hard as it sounds
		$.ajax({
			url: "/api/file-system" + result.Url,
			data: {},
			method: "DELETE",
			success: function (response) {
				if (response.Success) {
					// Update our cache of file names
					_getFileSystemItems(false);
					callback(null, _makeStat(data), response.IsNew);
				}
			}
		});
	}

	function unlink(path, callback) {
		callback("Cannot modify files on HTTP demo server");
	}



	function initWatchers(changeCallback, offlineCallback) {
		// Ignore - since this FS is immutable, we're never going to call these
	}

	function watchPath(path, callback) {
		//console.warn("File watching is not supported on immutable HTTP demo server");
		callback();
	}

	function unwatchPath(path, callback) {
		callback();
	}

	function unwatchAll(callback) {
		callback();
	}

	function showOpenDialog(allowMultipleSelection, chooseDirectories, title, initialPath, fileTypes, callback) {
		// FIXME
		throw new Error();
	}

	function showSaveDialog(title, initialPath, proposedNewFilename, callback) {
		// FIXME
		throw new Error();
	}


	// Export public API
	exports.showOpenDialog = showOpenDialog;
	exports.showSaveDialog = showSaveDialog;
	exports.exists = exists;
	exports.readdir = readdir;
	exports.mkdir = mkdir;
	exports.rename = rename;
	exports.stat = stat;
	exports.readFile = readFile;
	exports.writeFile = writeFile;
	exports.unlink = unlink;
	exports.moveToTrash = moveToTrash;
	exports.initWatchers = initWatchers;
	exports.watchPath = watchPath;
	exports.unwatchPath = unwatchPath;
	exports.unwatchAll = unwatchAll;

	exports.recursiveWatch = true;
	exports.normalizeUNCPaths = false;
});