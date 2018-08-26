# VAM-MapLoader
<b>Description</b> 

A mod for virt-a-mate to allow you to load in environments constructed in unity. These environments are not saved as part of the VAM scene save so you'll have to load them in seperately once you've loaded your scene again. The latest release has been built against VAM version 1.10.0.12.

<b>Installation</b> 

Simply extract the mod rar file into your VAM directory. Drag VAM.exe onto IPA.exe and then run the newly created shortcut VAM (Patch & Launch). Download the sample map and extract that into the VAM directory too.

<b>Configuration</b> 

The mod needs to know where to look for map files and of what type. Within your VAM directory you will have a MapLoaderConfig.json. Open this file and edit the directory for additional files. You can have more than 1 directory per type of map. The map type options are; Unity, PlayHome, Koikatu. The config IS case sensitive.  Koikatu is not yet full implemented.


<b>Usage</b> 

Once VAM has started, open the main menu from the HUD and you'll find a new button just above quit called "Load External Map". Press the button and it'll toggle a new panel with a list of all the available maps. Click on of the maps to load. Only one map can be loaded at once. It can be a bit bugged quickly change maps as the mod may not yet have unloaded the existing map before you get to the next one.
