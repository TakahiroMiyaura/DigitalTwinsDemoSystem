#!/usr/bin/env python

import os
import sys
from iotedgeconfig import IoTEdgeConfig
from logging import  getLogger, DEBUG, INFO, handlers, Formatter, StreamHandler
from time import sleep
from ccs811 import CCS811
import math
import json

class SensorMonitorCCS811:
    
    CO2_PPM_THRESHOLD_1 = 1000
    CO2_PPM_THRESHOLD_2 = 2000

    CO2_LOWER_LIMIT  =  400
    CO2_HIGHER_LIMIT = 8192

    CO2_STATUS_CONDITIONING = 'CONDITIONING'
    CO2_STATUS_LOW          = 'LOW'
    CO2_STATUS_HIGH         = 'HIGH'
    CO2_STATUS_TOO_HIGH     = 'TOO HIGH'
    CO2_STATUS_ERROR        = 'ERROR'
       
    SENSOR_LOG_FILE         = 'ccs811.txt'
    
    LOG_FILE = '{script_dir}/logs/sensor_monitor_ccs811.log'.format(
        script_dir = os.path.dirname(os.path.abspath(__file__))
    )

    def __init__(self):
        self._ccs811 = CCS811()
        self._config = IoTEdgeConfig()
        self.co2_status = self.CO2_STATUS_LOW
        self.init_logger()
        self.INTERVAL = (self._config.interval / 1000)
        self.UpdateEnvironmetCycle = int(600 / self.INTERVAL)

    def init_logger(self):
        self._logger = getLogger(__class__.__name__)
        file_handler = handlers.RotatingFileHandler(self.LOG_FILE, maxBytes=100000, backupCount = 1)
        formatter = Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
        file_handler.setFormatter(formatter)
        file_handler.setLevel(DEBUG)
        self._logger.addHandler(file_handler)
        #stream_handler = StreamHandler()
        #stream_handler.setLevel(DEBUG)
        #self._logger.addHandler(stream_handler)
        self._logger.setLevel(DEBUG)
        

    def status(self, co2):
        if co2 < self.CO2_LOWER_LIMIT or co2 > self.CO2_HIGHER_LIMIT:
            return self.CO2_STATUS_CONDITIONING
        elif co2 < self.CO2_PPM_THRESHOLD_1:
            return self.CO2_STATUS_LOW
        elif co2 < self.CO2_PPM_THRESHOLD_2:
            return self.CO2_STATUS_HIGH
        else:
            return self.CO2_STATUS_TOO_HIGH

    def execute(self):
        self._logger.debug("Sensor Initialize...")
        path = os.path.dirname(self._config.sensorInfoPath)
        if not os.path.exists(path):
            os.makedirs(path)
        filePath = os.path.join(path,self.SENSOR_LOG_FILE)
        if not os.path.exists(filePath):
            f=open(filePath,'w')
            f.close()
        while not self._ccs811.available():
            pass
        self._logger.debug("Sensor Initialize...OK!")

        counter=self.UpdateEnvironmetCycle
        while True:
            if not self._ccs811.available():
                sleep(1)
                continue

            try:
                if not self._ccs811.readData():
                    co2 = self._ccs811.geteCO2()
                    co2_status = self.status(co2)
                    if co2_status == self.CO2_STATUS_CONDITIONING:
                        self._logger.debug("Under Conditioning...")
                        sleep(self.INTERVAL)
                        continue

                    tvoc=self._ccs811.getTVOC()
                    self._logger.debug("CO2: {0}ppm, TVOC: {1}".format(co2, tvoc))
                    f=open(filePath,'w')
                    f.write("{0},{1}\n".format(co2,tvoc))
                    f.close()

                    if co2_status != self.co2_status:
                        self.co2_status = co2_status
                        self._logger.info("CO2: {0}ppm, TVOC: {1}ppb".format(co2, self._ccs811.getTVOC()))

                    if(counter > self.UpdateEnvironmetCycle & os.path.exists(self._config.sensorInfoPath)):
                        counter=0
                        f = open(self._config.sensorInfoPath)
                        jsonStr = f.read()
                        f.close()
                        data = json.loads(jsonStr)
                        self._ccs811.setEnvironmentalData(math.ceil(data["humidity"]),data["temperature"])

                else:
                    self._logger.error('ERROR!')
                    self.reset()
                    while True:
                        pass
            except:
                self._logger.error(sys.exc_info())

            sleep(self.INTERVAL)
            counter = counter + 1

if __name__ == '__main__':
    sensor_monitor_ccs881 = SensorMonitorCCS811()
    sensor_monitor_ccs881.execute()

