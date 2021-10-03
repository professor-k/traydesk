#include "SR04.h"

#define TRIG_PIN 12
#define ECHO_PIN 11
SR04 sr04 = SR04(ECHO_PIN, TRIG_PIN);

void setup()
{
   Serial.begin(9600);
}

void loop()
{
   long a = sr04.Distance();
   Serial.println(a);
   delay(1000);
}
