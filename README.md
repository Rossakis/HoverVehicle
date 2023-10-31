# HoverVehicle

A simple Unity C# script that utilizes the Rigidbody component to enable the object that this script is attached to to hover. 

Attach this script to a GameObject in the Scene Hierarchy, and create under it four empty GameObject children, which you can call for example:
- "_HoverPoint-FrontLeft_" 
- "_HoverPoint-FrontRight_"
- "_HoverPoint-BackLeft_"
- "_HoverPoint-BackRight_" 

They'll act as the Rigidbody's pillars through which AddForceAtPosition() is applied. 

**NOTE**: The four HoverPoint(s) should be symmetrical from one another in terms of distance, or else the parent Game Object will fall to the side of the closest to it HoverPoint.


https://github.com/Rossakis/HoverVehicle/assets/70864643/90314d4c-b2d0-46be-82f3-1311a7e1707d

