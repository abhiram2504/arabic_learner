# Arabic Learner - VR Cooking Experience

An immersive VR cooking learning application built with Unity, designed to teach Arabic cooking through interactive minigames and hands-on experiences.

## 🎮 Overview

This project is a virtual reality cooking simulation that guides users through Arabic cuisine preparation. Players learn to cook traditional dishes like hummus through interactive tasks including ingredient collection, cutting, blending, and timing challenges.

## ✨ Features

### Core Gameplay
- **Interactive Cooking Minigames**: Learn to prepare Arabic dishes step-by-step
- **Ingredient Collection System**: Collect and combine ingredients using VR interactions
- **Cutting Mechanics**: Practice cutting ingredients like garlic with proper technique
- **Blending System**: Use blender to combine ingredients
- **Timer-Based Challenges**: Complete tasks within time limits
- **Success/Failure Feedback**: Visual and audio feedback for player actions

### VR/XR Support
- **Meta Quest/XR Compatible**: Built with XR Interaction Toolkit
- **Hand Tracking**: Support for hand-based interactions
- **Controller Support**: Works with VR controllers
- **Immersive UI**: Screen-space overlays for timers and feedback

### Scene Management
- **Automatic Scene Transitions**: Seamless flow between learning scenes
- **Scene-Specific Narrations**: Audio instructions for each task
- **Fail Screen System**: Restart mechanism for failed attempts

## 🛠️ Technical Requirements

- **Unity Version**: 6000.0.62f1 (Unity 6)
- **XR Interaction Toolkit**: 3.0.9
- **Platform**: Meta Quest / Oculus VR (or compatible XR devices)
- **Rendering**: Built-in Render Pipeline

## 📁 Project Structure

### Scenes
- `learning_minigame_scene1.unity` - Main learning minigame scene (starts here)
- `Kitchen_scene.unity` - Kitchen environment scene
- `cutting_garlic.unity` - Garlic cutting practice scene
- `notkitchscene.unity` - Alternative scene
- `Fail_screen.unity` - Failure/restart screen

### Scripts (`Assets/Scripts/`)

#### Core Gameplay Scripts
- **`HummusBowlChecker.cs`** - Main game logic for hummus preparation
  - Checks ingredient completion
  - Manages timer (5-minute countdown)
  - Shows success (hummus bowl) or failure (fail canvas)
  - Auto-reloads scene on failure
  
- **`FoodItems.cs`** - Ingredient tracking system
  - Tracks solid ingredients (collision-based)
  - Tracks pourable ingredients (rotation-based)
  - Fires completion event when all items collected
  
- **`BlenderZone.cs`** - Blender interaction zone
  - Detects when ingredients enter blender
  - Plays blending sounds
  - Destroys ingredients on blend

#### Scene Management Scripts
- **`SceneTransition.cs`** - Automatic scene transitions
  - Transitions between scenes after time delay
  - Displays countdown timer
  - Configurable transition timing

- **`SceneNarrator.cs`** - Audio narration system
  - Plays instruction audio clips
  - Sequential narration support
  - Configurable delays and completion waits

#### Interaction Scripts
- **`CutMultipleTimes.cs`** - Cutting action detection
  - Tracks collision count for cutting actions
  - Fires events when cutting threshold reached

- **`TriggerOnCollision.cs`** - Generic collision trigger
  - Fires UnityEvents on collision
  - Reusable for various interactions

#### Audio Scripts
- **`AudioManager.cs`** - Master audio control
  - Volume and pitch management
  - UI integration for audio settings

- **`AudioUISetup.cs`** - Automatic audio UI creation
  - Creates sliders for volume/pitch control
  - Auto-configures audio UI elements

#### Utility Scripts
- **`ShowTarget.cs`** / **`HideTarget.cs`** - Show/hide GameObjects
- **`CompareTrigger.cs`** - Comparison-based triggers
- **`PourableObject.cs`** - Pourable ingredient behavior

## 🚀 Getting Started

### Setup Instructions

1. **Open Project in Unity**
   ```bash
   # Open Unity Hub
   # Select Unity 6000.0.62f1
   # Open project from: /path/to/arabic_learner
   ```

2. **Configure Build Settings**
   - File → Build Settings
   - Ensure scenes are in correct order:
     1. `learning_minigame_scene1.unity` (index 0)
     2. `Kitchen_scene.unity` (index 1)
   - Select Android platform for Quest
   - Configure XR settings for Meta Quest

3. **XR Setup**
   - Ensure XR Interaction Toolkit package is installed
   - Configure XR settings in Project Settings → XR Plug-in Management
   - Set up Oculus/Meta XR provider

### Running the Game

