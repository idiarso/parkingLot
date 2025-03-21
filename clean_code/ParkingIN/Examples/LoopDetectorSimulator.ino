/*
 * Loop Detector Simulator untuk ParkingIN
 * 
 * File ini berisi kode ATmega (Arduino) untuk mensimulasikan loop detector
 * yang terhubung ke sistem ParkingIN melalui RS-232 (DB9).
 * 
 * PENTING: Perangkat Keras yang dibutuhkan:
 * - Arduino Uno/Nano atau board kompatibel ATmega lainnya
 * - Sensor IR atau koil induktif (opsional, untuk deteksi aktual)
 * - Tombol push button untuk mensimulasikan kendaraan (opsional)
 * - Kabel USB-to-Serial dengan konektor DB9
 * - MAX232 atau konverter level tegangan RS-232 (jika tidak menggunakan adapter USB-to-Serial)
 * 
 * Koneksi:
 * - Pin 2: Input untuk sensor/button (terhubung ke GND saat aktif)
 * - Pin 13: LED indikator (menyala saat kendaraan terdeteksi)
 * - TX (Pin 1): Terhubung ke RX pin DB9 (atau MAX232 jika digunakan)
 * - RX (Pin 0): Terhubung ke TX pin DB9 (atau MAX232 jika digunakan)
 * - GND: Terhubung ke GND pin DB9
 * 
 * Komunikasi RS-232:
 * - Baud Rate: 9600
 * - Data Bits: 8
 * - Stop Bits: 1
 * - Parity: None
 * - Flow Control: None
 */

// Pin untuk sensor atau push button
const int SENSOR_PIN = 2;  
// Pin untuk LED indikator
const int LED_PIN = 13;    

// Status kendaraan
bool vehiclePresent = false;
bool lastVehicleState = false;

// Status gerbang
bool gateOpen = false;

// Timer untuk anti-bouncing dan debouncing
unsigned long lastDebounceTime = 0;
unsigned long lastStatusChangeTime = 0;
const unsigned long DEBOUNCE_DELAY = 50;    // 50 ms untuk debouncing
const unsigned long MIN_STATUS_DELAY = 2000; // 2 detik minimal antara perubahan status
const unsigned long COMMAND_TIMEOUT = 1000;  // 1 detik timeout untuk respons perintah

// Timer untuk simulasi random
unsigned long lastSimTime = 0;
unsigned long simInterval = 10000; // 10 detik
const unsigned long MIN_VEHICLE_TIME = 5000;  // Minimal 5 detik kendaraan terdeteksi
const unsigned long MAX_VEHICLE_TIME = 10000; // Maksimal 10 detik kendaraan terdeteksi

// Mode simulator (1 = manual dengan button, 2 = otomatis random)
int simulatorMode = 2;  // Default: otomatis

// Buffer untuk perintah
String commandBuffer = "";
unsigned long lastCommandTime = 0;
bool waitingForResponse = false;

// Status error
bool serialError = false;

void setup() {
  // Inisialisasi serial dengan konfigurasi RS-232
  Serial.begin(9600);
  
  // Setup pin
  pinMode(SENSOR_PIN, INPUT_PULLUP);  // Input dengan pullup, aktif low
  pinMode(LED_PIN, OUTPUT);
  
  // Indikator startup
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
  delay(500);
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
  
  // Kirim pesan startup
  Serial.println("LOOP_DETECTOR: INITIALIZED");
  Serial.println("NO_VEHICLE");
}

void loop() {
  // Cek error serial
  if (Serial.available() == 0 && serialError) {
    Serial.println("ERROR: SERIAL_FAILURE");
    delay(1000); // Tunggu 1 detik sebelum mencoba lagi
    return;
  }
  
  // Cek timeout perintah
  if (waitingForResponse && (millis() - lastCommandTime > COMMAND_TIMEOUT)) {
    waitingForResponse = false;
    Serial.println("ERROR: COMMAND_TIMEOUT");
  }
  
  // Cek mode
  if (simulatorMode == 1) {
    // Mode manual (button)
    readSensor();
  } else {
    // Mode otomatis (random)
    randomSimulation();
  }
  
  // Baca perintah dari serial
  readCommands();
  
  // Update LED
  digitalWrite(LED_PIN, vehiclePresent);
}

