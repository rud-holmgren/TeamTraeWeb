
import * as jquery from 'jquery';

global['$'] = jquery;
global['jQuery'] = jquery;

import 'popper.js';
import 'bootstrap';
import 'leaflet';

import '../js_modules/fontawesome-free-5.0.9/svg-with-js/js/fontawesome-all.js';


// AJAX Get wrapper
function doAjaxGet(path: string, args: any, resolve: Function, reject: Function) {
    $.getJSON(path, args).then(function (result) {
        if (!result.success) {
            reject(result);
        }
        else {
            resolve(result.data);
        }
    });
}

export function ajaxGet(path: string, args: any): Promise<any> {
    var retval = new Promise<any>(function (resolve, reject) {
        $(function () { doAjaxGet(path, args, resolve, reject); });
    });
    return retval;
}

import { makeJsVisible } from './util/jsutil';
makeJsVisible('GG', "ajaxGet", ajaxGet);

$(function () {
    console.log('all systems go');
});

