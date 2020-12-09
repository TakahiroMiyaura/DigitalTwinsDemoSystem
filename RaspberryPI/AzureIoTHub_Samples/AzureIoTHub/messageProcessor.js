/*
* Azure Digital Twins Demo - Copyright (c) Takahiro Miyaura 2020 - Licensed MIT
*/
'use strict';

const SimulatedSensor = require('./simulatedSensor.js');
const DemoSensor = require('./DemoSensor.js');

function MessageProcessor(option) {
  option = Object.assign({
    deviceId: '[Unknown device] node',
    uuid: '[Unknown device] node'
  }, option);
  this.option=option;
  this.sensor = option.isEmulated ? new SimulatedSensor() : new DemoSensor(option);
  this.deviceId = option.deviceId;
  this.sensorName = option.sensorName;
  this.uuid = option.iBeaconOption.uuid;
  this.sensor.init(() => {
    this.inited = true;
  });
}

MessageProcessor.prototype.getMessage = function (cb) {
  if (!this.inited) { return; }
  this.sensor.read(this.option,(err, data) => {
    if (err) {
      console.log('[Sensor] Read data failed due to:\n\t' + err.message);
      return;
    }

    cb(JSON.stringify({
      deviceId: this.deviceId,
      sensorName : this.sensorName,
      uuid:this.uuid,
      temperature : data.temperature,
      pressure : data.pressure,
      humidity : data.humidity,
      light : data.light,
      Co2 : data.Co2,
      TVOC : data.TVOC,
      statusCode : data.statusCode,
      isAlert : data.isAlert
    }));
  });
}

module.exports = MessageProcessor;
