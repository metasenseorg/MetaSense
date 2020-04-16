# README #

[MetaSense Project](http://metasense.ucsd.edu)

# MetaSense Hardware #
This repository contains the hardware (electrical and mechanical enclosures) for the MetaSense Air Quality Sensing Platform.

## MetaSenseBoard ##
The electrical design for the MetaSense Air Quality Sensing Platform can be found here. A schematic can be found in each MetaSenseBoardDesign version folder along with gerbers that can be used for ordering and building PCBs.

If you want to modify or update the board files, the .brd and .sch files can be opened in Eagle.

### VOC Interface Board ###
The VOC interface board connects into the VOC and I2C Extension header (JP14). The board connects to two different VOC sensors:
* Mocon pID-TECH eVx: photoionization VOC sensor with 4-pin connection and analog output
* ams iAQ-core: MOS VOC sensor with I2C interface for TVOC equivalent measurement

## MetaSense Enclosure ##
The enclosures were developed in Autodesk Fusion. Both are meant to be used without the VOC interface board.

### Enclosure v1.3 ###
The standard enclosure for the MetaSense Air Quality Sensing Platform. The shell is split into top and bottom halves, which are connected by through bolts that both hold the board in place and secure the shell. Velcro straps can be run through the bottom to allow attachment to objects.

### Alternate Drone v1.3 ###
The alternate enclosure for the MetaSense Air Quality Sensing Platform allows the sensor to be connected directly to the bottom of a DJI F450 Quadcopter base.
