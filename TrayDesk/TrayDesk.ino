#include "HCSR04.h"

#define VCC_PIN 4
#define TRIG_PIN 5
#define ECHO_PIN 6
#define GROUND_PIN 7
UltraSonicDistanceSensor sr04(TRIG_PIN, ECHO_PIN);

void setup()
{
   pinMode(VCC_PIN, OUTPUT);
   pinMode(GROUND_PIN, OUTPUT);
   digitalWrite(VCC_PIN, HIGH);
   digitalWrite(GROUND_PIN, LOW);
   Serial.begin(9600);
}

void loop()
{
   long a = sr04.measureDistanceCm();
   Serial.println(a);
   delay(1000);
}
