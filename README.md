VR Tunnelling Pro (VRTP) is the most advanced VR comfort solution for Unity 5.6+. It can be dropped in to almost any project for plug-and-play comfort options. It's developed by Sigtrap with support from Oculus, and supports all major VR platforms including
* Oculus Rift, Go and Quest
* HTC Vive
* Playstation VR
* Samsung GearVR
* Google Daydream

VRTP is designed not only as a plugin, but a platform for experimenting with and developing new comfort techniques. Familiarity with the source will allow developers to quickly implement new techniques.

Full documentation for the current release is available at [http://www.sigtrapgames.com/VrTunnellingPro/html](http://www.sigtrapgames.com/VrTunnellingPro/html). Check out the [Quickstart Guide](http://www.sigtrapgames.com/VrTunnellingPro/html/quickstart.html) to dive right in, or see our talk [Integrating Locomotion in Unity, Unreal, and Native Engines](https://www.youtube.com/watch?v=dBs65za8fhM) from Oculus Connect 5.

VRTP is also available on the Unity Asset Store [here](https://assetstore.unity.com/packages/tools/camera/vr-tunnelling-pro-106782).

## What is Tunnelling?
Much of VRTP's core is based on "Tunnelling", a technique to reduce sim-sickness in VR games, experiences and apps.

Artificial locomotion in VR - cars, spaceships, first-person "thumbstick" movement - causes sim-sickness in many users. This is a result of a mismatch between the motion they feel and the motion they see. Tunnelling is a highly effective method to reduce this for a large number of users.

It works by fading out peripheral vision. The subconscious uses peripheral vision heavily to interpret motion but the conscious brain largely ignores it - as such, reducing motion in the periphery combats the overall motion disparity users feel without significant information loss.

Additionally, the periphery can be replaced with static imagery to counteract motion even more strongly, "grounding" users in a static reference frame. This is called a "cage" as including a cage or grid maximises this effect.

![VR Tunnelling Pro](https://thumbs.gfycat.com/EntireSelfishBlackfootedferret-size_restricted.gif)

## Key Features
* Multiple modes
  * Color vignette
  * Replace periphery with skybox/cubemap cage
  * Replace periphery with customised full 3D cage
  * View VR scene through static "windows"
  * View cage through world-space portals
* Masking
  * Exclude objects from the tunnelling effect
    * e.g. static cockpit to help ground users
* Fully configurable
  * Tweak any settings in-editor or at runtime for full control
  * Preset system
    * Easily define multiple presets that users can switch between at runtime
  * Rich API
* Mobile-friendly version included
* Compatible with Multipass and Single Pass Stereo
* High performance

## Roadmap
We are working on multiple new techniques which are being developed on the *dev* branch. These include counter-optic flow, artificial tilt and more. For full details please see the Oculus Connect 5 talk linked above.

We are also working on the Unreal Engine 4 version which will be available on our Github and on the Marketplace.

## Documentation
HTML documentation (mirroring that at the official docs URL) can be generated using Doxygen. If you have Doxygen installed on Windows, you can just run `Docs~/BuildDoxygen.bat` to do so. The *Docs~* folder will be ignored by Unity, and the resulting HTML docs will be ignored by git.

## Credits
Developed and maintained by Dr Luke Thompson ([@six_ways](https://twitter.com/six_ways) | [github](https://github.com/SixWays))  
Research and support by Tom Heath on behalf of Oculus  
Art assets by Gary Lloyd ([@garylloyd89](https://twitter.com/garylloyd89))
