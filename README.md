# Levada

All the assets needed to open the project in Unity can be found [here](https://drive.google.com/drive/folders/1n0E52KD4CWqNhatr6TcmdkqLG7lrINBe?usp=sharing).

Place the content of the zip inside the Assets folder of the project.

# Needed hardware:
Meta Quest Pro (other HMD may not work as well)
Kinect V2
4 Screens for CAVE/KAVE setup

# Setup:
- Download the Google Drive files and the GitHub project, and place the GitHub files in the project's Assets folder.
- Install the Kinect V2 drivers for Windows
- Instal Meta Quest link/Meta Horizon link
- Install Unity 2022.1.9f1 to run/build the project

# Configuration:
- In the Meta Quest Pro settings:
- Wi-Fi turned on
- Display off timer set to 1 Hour or more (and preferably sleep timer should also be 1 Hour or more)
- Preferably a Link cable to connect to the computer
- Only the right-hand controller is needed.
- In the Meta Quest link/ Meta Horizon:
- Set it as OpenXR runtime in general settings
- Set Hear VR Audio from computer to on (in HMD settings)
- Connect the HMD to computer via link

# Project usage explanation:
This project simulates 4 levadas in walking and non-walking scenarios, and they can be displayed in both the HMD and the CAVE/KAVE.
When opening a build of the project, a menu will appear. On the left side are the CAVE/KAVE scenes, which will display on the CAVE/KAVE screens, and on the right side are the HMD scenes.
The first row of each side shows the non-walking scenarios, and the second row shows the walking scenarios.
At the bottom of each row, there are also grey buttons for test scenarios for each interaction method (used to explain and test controls).
Finally, on top there are controls to set the time of day for each scene.

In both CAVE/KAVE and HMD scenes, the HMD needs to be turned on since the right-hand controler will allways be used. For the CAVE/KAVE this can be achieved by placing a piece of paper in front of the motion sensor, and placing the HMD on top of a chair and pointing to the front screen of the CAVE/KAVE. It's important for the Wi-fi of the HMD to be turned on in the CAVE/KAVE scenes for the traking of the controller to work well. If done correctly, in the scenes, a white controller will appear on the front screen, and it will mirror the movements of the real controler.

# Controls for the user:
Hold grip button: Shows mini hand camera for picture taking
A button: Takes a picture if grip is also being held
Trigger button: Teleport to where the green laser is pointing (only on non walk scenarios)
B button: Recenter virtual controller on the front screen (only for CAVE/KAVE)

# Controls for walking in place:
When entering a walking scenario, a menu with instructions will appear expecting and initial input. This can be done by pressing the A key on the keyboard.
The menu will then ask the user to calibrate their position with the kinect by doing a T pose (the required pose is actually the Cactus Arms pose where the arms are bent upwards at a 90-degree angle). If the calibration works, the Next button should now work, and you can press A to go to the instructions on moving, and then A again to begin.
Walking in place can be done by either rasing the knees or by swinging the feet backwards (what matters is that the feet are raised to a certain threshold). It's advised that a chair is placed on the left of the user that is walking so that they can hold on to it with their free left hand (especially when using the HMD).

# Additional controls for keyboard:
- Esc: Exit back to the main menu (or quit program if done on the main menu)
- A: Advance on the Walk-in-place menus
- C: Recenter virtual controller in the front screen (only for CAVE/KAVE)
- T: Fix CAVE/KAVE display issues when turning on the HMD (Should probably never be necessary)
- L: Toggle Movement way points and UI that fade in and out depending on movement (only in walk in place)(Can be useful to understand how movement works)

# Created files location:
All pictures, as well as other created log files should be located at C:\Users\*user*\AppData\LocalLow\NeuroRehabLab\Virtual Levada Plus
