
// Make object visible to global (window) scope as module.key = value
export function makeJsVisible(module: string, key: string, value: any) {
    var jsmod = global[module] || {};
    jsmod[key] = value;
    global[module] = jsmod;
}
