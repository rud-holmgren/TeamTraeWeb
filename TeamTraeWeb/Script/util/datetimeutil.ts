import * as moment from '../../node_modules/moment-timezone/builds/moment-timezone-with-data-2012-2022';

export function makeMomentGlobal() {
    global['moment'] = moment;
}

// Guess browser time zone (moment best guess)
var browserTimeZone: string = moment.tz.guess();

// Convert date to browser time
export function toBrowserTime(dtime: Date): moment.Moment {
    return moment(dtime).tz(browserTimeZone);
}

// Guess browser time zone
export function guessBrowserTimezone(): string {
    return browserTimeZone;
}

// Parse string as javascript tics in UTC
export function parseStringAsUtcTicks(str: string) : moment.Moment {
    return moment.utc(Number(str));
}

// Convert time to javascript tics in UTC
export function toUtcTicks(stamp: moment.Moment) : Number {
    return moment(stamp).add(stamp.utcOffset(), 'm').valueOf();
}
