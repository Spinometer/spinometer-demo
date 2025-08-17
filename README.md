# Spinometer
## Spinal Alignment / Posture Estimator Based on Face-to-display Distance and Head Tilting Angle

This program detects faces from webcam footage, estimates distance and angle, and then predicts spine posture.  The algorithm for estimating spine posture from distance and angle is a proprietary patent, and this software serves as a demonstration of this patented technology.

![](./doc/ss-0.webp)

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Building from Source](#building-from-source)
- [License](#license)

## Features

- Made with Unity and Sentis
- Face detection from webcam feed
- Distance and angle estimation
- **Spine posture prediction** (patented algorithm)
- Real-time display of:
    - Estimated distance and angle (top left of the screen)
    - Predicted spine posture (skeleton on the right side) (with some assumptions ([see official web](https://www.get-back.jp/spinometer/#background)))

## Installation

A binary package is available for Windows.  Download and install it from [the distribution page](https://www.get-back.jp/spinometer).

## Usage

1. Upon launching, you'll see the initial setup screen.
2. Follow the setup process:
    - Select your webcam device from the dropdown and press "Next".
    - Set the relative angle between the webcam and your face (face the camera directly and press "Next").
    - Set the webcam's field of view (position your face 50 cm from the camera and press "Next").
3. After setup, the program will begin normal operation.
4. To redo the setup, press Settings in the bottom right corner.

Note: Setup information is saved in the registry. If you uninstall the program, this data will be preserved.  To remove it completely, use the Registry Editor.

## Building from Source

This is a [Unity](https://unity.com/) project.  At the time of writing, development is done using Unity Editor version 6000.2.0f1.

Following third-party assets/files are not included in this repository.  You will need download and place them separately.

- Head tracker models (from opentrack project) into `Assets/00_SpineMeter/Tracker/NeuralNet/Model/`
    - head-localizer: [https://github.com/opentrack/opentrack/raw/master/tracker-neuralnet/models/head-localizer.onnx](https://github.com/opentrack/opentrack/raw/master/tracker-neuralnet/models/head-localizer.onnx)
    - head-pose-0.2-small: [https://github.com/opentrack/opentrack/raw/master/tracker-neuralnet/models/head-pose-0.2-small.onnx](https://github.com/opentrack/opentrack/raw/master/tracker-neuralnet/models/head-pose-0.2-small.onnx)

  Note that `.meta` files are also needed and they are already included in this repo.  If you lose or overwritten them by an accident (e.g. by launching Unity Editor before placing the onnx files), restore them by `git checkout -f Assets/00_Spinometer/Tracker/NeuralNet/Model/head-{localizer,pose-0.2-small}.onnx.meta`.

- ALINE: [https://assetstore.unity.com/packages/tools/gui/aline-162772](https://assetstore.unity.com/packages/tools/gui/aline-162772)

- Skeleton 2.0: [https://assetstore.unity.com/packages/3d/characters/humanoids/skeleton-2-0-160089](https://assetstore.unity.com/packages/3d/characters/humanoids/skeleton-2-0-160089)

  You need to convert the BRP materials to URP in the Editor (Window > Rendering > Render Pipeline Converter).

- Final IK: [https://assetstore.unity.com/packages/p/final-ik-14290](https://assetstore.unity.com/packages/p/final-ik-14290)

- DOTween: [https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)<br/>
  or DOTween Pro: [https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416)

After placing the assets, open `Assets/00_Spinometer/00_App`.
Now you can run the application.

## Thanks

- [opentrack](https://github.com/opentrack/opentrack) authors and maintainers, as we adopted their code of neuralnet tracker module as a crucial part of Spinometer.

## License

The proprietary portions of this software (the implementation of the algorithm for estimating spine posture) are subject to separate license terms from the other parts.  See [https://github.com/Spinometer/spinometer-core/blob/main/LICENSE.md](https://github.com/Spinometer/spinometer-core/blob/main/LICENSE.md) for more information.

Other parts are licensed under [The ISC License](https://en.wikipedia.org/wiki/ISC_license).
