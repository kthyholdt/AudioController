 /* Basic Raw HID Example
   Teensy can send/receive 64 byte packets with a
   dedicated program running on a PC or Mac.

   You must select Raw HID from the "Tools > USB Type" menu

   Optional: LEDs should be connected to pins 0-7,
   and analog signals to the analog inputs.

   This example code is in the public domain.
*/

#define POTS 4

const int LedPin = 5;
const int SwitchPin = 8;
const int PotPin1 = 0;
const int PotPin2 = 1;
const int PotPin3 = A2;
const int PotPin4 = A3;
const int PotPin5 = A4;
const int NoiseValue = 2;

float EMA_a = 0.6;

int ledState = 0;

int switchState;

int value1;
int EMA_Value1;
int oldValue1 = 0;
int potVal1;

int value2;
int EMA_Value2;
int oldValue2 = 0;
int potVal2;

int value3;
int EMA_Value3;
int oldValue3 = 0;
int potVal3;

int value4;
int EMA_Value4;
int oldValue4 = 0;
int potVal4;

int value5;
int EMA_Value5;
int oldValue5 = 0;
int potVal5;

int previousSwitchState;

// RawHID packets are always 64 bytes
byte buffer[64];
elapsedMillis msUntilNextSend;
unsigned int packetCount = 0;

int valueChanged = 1;

void setup() {
  Serial.begin(9600);
  Serial.println(("RawHID Example"));
  
  pinMode(LedPin, OUTPUT);
  pinMode(SwitchPin, INPUT);

  EMA_Value1 = analogRead(PotPin1);
  EMA_Value2 = analogRead(PotPin2);
  EMA_Value3 = analogRead(PotPin3);
  EMA_Value4 = analogRead(PotPin4);
  EMA_Value5 = analogRead(PotPin5);

  buffer[0] = 0xAB;
  buffer[1] = 0xCD;
  buffer[2] = 0;
  buffer[3] = 0;
  buffer[4] = 0;
  buffer[5] = 0;
  buffer[6] = 0;
  buffer[7] = 0;
}

int light = 1;
int requestValues = 0;

void loop() {
  int n;
  
  //n = RawHID.recv(buffer, 100); // 0 timeout = do not wait
//  if (n > 0) {
//////    light = (int)buffer[0];
//	requestValues = (int)buffer[1];
//  }

  // Serial.println(light);
    
  //digitalWrite(LedPin, light);

  // first 2 bytes are a signature
  switchState = digitalRead(SwitchPin);

  if (switchState == HIGH && previousSwitchState != switchState)
  {
    buffer[2] = 1;
    valueChanged = 1;
    Serial.println("Switch");
  }
  
  previousSwitchState = switchState;

  potVal1 = analogRead(PotPin1);
  EMA_Value1 = (EMA_a*potVal1) + ((1-EMA_a)*EMA_Value1);
  value1 = map(potVal1, 0, 1022, 0, 100);

  if (oldValue1 != value1)
  {
    oldValue1 = value1;
    valueChanged = 1;
    Serial.print("Value1 =");
    Serial.println(oldValue1);
  }
  
  buffer[3] = (byte)oldValue1;

#if(POTS > 1)
  potVal2 = analogRead(PotPin2);
  EMA_Value2 = (EMA_a*potVal2) + ((1-EMA_a)*EMA_Value2);
  value2 = map(potVal2, 0, 1022, 0, 100);

  if (oldValue2 != value2)
  {
    oldValue2 = value2;
    valueChanged = 1;
    Serial.print("Value2 =");
    Serial.println(oldValue2);
  }
  buffer[4] = (byte)oldValue2;
#endif

#if(POTS > 2)
  potVal3 = analogRead(PotPin3);
  EMA_Value3 = (EMA_a*potVal3) + ((1-EMA_a)*EMA_Value3);
  value3 = map(potVal3, 0, 1022, 0, 100);

  if (oldValue3 != value3)
  {
    oldValue3 = value3;
    valueChanged = 1;
    Serial.print("Value3 =");
    Serial.println(oldValue3);
  }
  buffer[5] = (byte)oldValue3;
#endif

#if(POTS > 3)
  potVal4 = analogRead(PotPin4);
  EMA_Value4 = (EMA_a*potVal4) + ((1-EMA_a)*EMA_Value4);
  value4 = map(potVal4, 0, 1022, 0, 100);

  if (oldValue4 != value4)
  {
    oldValue4 = value4;
    valueChanged = 1;
    Serial.print("Value4 =");
    Serial.println(oldValue4);
  }
  buffer[6] = (byte)oldValue4;
#endif

#if(POTS > 4)
  potVal5 = analogRead(PotPin5);
  EMA_Value5 = (EMA_a*potVal5) + ((1-EMA_a)*EMA_Value5);
  value5 = map(EMA_Value5, 0, 1023, 0, 100);

  if (oldValue5 != value5)
  {
    oldValue5 = value5;
    valueChanged = 1;
    Serial.print("Value5 =");
    Serial.println(oldValue5);
  }
  buffer[7] = (byte)oldValue5;
#endif
  // and put a count of packets sent at the end
  buffer[62] = highByte(packetCount);
  buffer[63] = lowByte(packetCount);

  if(valueChanged == 1 || requestValues == 1)
  {

	Serial.println("Transitting");
    Serial.print("Switch :");
    Serial.print(switchState);
    Serial.print(", 0 :");
    Serial.print(buffer[0]);
    Serial.print(", 1 :");
    Serial.print(buffer[1]);
    Serial.print(", 2 :");
    Serial.print(buffer[2]);
    Serial.print(", 3 :");
    Serial.print(buffer[3]);
    Serial.print(", 4 :");
    Serial.print(buffer[4]);
    Serial.print(", 5 :");
    Serial.print(buffer[5]);
    Serial.print(", 6 :");
    Serial.print(buffer[6]);
    Serial.print(", 7 :");
    Serial.print(buffer[7]);
    Serial.print(", 62 :");
    Serial.print(buffer[62]);
    Serial.print(", 63 :");
    Serial.println(buffer[63]);
    n = RawHID.send(buffer, 100);
    if (n > 0) {
      packetCount = packetCount + 1;
    } else {
      Serial.println(("Unable to transmit packet"));
    }

    requestValues = 0;
  }
  
  valueChanged = 0;
  // delay(50);

}
