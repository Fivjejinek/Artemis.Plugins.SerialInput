// Arduino Mega sketch
void setup() {
  Serial.begin(9600);
  for (int p = 0; p <= 53; p++) pinMode(p, INPUT_PULLUP);
}

void loop() {
  Serial.print("D:");
  for (int p = 0; p <= 53; p++) {
    int v = digitalRead(p);
    Serial.print(p);
    Serial.print('=');
    Serial.print(v == HIGH ? 1 : 0);
    if (p < 53) Serial.print(',');
  }

  Serial.print(';');

  Serial.print("A:");
  for (int i = 0; i <= 15; i++) {
    int raw = analogRead(i);
    Serial.print(i);
    Serial.print('=');
    Serial.print(raw);
    if (i < 15) Serial.print(',');
  }

  Serial.println();
}
