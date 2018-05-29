
declare interface String {
    // Convert string to camelcase
    toCamel(): string;
}

// Convert string to camelcase
String.prototype.toCamel = function () {
    return this.replace(/(\-[a-z])/g, function ($1) { return $1.toUpperCase().replace('-', ''); });
};
