# README #

[MetaSense Project](http://metasense.ucsd.edu)

### MetaSense App ###
This repository contains multiple apps for different platforms to control and receive data for the MetaSense node.

### How do I compile this project? ###

This code works with Visual Studio 2017. You need to have Xamarin installed.

You need to add Syncfusion packages to the NuGet sources. Then go to the NuGet manager and restore all the libraries used by the app.

To do this:
 
 * Right click on the solution in the solution explorer click Manage NuGet Packages for Solution… 
 * Click on the small gear on the right next to Package source: nugget.org
 * To be able to compile it you need to add the following library source.  
Name: Syncfusion  
Source: http://nuget.syncfusion.com/nuget_xamarin/nuget/getsyncfusionpackages/xamarin
 * Restore packages