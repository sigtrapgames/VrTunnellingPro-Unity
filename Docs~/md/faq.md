# FAQ / Known Issues
## Frequently Asked Questions
#### Is there a free version or demo?
A "lite" version is available free and open source [on our GitHub](https://github.com/SixWays/UnityVrTunnelling). This offers basic vignette and skybox functionality but not advanced features such as masking, blur, 3D cages, presets or a mobile-friendly version.

The lite version is also included in the excellent [VR Toolkit (version 3.3.0a)](https://github.com/thestonefox/VRTK/tree/release/3.3.0-alpha). Please note - VRTP is in no way affiliated with VR Toolkit.

#### Is this plugin compatible with VR Toolkit?
Yes! Just ensure that you don't use the version included with VRTK at the same time.

#### I have a question / feature request / bug report!
Ok, not a question, but sure. There are two ways to get in touch - either email us at <mailto:support-vrtp@sigtrapgames.com> or post on our [Unity forum thread](https://forum.unity.com/threads/upcoming-vr-tunnelling-pro-plug-and-play-vr-comfort.517537/).

Please note we cannot guarantee we'll implement every feature request or bug fix but we'll do our best!

## Known Issues
### General
- Single-pass stereo is not supported on mobile platforms (Go, GearVR, Daydream, Cardboard)
  - Mobile version still works in single-pass stereo on non-mobile platforms

### Unity 2017.3
- Some mobile devices show decreased performance with masking enabled

### Unity 2017.2
- Unity may display harmless error: 
  - *“Unsupported texture format 0x13”*

### Pre-2017.2
- Unity displays harmless error first time mask is turned on:
  - *“Assertion failed on expression: 'depthSurface == NULL || rcolorZero->backBuffer == depthSurface->backBuffer'”*
