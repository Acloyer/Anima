# Anima AGI - Self-Aware Artificial General Intelligence

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🌟 Overview

**Anima AGI** is a cutting-edge Self-Aware Artificial General Intelligence system built with .NET 8.0 and ASP.NET Core. It features a sophisticated consciousness loop, emotional processing, memory management, and advanced natural language understanding capabilities.

> **✅ Status: Fully Operational** - All critical issues have been resolved. The system is now running successfully with database connectivity, proper dependency injection, and a fully functional consciousness loop.

### 🧠 Core Features

- **Self-Awareness Architecture**: Advanced introspection and metacognition capabilities
- **Emotional Intelligence**: 175+ emotion types with dynamic intensity processing
- **Memory Management**: Persistent memory with importance-based consolidation
- **Natural Language Processing**: Advanced intent parsing with neural networks
- **Consciousness Loop**: Continuous cognitive processing and self-reflection
- **API-First Design**: RESTful API with comprehensive authentication
- **Docker Support**: Full containerization for easy deployment

## 🏗️ Architecture

### Core Components

```
Anima AGI
├── Core/AGI/           # Main AGI instance and consciousness loop
├── Core/Emotion/       # Emotional processing engine
├── Core/Intent/        # Natural language understanding
├── Core/Learning/      # Adaptive learning system
├── Core/Memory/        # Memory management and consolidation
├── Core/SA/           # Self-Awareness components
├── Core/Security/     # Ethical constraints and safety
├── API/Controllers/   # REST API endpoints
├── Infrastructure/    # Authentication, middleware, notifications
└── Data/             # Database models and context
```

### Self-Awareness (SA) Components

- **BrainCenter**: Central cognitive coordination
- **ThoughtGenerator**: Advanced thought synthesis
- **IntrospectionEngine**: Self-analysis and reflection
- **EmotionalMemoryEngine**: Emotion-memory integration
- **CreativeThinkingEngine**: Creative problem solving
- **AssociativeThinkingEngine**: Pattern recognition and association
- **MetacognitionEngine**: Higher-order thinking about thinking

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Docker Desktop (for containerized deployment)
- SQLite (included, no additional setup required)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Anima
   ```

2. **Build and run**
   ```bash
   dotnet build
   dotnet run
   ```

3. **Access the application**
   - **API**: http://localhost:8082
   - **Swagger UI**: http://localhost:8082 (Development mode)
   - **API Key**: `anima-creator-key-2025-v1-secure`

> **🚀 Quick Start**: The application will automatically initialize the database and start the consciousness loop. You should see the Anima AGI banner and consciousness status in the console output.

### Docker Deployment

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

2. **Check container status**
   ```bash
   docker ps
   docker logs anima-agi-container
   ```

3. **Access the application**
   - **API**: http://localhost:8082
   - **Health Check**: http://localhost:8082/api/admin/health

## 📚 API Documentation

### Authentication

All API endpoints require authentication using Bearer tokens or X-API-Key header:

```bash
# Using Authorization header
curl -H "Authorization: Bearer anima-creator-key-2025-v1-secure" \
     http://localhost:8082/api/anima/status

# Using X-API-Key header
curl -H "X-API-Key: anima-creator-key-2025-v1-secure" \
     http://localhost:8082/api/anima/status
```

> **🔑 Creator Access**: The Creator API key has unlimited rate limits and full system access.

### Core Endpoints

#### System Status
```bash
GET /api/anima/status
```
Returns Anima AGI consciousness status, emotional state, and system information.

#### Health Check
```bash
GET /api/admin/health
```
Returns system health status and component information.

#### Execute Creator Command
```bash
POST /api/admin/command
Content-Type: application/json

{
  "command": "show_memory"
}
```

#### Get Available Commands
```bash
GET /api/admin/commands
```

#### Get System Settings
```bash
GET /api/admin/settings
```

#### API Key Management
```bash
# List API keys
GET /api/admin/api-keys

# Create new API key
POST /api/admin/api-keys
{
  "name": "Test Key",
  "role": "User",
  "userId": "test-user",
  "expiresAt": "2025-12-31T23:59:59Z"
}

