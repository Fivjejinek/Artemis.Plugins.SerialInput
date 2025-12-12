// Unified Arduino Uno/Mega sketch for Artemis push protocol
// Handshake: replies "Uno"/"Mega" on 0x01
// Pushes pin changes immediately, expects 0x02 from Artemis
// Sends heartbeat "ping" every 10s

struct PinConfig {
  uint8_t pin;
  const char* type; // "None", "Switch", "Button", "Dial"
};

// Digital pins: all None by default
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

// Analog pins: all None by default
PinConfig analogPins[] = {
  {0, "None"}, {1, "None"}, {2, "None"}, {3, "None"}, {4, "None"}, {5, "None"},
#if defined(__AVR_ATmega2560__)
  {6, "None"}, {7, "None"}, {8, "None"}, {9, "None"}, {10, "None"},
  {11, "None"}, {12, "None"}, {13, "None"}, {14, "None"}, {15, "None"},
#endif
};

bool lastDigital[54];
int  lastAnalog[16];
unsigned long lastHeartbeat = 0;

void setup() {
  Serial.begin(115200);

  for (auto &cfg : digitalPins) {
    if (strcmp(cfg.type, "None") != 0) {
      pinMode(cfg.pin, INPUT_PULLUP);
      lastDigital[cfg.pin] = digitalRead(cfg.pin);
    }
  }
  for (auto &cfg : analogPins) {
    if (strcmp(cfg.type, "Dial") == 0) {
      lastAnalog[cfg.pin] = analogRead(cfg.pin);
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
    // Artemis sends 0x02 after frames; we can ignore or use it to confirm
  }

  // Check digital pins
  String frame = "";
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

  // Check analog pins
  bool anyAnalog = false;
  frame += ";A:";
  for (auto &cfg : analogPins) {
    if (strcmp(cfg.type, "Dial") == 0) {
      int val = analogRead(cfg.pin);
      if (abs(val - lastAnalog[cfg.pin]) > 2) {
        if (anyAnalog) frame += ",";
        frame += String(cfg.pin) + "=" + val;
        lastAnalog[cfg.pin] = val;
        anyAnalog = true;
      }
    }
  }

  if (anyDigital || anyAnalog) {
    Serial.println(frame);
  }

  // Heartbeat every 10s
  if (millis() - lastHeartbeat >= 10000) {
    Serial.println("ping");
    lastHeartbeat = millis();
  }
}
