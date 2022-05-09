# Memory-Palace
This project is developed by Unity and based on CS464(Human Computer Interactive) research of AR Memory Palace. The main purpose of this project is to provide a mobile application(iOS) for user building up a memory palace and train their memory. It also collects data from users for Ar Memory Palace analysis.

### Install
The project is fully developed by Unity. 
* Make sure your Unity is latest version.
* For iOS, Xcode is necessary.
* Make sure your device is already connected before build and run.
1. Clone or download whole project to your local.
  * ***Assets*** folder contains all resources this project needed. 
  * ***Package*** folder contains all esential packages used in the project, such as AR Foundation and UIMaterial.
2. Open Unity, Open project
3. File -> Build setting, click on scene in the build, build and run
  * For iOS, make sure your provision profile in Xcode set collectly.

### ARMemory direction
The application named "ARMemory" should appear in your device.
1. For iOS, there should be a notification that the developer of the app isn't trusted on your device. 
  * Go to Settings > General >  Profiles or Profiles & Device Management. 
  * Under the "Enterprise App" heading, there is a profile for the developer.
  * Tap in and click Trust.
2. Turn on appliaction, there 10 objects with different content you can chose to place around the mapped area. 
  * Place objects and try to memorize.
  * Click on "Save" button to save map information.
  * Click on "Load" button to load map information you alrady saved.
  * Click on "Reset" button to reset the session.
  * Have fun for yourself!
