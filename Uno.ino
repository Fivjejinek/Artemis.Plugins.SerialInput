// Arduino Uno sketch
const int digitalPins[] = {2,3,4,5,6,7,8,9,10,11,12,13};
const int analogPins[]  = {A0, A1, A2, A3, A4, A5};

void setup() {
  Serial.begin(9600);
  for (int i = 0; i < 12; i++) pinMode(digitalPins[i], INPUT_PULLUP);
}

void loop() {
  Serial.print("D:");
  for (int i = 0; i < 12; i++) {
    int v = digitalRead(digitalPins[i]);
    Serial.print(digitalPins[i]);
    Serial.print('=');
    Serial.print(v == HIGH ? 1 : 0);
    if (i < 11) Serial.print(',');
  }

  Serial.print(';');

  Serial.print("A:");
  for (int i = 0; i < 6; i++) {
    int raw = analogRead(analogPins[i]);
    Serial.print(i);
    Serial.print('=');
    Serial.print(raw);
    if (i < 5) Serial.print(',');
  }

  Serial.println();
}
