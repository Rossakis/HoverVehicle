# HoverVehicle

A simple Unity C# script that utilizes the Rigidbody component to enable the object that this script is attached to to hover. Attach this script to a GameObject in the Scene Hierarchy and add to it four empty Game Objects (you can call them "HoverPoint-Front Left", "HoverPoint- Front Right", "HoverPoint-Back Left" and "HoverPoint-Back Right") that will act as the Rigidbody's pillars through which AddForceAtPosition() is applied. 

NOTE: The four points should be in symmetrical distance from one another, or else the parent Game Object will fall to the side of the closest HoverPoint.

