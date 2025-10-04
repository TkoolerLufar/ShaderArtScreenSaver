[日本語で読む](README.ja.md)

UnityScreenSaver
================

Can I make a screen saver with Unity?

----

Files for Android
-----------------

For Android screen saver (known as Daydream but not VR platform),
I made these files:

- Assets/
  - Plugins/
    - Android/
      - AndroidManifest.xml
      - UnityPlayerDreamService.kt
      - UnityPlayerDream.kt

This Daydream can close itself by C# managed code. For example,
see **Quit()** method on **Assets/QuitTimerScript.cs**.

Files for Windows
-----------------

I made a screen saver launcher for Windows on ExternalPrograms/Windows/UnityScreenSaverLauncher.

How to build:

1. Prepare your Visual Studio with ".NET desktop development" feature.
2. Open this Unity project on Unity and build it in *Build/Windows*.
3. Open *ExternalPrograms/Windows/UnityScreenSaverLauncher/UnityScreenSaverLauncher.sln*.
4. **Publish** the solution. *Launcher.exe* will be put in *Build/Windows*.
5. Rename Launcher.exe "**(your Unity project name).scr**"
6. Complete!

When you use the solution with your Unity project, make sure:

- Put on a folder 3-level below the Unity project root.
  (for example: /ExternalPrograms/Windows/UnityScreenSaverLauncher/UnityScreenSaverLauncher.sln)
- There is a *project* in the solution.
  Change the project's name as same as the Unity Project's product name.

License
-------

MIT
