// Arduino Uno sketch for Artemis handshake + pin change reporting
// - Responds "Uno" on 0x01
// - On 0x02, sends only changed pins
// - Digital changes first, then analog

// Track previous states
bool lastDigital[14];   // Uno has digital pins 0–13
int  lastAnalog[6];     // Uno has analog pins A0–A5

void setup() {
  Serial.begin(115200); // Match Artemis baud rate

  // Initialize caches
  for (int p = 0; p <= 13; p++) {
    pinMode(p, INPUT_PULLUP);   // or INPUT depending on your wiring
    lastDigital[p] = digitalRead(p);
  }
  for (int a = 0; a <= 5; a++) {
    lastAnalog[a] = analogRead(a);
  }
}

void loop() {
  if (Serial.available() > 0) {
    int code = Serial.read();

    // Handshake
    if (code == 0x01) {
      Serial.println("Uno");
    }

    // Report changes
    else if (code == 0x02) {
      String frame = "";

      // Digital changes first
      bool anyDigital = false;
      frame += "D:";
      for (int p = 2; p <= 13; p++) {   // skip pins 0/1 (serial RX/TX)
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
      for (int a = 0; a <= 5; a++) {
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
