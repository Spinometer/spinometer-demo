# Product Context

## Problem Statement

Poor posture while using computers and other display devices is a widespread issue that can lead to various health problems:

- **Neck and back pain**: Extended periods with the head tilted forward or downward can strain the neck and upper back muscles
- **Spinal misalignment**: Prolonged poor posture can lead to changes in spinal curvature
- **Reduced lung capacity**: Slouching compresses the chest cavity, potentially affecting breathing
- **Headaches**: Poor neck posture can contribute to tension headaches
- **Reduced concentration and productivity**: Discomfort from poor posture can impact focus and work efficiency

Despite these known issues, many people lack awareness of their posture while working, and there are limited tools available for real-time posture monitoring and feedback.

## Solution Approach

Spinometer addresses these problems through a novel approach:

1. **Non-invasive monitoring**: Uses standard webcam footage rather than requiring wearable sensors
2. **Face-to-display distance measurement**: Tracks how far the user is from their screen
3. **Head tilting angle detection**: Measures the angle of the user's head
4. **Proprietary algorithm**: Applies a patented method to estimate spine posture based on these measurements
5. **Real-time feedback**: Provides immediate visual representation of the user's estimated posture

This approach makes posture monitoring accessible, requiring only a webcam and no additional hardware, while providing valuable insights into spinal alignment during computer use.

## User Experience Flow

### Initial Setup

1. **Launch application**: User opens Spinometer for the first time
2. **Opening and disclaimer**: Brief introduction and legal disclaimers are displayed
3. **Guided setup process**:
   - Select webcam device from available options
   - Calibrate the relative angle between webcam and face (user faces camera directly)
   - Set webcam's field of view (user positions face 50cm from camera)
4. **Settings saved**: Configuration is stored for future sessions

### Normal Operation

1. **Face tracking**: System detects and tracks the user's face
2. **Measurement calculation**: Distance and angle are computed in real-time
3. **Posture estimation**: Spine posture is predicted using the proprietary algorithm
4. **Visual feedback**: User sees:
   - Current distance and angle measurements
   - Skeleton or stick figure representation of estimated spine posture
   - Warning messages if tracking is lost or unstable
5. **Visualization options**: User can switch between different display modes using keyboard shortcuts

### Settings Adjustment

- User can access settings at any time via the UI
- Options to recalibrate or adjust parameters
- Language selection (English/Japanese)

## Key Use Cases

### Individual Posture Awareness

- **User**: Office worker spending long hours at a computer
- **Goal**: Become aware of posture habits and make improvements
- **Usage pattern**: Runs application during work hours, glances at visualization periodically

### Ergonomic Assessment

- **User**: Workplace health specialist
- **Goal**: Evaluate employee workstation setups
- **Usage pattern**: Uses application during ergonomic assessments to visualize posture issues

### Health Monitoring

- **User**: Individual with existing neck/back issues
- **Goal**: Monitor posture to prevent pain exacerbation
- **Usage pattern**: Regular checks throughout the day, makes adjustments based on feedback

### Future: Long-term Monitoring

- **User**: Anyone concerned about posture health
- **Goal**: Track posture patterns over time
- **Usage pattern**: Application runs in background, logs data, and provides alerts for poor alignment

## Value Proposition

Spinometer offers unique value through:

1. **Accessibility**: Works with standard webcams, no special hardware required
2. **Visual feedback**: Intuitive representation of posture makes issues immediately apparent
3. **Proprietary technology**: Patented algorithm provides insights not available in other solutions
4. **Non-invasive**: No wearable sensors or physical attachments needed
5. **Real-time**: Immediate feedback allows for instant posture corrections
6. **Future expandability**: Planned features for background monitoring and historical data analysis will add long-term value

By making posture awareness simple and accessible, Spinometer helps users develop better habits and potentially reduce health issues related to poor posture during computer use.
