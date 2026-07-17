# EventFlow Controller - Microservice-Based IoT Solution

A modern, modular IoT system built on .NET 10 that combines embedded device control (ESP32), cloud infrastructure, background processing, and cross-platform mobile applications.

**Repository:** [GitHub - microservice-based_IoT](https://github.com/S-iba/microservice-based_IoT)

---

## 📋 Solution Overview

This solution implements an event-driven microservice architecture for IoT device management. It features real-time communication between IoT devices, backend services, and client applications through a message-oriented approach using RabbitMQ and MassTransit.

### Architecture Pattern
- **Microservices**: API gateway and background worker services
- **Message-Driven**: Event-based communication via RabbitMQ
- **Cross-Platform**: Mobile apps (iOS, Android, macOS, Windows) via .NET MAUI
- **Embedded IoT**: Direct hardware control via ESP32 nanoFramework
- **Data Layer**: Entity Framework Core with SQL Server and PostgreSQL support

---

## 🏗️ Projects Overview

### 1. **IoT.Shared**
**Framework:** .NET 10 Class Library

Shared domain models and utilities used across the entire solution.

**Purpose:**
- Common data contracts and DTOs
- Shared business logic
- Cross-project utilities

**Dependencies:** None

---

### 2. **IoT.Api**
**Framework:** .NET 10 ASP.NET Core Web API  
**Port:** Configured via ASP.NET Core defaults

The central API gateway for the IoT ecosystem. Provides RESTful endpoints and orchestrates communication between devices, clients, and backend services.

**Key Features:**
- RESTful API endpoints for device control
- OpenAPI/Swagger documentation
- Message publishing via MassTransit
- Entity Framework Core with SQL Server and PostgreSQL support
- User Secrets for configuration management

**Dependencies:**
- MassTransit 8.3.6
- MassTransit.RabbitMQ 8.3.6
- Entity Framework Core 10.0.9 (SQL Server + PostgreSQL)
- Microsoft.AspNetCore.OpenApi 10.0.9
- IoT.Shared

**Configuration:**
- Database: SQL Server or PostgreSQL (configurable via EF Core)
- Message Broker: RabbitMQ (via MassTransit)

---

### 3. **IoT.Worker**
**Framework:** .NET 10 Worker Service

Background service that processes events and performs asynchronous tasks from the message queue.

**Key Features:**
- Background event processing
- Message consumption via MassTransit
- HTTP client for outbound calls
- Hosted service pattern
- User Secrets for secure configuration

**Dependencies:**
- MassTransit 8.3.6
- MassTransit.RabbitMQ 8.3.6
- Microsoft.Extensions.Hosting 10.0.9
- Microsoft.Extensions.Http 10.0.9
- Microsoft.Extensions.Configuration.UserSecrets 10.0.9
- IoT.Shared

**Configuration:**
- RabbitMQ connection details (via User Secrets)
- Queue names for event subscriptions

---

### 4. **IoT.App**
**Framework:** .NET 10 MAUI (Multi-platform App UI)  
**Target Platforms:**
- Android (API 21+)
- iOS (15.0+)
- macOS via Mac Catalyst (15.0+)
- Windows (10.0.17763.0+)

Cross-platform mobile application for controlling and monitoring IoT devices.

**Key Features:**
- Native iOS, Android, macOS, and Windows UI
- Real-time device status monitoring
- Command interface for device control
- XAML source generation for optimized compile time
- Responsive design across all platforms

**Architecture:**
- XAML-based UI with source code generation
- Platform-specific resource files
- Single-project structure for multi-platform deployment

---

### 5. **IoT.Esp32**
**Framework:** .NET nanoFramework  
**Target Device:** ESP32 microcontroller

Embedded firmware for the ESP32 IoT device. Provides direct hardware control with minimal resource footprint.

**Key Features:**
- Lightweight HTTP web server on onboard hardware
- GPIO control (LED demonstration)
- I2C LCD display driver (HD44780 via PCF8574)
- Wi-Fi connectivity with DHCP
- RESTful command interface for remote control

**Hardware:**
- **ESP32 Microcontroller**
- **I2C LCD Display** (16x2, via PCF8574 I2C expander)
  - SDA Pin: GPIO 21
  - SCL Pin: GPIO 22
  - I2C Address: 0x27 (or 0x3F if display shows blank)
- **Onboard LED** (Pin 2)

**API Endpoints:**
- `GET /control?action=FLASH_LED` - Flash LED 3 times
- `GET /control?action=TURN_ON` - Turn LED on
- `GET /control?action=TURN_OFF` - Turn LED off

**Response Format:**
```json
{
  "status": "success",
  "action": "FLASH_LED"
}
```

**Device Configuration:**
- Wi-Fi SSID and password (hardcoded for embedded device)
- HTTP server listens on port 80
- GPIO and I2C pins configured in `Program.cs`

---

## 🔄 Data Flow

```
┌──────────────┐
│   IoT.App    │  Mobile/Desktop Client
│   (MAUI)     │
└──────┬───────┘
	   │ HTTP/REST
	   ▼
┌──────────────┐
│   IoT.Api    │  API Gateway & Orchestrator
│  (Web API)   │
└──────┬───────┘
	   │ MassTransit/RabbitMQ
	   ▼
┌──────────────────────────────┬──────────────┐
│  Message Queue (RabbitMQ)    │              │
└──────────────────────────────┘              │
	   │                                       │
	   ▼                                       │
┌──────────────┐                              │
│  IoT.Worker  │  Background Processing       │
│  (Service)   │                              │
└──────────────┘                              │
	   │ HTTP                                  │
	   ▼                                       │
┌──────────────┐                              │
│  IoT.Esp32   │◄─────────────────────────────┘
│ (Embedded)   │  HTTP/REST Commands
└──────────────┘
	   │ GPIO Control
	   ▼
	Hardware
(LED, LCD, etc.)
```

---

## 🚀 Setup & Configuration

### Prerequisites
- .NET 10 SDK
- Visual Studio 2026 or later (or Visual Studio Code)
- RabbitMQ server running (for messaging)
- SQL Server or PostgreSQL (for data storage)
- ESP32 microcontroller with nanoFramework firmware installed

### Environment Setup

#### 1. **Configure User Secrets**

For **IoT.Api**:
```powershell
cd IoT.Api
dotnet user-secrets init
dotnet user-secrets set "RabbitMq:Host" "localhost"
dotnet user-secrets set "RabbitMq:Username" "guest"
dotnet user-secrets set "RabbitMq:Password" "guest"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=IoT;User Id=sa;Password=YourPassword;"
```

For **IoT.Worker**:
```powershell
cd IoT.Worker
dotnet user-secrets init
dotnet user-secrets set "RabbitMq:Host" "localhost"
dotnet user-secrets set "RabbitMq:Username" "guest"
dotnet user-secrets set "RabbitMq:Password" "guest"
```

#### 2. **Database Setup**

Create and migrate the database:
```powershell
cd IoT.Api
dotnet ef database update
```

#### 3. **RabbitMQ Setup**

Ensure RabbitMQ is running on `localhost:5672`:
```powershell
# Docker example
docker run -d --hostname rabbitmq --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Building the Solution

```powershell
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Or build specific projects
dotnet build IoT.Api
dotnet build IoT.Worker
dotnet build IoT.App
dotnet build IoT.Esp32
```

### Running the Solution

#### API Service
```powershell
cd IoT.Api
dotnet run
# Swagger UI available at https://localhost:7001/swagger
```

#### Worker Service
```powershell
cd IoT.Worker
dotnet run
```

#### Mobile App (MAUI)
```powershell
cd IoT.App
# Build for Android
dotnet build -f net10.0-android

# Build for iOS (macOS only)
dotnet build -f net10.0-ios

# Build for Windows
dotnet build -f net10.0-windows10.0.19041.0
```

#### ESP32 Firmware Deployment
1. Open `IoT.Esp32` in Visual Studio with nanoFramework tools installed
2. Connect ESP32 device via USB
3. Configure nanoFramework Device Manager
4. Deploy the project to the device

---

## 📡 API Endpoints

### Device Control

**Turn LED On**
```
GET /control?action=TURN_ON
```

**Turn LED Off**
```
GET /control?action=TURN_OFF
```

**Flash LED**
```
GET /control?action=FLASH_LED
```

**Response**
```json
{
  "status": "success",
  "action": "TURN_ON"
}
```

---

## 🔧 Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Language** | C# | 9.0+ |
| **Framework** | .NET | 10.0 |
| **Web Framework** | ASP.NET Core | 10.0.9 |
| **Mobile Framework** | .NET MAUI | Latest |
| **Embedded Platform** | nanoFramework | Latest |
| **Message Broker** | RabbitMQ | 3+ |
| **Message Library** | MassTransit | 8.3.6 |
| **ORM** | Entity Framework Core | 10.0.9 |
| **Databases** | SQL Server, PostgreSQL | Latest |

---

## 📦 NuGet Dependencies

### Shared Across Services
- **MassTransit** - Event bus and message orchestration
- **MassTransit.RabbitMQ** - RabbitMQ transport for MassTransit

### API-Specific
- Entity Framework Core (SQL Server & PostgreSQL)
- Microsoft.AspNetCore.OpenApi
- Microsoft.Extensions.Http

### Worker-Specific
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration.UserSecrets

---

## 🧪 Testing

The solution follows a modular architecture making each service easily testable. Unit tests should target:
- Business logic in `IoT.Shared`
- API endpoints in `IoT.Api`
- Event handling in `IoT.Worker`
- UI interactions in `IoT.App`

---

## 🔐 Security Considerations

- **User Secrets:** Sensitive configuration stored in User Secrets vault (development)
- **Environment Variables:** Use for production deployment
- **RabbitMQ:** Credentials configured via User Secrets or environment variables
- **ESP32:** Device deployed on trusted internal network only
- **HTTPS:** API enforces HTTPS in production

---

## 🤝 Contributing

1. Clone the repository
2. Create a feature branch
3. Make your changes
4. Test on the target platforms
5. Submit a pull request

---

## 📄 Project Structure

```
IoT.SharedSln/
├── IoT.Shared/                  # Shared models and utilities
│   └── IoT.Shared.csproj
├── IoT.Api/                     # ASP.NET Core API Gateway
│   └── IoT.Api.csproj
├── IoT.Worker/                  # Background Worker Service
│   └── IoT.Worker.csproj
├── IoT.App/                     # MAUI Cross-platform App
│   └── IoT.App.csproj
├── IoT.Esp32/                   # nanoFramework ESP32 Firmware
│   ├── Program.cs               # Main firmware code
│   ├── MinimalI2cLcd.cs         # LCD display driver
│   └── IoT.Esp32.nfproj
└── README.md                    # This file
```

---

## ⚙️ Troubleshooting

### ESP32 LCD Display Shows Blank
- Check I2C address (default 0x27, try 0x3F)
- Verify SDA (GPIO 21) and SCL (GPIO 22) are properly connected
- Check PCF8574 I2C expander power supply

### RabbitMQ Connection Failed
- Ensure RabbitMQ service is running
- Verify credentials in User Secrets
- Check firewall allows port 5672

### Database Migration Issues
```powershell
cd IoT.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 📞 Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/S-iba/microservice-based_IoT).

---

## 📝 License


---
  
**Version:** 1.0.0
