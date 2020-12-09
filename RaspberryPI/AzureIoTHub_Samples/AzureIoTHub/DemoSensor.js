/*
* IoT Hub Raspberry Pi NodeJS - Microsoft Sample Code - Copyright (c) 2017 - Licensed MIT
*/
'use strict';

const fs = require('fs');
const path = require('path');
const sensor_data_file = '/all_sensor_infos.json';

function Sensor(/* options */) {
  // nothing todo
}

Sensor.prototype.init = function (callback) {
  // nothing todo
  callback();
}

Sensor.prototype.read = function (config,callback) {

  var fileName = config.sensorInfoPath.replace('~',process.env.HOME);
  if(fs.existsSync(fileName))
  {
    var fileStr = fs.readFileSync(fileName);
    console.log(fileStr);
    var sensor_infos = JSON.parse(fileStr);
    console.log(sensor_infos);
    callback(null, {
      temperature: sensor_infos.temperature,
      pressure : sensor_infos.pressure,
      humidity : sensor_infos.humidity,
      light : sensor_infos.light,
      Co2 :  sensor_infos.Co2,
      TVOC : sensor_infos.TVOC,
      statusCode : sensor_infos.statusCode,
      isAlert : sensor_infos.isAlert,
    });
  }
  else
  {
    callback(null, {
      temperature: 0,
      pressure : 0,
      humidity : 0,
      light : 0,
      Co2 : 0,
      TVOC : 0,
      statusCode : [],
      isAlert : false
    });
  }
}

module.exports = Sensor;
