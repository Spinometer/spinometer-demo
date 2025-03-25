# Technical Context

## Technologies Used

### Core Platform

- **Unity**: Game engine used as the application framework
  - Version: 6000.0.36f1 (as noted in the README)
  - Universal Render Pipeline (URP) for graphics rendering

### Neural Network and AI

- **Unity Sentis**: Neural network inference engine for running ML models
  - Used for face detection and pose estimation
- **Pre-trained Neural Network Models** (from opentrack project):
  - head-localizer.onnx: For detecting faces in the webcam feed
  - head-pose-0.2-small.onnx: For estimating head position and orientation

### UI and Visualization

- **Unity UI Toolkit**: Modern UI system for creating user interfaces
  - Used for all UI elements across different scenes
- **DOTween**: Animation library for smooth transitions and effects
- **TextMesh Pro**: Advanced text rendering for UI elements
- **Localization Package**: For multi-language support (English and Japanese)

### 3D Models and Animation

- **Skeleton 2.0**: 3D skeleton model used for posture visualization
- **Final IK**: Inverse kinematics system for skeleton animation
- **ALINE**: Drawing library used for visualization of angles and measurements

### Data Management

- **PlayerPrefs**: Unity's built-in system for storing user preferences
  - Used for saving calibration data and settings between sessions
- **ScriptableObjects**: For configuration and shared data

## Development Setup

### Required Software

- Unity Editor version 6000.0.36f1
- Visual Studio or other C# IDE
- Git for version control

### Project Structure

- **Assets/00_Spinometer/**: Main project folder containing all application code
  - **App.cs**: Main application controller
  - **Settings.cs**: Settings management
  - **Tracker/**: Face tracking components
  - **SpinalAlignmentVisualizer/**: Visualization components
  - **UI/**: User interface elements
  - **Util/**: Utility functions and helpers
- **Assets/Settings/**: Unity URP configuration
- **Assets/Resources/**: Shared resources
- **Assets/TextMesh Pro/**: Text rendering assets
- **Assets/UI Toolkit/**: UI configuration

### Required Third-Party Assets

The following assets are required but not included in the repository:

1. **Head tracker models** (from opentrack project):
   - head-localizer.onnx
   - head-pose-0.2-small.onnx
   - Must be placed in `Assets/00_SpineMeter/Tracker/NeuralNet/Model/`

2. **ALINE**: Drawing library
   - Available from Unity Asset Store: [https://assetstore.unity.com/packages/tools/gui/aline-162772](https://assetstore.unity.com/packages/tools/gui/aline-162772)

3. **Skeleton 2.0**: 3D skeleton model
   - Available from Unity Asset Store: [https://assetstore.unity.com/packages/3d/characters/humanoids/skeleton-2-0-160089](https://assetstore.unity.com/packages/3d/characters/humanoids/skeleton-2-0-160089)
   - Requires conversion of BRP materials to URP in the Editor

4. **Final IK**: Inverse kinematics system
   - Available from Unity Asset Store: [https://assetstore.unity.com/packages/p/final-ik-14290](https://assetstore.unity.com/packages/p/final-ik-14290)

5. **DOTween**: Animation library
   - Available from Unity Asset Store: [https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
   - Or DOTween Pro: [https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416)

## Technical Constraints

### Performance Considerations

- **Frame Rate Management**: 
  - Target frame rate configurable in settings
  - VSync count adjusted based on screen refresh rate
  - Default target is 60 FPS

- **Neural Network Performance**:
  - Face detection frequency configurable (default: 15 times per second)
  - Optimizations to avoid running the localizer model every frame
  - EWA filtering for smoothing tracking results

### Hardware Requirements

- **Webcam**: Required for face tracking
- **Display**: Standard computer display
- **Processing Power**: Sufficient for neural network inference
  - Specific requirements not documented, but should run on modern computers capable of running Unity applications

### Platform Support

- Currently built for Windows
- Potential for cross-platform support given Unity's capabilities, but not currently implemented

## Dependencies and Integration

### External Dependencies

- **OpenTrack Project**: Source of neural network models for face tracking
  - Models are adapted from their neuralnet tracker module

### Internal Dependencies

The application has several internal dependencies between components:

- **TrackerNeuralNet** depends on:
  - WebCam for input
  - Settings for configuration
  - UiDataSource for output

- **SpinalAlignmentEstimator** depends on:
  - TrackerNeuralNet for distance and angle measurements
  - Settings for algorithm parameters

- **Visualization Components** depend on:
  - SpinalAlignmentEstimator for posture data
  - Settings for display preferences

## Future Technical Considerations

To support the planned features for background monitoring and data logging, the following technical aspects will need to be addressed:

1. **Background Processing**:
   - System for running the application as a background service
   - Minimizing resource usage when not in focus

2. **Data Storage**:
   - Database or file-based storage for posture history
   - Data compression for efficient storage of time-series data

3. **Notification System**:
   - Integration with OS notification systems
   - Configurable alert thresholds

4. **Data Analysis**:
   - Statistical analysis of posture data
   - Visualization of trends over time

5. **Potential Cloud Integration**:
   - Secure data transmission
   - Authentication and user management
   - Cross-device synchronization
