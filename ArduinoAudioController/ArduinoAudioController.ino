const int LedPin = 3;
const int SwitchPin = 2;
const int PotPin1 = A0;
const int PotPin2 = A1;
const int PotPin3 = A2;
const int PotPin4 = A3;
const int PotPin5 = A4;

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

void setup() 
{
	pinMode(LedPin, OUTPUT);
	pinMode(SwitchPin, INPUT);
	Serial.begin(9600);
}

void loop() 
{
	char receiveVal;
	switchState = digitalRead(SwitchPin);

	if (switchState == HIGH && previousSwitchState != switchState)
	{
		Serial.println("M");
	}
	previousSwitchState = switchState;

	potVal1 = analogRead(PotPin1);
	value1 = map(potVal1, 0, 1023, 0, 100);

	if (oldValue1 != value1 && abs(previousPotVal1 - potVal1) > 1)
	{
		oldValue1 = value1;
		previousPotVal1 = potVal1;
		Serial.print("P1:");
		Serial.println(value1);
	}

	potVal2 = analogRead(PotPin2);
	value2 = map(potVal2, 0, 1023, 0, 100);

	if (oldValue2 != value2 && abs(previousPotVal2 - potVal2) > 1)
	{
		oldValue2 = value2;
		previousPotVal2 = potVal2;
		Serial.print("P2:");
		Serial.println(value2);
	}

	potVal3 = analogRead(PotPin3);
	value3 = map(potVal3, 0, 1023, 0, 100);

	if (oldValue3 != value3 && abs(previousPotVal3 - potVal3) > 1)
	{
		oldValue3 = value3;
		previousPotVal3 = potVal3;
		Serial.print("P3:");
		Serial.println(value3);
	}

	potVal4 = analogRead(PotPin4);
	value4 = map(potVal4, 0, 1023, 0, 100);

	if (oldValue4 != value4 && abs(previousPotVal4 - potVal4) > 1)
	{
		oldValue4 = value4;
		previousPotVal4 = potVal4;
		Serial.print("P4:");
		Serial.println(value4);
	}

	potVal5 = analogRead(PotPin5);
	value5 = map(potVal5, 0, 1023, 0, 100);

	if (oldValue5 != value5 && abs(previousPotVal5 - potVal5) > 1)
	{
		oldValue5 = value5;
		previousPotVal5 = potVal5;
		Serial.print("P5:");
		Serial.println(value5);
	}

	if (Serial.available() > 0)
	{
		receiveVal = Serial.read();

		if (receiveVal == 'G')
		{
			Serial.print("P1:");
			Serial.println(value1);
			Serial.print("P2:");
			Serial.println(value2);
			Serial.print("P3:");
			Serial.println(value3);
			Serial.print("P4:");
			Serial.println(value4);
			Serial.print("P5:");
			Serial.println(value5);
		}

		if (receiveVal == '1')
			ledState = 1;
		else  if (receiveVal == '0')
			ledState = 0;
	}

	digitalWrite(LedPin, ledState);
}