1. **Start Scene**: `learning_minigame_scene1.unity`
2. **Game Flow**:
   - Scene starts with 45-second timer
   - After 45 seconds, automatically transitions to `Kitchen_scene`
   - In kitchen scene, collect ingredients and blend them
   - Press XR button to check completion
   - Success: Hummus bowl appears
   - Failure: Fail canvas appears, scene reloads after 3 seconds

## 🎯 Gameplay Guide

### Hummus Preparation Minigame

1. **Timer Starts**: 5-minute countdown begins
2. **Collect Ingredients**: 
   - Pick up solid ingredients and place in blender
   - Pour pourable ingredients (rotate to pour)
3. **Check Completion**: Press XR button when done
4. **Result**:
   - ✅ **Success**: All ingredients collected → Hummus bowl appears
   - ❌ **Failure**: Missing ingredients or time expired → Fail screen → Scene reloads

### Scene Transition System

- **Auto-Transition**: `learning_minigame_scene1` → `Kitchen_scene` (after 45 seconds)
- **Timer Display**: Shows countdown in MM:SS format
- **Configurable**: Adjust timing in `SceneTransition` component

## 🔧 Script Configuration

### HummusBowlChecker Setup

1. Create empty GameObject
2. Add `HummusBowlChecker` component
3. Assign references:
   - **Food Items Script**: Drag blender GameObject with `FoodItems` component
   - **XR Button**: Drag button GameObject with `XR Simple Interactable`
   - **Hummus Bowl**: Drag hummus bowl GameObject
4. Configure settings:
   - Timer: Enable/disable, set time limit (default: 300 seconds)
   - Failure: Customize fail message, reload delay

### FoodItems Setup

1. Add `FoodItems` component to blender/dish GameObject
2. Assign ingredient lists:
   - **Solid Ingredients**: Drag all solid ingredient GameObjects
   - **Pour Ingredients**: Drag all pourable ingredient GameObjects
3. Set up pour zone:
   - Create Collider (trigger) above dish
   - Assign to "Pour Zone" field
4. Configure rotation threshold (default: 60 degrees)

### SceneTransition Setup

1. Add `SceneTransition` component to any GameObject
2. Set target scene name: `"Kitchen_scene"`
3. Set transition delay: `45` seconds
4. Timer UI will auto-create

## 🎨 Assets & Resources

### Included Assets
- **Fantasy Skybox FREE**: Skybox materials for environment
- **Arabic Skybox**: Custom Arabic-themed skybox material
- **Terrain Assets**: Terrain for Arabic-themed environment
- **Minigame Images**: Ingredient images (chaku, garlic, namak, numbu, oil, rajma, tahini)
- **Audio Files**: Narration and sound effects
- **Materials**: Custom materials for Arabic theme

### XR Samples
- XR Interaction Toolkit samples included
- Hand tracking demos
- Controller interaction examples

## 🐛 Troubleshooting

### Common Issues

**Timer not showing:**
- Check "Enable Timer" is checked in HummusBowlChecker
- Verify Canvas is created (check Hierarchy for "Timer Canvas")

**Button not working:**
- Ensure button has `XR Simple Interactable` component
- Check button is assigned in HummusBowlChecker component
- Verify XR Interaction Toolkit is installed

**Scene not transitioning:**
- Check scene name matches exactly (case-sensitive)
- Verify scene is in Build Settings
- Check "Transition On Start" is enabled

**Ingredients not detecting:**
- Ensure ingredients have Colliders
- Check FoodItems component has ingredients assigned
- Verify pour zone collider is set as Trigger

## 📝 Development Notes

### Branch Structure
- `main`: Stable release branch
- `dev_ar`: Active development branch

### Recent Features
- Hummus bowl checker with timer system
- Automatic scene transitions
- Arabic-themed environment (skybox, terrain)
- XR button integration
- Fail screen with auto-reload

## 🤝 Contributing

When contributing:
1. Create feature branch from `dev_ar`
2. Test in VR before committing
3. Update documentation for new features
4. Submit pull request to `dev_ar` branch

## 📄 License

[Add your license information here]

## 👥 Credits

- Unity XR Interaction Toolkit team
- Fantasy Skybox FREE asset creators
- Arabic learning content creators

## 🔮 Future Enhancements

- [ ] More Arabic dishes (baba ganoush, falafel, etc.)
- [ ] Multiplayer cooking challenges
- [ ] Arabic language learning integration
- [ ] Recipe book system
- [ ] Achievement system
- [ ] Advanced cutting mechanics
- [ ] More immersive Arabic environment

---

**Built with Unity 6 and XR Interaction Toolkit**

For questions or issues, please check the troubleshooting section or create an issue in the repository.

