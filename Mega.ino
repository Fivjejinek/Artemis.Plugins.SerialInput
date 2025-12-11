void setup() {
  Serial.begin(115200);
  while (!Serial) { }
}

bool identified = false;

void loop() {
  while (Serial.available() > 0) {
    int code = Serial.read();

    if (code == 0x01) {
      Serial.println("Mega");
      identified = true; // allow re-identification anytime
    }
    else if (identified && code == 0x02) {
      Serial.print("D:");
      for (int p = 2; p <= 53; p++) {
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

      Serial.println(); // newline terminator
    }
  }
}
