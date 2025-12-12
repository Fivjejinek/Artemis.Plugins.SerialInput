// Arduino Uno/Mega sketch for Artemis
// Handshake: replies "Uno" or "Mega" on 0x01
// Reports pin changes automatically (no 0x02 required)
// Heartbeat: sends raw 0x03 every 10s
// Analog pins A0, A1, A2 configured as dials with frame averaging

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
  {0, "None"}, // A0
  {1, "None"}, // A1
  {2, "None"}, // A2
  {3, "None"}, {4, "None"}, {5, "None"},
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
unsigned long lastHeartbeat = 0;

// --------------------
// Frame averaging
// --------------------
const int avgFrames = 64; // configurable number of frames to average (default 4)
int analogSums[16];      // running sum of samples
int analogCounts[16];    // count of frames accumulated

void setup() {
  Serial.begin(115200);

  // Initialize digital pins
  for (auto &cfg : digitalPins) {
    if (strcmp(cfg.type, "Switch") == 0 || strcmp(cfg.type, "Button") == 0) {
      pinMode(cfg.pin, INPUT_PULLUP);
      lastDigital[cfg.pin] = digitalRead(cfg.pin);
    }
  }

  // Initialize analog pins
  for (auto &cfg : analogPins) {
    if (strcmp(cfg.type, "Dial") == 0) {
      int val = analogRead(cfg.pin);
      lastAnalog[cfg.pin] = val;
      analogSums[cfg.pin] = val;
      analogCounts[cfg.pin] = 1;
    }
  }
}

void loop() {
  // Handshake
  if (Serial.available() > 0) {
    int code = Serial.read();
    if (code == 0x01) {
#if defined(__AVR_ATmega2560__)
      Serial.println("Mega");
#else
      Serial.println("Uno");
#endif
    }
  }

  // Build frame of changed pins automatically
  String frame = "";

  // Digital first
  bool anyDigital = false;
  frame += "D:";
  for (auto &cfg : digitalPins) {
    if (strcmp(cfg.type, "Switch") == 0 || strcmp(cfg.type, "Button") == 0) {
      bool state = digitalRead(cfg.pin);
      if (state != lastDigital[cfg.pin]) {
        if (anyDigital) frame += ",";
        frame += String(cfg.pin) + "=" + (state ? "1" : "0");
        lastDigital[cfg.pin] = state;
        anyDigital = true;
      }
    }
  }

  // Analog next with frame averaging
  bool anyAnalog = false;
  frame += ";A:";
  for (auto &cfg : analogPins) {
    if (strcmp(cfg.type, "Dial") == 0) {
      int raw = analogRead(cfg.pin);
      analogSums[cfg.pin] += raw;
      analogCounts[cfg.pin]++;

      if (analogCounts[cfg.pin] >= avgFrames) {
        int avgVal = analogSums[cfg.pin] / analogCounts[cfg.pin];
        analogSums[cfg.pin] = 0;
        analogCounts[cfg.pin] = 0;

        if (avgVal != lastAnalog[cfg.pin]) {
          if (anyAnalog) frame += ",";
          frame += String(cfg.pin) + "=" + avgVal;
          lastAnalog[cfg.pin] = avgVal;
          anyAnalog = true;
        }
      }
    }
  }

  if (anyDigital || anyAnalog) {
    Serial.println(frame);
  }

  // Heartbeat every 10s
  if (millis() - lastHeartbeat >= 10000) {
    Serial.write((uint8_t)0x03);   // raw heartbeat byte
    lastHeartbeat = millis();
  }
}
