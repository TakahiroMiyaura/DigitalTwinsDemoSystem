#!/usr/bin/env python
# Azure Digital Twins Demo - Copyright (c) Takahiro Miyaura 2020 - Licensed MIT

import RPi.GPIO as GPIO # RPi.GPIOモジュールを使用
from time import sleep
import subprocess

gpio_sw = 6

# GPIO番号指定の準備
GPIO.setmode(GPIO.BCM)
# LEDピンを出力に設定
GPIO.setup(gpio_sw, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)
execOn = False
execOff = False
while True:
    sw = GPIO.input(gpio_sw)
    if 0==sw:
        if not execOff:
            execOff = True
            execOn = False
            print("Off")
            subprocess.call(["systemctl", "stop", "AzureIoTHub.service"])

    # スイッチが離されていた場合(OFF)
    else:
        if not execOn:
            execOn = True
            execOff = False
            print("On")
            subprocess.call(["systemctl", "start", "AzureIoTHub.service"])

# 後処理 GPIOを解放
GPIO.cleanup(gpio_sw)