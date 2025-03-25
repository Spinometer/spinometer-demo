# Spinometer Project Brief

## Project Overview

Spinometer is a specialized application that detects faces from webcam footage, estimates distance and angle, and then predicts spine posture. The core of the application is built around a proprietary patented algorithm for estimating spine posture from distance and angle measurements, serving as a demonstration of this technology.

## Core Functionality

1. **Face Detection and Tracking**: Using neural network models to detect and track faces from webcam input
2. **Distance and Angle Estimation**: Calculating the distance between the user and the display, as well as the head tilting angle
3. **Spine Posture Prediction**: Applying a proprietary algorithm to estimate spinal alignment based on the measured distance and angle
4. **Real-time Visualization**: Displaying the estimated posture using a skeleton model and/or stick figure representation
5. **Calibration System**: Guided setup process to calibrate the system for accurate measurements

## Key Features

- Built with Unity and Sentis for neural network processing
- Real-time display of estimated distance and angle
- Visual representation of predicted spine posture using a skeleton model
- Multiple visualization modes (webcam only, skeleton only, stick figure, side-by-side, overlayed)
- Guided setup process for webcam selection and calibration
- Localization support (currently English and Japanese)
- Warning messages for tracking issues (lost tracking, unstable tracking, camera offline)

## Project Goals and Scope

The primary goal of the Spinometer is to provide users with real-time feedback on their spinal posture while using a computer or other display device. This helps users:

1. Become aware of poor posture habits
2. Make adjustments to improve spinal alignment
3. Reduce potential health issues related to poor posture

The current scope includes a standalone application for Windows, with future plans to:
- Enable background monitoring with alerts for poor alignment
- Implement data logging to local/remote storage
- Add historical data review functionality

## Target Audience

- Computer users concerned about posture and ergonomics
- Health professionals monitoring patient posture
- Ergonomics researchers and specialists
- Organizations promoting workplace health and wellness

## Project Status

This is an ongoing development project with a current focus on improving the UI and user experience, while maintaining the core functionality of spine posture estimation.
