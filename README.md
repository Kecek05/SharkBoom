# Shark'Boom

![Unity Version](https://img.shields.io/badge/Unity-6000.0.35f1-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Mobile-green.svg)
![Netcode](https://img.shields.io/badge/Netcode-Unity%20Netcode%20for%20GameObjects-orange.svg)

## 🌊 Welcome to Shark'Boom!

**Shark'Boom** is a thrilling 3D multiplayer 1v1 PvP turn-based mobile game built with **Unity 6**. Dive into oceanic battles where strategy meets physics-based mayhem!

---

## 🎮 Core Mechanics

### Turn-Based Combat System
- **1v1 PvP Battles**: Face off against another player in intense turn-based combat
- **Time-Limited Turns**: Each player has a set time to execute their move
- **Turn States**:
  - `Player1Playing` / `Player2Playing` - Active player's turn
  - `Player1Played` / `Player2Played` - Completed turn states
  - Turn transitions managed by `TurnManager`

### Drag-and-Shoot Physics
- **Intuitive Controls**: Drag to aim, release to launch weapons and items
- **Force Calculation**: Drag distance determines projectile force (min to max force)
- **Trajectory Prediction**: Real-time visual trajectory system
- **Angle & Power Indicators**: Visual feedback for aiming precision
- **Dynamic Camera Zoom**: Camera adjusts during drag for better aiming

### Health & Damage System
- **Body Part Multipliers**: Damage varies by hit location
  - **Head**: Maximum damage multiplier
  - **Body**: Standard damage multiplier
  - **Foot**: Reduced damage multiplier
- **Ragdoll Physics**: Players react realistically to impacts
- **Knockback System**: Items can push players around the arena
- **Health Synchronization**: Client and server health states stay in sync

---

## 🎯 Gameplay

### Game Flow
1. **Authentication**: Players authenticate via Anonymous, Unity, or Android auth
2. **Matchmaking**: Players are matched based on their "Pearls" (ELO-like score)
3. **Player Spawning**: Both players spawn into the arena
4. **Item Distribution**: Players receive random items from their inventory
5. **Turn-Based Combat**: Players alternate throwing items at each other
6. **Victory Condition**: Last player standing or highest health when timer expires

### Game States
The game progresses through several states managed by `GameStateManager`:
- `WaitingForPlayers` - Lobby state
- `SpawningPlayers` - Players joining the match
- `CalculatingResults` - Server calculating match setup
- `ShowingPlayersInfo` - Displaying player information
- `GameStarted` - Active gameplay
- `GameEnded` - Match conclusion

### Weapons & Items
Players can throw various items with unique properties:
- **Anchor**: Heavy projectile
- **Sword**: Melee-ranged weapon
- **Sea Star**: Multi-directional projectile
- **Harpoon**: Piercing weapon
- **Molotov**: Area damage over time
- **Bomb**: Explosive area damage
- **Jump**: Mobility item (player can jump)
- **Coconut**: Basic projectile
- **Barrel**: Heavy damage item
- **Banana**: Slippery projectile with special effects

Each item has:
- Custom animations (`ItemAnimationSO`)
- Damage properties (`DamageableSO`)
- Knockback effects (`KnockbackSO`)
- Particle effects and collision behavior

---

## 🌐 Networking Implementation

### Multi-Architecture Support
Shark'Boom supports multiple networking architectures:

#### 1. **Unity Relay (Peer-to-Peer)**
- Uses **Unity Relay Service** for NAT traversal
- Host-client architecture where one player acts as host
- Managed by `HostGameManager`
- **Features**:
  - Automatic relay allocation
  - Join code generation for matchmaking
  - Lobby system integration
  - Maximum 2 connections (1v1)

#### 2. **Dedicated Server (Unity Multiplay)**
- Full dedicated server support using **Unity Multiplay Hosting**
- Managed by `ServerGameManager`
- **Features**:
  - Authoritative server logic
  - Matchmaker payload handling
  - Health checks and server allocation
  - Player reconnection support
  - Match data persistence

### Matchmaking System
- **Skill-Based Matchmaking**: Players matched by "Pearls" rating
- **MatchplayMatchmaker**: Handles ticket creation and polling
- **Queue System**: Dedicated queues for different game modes
- **Ticket Lifecycle**:
  1. Create matchmaking ticket with player data
  2. Poll for match assignment
  3. Connect to assigned server/relay
  4. Handle timeouts and failures

### Lobby System
- **Unity Lobby Service** integration
- Public/Private lobby support
- Join code sharing
- Lobby heartbeat system (30-second intervals)
- Automatic cleanup on disconnect

---

## 🔧 Technical Implementations

### Client Prediction & Compensation
- **Local Health Tracking**: Clients maintain `localTargetHealth` for immediate feedback
- **Server Reconciliation**: Health values synchronized via `NetworkVariable`
- **Callback Synchronization**: `BaseItemThrowable.OnItemCallbackAction` ensures state consistency
- **Item Impact Prediction**: Clients predict item hits locally, server validates

### Ownership & Authority
- **Network Ownership**: Managed by `OwnershipHandler`
- **Server Authority**: Critical game state managed server-side
- **Client Authority**: Input and local predictions on client
- **Owner-Only Components**: Special components only active for the owner (e.g., `OwnerNetworkAnimator`)

### State Synchronization
- **Player State Machine**: Manages player states across network
- **Network Variables**: Used for synchronized game state
- **RPC System**: Server and client RPCs for remote procedure calls
- **Turn Synchronization**: Ensures both players see consistent turn states

### Service Locator Pattern
- **Dependency Injection**: Clean architecture using `ServiceLocator`
- **Global Managers**:
  - `GameStateManager` - Overall game flow
  - `TurnManager` - Turn management
  - `TimerManager` - Game and turn timers
  - `PearlsManager` - Player score calculation
  - `AchievementManager` - Achievement tracking
  - `PlayersPublicInfoManager` - Player information sharing

### Network Security
- **Authentication Service**: Unity Authentication integration
- **Server Authentication**: `ServerAuthenticationService` validates players
- **Cloud Code**: Server-side logic execution for sensitive operations
- **Anti-Cheat**: Server-authoritative validation of all game actions

### Performance Optimizations
- **Adaptive Performance**: Unity Adaptive Performance package integration
- **Object Pooling**: Efficient reuse of frequently spawned objects
- **Network Bandwidth**: Optimized data structures for minimal bandwidth
- **Scene Loading**: Async scene loading with `Loader` system

---

## 🛠️ Technical Stack

### Unity & Packages
- **Unity Version**: 6000.0.35f1 (Unity 6)
- **Netcode for GameObjects**: Multiplayer networking
- **Unity Services**:
  - Relay
  - Lobby
  - Matchmaker
  - Authentication
  - Multiplay Hosting
- **Input System**: New Unity Input System
- **TextMeshPro**: UI text rendering
- **Cinemachine**: Camera system (if used)

### Third-Party Assets
- **Feel/Nice Vibrations**: Haptic feedback system
- **Adaptive Performance**: Performance optimization
- **Asset Inventory**: Asset management
- **Hot Reload**: Development tool for faster iteration

### Build Targets
- **Primary**: Android Mobile
- **Platform**: Mobile-optimized 3D graphics

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Network/           # All networking code
│   │   ├── Client/        # Client-side networking
│   │   ├── Server/        # Dedicated server code
│   │   ├── Host/          # Relay host code
│   │   └── ...            # Common networking
│   ├── Player/            # Player controllers, states, health
│   ├── Items/             # Weapon and item systems
│   ├── GlobalManagers/    # Game state, turns, timers
│   ├── UI/                # All UI components
│   ├── Camera/            # Camera management
│   ├── ServiceLocator/    # Dependency injection
│   └── UGSWrapper/        # Unity Gaming Services wrappers
├── Scenes/                # Game scenes
├── Prefabs/               # Reusable game objects
├── ScriptableObjects/     # Game data (items, audio, etc.)
└── ...
```

---

## 🚀 Getting Started

### Prerequisites
- Unity 6000.0.35f1 or newer
- Unity Gaming Services account
- Android SDK (for mobile builds)

### Setup
1. Clone the repository
2. Open project in Unity 6
3. Configure Unity Gaming Services in Project Settings
4. Set up Relay, Lobby, and Matchmaker services
5. Configure authentication methods
6. Build and deploy!

---

## 🎨 Features Highlight

### Visual & Audio
- **Ragdoll Physics**: Realistic character reactions
- **Particle Effects**: Item impacts and explosions
- **Audio System**: Dynamic sound effects (`AudioClipRefsSO`)
- **Haptic Feedback**: Mobile vibration support via Nice Vibrations

### Player Progression
- **Pearl System**: ELO-like ranking system
- **Achievement System**: Track player accomplishments
- **Save System**: Cloud save integration
- **Tutorial**: Guided tutorial for new players

### Quality of Life
- **Reconnection System**: Players can reconnect to ongoing matches
- **Debug Tools**: Comprehensive debugging UI and tools
- **FPS Counter**: Performance monitoring
- **Testing Fields**: Dedicated scenes for testing items and mechanics

---

## 📝 Game Modes

Currently supports:
- **1v1 Ranked**: Competitive matchmaking based on Pearls
- **Testing/Practice**: Non-networked mode for item testing

---

## 🔐 Security & Anti-Cheat

- **Server Authority**: All critical game logic runs on server/host
- **Validation**: Server validates all client actions
- **Authentication**: Secure player authentication
- **Cloud Code**: Sensitive logic executed server-side
- **Health Checks**: Dedicated server health monitoring

---

## 🤝 Contributing

This is a learning project showcasing modern Unity multiplayer architecture. Feel free to explore the codebase to learn about:
- Unity Netcode for GameObjects
- Unity Gaming Services (Relay, Lobby, Matchmaker)
- Dedicated server implementation
- Client prediction and lag compensation
- Service Locator pattern
- Turn-based multiplayer systems

---

## 🙏 Acknowledgments

- Unity Technologies for Unity Gaming Services
- More Mountains for Feel/Nice Vibrations
- Community contributors and testers

---

**Dive in and start your oceanic battle today! 🦈💥**
