// Arduino Mega sketch for Artemis handshake + pin change reporting
// - Responds "Mega" on 0x01
// - On 0x02, sends only changed pins
// - Digital changes first, then analog

// Track previous states
bool lastDigital[54];   // Mega has digital pins 0–53
int  lastAnalog[16];    // Mega has analog pins A0–A15

void setup() {
  Serial.begin(115200); // Match Artemis baud rate

  // Initialize caches
  for (int p = 0; p <= 53; p++) {
    pinMode(p, INPUT_PULLUP);   // or INPUT depending on your wiring
    lastDigital[p] = digitalRead(p);
  }
  for (int a = 0; a <= 15; a++) {
    lastAnalog[a] = analogRead(a);
  }
}

void loop() {
  if (Serial.available() > 0) {
    int code = Serial.read();

    // Handshake
    if (code == 0x01) {
      Serial.println("Mega");
    }

    // Report changes
    else if (code == 0x02) {
      String frame = "";

      // Digital changes first
      bool anyDigital = false;
      frame += "D:";
      for (int p = 2; p <= 53; p++) {   // skip pins 0/1 (serial RX/TX)
        bool state = digitalRead(p);
        if (state != lastDigital[p]) {
          if (anyDigital) frame += ",";
          frame += String(p) + "=" + (state ? "1" : "0");
          lastDigital[p] = state;
          anyDigital = true;
        }
      }

      // Analog changes second
      bool anyAnalog = false;
      frame += ";A:";
      for (int a = 0; a <= 15; a++) {
        int val = analogRead(a);
        if (abs(val - lastAnalog[a]) > 2) { // threshold to avoid noise
          if (anyAnalog) frame += ",";
          frame += String(a) + "=" + val;
          lastAnalog[a] = val;
          anyAnalog = true;
        }
      }

      // Only send if something changed
      if (anyDigital || anyAnalog) {
        Serial.println(frame);
      }
    }
  }
}
