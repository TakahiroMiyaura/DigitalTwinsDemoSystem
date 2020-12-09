bleacon = require('bleacon');



  // read in configuration in config.json
  try {
    config = require('../config.json');
  } catch (err) {
    console.error('Failed to load config.json:\n\t' + err.message);
    return;
  }
  var uuid= config.iBeaconOption.uuid;
  var major = config.iBeaconOption.major;
  var minor = config.iBeaconOption.minor;
  var measuredPower = config.iBeaconOption.measuredPower;
bleacon.startAdvertising(uuid, major, minor, measuredPower);
