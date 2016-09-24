# woundvac_unity

Augmented reality medical device remote training tool for wound vacuum using Microsoft HoloLens, Oculus Rift, and Razer Hydra.

## Unity HoloLens Deployment Instructions

### In Unity Editor:
1. Make sure Main Camera's background is set to black because that's the default color used by HoloLens for transparency.
2. Open Project Settings -> Quality, under the down arrow below the windows store icon on top, check Fastest.
2. Open Player Settings, go to Other Settings, enable Virtual Reality Supported (if you have the correct Unity version for HoloLens you'll see the only Virtual Reality SDKs is Windows Holographic), go to Publishing Settings, in Capabilities, enable Microphone, Bluetooth and Spatial Perception.
2. Open Build Settings, make sure the Windows Store Platform is selected, set SDK to Universal 10, set UWP Build Type to D3D, set Build and Run to Local Machine, set Unity3d C# Projects to be checked.
3. Click on Build and Run

### In Visual Studio:
1. Open the built .sln file in Visual Studio.
2. Make sure to select the following configurations: Release (DEBUG compiles faster) | x86 | Remote Machine.
3. Once Remote Machine is chosen, a Remote Connections dialog shows up, enter your HoloLens' IP address (Find the IP address on the device under Settings > Network & Internet > Wi-Fi > Advanced Options.), Set Authentication Mode to be Unencrypted Protocol. Then press Select to establish a connection to your device.
4. If this is the first time you deploy, you will need to enter a pin. The pin should be shown on your device, but if it is not, go to Settings, Update, For Developers, Pair. Then enter the pin into the Visual Studio dialog.
5. Finally, select Debug / Start without Debugging.

### In HoloLens:
1. Simple run the newly deployed Unity app in HoloLens.

