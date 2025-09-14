# PowerTrade - Automated Power Trading Data Extraction System

A robust, enterprise-grade .NET 8 application that automatically extracts power trading data from external services and generates CSV reports on a scheduled basis.

## 🏗️ Architecture Overview

This project follows **Clean Architecture** principles with clear separation of concerns across multiple layers:

```
┌───────────────────┐
│   Pwr.App         │ ← Console Application (Entry Point)
├───────────────────┤
│ Pwr.Application   │ ← Business Logic & Use Cases
├───────────────────┤
│ Pwr.Infrastructure│ ← External Services & Data Access
├───────────────────┤
│   Pwr.Domain      │ ← Core Business Models
└───────────────────┘
```

## 🚀 Key Features

- **Scheduled Data Extraction**: Automated background service with configurable intervals
- **Robust Retry Mechanism**: Configurable retry logic to ensure no data is missed
- **DST-Safe Processing**: Handles Daylight Saving Time transitions correctly
- **CSV Report Generation**: Automated CSV file creation with proper formatting
- **Comprehensive Logging**: Structured logging with correlation IDs for monitoring
- **Thread-Safe Operations**: Safe concurrent execution with proper synchronization
- **Configuration-Driven**: All settings configurable via appsettings.json

## 📁 Project Structure

### **Pwr.App** (Console Application)
- **Purpose**: Application entry point and host configuration
- **Key Files**:
  - `Program.cs`: Host setup and service registration
  - `appsettings.json`: Configuration settings

### **Pwr.Application** (Business Logic Layer)
- **Purpose**: Contains business logic, use cases, and application services
- **Key Components**:
  - `ScheduledExtractService`: Main background service orchestrating the extraction process
  - `ExtractService`: Handles the core extraction logic
  - `RetryService`: Manages retry mechanisms with configurable strategies
  - `TimerService`: Thread-safe timer management for scheduling
  - `ExtractContext` & `ExtractResult`: Data models for extraction operations

### **Pwr.Infrastructure** (External Services Layer)
- **Purpose**: Handles external service integrations and data persistence
- **Key Components**:
  - `ForecastCallingService`: Integrates with external power trading API
  - `ExportService`: Generates CSV reports from extracted data
  - `CsvOptions`: Configuration for CSV generation

### **Pwr.Domain** (Core Business Models)
- **Purpose**: Contains core business entities and domain logic
- **Key Components**:
  - `InputItemDto` & `OutputItemDto`: Data transfer objects
  - `DateTimeExtensions`: Utility methods for date formatting

## 🔧 Design Decisions & Benefits

### **1. Clean Architecture Implementation**

**Choice**: Separated concerns into distinct layers with dependency inversion.

**Benefits**:
- **Testability**: Each layer can be tested independently
- **Maintainability**: Changes in one layer don't affect others
- **Flexibility**: Easy to swap implementations (e.g., different export formats)
- **SOLID Principles**: Follows Single Responsibility and Dependency Inversion principles

### **2. Background Service with Timer Management**

**Choice**: Implemented `ScheduledExtractService` as a `BackgroundService` with custom `TimerService`.

**Benefits**:
- **Reliability**: Built-in lifecycle management by .NET Host
- **Thread Safety**: Semaphore-based synchronization prevents overlapping executions
- **Configurable**: Dynamic interval updates without service restart
- **Observability**: Comprehensive logging for monitoring and debugging

### **3. Retry Service with Configurable Strategies**

**Choice**: Extracted retry logic into a dedicated `RetryService` with configurable parameters.

**Benefits**:
- **Reusability**: Can be used by any service requiring retry logic
- **Configurability**: Retry attempts and delays configurable via settings
- **Testability**: Easy to unit test retry scenarios
- **Maintainability**: Centralized retry logic reduces code duplication

### **4. DST-Safe Time Handling**

**Choice**: Simplified DST handling by working exclusively in UTC.

**Benefits**:
- **Simplicity**: No complex time zone conversion logic needed
- **Reliability**: UTC is DST-free, eliminating edge cases
- **Performance**: No time zone lookups or validations required
- **Maintainability**: Much simpler codebase (1 line vs 200+ lines of complex logic)

### **5. Structured Logging with Correlation IDs**

**Choice**: Implemented correlation IDs and structured logging throughout the application.

**Benefits**:
- **Traceability**: Track operations across multiple services
- **Debugging**: Easy to correlate logs from different components
- **Monitoring**: Better observability for production environments
- **Compliance**: Audit trail for data processing operations

### **6. Configuration-Driven Architecture**

**Choice**: All settings externalized to configuration files.

