 /* Basic Raw HID Example
   Teensy can send/receive 64 byte packets with a
   dedicated program running on a PC or Mac.

   You must select Raw HID from the "Tools > USB Type" menu

   Optional: LEDs should be connected to pins 0-7,
   and analog signals to the analog inputs.

   This example code is in the public domain.
*/

const int LedPin = 5;
const int SwitchPin = 7;
const int PotPin1 = 0;
const int PotPin2 = 1;
const int PotPin3 = A2;
const int PotPin4 = A3;
const int PotPin5 = A4;
const int NoiseValue = 2;

int ledState = 0;

int switchState;

int value1;
int oldValue1;
int potVal1;
int previousPotVal1;

int value2;
int oldValue2;
int potVal2;
int previousPotVal2;

int value3;
int oldValue3;
int potVal3;
int previousPotVal3;

int value4;
int oldValue4;
int potVal4;
int previousPotVal4;

int value5;
int oldValue5;
int potVal5;
int previousPotVal5;

int previousSwitchState;

// RawHID packets are always 64 bytes
byte buffer[64];
elapsedMillis msUntilNextSend;
unsigned int packetCount = 0;

int valueChanged = 1;

void setup() {
  Serial.begin(9600);
  Serial.println(F("RawHID Example"));
  
  pinMode(LedPin, OUTPUT);
  pinMode(SwitchPin, INPUT);
}

int light = 0;

void loop() {
  int n;
  
  n = RawHID.recv(buffer, 100); // 0 timeout = do not wait
  if (n > 0) {
    light = (int)buffer[0];
  }
    
  digitalWrite(LedPin, light);

  // first 2 bytes are a signature
  buffer[0] = 0xAB;
  buffer[1] = 0xCD;
  buffer[2] = 0;

  switchState = digitalRead(SwitchPin);

  if (switchState == HIGH && previousSwitchState != switchState)
  {
    buffer[2] = 1;
    valueChanged = 1;
  }
  
  previousSwitchState = switchState;

  potVal1 = analogRead(PotPin1);
  value1 = map(potVal1, 0, 1023, 0, 100);

  if (oldValue1 != value1 && abs(previousPotVal1 - potVal1) > NoiseValue)
  {
    oldValue1 = value1;
    previousPotVal1 = potVal1;
    valueChanged = 1;
  }
  
  buffer[3] = (byte)oldValue1;

  potVal2 = analogRead(PotPin2);
  value2 = map(potVal2, 0, 1023, 0, 100);

  if (oldValue2 != value2 && abs(previousPotVal2 - potVal2) > NoiseValue)
  {
    oldValue2 = value2;
    previousPotVal2 = potVal2;
    valueChanged = 1;
  }

  buffer[4] = (byte)oldValue2;

  potVal3 = analogRead(PotPin3);
  value3 = map(potVal3, 0, 1023, 0, 100);

  if (oldValue3 != value3 && abs(previousPotVal3 - potVal3) > NoiseValue)
  {
    oldValue3 = value3;
    previousPotVal3 = potVal3;
    valueChanged = 1;
  }
  
  buffer[5] = (byte)oldValue3;

  potVal4 = analogRead(PotPin4);
  value4 = map(potVal4, 0, 1023, 0, 100);

  if (oldValue4 != value4 && abs(previousPotVal4 - potVal4) > NoiseValue)
  {
    oldValue4 = value4;
    previousPotVal4 = potVal4;
    valueChanged = 1;
  }
  buffer[6] = (byte)oldValue4;

  potVal5 = analogRead(PotPin5);
  value5 = map(potVal5, 0, 1023, 0, 100);

  if (oldValue5 != value5 && abs(previousPotVal5 - potVal5) > NoiseValue)
  {
    oldValue5 = value5;
    previousPotVal5 = potVal5;
    valueChanged = 1;
  }
  buffer[7] = (byte)oldValue5;

  // and put a count of packets sent at the end
  buffer[62] = highByte(packetCount);
  buffer[63] = lowByte(packetCount);

  if(valueChanged == 1)
  {
    n = RawHID.send(buffer, 100);
    if (n > 0) {
      packetCount = packetCount + 1;
    } else {
      Serial.println(F("Unable to transmit packet"));
    }
  }
  
  valueChanged = 0;

}