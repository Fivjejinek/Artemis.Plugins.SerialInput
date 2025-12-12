// Unified Arduino Uno/Mega sketch for Artemis
// Handshake: replies "Uno" or "Mega" on 0x01
// On 0x02: reports only changed pins, prioritized digital then analog
// Pin configuration: all "None" by default

struct PinConfig {
  uint8_t pin;
  const char* type; // "None", "Switch", "Button", "Dial"
};

// --------------------
// Digital pin configs
// --------------------
PinConfig digitalPins[] = {
  {2, "None"}, {3, "None"}, {4, "None"}, {5, "None"}, {6, "None"},
  {7, "None"}, {8, "None"}, {9, "None"}, {10, "None"}, {11, "None"},
  {12, "None"}, {13, "None"},
#if defined(__AVR_ATmega2560__)
  // Mega extra pins 14–53
  {14, "None"}, {15, "None"}, {16, "None"}, {17, "None"}, {18, "None"},
  {19, "None"}, {20, "None"}, {21, "None"}, {22, "None"}, {23, "None"},
  {24, "None"}, {25, "None"}, {26, "None"}, {27, "None"}, {28, "None"},
  {29, "None"}, {30, "None"}, {31, "None"}, {32, "None"}, {33, "None"},
  {34, "None"}, {35, "None"}, {36, "None"}, {37, "None"}, {38, "None"},
  {39, "None"}, {40, "None"}, {41, "None"}, {42, "None"}, {43, "None"},
  {44, "None"}, {45, "None"}, {46, "None"}, {47, "None"}, {48, "None"},
  {49, "None"}, {50, "None"}, {51, "None"}, {52, "None"}, {53, "None"},
#endif
};

// --------------------
// Analog pin configs
// --------------------
PinConfig analogPins[] = {
  {0, "None"}, {1, "None"}, {2, "None"}, {3, "None"}, {4, "None"}, {5, "None"},
#if defined(__AVR_ATmega2560__)
  {6, "None"}, {7, "None"}, {8, "None"}, {9, "None"}, {10, "None"},
  {11, "None"}, {12, "None"}, {13, "None"}, {14, "None"}, {15, "None"},
#endif
};

// --------------------
// State caches
// --------------------
bool lastDigital[54];
int  lastAnalog[16];

void setup() {
  Serial.begin(115200);

  // Initialize digital pins
  for (auto &cfg : digitalPins) {
    if (strcmp(cfg.type, "None") != 0) {
      pinMode(cfg.pin, INPUT_PULLUP); // or INPUT depending on wiring
      lastDigital[cfg.pin] = digitalRead(cfg.pin);
    }
  }

  // Initialize analog pins
  for (auto &cfg : analogPins) {
    if (strcmp(cfg.type, "Dial") == 0) {
      lastAnalog[cfg.pin] = analogRead(cfg.pin);
    }
  }
}

void loop() {
  if (Serial.available() > 0) {
    int code = Serial.read();

    // Handshake
    if (code == 0x01) {
#if defined(__AVR_ATmega2560__)
      Serial.println("Mega");
#else
      Serial.println("Uno");
#endif
    }

    // Report changes
    else if (code == 0x02) {
      String frame = "";

      // Digital changes first
      bool anyDigital = false;
      frame += "D:";
      for (auto &cfg : digitalPins) {
        if (strcmp(cfg.type, "None") != 0) {
          bool state = digitalRead(cfg.pin);
          if (state != lastDigital[cfg.pin]) {
            if (anyDigital) frame += ",";
            frame += String(cfg.pin) + "=" + (state ? "1" : "0");
            lastDigital[cfg.pin] = state;
            anyDigital = true;
          }
        }
      }

      // Analog changes second
      bool anyAnalog = false;
      frame += ";A:";
      for (auto &cfg : analogPins) {
        if (strcmp(cfg.type, "Dial") == 0) {
          int val = analogRead(cfg.pin);
          if (abs(val - lastAnalog[cfg.pin]) > 2) { // threshold
            if (anyAnalog) frame += ",";
            frame += String(cfg.pin) + "=" + val;
            lastAnalog[cfg.pin] = val;
            anyAnalog = true;
          }
        }
      }

      // Only send if something changed
      if (anyDigital || anyAnalog) {
        Serial.println(frame);
      }
    }
  }
}
