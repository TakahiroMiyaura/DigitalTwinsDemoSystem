#!/usr/bin/env python3

import os
import time
import colorsys
import sys
import ST7735
from time import sleep
from iotedgeconfig import IoTEdgeConfig
from bme280 import BME280
from enviroplus import gas
from subprocess import PIPE, Popen
from PIL import Image
from PIL import ImageDraw
from PIL import ImageFont
from fonts.ttf import RobotoMedium as UserFont
from logging import  getLogger, INFO, DEBUG, handlers, Formatter, StreamHandler


class SensorMonitorEnviroplus:

    SENSOR_LOG_FILE = 'ccs811.txt'
    LOG_FILE = '{script_dir}/logs/sensor_monitor_enviroplus.log'.format(
        script_dir = os.path.dirname(os.path.abspath(__file__))
    )

    font_size_small = 10
    font_size_large = 20
    font = ImageFont.truetype(UserFont, font_size_large)
    smallfont = ImageFont.truetype(UserFont, font_size_small)
    x_offset = 2
    y_offset = 2

    message = ""

    # The position of the top bar
    top_pos = 25

    # Create a values dict to store the data
    variables = ["temperature",
                "pressure",
                "humidity",
                "light",
                "Co2",
                "TVOC"]

    units = ["C",
            "hPa",
            "%",
            "Lux",
            "ppm",
            "ppb"]

    # Define your own warning limits
    # The limits definition follows the order of the variables array
    # Example limits explanation for temperature:
    # [4,18,28,35] means
    # [-273.15 .. 4] -> Dangerously Low
    # (4 .. 18]      -> Low
    # (18 .. 28]     -> Normal
    # (28 .. 35]     -> High
    # (35 .. MAX]    -> Dangerously High
    # DISCLAIMER: The limits provided here are just examples and come
    # with NO WARRANTY. The authors of this example code claim
    # NO RESPONSIBILITY if reliance on the following values or this
    # code in general leads to ANY DAMAGES or DEATH.
    limits = [[4, 18, 28, 35],
            [250, 650, 1013.25, 1015],
            [20, 30, 60, 70],
            [-1, -1, 30000, 100000],
            [-1, -1, 650, 1000],
            [-1, -1, 400, 1000]]

    statusCode = [["100000","100001","000000","100002","100003"], #Temparature
            ["200000","200001","000000","200002","200003"], #Pressure
            ["300000","300001","000000","300002","300003"], #Humidity
            ["000000","000000","000000","400002","400003"], #Light
            ["000000","000000","000000","500002","500003"], #Co2
            ["000000","000000","000000","600002","600003"]] #TVOC

    # RGB palette for values on the combined screen
    palette = [(0, 0, 255),           # Dangerously Low
            (0, 255, 255),         # Low
            (0, 255, 0),           # Normal
            (255, 255, 0),         # High
            (255, 0, 0)]           # Dangerously High

    values = {}
    def __init__(self):
        # BME280 temperature/pressure/humidity sensor
        self.bme280 = BME280()
        self.config = IoTEdgeConfig()
        # Create ST7735 LCD display class
        self.st7735 = ST7735.ST7735(
            port=0,
            cs=1,
            dc=9,
            backlight=12,
            rotation=270,
            spi_speed_hz=10000000
        )
        # Initialize display
        self.st7735.begin()
        self.init_logger()
        self.WIDTH = self.st7735.width
        self.HEIGHT = self.st7735.height
        # Set up canvas and font
        self.img = Image.new('RGB', (self.WIDTH, self.HEIGHT), color=(0, 0, 0))
        self.draw = ImageDraw.Draw(self.img)
        try:
            # Transitional fix for breaking change in LTR559
            from ltr559 import LTR559
            self.ltr559 = LTR559()
        except ImportError:
            import ltr559

    def init_logger(self):
        self._logger = getLogger(__class__.__name__)
        file_handler = handlers.RotatingFileHandler(self.LOG_FILE, maxBytes=100000, backupCount = 1)
        formatter = Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
        file_handler.setFormatter(formatter)
        file_handler.setLevel(INFO)
        self._logger.addHandler(file_handler)
        stream_handler = StreamHandler()
        stream_handler.setLevel(DEBUG)
        self._logger.addHandler(stream_handler)
        self._logger.setLevel(INFO)
        

    # Saves the data to be used in the graphs later and prints to the log
    def save_data(self,idx, data):
        variable = self.variables[idx]
        # Maintain length of list
        self.values[variable] = self.values[variable][1:] + [data]
        unit = self.units[idx]
        message = "{}: {:.1f} {}".format(variable[:4], data, unit)
        self._logger.info(message)


    # Displays all the text on the 0.96" LCD
    def display_everything(self):
        self.draw.rectangle((0, 0, self.WIDTH, self.HEIGHT), (0, 0, 0))
        column_count = 1
        row_count = (len(self.variables) / column_count)
        for i in range(len(self.variables)):
            variable = self.variables[i]
            data_value = self.values[variable][-1]
            unit = self.units[i]
            x = self.x_offset + ((self.WIDTH // column_count) * (i // row_count))
            y = self.y_offset + ((self.HEIGHT / row_count) * (i % row_count))
            message = "{}: {:.1f} {}".format(variable[:4], data_value, unit)
            lim = self.limits[i]
            rgb = self.palette[0]
            for j in range(len(lim)):
                if data_value > lim[j]:
                    rgb = self.palette[j + 1]
            self.draw.text((x, y), message, font=self.smallfont, fill=rgb)
        self.st7735.display(self.img)
    
    def write_sendorData(self):
        f = open(self.config.sensorInfoPath,'w')
        f.writelines('{\n')
        statusDic={}
        for i in range(len(self.variables)):
            variable = self.variables[i]
            data_value = self.values[variable][-1]
            f.writelines("    \"{}\" : {:.2f},\n".format(variable, data_value))
            lim = self.limits[i]
            code = self.statusCode[0]
            for j in range(len(lim)):
                if data_value > lim[j]:
                    code = self.statusCode[i][j+1]
            statusDic[code] = code
        f.writelines("    \"statusCode\" : {}\n".format("[\""+"\",\"".join(statusDic.keys())+"\"],"))
        isAlert = "false"
        if len(statusDic) > 1 and '000000' in statusDic:
            isAlert = "true"
        f.writelines("    \"isAlert\" : {}\n".format(isAlert))
        f.writelines('}\n')
        f.close()

    # Get the temperature of the CPU for compensation
    def get_cpu_temperature(self):
        process = Popen(['vcgencmd', 'measure_temp'], stdout=PIPE, universal_newlines=True)
        output, _error = process.communicate()
        return float(output[output.index('=') + 1:output.rindex("'")])


    def execute(self):
        self._logger.info("""combined.py - Displays readings from all of Enviro plus' sensors 

        Press Ctrl+C to exit!

        """)

        # Tuning factor for compensation. Decrease this number to adjust the
        # temperature down, and increase to adjust up
        factor = 2.25

        cpu_temps = [self.get_cpu_temperature()] * 5

        for v in self.variables:
            self.values[v] = [1] * self.WIDTH

        # The main loop
        try:
            while True:

                proximity = self.ltr559.get_proximity()
                
                # Everything on one screen
                cpu_temp = self.get_cpu_temperature()
                # Smooth out with some averaging to decrease jitter
                cpu_temps = cpu_temps[1:] + [cpu_temp]
                avg_cpu_temp = sum(cpu_temps) / float(len(cpu_temps))
                raw_temp = self.bme280.get_temperature()
                raw_data = raw_temp - ((avg_cpu_temp - raw_temp) / factor)
                self.save_data(0, raw_data)
                self.display_everything()
                raw_data = self.bme280.get_pressure()
                self.save_data(1, raw_data)
                self.display_everything()
                raw_data = self.bme280.get_humidity()
                self.save_data(2, raw_data)
                if proximity < 10:
                    raw_data = self.ltr559.get_lux()
                else:
                    raw_data = 1
                self.save_data(3, raw_data)
                self.display_everything()
                try:
                    f = open(os.path.join(os.path.dirname(self.config.sensorInfoPath),self.SENSOR_LOG_FILE))
                    data = f.read().split(',')
                    self.save_data(4, int(data[0]))
                    self.save_data(5, int(data[1]))
                    self.display_everything()
                except:
                    self._logger.warning("Failed to read CCS811")
                self.write_sendorData()
                sleep(self.config.interval / 1000)
        # Exit cleanly
        except KeyboardInterrupt:
            self.st7735.set_backlight(0)
            sys.exit(0)


if __name__ == '__main__':
    sensor_monitor_enviroplus = SensorMonitorEnviroplus()
    sensor_monitor_enviroplus.execute()