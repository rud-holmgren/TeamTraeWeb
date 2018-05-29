"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const jquery = require("jquery");
global['$'] = jquery;
global['jQuery'] = jquery;
require("popper.js");
require("bootstrap");
require("../js_modules/fontawesome-free-5.0.9/svg-with-js/js/fontawesome-all.js");
// AJAX Get wrapper
function doAjaxGet(path, args, resolve, reject) {
    $.getJSON(path, args).then(function (result) {
        if (!result.success) {
            reject(result);
        }
        else {
            resolve(result.data);
        }
    });
}
function ajaxGet(path, args) {
    var retval = new Promise(function (resolve, reject) {
        $(function () { doAjaxGet(path, args, resolve, reject); });
    });
    return retval;
}
exports.ajaxGet = ajaxGet;
const jsutil_1 = require("./util/jsutil");
jsutil_1.makeJsVisible('GG', "ajaxGet", ajaxGet);
$(function () {
    console.log('all systems go');
});
//# sourceMappingURL=main.js.map