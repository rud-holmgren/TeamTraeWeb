
declare interface Array<T> {
    // Convert array to dictionary
    toDict(keySelector: (elm: T) => string): any;
}

// Convert array to dictionary
Array.prototype.toDict = function <T>(keySelector): any {
    return this.reduce(function (acc, elm) { acc[keySelector(elm)] = elm; return acc; }, {});
}