// Baca status sensor (atau button)
void readSensor() {
  // Baca input (dibalik karena menggunakan INPUT_PULLUP)
  bool sensorState = !digitalRead(SENSOR_PIN);
  
  // Debouncing
  if (sensorState != lastVehicleState) {
    lastDebounceTime = millis();
  }
  
  // Jika status stabil untuk waktu debounce
  if ((millis() - lastDebounceTime) > DEBOUNCE_DELAY) {
    // Jika status berubah dan sudah melewati minimal delay
    if (sensorState != vehiclePresent && 
        (millis() - lastStatusChangeTime) > MIN_STATUS_DELAY) {
      
      vehiclePresent = sensorState;
      lastStatusChangeTime = millis();
      
      if (vehiclePresent) {
        Serial.println("VEHICLE_DETECTED");
      } else {
        Serial.println("NO_VEHICLE");
      }
    }
  }
  
  lastVehicleState = sensorState;
}

// Simulasi random untuk testing
void randomSimulation() {
  if (millis() - lastSimTime > simInterval) {
    lastSimTime = millis();
    
    // Generate random vehicle state (20% kemungkinan berubah)
    if (random(100) < 20 && (millis() - lastStatusChangeTime) > MIN_STATUS_DELAY) {
      vehiclePresent = !vehiclePresent;
      lastStatusChangeTime = millis();
      
      if (vehiclePresent) {
        Serial.println("VEHICLE_DETECTED");
        // Set random duration for vehicle presence
        simInterval = random(MIN_VEHICLE_TIME, MAX_VEHICLE_TIME);
      } else {
        Serial.println("NO_VEHICLE");
        // Reset interval ke nilai acak antara 5-15 detik
        simInterval = random(5000, 15000);
      }
    }
  }
}

// Baca perintah dari serial
void readCommands() {
  while (Serial.available()) {
    char c = Serial.read();
    
    // Akumulasi karakter sampai newline
    if (c != '\n' && c != '\r') {
      commandBuffer += c;
    }
    
    // Proses perintah saat menerima newline
    if (c == '\n' || c == '\r') {
      commandBuffer.trim();
      lastCommandTime = millis();
      
      if (commandBuffer == "OPEN_GATE") {
        // Buka gerbang
        gateOpen = true;
        Serial.println("GATE_STATUS: OPEN");
        waitingForResponse = false;
      } 
      else if (commandBuffer == "CLOSE_GATE") {
        // Tutup gerbang
        gateOpen = false;
        Serial.println("GATE_STATUS: CLOSED");
        waitingForResponse = false;
      }
      else if (commandBuffer == "GET_STATUS") {
        // Kirim status
        if (vehiclePresent) {
          Serial.println("VEHICLE_DETECTED");
        } else {
          Serial.println("NO_VEHICLE");
        }
        
        if (gateOpen) {
          Serial.println("GATE_STATUS: OPEN");
        } else {
          Serial.println("GATE_STATUS: CLOSED");
        }
        waitingForResponse = false;
      }
      else if (commandBuffer == "SET_MODE_MANUAL") {
        simulatorMode = 1;
        Serial.println("MODE: MANUAL (BUTTON)");
        waitingForResponse = false;
      }
      else if (commandBuffer == "SET_MODE_AUTO") {
        simulatorMode = 2;
        Serial.println("MODE: AUTO (RANDOM)");
        waitingForResponse = false;
      }
      
      // Clear buffer
      commandBuffer = "";
    }
  }
} 