**Benefits**:
- **Flexibility**: Change behavior without code changes
- **Environment-Specific**: Different settings for dev/staging/prod
- **Runtime Updates**: Configuration changes picked up automatically
- **Security**: Sensitive settings can be externalized to secure stores

## 🛠️ Configuration

### **ExtractTradesOptions**
```json
{
  "ExtractTradesOptions": {
    "ExtractIntervalMinutes": 15,    // How often to run extraction
    "MaxRetryAttempts": 3,          // Number of retry attempts
    "RetryDelaySeconds": 30         // Delay between retries
  }
}
```

### **CsvOptions**
```json
{
  "CsvOptions": {
    "FolderPath": "C:\\CsvFiles\\"  // Output directory for CSV files
  }
}
```

## 🧪 Testing Strategy

### **Comprehensive Test Coverage**
- **100 Total Tests**: Comprehensive coverage across all layers
- **Unit Tests**: Each service tested in isolation
- **Edge Cases**: 35 tests covering year boundaries, leap years, DST transitions, and boundary values
- **Error Handling**: tests covering exception scenarios and failure modes

### **Test Categories**
- **Service Tests**: Individual service functionality with internal method access
- **Configuration Tests**: Settings validation and edge cases
- **Error Handling Tests**: Exception scenarios and retry mechanisms
- **DST Tests**: Time zone transition handling and UTC safety
- **Boundary Value Tests**: 15 tests covering min/max values and extreme inputs

### **Internal Method Testing**
The application uses `InternalsVisibleTo` attribute to allow the test project to access internal methods for comprehensive testing:
- `CreateExtractContext()`: Context creation and configuration
- `GetIntervalMilliseconds()`: Timer interval calculations
- `ExecuteExtractAsync()`: Core extraction execution
- `OnConfigurationChanged()`: Dynamic configuration updates

## 🚦 How It Works

### **1. Service Startup**
1. Application host starts and registers all services
2. `ScheduledExtractService` begins running as a background service
3. Timer is configured with the specified interval

### **2. Scheduled Execution**
1. Timer triggers at configured intervals
2. Service checks if previous execution is still running (prevents overlaps)
3. Creates `ExtractContext` with correlation ID and configuration
4. Calls `RetryService` to execute extraction with retry logic

### **3. Data Extraction Process**
1. `ExtractService` calls `ForecastCallingService` to get power trading data
2. Data is processed and aggregated by time periods
3. DST-safe time calculations ensure valid timestamps
4. `ExportService` generates CSV report from processed data

### **4. Error Handling & Retry**
1. Any failures trigger retry mechanism
2. Configurable number of retry attempts with delays
3. Comprehensive logging for monitoring and debugging
4. Critical failures logged for manual intervention

## 🔍 Key Benefits of This Architecture

### **Reliability**
- **No Missed Extracts**: Retry mechanism ensures data is never lost
- **Thread Safety**: Proper synchronization prevents data corruption
- **Error Recovery**: Graceful handling of failures with detailed logging

### **Maintainability**
- **Separation of Concerns**: Each service has a single responsibility
- **Dependency Injection**: Easy to swap implementations
- **Configuration-Driven**: Behavior changes without code changes

### **Scalability**
- **Background Service**: Runs independently of user interaction
- **Resource Efficient**: Proper disposal and memory management
- **Configurable**: Adjust intervals and retry behavior as needed

### **Observability**
- **Structured Logging**: Easy to parse and analyze logs
- **Correlation IDs**: Track operations across services
- **Comprehensive Coverage**: All operations logged with context

## 🚀 Getting Started

### **Prerequisites**
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Windows OS (for CSV file paths)

### **Running the Application**
1. Clone the repository
2. Navigate to `src/Pwr.App`
3. Run `dotnet run` or execute in Visual Studio
4. The background service will start automatically

### **Configuration**
1. Modify `appsettings.json` to adjust settings
2. Change `ExtractIntervalMinutes` to control frequency
3. Update `CsvOptions.FolderPath` for output location

## 📊 Monitoring & Logging

The application provides comprehensive logging at all levels:
- **Information**: Normal operation flow
- **Warning**: Non-critical issues (e.g., no data available)
- **Error**: Retryable failures
- **Critical**: Unrecoverable failures requiring manual intervention

All logs include correlation IDs for easy tracking across services.

## 🔧 Future Enhancements

Potential areas for future development:
- **Database Integration**: Store extraction history and metadata
- **Web API**: REST endpoints for manual triggers and monitoring
- **Health Checks**: Built-in health monitoring
- **Metrics**: Performance and usage metrics
- **Multiple Export Formats**: Support for Excel, JSON, etc.
- **Cloud Deployment**: Containerization and cloud deployment support

---

This architecture provides a solid foundation for a production-ready power trading data extraction system with room for future enhancements and scaling.
