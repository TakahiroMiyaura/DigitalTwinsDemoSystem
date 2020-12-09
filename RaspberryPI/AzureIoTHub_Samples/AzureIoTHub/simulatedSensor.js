/*
* IoT Hub Raspberry Pi NodeJS - Microsoft Sample Code - Copyright (c) 2017 - Licensed MIT
* Azure Digital Twins Demo - Copyright (c) Takahiro Miyaura 2020 - Licensed MIT
*/
'use strict';

function Sensor(/* options */) {
  // nothing todo
}

Sensor.prototype.init = function (callback) {
  // nothing todo
  callback();
}

Sensor.prototype.read = function (config,callback) {
  callback(null, {
    temperature: random(20, 30),
    pressure : random(990, 1080),
    humidity : random(60, 80),
    light : random(100, 800),
    Co2 :  random(400, 2000),
    TVOC : random(100, 800),
  });
}

function random(min, max) {
  return Math.random() * (max - min) + min;
}

module.exports = Sensor;
