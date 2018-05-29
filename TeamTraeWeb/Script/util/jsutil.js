"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
// Make object visible to global (window) scope as module.key = value
function makeJsVisible(module, key, value) {
    var jsmod = global[module] || {};
    jsmod[key] = value;
    global[module] = jsmod;
}
exports.makeJsVisible = makeJsVisible;
//# sourceMappingURL=jsutil.js.map