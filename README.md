# BrotherControlCenterManager

## Purpose  
This software was developed for personal use to create an executable that can:
1.  directly launch Brother's Control Center 4 (CC4) program without launching through Brother Utilities program
2.  circumvent the persistent system tray item created on CC4 launch. After execution, it:
	1.  waits in the background until the CC4 window is closed
	2.  closes the CC4 background processes
	3.  removes the CC4 icon from the system tray
	4.  exits

This repository should provide an example of how the Brother software is configured and how to work with it. The key area is the Program.cs file, which is documented to provide this information.

Anyone who wishes to actually build and use the program will have to change paths, printer models, and specific variables as suits their printer hardware and Brother software install.

A full installer _could_ be made that would detect install location, allow choosing from installed models by looking for the .ini files (and pulling the necessary paths / variables / etc. from within), and creating/placing the .exe and shortcut. I currently have no plans to do this as I don't think there is any demand for such a thing, and I have not looked into handling cases such as multiple models set up. I also have no intention to provide support across different Brother products, releases, etc.

However, if you have come across this repository seeking something particular (ex. information, a specific build of this software), I may be able to help you if you ask for it.

## Function
Once paths and variables are modified as necessary in Program.cs and GetBrotherCC4Icon.bat (and install.bat), building the software will create an executable "BrCcClean.exe" in the "latest_build" folder. The install.bat file can then be run to place it in the CC4 install directory with the similarly-named .exe files (though it can be placed anywhere and still function). A shortcut to the BrCcClean.exe file can then serve as the means of launching the CC4 software.

## Licensing
MIT

## Included Software
**NirSoft Icons Extract**

This program is used to pull the icon used for this software from the existing Brother software installation. This avoids infringing on Brother's copyright by distributing the CC4 icon while still matching its appearance.

The full "iconsext" folder has been included, unchanged, in order to comply with the licensing outlined by [NirSoft](https://www.nirsoft.net/utils/iconsext.html):  
> This utility is released as freeware. You can freely use and distribute it. If you distribute this utility, you must include all files in the distribution package including the readme.txt, without any modification !
