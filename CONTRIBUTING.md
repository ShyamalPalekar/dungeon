# Contributing to Dungeon Game

Welcome to the Dungeon Game project! We're excited to have you contribute to this AI-powered strategic adventure game. This guide will help you get started with contributing code, documentation, or ideas.

## 🎯 Project Overview

Dungeon Game is a strategic AI-powered adventure game that uses Q-Learning algorithms to create intelligent gameplay. The project is built with:

- **.NET 9** with WPF for the user interface
- **C#** for core game logic and AI implementation
- **Q-Learning Algorithm** for intelligent agent behavior
- **MVVM Pattern** for clean architecture
- **GitHub Actions** for CI/CD automation

## � Getting Started

### Prerequisites

Before contributing, ensure you have:

- **Visual Studio 2022** or **VS Code** with C# extension
- **.NET 9 SDK** or later
- **Git** for version control
- **Windows 10/11** for testing (WPF requirement)

### Setting Up Development Environment

1. **Fork the repository**
   ```bash
   # Click "Fork" on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/dungeon.git
   cd dungeon
   ```

2. **Set up upstream remote**
   ```bash
   git remote add upstream https://github.com/galihru/dungeon.git
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Build and test**
   ```bash
   dotnet build
   dotnet run
   ```

## 📝 Types of Contributions

### 🐛 Bug Reports
- Use GitHub Issues with the "bug" label
- Include detailed reproduction steps
- Provide system information (OS, .NET version)
- Include screenshots or error logs

### ✨ Feature Requests
- Use GitHub Issues with the "enhancement" label
- Clearly describe the proposed feature
- Explain the use case and benefits
- Consider AI/ML implications if applicable

### 🔧 Code Contributions
- AI algorithm improvements
- Game mechanics enhancements
- Performance optimizations
- UI/UX improvements
- Bug fixes and stability improvements

### 📚 Documentation
- Code documentation and comments
- README improvements
- API documentation
- Tutorial content
- Community guidelines


## � Development Workflow

### 1. Create a Branch
```bash
# Always create from main branch
git checkout main
git pull upstream main
git checkout -b feature/your-feature-name
```

### 2. Branch Naming Convention
- `feature/description` - New features
- `fix/issue-number` - Bug fixes
- `docs/description` - Documentation updates
- `ai/algorithm-name` - AI improvements
- `ui/component-name` - UI changes

### 3. Code Standards

#### C# Code Style
```csharp
// Use PascalCase for public members
public class GameEngine
{
    // Use camelCase for private fields
    private readonly IQLearningAgent _agent;
    
    // Use descriptive names
    public void UpdateGameState(GameState currentState)
    {
        // Clear, readable code with comments
        var bestMove = _agent.SelectOptimalMove(currentState);
        ApplyMove(bestMove);
    }
}
```

#### Code Quality Requirements
- **Null safety**: Use nullable reference types
- **Error handling**: Proper try-catch blocks
- **Logging**: Use structured logging
- **Documentation**: XML comments for public APIs
- **Testing**: Unit tests for new features

### 4. AI/ML Specific Guidelines

#### Q-Learning Implementation
```csharp
// Follow established patterns for AI components
public class QLearningAgent : IAgent
{
    // Use consistent parameter naming
    public Move SelectMove(State state, double epsilon = 0.1)
    {
        // Document algorithm decisions
        // Epsilon-greedy exploration strategy
        if (Random.NextDouble() < epsilon)
        {
            return SelectRandomMove(state);
        }
        
        return SelectOptimalMove(state);
    }
}
```

#### Performance Considerations
- Optimize training loops
- Implement efficient state representation
- Use appropriate data structures
- Profile memory usage for large Q-tables

## 🧪 Testing Guidelines

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=AI
dotnet test --filter Category=UI
```

### Test Structure
```csharp
[Test]
public void QLearningAgent_ShouldLearnOptimalPath()
{
    // Arrange
    var agent = new QLearningAgent();
    var environment = CreateTestEnvironment();
    
    // Act
    TrainAgent(agent, environment, episodes: 1000);
    
    // Assert
    var optimalPath = agent.FindOptimalPath(startState, goalState);
    Assert.That(optimalPath.Length, Is.LessThan(expectedMaxLength));
}
```

## 📋 Pull Request Process

### 1. Before Submitting
- [ ] Code builds without errors
- [ ] All tests pass
- [ ] Code follows style guidelines
- [ ] Documentation updated if needed
- [ ] Self-review completed

### 2. Pull Request Template
Your PR should include:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] AI/Algorithm improvement
- [ ] Documentation update
- [ ] Performance improvement

## AI/ML Impact (if applicable)
- [ ] Changes Q-Learning algorithm
- [ ] Affects training performance
- [ ] Modifies reward system
- [ ] Updates exploration strategy

## Testing
- [ ] Added new tests
- [ ] All existing tests pass
- [ ] Manually tested changes
- [ ] Performance benchmarked

## Screenshots (if UI changes)
[Add screenshots here]

