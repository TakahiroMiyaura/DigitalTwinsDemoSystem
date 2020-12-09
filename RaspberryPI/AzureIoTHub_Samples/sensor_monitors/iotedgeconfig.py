
import os
import sys
import json
from collections import OrderedDict


class IoTEdgeConfig:

    interval=0
    sensorInfoPath= ''

    CONFIG_PATH = '../config.json'

    def __init__(self):
        jsonStr = '{' \
            '\"interval\": 10000,' \
            '\"sensorInfoPath\": \"~/.sensor-infos/.sensor-infos/all_sensor_infos.json\"' \
            '}'
        if(os.path.exists(self.CONFIG_PATH)):
            f = open(self.CONFIG_PATH)
            jsonStr = f.read()
            f.close()
        __config = json.loads(jsonStr)
        self.interval=__config["interval"]
        self.sensorInfoPath=__config["sensorInfoPath"]
        if(type(self.sensorInfoPath) is str and '~/' in self.sensorInfoPath):
            self.sensorInfoPath = self.sensorInfoPath.replace('~',os.path.expanduser('~'))



