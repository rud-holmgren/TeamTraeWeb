"use strict";

/*
    This script bundles and minifies Ts and Css source files.
    All external libs are copied into two files: site.js and site.css
    These are then minified to site.min.js and site.min.css

    It also builds the bootstrap theme from a bootswatch template.
*/

var externalCssSources = [
    "node_modules/leaflet/dist/leaflet.css"
];

var gulp = require("gulp"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    rename = require('gulp-rename'),
    merge = require('gulp-merge'),
    sass = require('gulp-sass'),
    postcss = require('gulp-postcss'),
    del = require("del"),
    pump = require('pump'),
    runSequence = require('run-sequence');

gulp.task("build", ["build:ts", "build:css", "build:scss"]);
gulp.task("minify", ["minify:js", "minify:css"]);

gulp.task("default", function (done) {
    runSequence("build", "minify", done);
});

// Minify JavaScript
gulp.task("minify:js", function (done) {
    pump([
        gulp.src(['wwwroot/js/*.js', '!wwwroot/js/*.min.js']),
        uglify(),
        rename({ suffix: '.min' }),
        gulp.dest('wwwroot/js/')
    ], done);
});

// Minify CSS
gulp.task("minify:css", function (done) {
    pump([
        gulp.src(['wwwroot/css/*.css', '!wwwroot/css/*.min.css']),
        cssmin(),
        rename({ suffix: '.min' }),
        gulp.dest('wwwroot/css/')
    ], done);
});

var flexbugfixes = require("postcss-flexbugs-fixes");
var autoprefixer = require('autoprefixer');

var postcss_plugins = [
    flexbugfixes(),
    autoprefixer({
        browsers: [
            'Chrome >= 35',
            'Firefox >= 38',
            'Edge >= 12',
            'Explorer >= 9',
            'iOS >= 8',
            'Safari >= 8',
            'Android 2.3',
            'Android >= 4',
            'Opera >= 12']
    })
];


// Build theme.css
gulp.task("build:scss", function (done) {
    pump([
        gulp.src("Theme/build.scss"),
        sass({ outputStyle: 'expanded' }),
        postcss(postcss_plugins),
        rename("site.css"),
        gulp.dest('wwwroot/css/')
    ], done);
});

// Build packages.css
gulp.task("build:css", function (done) {
    pump([
        gulp.src(externalCssSources),
        concat('packages.css'),
        gulp.dest('wwwroot/css/')
    ], done);
});


// Build typescript (ie. compile ts files to site.js)
// Notes:
//   browserify: create commonjs-compatible module system that runs in browsers
//   tsify: TypeScript compiler plugin for browserify - reads config from tsconfig.json
//   babelify: Transpiler plugin for browserify - making javascript compatible with old browsers
var browserify = require('browserify'),
    source = require('vinyl-source-stream'),
    tsify = require('tsify'),
    sourcemaps = require('gulp-sourcemaps'),
    buffer = require('vinyl-buffer'),
    watchify = require('watchify'),
    gulputil = require('gulp-util');

function doBrowserify() {
    return browserify({
        basedir: '.',
        debug: true,
        entries: ['Script/main.ts'],
        cache: {},
        packageCache: {}
    })
        .plugin(tsify)
        .transform('babelify', {
            presets: ['env'],
            extensions: ['.ts']
        });
}

function pumpTypeScript(compile, done) {
    pump([
        compile.bundle(),
        source('site.js'),
        buffer(),
        sourcemaps.init({ loadMaps: true }),
        sourcemaps.write('./'),
        gulp.dest('wwwroot/js/')
    ], done);
}

gulp.task('build:ts', function (done) {
    pumpTypeScript(doBrowserify(), done);
});

var watchedBrowserify = watchify(doBrowserify());

function watchifyBundle(done) {
    pumpTypeScript(watchedBrowserify, done);

//    return watchedBrowserify
//        .bundle()
//        .pipe(source('site.js'))
//        .pipe(buffer())
//        .pipe(sourcemaps.init({ loadMaps: true }))
//        .pipe(sourcemaps.write('./'))
//        .pipe(gulp.dest('wwwroot/js/'));
}

gulp.task('watch:ts', watchifyBundle);
watchedBrowserify.on("update", watchifyBundle);
watchedBrowserify.on("log", gulputil.log);