## Checklist
- [ ] Code follows project standards
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
- [ ] Ready for review
```

### 3. Automated Validation
Our CI/CD pipeline automatically:
- ✅ Builds the project
- ✅ Runs all tests
- ✅ Checks code formatting
- ✅ Validates AI performance benchmarks
- ✅ Scans for security issues
- ✅ Auto-merges if all checks pass

## 🤖 AI Development Guidelines

### Algorithm Modifications
When working on Q-Learning or other AI components:

1. **Preserve backward compatibility** where possible
2. **Benchmark performance** before and after changes
3. **Document algorithm parameters** and their effects
4. **Test with various environments** and difficulty levels

### Training Data and Models
- Store training configurations in `configs/` directory
- Document hyperparameter choices
- Include performance metrics in commit messages
- Version control Q-table snapshots for major improvements

## 🎨 UI/UX Guidelines

### WPF Best Practices
- Follow MVVM pattern consistently
- Use data binding instead of direct manipulation
- Implement proper command patterns
- Ensure responsive design principles

### Accessibility
- Support keyboard navigation
- Use appropriate contrast ratios
- Include tooltips and help text
- Test with screen readers when possible

## 📊 Performance Guidelines

### Profiling and Optimization
```csharp
// Example: Optimize Q-table access
private readonly ConcurrentDictionary<State, ActionValues> _qTable = new();

public double GetQValue(State state, Action action)
{
    // Efficient lookup with default handling
    return _qTable.TryGetValue(state, out var values) 
        ? values[action] 
        : 0.0;
}
```

### Memory Management
- Dispose of resources properly
- Use object pooling for frequent allocations
- Monitor memory usage during long training sessions
- Implement efficient serialization for Q-tables

## 🛡️ Security Considerations

- Validate all user inputs
- Sanitize file paths and names
- Use secure random number generation
- Follow OWASP guidelines for any web components

## 🔄 Release Process

### Version Numbering
- **Major.Minor.Patch** (e.g., 1.2.3)
- Major: Breaking changes or major features
- Minor: New features, significant AI improvements
- Patch: Bug fixes, small improvements

### Release Checklist
- [ ] All tests pass
- [ ] Documentation updated
- [ ] Performance benchmarked
- [ ] Security scan completed
- [ ] Release notes prepared

## 🤝 Community Guidelines

### Code Reviews
- Be constructive and respectful
- Focus on code quality and maintainability
- Suggest alternatives when identifying issues
- Recognize good practices and improvements

### Communication
- Use GitHub Discussions for design questions
- Create issues for bugs and feature requests
- Join community channels for real-time collaboration
- Be patient with newcomers and offer help

## 📚 Resources

### Learning Materials
- [Q-Learning Tutorial](https://example.com/qlearning)
- [WPF MVVM Guide](https://example.com/wpf-mvvm)
- [.NET Performance Tips](https://example.com/dotnet-perf)
- [C# Coding Standards](https://example.com/csharp-standards)

### Development Tools
- **Visual Studio Community** (free)
- **Git Extensions** for Git GUI
- **dotMemory** for memory profiling
- **BenchmarkDotNet** for performance testing

## 🙏 Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes for significant contributions
- GitHub contributors page
- Community showcase for outstanding work

## 📞 Getting Help

- **GitHub Issues**: Technical problems or bugs
- **GitHub Discussions**: General questions and ideas
- **Community Chat**: Real-time help and collaboration
- **Maintainer Contact**: Direct communication for sensitive issues

---

## 🎯 Quick Start Checklist

Ready to contribute? Follow this checklist:

1. [ ] Read and understand the Code of Conduct
2. [ ] Set up development environment
3. [ ] Pick an issue or feature to work on
4. [ ] Create a feature branch
5. [ ] Make your changes following our guidelines
6. [ ] Write tests for your changes
7. [ ] Submit a pull request
8. [ ] Respond to review feedback
9. [ ] Celebrate when merged! 🎉

**Thank you for contributing to Dungeon Game!** Your help makes this project better for everyone in our community.

---

*For questions about contributing, please open a GitHub Discussion or contact the maintainers.*

**Project Lead**: GALIH RIDHO UTOMO  
**License**: Proprietary  
**Last Updated**: August 2025

### AI and Machine Learning
- [Q-Learning Tutorial](https://en.wikipedia.org/wiki/Q-learning)
- [Reinforcement Learning: An Introduction](http://incompleteideas.net/book/the-book.html)
- [Deep Reinforcement Learning](https://arxiv.org/abs/1312.5602)

### Game Development
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [C# Game Programming](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)

## 📞 Getting Help

- **GitHub Discussions** - For general questions and community support
- **GitHub Issues** - For bug reports and feature requests  
- **Code Comments** - For specific implementation questions
- **Documentation** - Check README.md and project wiki

---

**Happy Contributing!** 🎮🤖

Together, we're building the future of AI-powered gaming education and entertainment.

*Dungeon Game - Developed by GALIH RIDHO UTOMO*