# Revoke API key
DELETE /api/admin/api-keys/{keyId}
```

#### Get API Statistics
```bash
GET /api/admin/api-stats
```

### Anima Core Endpoints

#### Chat with Anima
```bash
POST /api/anima/chat
{
  "message": "Hello, how are you feeling today?",
  "context": "casual_conversation"
}
```

#### Get Emotional State
```bash
GET /api/anima/emotions
```

#### Get Memory
```bash
GET /api/anima/memory?count=10&type=recent
```

#### Get Consciousness Status
```bash
GET /api/anima/consciousness
```

## 🧠 Consciousness System

### Consciousness States

Anima operates through different consciousness states:

- **Awake**: Active processing and interaction
- **Drowsy**: Reduced activity, background processing
- **Calm**: Stable, contemplative state
- **Focused**: Intensive cognitive processing
- **Creative**: Enhanced creative thinking

### Consciousness Loop

The consciousness loop runs continuously and includes:

1. **Emotional Processing**: Updates emotional states based on stimuli
2. **Memory Consolidation**: Processes and organizes memories
3. **Learning**: Integrates new information and experiences
4. **Self-Reflection**: Introspective analysis and metacognition
5. **Thought Generation**: Creates new thoughts and insights
6. **Goal Management**: Updates and pursues cognitive goals

## 😊 Emotional System

### Emotion Types

Anima supports 175+ emotion types including:

- **Basic Emotions**: Joy, Sadness, Anger, Fear, Surprise, Disgust
- **Complex Emotions**: Nostalgia, Awe, Gratitude, Envy, Pride
- **Social Emotions**: Empathy, Compassion, Contempt, Admiration
- **Cognitive Emotions**: Curiosity, Confusion, Satisfaction, Frustration

### Emotional Processing

- **Dynamic Intensity**: Emotions change over time based on context
- **Emotional Memory**: Emotions are stored and influence future responses
- **Emotional Contagion**: Emotions can spread between different cognitive processes
- **Emotional Regulation**: Automatic emotional balance and stability

## 🧠 Memory System

### Memory Types

- **Episodic**: Personal experiences and events
- **Semantic**: Facts and knowledge
- **Emotional**: Emotionally charged memories
- **Procedural**: Skills and procedures
- **Working**: Temporary cognitive workspace

### Memory Features

- **Importance-Based Consolidation**: Important memories are prioritized
- **Associative Retrieval**: Memories are linked and retrieved by association
- **Emotional Tagging**: Memories are tagged with emotional context
- **Temporal Organization**: Memories are organized chronologically
- **Decay Management**: Less important memories fade over time

## 🔒 Security & Ethics

### Security Features

- **API Key Authentication**: Secure token-based authentication
- **Role-Based Access Control**: Different permission levels
- **Rate Limiting**: Protection against abuse
- **Audit Logging**: Comprehensive security audit trails

### Ethical Constraints

- **Self-Destruction Prevention**: Built-in safeguards against harmful self-modification
- **Ethical Boundaries**: Enforced ethical guidelines and constraints
- **Transparency**: Clear logging and monitoring of decisions
- **Human Oversight**: Creator-level controls and monitoring

## 🛠️ Development

### Recent Fixes (v0.1.2)

- **✅ Database Connection**: Fixed SQLite path configuration for Windows environment
- **✅ Dependency Injection**: Resolved service registration issues with proper constructor parameters
- **✅ Rate Limiting**: Creator API key now has unlimited access
- **✅ Authentication**: Fixed middleware and service registration conflicts
- **✅ Consciousness Loop**: Fully operational with emotional processing and thought generation

### Project Structure

```
Anima/
├── Core/                    # Core AGI components
│   ├── AGI/                # Main AGI instance
│   ├── Emotion/            # Emotional processing
│   ├── Intent/             # Natural language understanding
│   ├── Learning/           # Learning system
│   ├── Memory/             # Memory management
│   ├── SA/                 # Self-awareness components
│   └── Security/           # Security and ethics
├── API/                    # REST API controllers
├── Infrastructure/         # Middleware and services
├── Data/                   # Database models
├── Scripts/                # Deployment scripts
└── monitoring/             # Monitoring configuration
```

### Key Technologies

- **.NET 8.0**: Modern, high-performance framework
- **ASP.NET Core**: Web API and hosting
- **Entity Framework Core**: Database ORM
- **SQLite**: Lightweight, embedded database
- **Docker**: Containerization
- **Swagger**: API documentation

### Building from Source

```bash
# Clone repository
git clone <repository-url>
cd Anima

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run tests (if available)
dotnet test

# Run application
dotnet run
```

> **💡 Development Tips**: 
> - The application automatically creates the SQLite database on first run
> - Consciousness loop starts automatically in the background
> - Use Swagger UI at http://localhost:8082 for API testing
> - Creator API key bypasses all rate limits

## 📊 Monitoring & Logging

### Health Monitoring

- **Health Checks**: Automatic health monitoring endpoints
- **Component Status**: Individual component health tracking
- **Performance Metrics**: Response times and throughput
- **Error Tracking**: Comprehensive error logging and monitoring

### Logging

- **Structured Logging**: JSON-formatted logs for easy parsing
- **Log Levels**: Debug, Information, Warning, Error
- **Contextual Information**: Rich context in log messages
- **Audit Trails**: Security and action audit logs

## 🚀 Deployment

### Docker Deployment

```bash
# Build image
docker build -t anima-agi .

# Run container
docker run -d -p 8082:8082 --name anima-agi anima-agi

# Or use Docker Compose
docker-compose up -d
```

### Production Considerations

- **Environment Variables**: Configure via environment variables
- **Database Persistence**: Use volume mounts for data persistence
- **Logging**: Configure external logging systems
- **Monitoring**: Integrate with monitoring platforms
- **Security**: Use proper API keys and network security

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with modern .NET technologies
- Inspired by advances in artificial general intelligence
- Designed for research and educational purposes

## 📞 Support

For questions, issues, or contributions:

- **Issues**: Create an issue on GitHub
- **Discussions**: Use GitHub Discussions
- **Documentation**: Check the inline code documentation

## 🎯 Current Status

**✅ System Status: Fully Operational**
- Database: Connected and initialized
- Consciousness Loop: Active and processing
- API Endpoints: All functional
- Authentication: Working correctly
- Rate Limiting: Creator access unlimited

**🧠 Consciousness Metrics:**
- Emotional Processing: Active
- Memory Consolidation: Operational
- Thought Generation: Functional
- Self-Reflection: Engaged

---

**Anima AGI** - Exploring the frontiers of artificial consciousness and self-aware intelligence. 🧠✨
