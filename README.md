# traydesk
Small app to track share of up time of standing desk with the help of little Arduino contraption.

I figured that to use standing desk effectively, one has to track how much time they actually spend standing. I used chess clock for this purpose for a bit, but it had considerable issues: I would frequently forget to flip the clock while changing the height of the table, also I would leave the desk without stopping the clock all the time. 
So, I started looking for a better solution, and here it is. This application uses hardware ultrasonic sensor to report actual position of the table in the real time.
Time is only tracked when computer is unlocked, so only action really needed from the user is to pay attention to blinking tray icon and respond to it with lifting the table up.
Another limitation is that it currently expects to find only one Arduino module connected to the PC, but it will find it automatically, as port may wary between sessions.

# Hardware part
To make it work, you'll need Arduino microcontroller (cheapest Nano you'll find will do just fine) and HC-SR04 ultrasonic distance sensor (pretty cheap too). You'll also need USB cable to connect this thing to your computer. All together this setup will cost you probably quarter of what cheapest chess clock would.
1. It is expected that HC-SR04 VCC, TRIG, ECHO and GND pins are connected to D4, D5, D6 and D7 pins of Arduino module respectively. No more wiring needed.
2. Use cable to connect Arduino to computer and flash TrayDesk.ino sketch. You'll need Arduino Studio for this and may be driver for your Arduino. Google for Arduino tutorials if you are new to it, it's all straightforward anyway. Pay attention to COM port that was assigned to Arduino. After you've confirmed it is working, you can uninstall Arduino studio.
3. Attach the sensor under the top of your desk pointing downwards, somewhere where it will have clear sight down to the floor.

![image](https://user-images.githubusercontent.com/572940/139666355-ca6abfb5-3cb5-4555-b089-4a4f5b208dd0.png)

# Software part
There is no configuration window at the moment, so all setting to be done manually via editing of App.config
1. Reporting span: leave it as is. Has to be same value as in TrayDesk.ino. 
2. Height threshold (in cm): app will distinguish "up" and "down" positions based of distance from the sensor to the floor. Pick number that is in between your up and down positions, make sure to account for difference between sensor outer edge and top of the desk.
3. MinUpShare: desired share of up time, between 0 (can be always down) and 1 (not allowed to go down). Some articles I saw on Internet said it has to be somewhere around .33 (twice sitting as standing) to .5(same sitting as standing). Currently 0.4 works for me.
4. Daybreak: time when counters reset back to zero. For the case if you're working late hours, and usual midnight doesn't work for you that well.
5. DontWarnBefore: just in case you'd like to start your day with sitting session first, and don't get warnings right away.
