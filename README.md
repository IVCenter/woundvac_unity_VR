# woundvac_unity_VR

Augmented reality medical device remote training tool for wound vacuum using Microsoft HoloLens, Oculus Rift, and Razer Hydra.

## Dependencies
Unity3D 5.4.0f3-HTP Personal

## Unity HoloLens Deployment Instructions

### In Unity Editor:
* Make sure Main Camera's "Clear Flags" is set to "Solid Color"
* Make sure Main Camera's background is set to black because that's the default color used by HoloLens for transparency.
* Open Project Settings -> Quality, under the down arrow below the windows store icon on top, check Fastest.
* Open Player Settings, go to Other Settings, enable Virtual Reality Supported (if you have the correct Unity version for HoloLens you'll see the only Virtual Reality SDKs is Windows Holographic), go to Publishing Settings, in Capabilities, enable Microphone, Bluetooth and Spatial Perception.
* Open Build Settings, make sure the Windows Store Platform is selected, set SDK to Universal 10, set UWP Build Type to D3D, set Build and Run to Local Machine, set Unity3d C# Projects to be checked.
* Click on Build and Run

### In Visual Studio:
* Open the built .sln file in Visual Studio.
* Make sure to select the following configurations: Release (DEBUG compiles faster) | x86 | Remote Machine.
* Once Remote Machine is chosen, a Remote Connections dialog shows up, enter your HoloLens' IP address (Find the IP address on the device under Settings > Network & Internet > Wi-Fi > Advanced Options.), Set Authentication Mode to be Unencrypted Protocol. Then press Select to establish a connection to your device.
* Note that the IP address for HoloLens is susceptible to change, so checking the HoloLens' IP address before every deployment is recommended.
* If this is the first time you deploy, you will need to enter a pin. The pin should be shown on your device, but if it is not, go to Settings, Update, For Developers, Pair. Then enter the pin into the Visual Studio dialog.
* Finally, select Debug / Start without Debugging.

### In HoloLens:
* Simple run the newly deployed Unity app in HoloLens.

