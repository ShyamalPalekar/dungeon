# 🎮 Dungeon Game - AI-Powered Strategic Adventure

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%2010/11-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![AI](https://img.shields.io/badge/AI-Q--Learning-green.svg)

> **Strategic AI-powered dungeon exploration game with Q-Learning algorithms**

## 🎯 Overview

Dungeon Game adalah sebuah permainan strategi berbasis AI yang menggabungkan kecerdasan buatan Q-Learning dengan gameplay turn-based yang menantang. Pemain akan berhadapan dengan AI opponent yang semakin pintar seiring berjalannya permainan.

### ✨ Key Features

- **🧠 Advanced AI System**: Q-Learning algorithm yang adaptif
- **⚡ Multiple Difficulty Levels**: Beginner → Intermediate → Expert → Master
- **🎮 Turn-Based Combat**: Strategic gameplay dengan HP management
- **🎨 Modern UI**: Dark theme dengan animasi smooth
- **🔄 Auto-Update System**: GitHub integration untuk update otomatis
- **📖 In-Game Tutorial**: Comprehensive game rules dan help system

## 🚀 Quick Start

### 📥 Download & Install

1. **Download**: Unduh `DungeonGame-v1.0.0-Windows-x64.zip` dari [Releases](https://github.com/galihru/dungeon/releases)
2. **Extract**: Extract file ZIP ke folder pilihan Anda
3. **Run**: Jalankan `DungeonGame.exe`
4. **Play**: Nikmati pertarungan strategis melawan AI!

### 🎲 How to Play

1. **Choose Difficulty**: Pilih level AI opponent (Beginner - Master)
2. **Navigate Dungeon**: Bergerak Right (→) atau Down (↓) saja
3. **Strategic Movement**: HP berkurang saat melewati trap/monster
4. **Win Condition**: Sampai di pojok kanan bawah dengan HP > 0
5. **AI Challenge**: Lawan AI yang menggunakan Q-Learning untuk strategi optimal

## 🤖 AI System Details

### Q-Learning Implementation

- **State Space**: Posisi (r,c) di grid dungeon
- **Action Space**: Right atau Down movement
- **Reward System**: Berbasis HP tersisa dan completion
- **Exploration vs Exploitation**: Epsilon-greedy dengan decay

### Difficulty Levels

- **Beginner**: High exploration, random moves
- **Intermediate**: Balanced strategy
- **Expert**: Low exploration, calculated moves
- **Master**: Minimal exploration, optimal strategy

## 🔧 System Requirements

- **OS**: Windows 10/11 (x64)
- **Runtime**: .NET 9.0 (self-contained, included)
- **RAM**: 4GB minimum
- **Storage**: 100MB free space
- **Display**: 1024x768 minimum resolution

## 🎨 Screenshots

### Main Menu

Modern dark theme dengan navigation yang intuitif

### Gameplay

Turn-based strategic combat dengan AI opponent

### AI Training

Train AI dengan berbagai dungeon layouts

## 🏗️ Technical Architecture

### Built With

- **Framework**: .NET 9, WPF (Windows Presentation Foundation)
- **AI**: Custom Q-Learning implementation
- **UI**: Modern XAML styling dengan dark theme
- **Audio**: WAV-based sound system
- **Updates**: GitHub API integration untuk auto-updates

### Project Structure

```
DungeonGame/
├── 🧠 AI/                    # Q-Learning algorithms
├── 🎮 Windows/               # WPF UI components
├── 🎵 Services/              # Game engine, audio, updates
├── 🎨 Theme/                 # XAML styling
├── 🔧 Models/                # Game logic, configuration
└── 📦 Assets/                # Audio, images, resources
```

## 🔄 Auto-Update System

Game dilengkapi dengan sistem update otomatis yang:

- ✅ Check GitHub releases secara background
- ✅ Notifikasi user ketika ada versi baru
- ✅ One-click download dan installation
- ✅ Seamless update experience

## 🛠️ Development

### Building from Source

```bash
git clone https://github.com/galihru/dungeon.git
cd dungeon
dotnet restore
dotnet build --configuration Release
dotnet run
```

### Creating Distribution

```bash
# Simple build
.\build-installer.bat

# Advanced build with GitHub integration
.\build-deploy.ps1 -Version "1.0.0" -CreateRelease
```

## 📈 Roadmap

### Version 1.1.0 (Planned)

- [ ] Multiplayer online support
- [ ] More dungeon layouts
- [ ] Achievement system
- [ ] Leaderboard integration

### Version 1.2.0 (Future)

- [ ] Custom dungeon editor
- [ ] AI vs AI tournament mode
- [ ] Steam integration
- [ ] Mobile version (Xamarin)

## 🤝 Contributing

Contributions welcome! Areas yang bisa di-improve:

- **AI Algorithm**: Enhance Q-Learning parameters
- **UI/UX**: New themes, animations, accessibility
- **Game Features**: New game modes, mechanics
- **Performance**: Optimization, memory management

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 👨‍💻 Developer

**GALIH RIDHO UTOMO**

- 📧 GitHub: [@galihru](https://github.com/galihru)
- 🎮 Repository: <https://github.com/galihru/dungeon>
- 💼 Professional Game Development & AI Implementation

---

## 🎉 Changelog

### v1.0.0 (2025-08-20)

- ✅ Initial release
- ✅ Q-Learning AI system
- ✅ Multiple difficulty levels
- ✅ Modern WPF interface
- ✅ Auto-update functionality
- ✅ Turn-based strategic gameplay

---

<div align="center">

**Made with ❤️ by GALIH RIDHO UTOMO**

[⬇️ Download Game](https://github.com/galihru/dungeon/releases) • [🐛 Report Bug](https://github.com/galihru/dungeon/issues) • [💡 Feature Request](https://github.com/galihru/dungeon/issues)

</div>
