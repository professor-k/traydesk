#include "SR04.h"

#define VCC_PIN 4
#define TRIG_PIN 5
#define ECHO_PIN 6
#define GROUND_PIN 7
SR04 sr04 = SR04(ECHO_PIN, TRIG_PIN);

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
   long a = sr04.Distance();
   Serial.println(a);
   delay(1000);
}
