REM this script is used to avoid infringing on Brother's copyright by distributing the ControlCenter4 icon
REM it is run as a pre-build event (Pre-BuildEvent in .csproj or Properties > Build Events)
REM the pre-build event also sets the working directory to the solution directory for the duration of this script

REM prevent overriding provided icon if included manually, also avoid doing if already done previously
IF NOT EXIST "CC4.ico" (
	REM extract .ico from Brother ControlCenter4 installation using NirSoft IconsExtract (see Program.cs for explanation of file path)
	"iconsext\\iconsext.exe" /save "C:\\Program Files (x86)\\ControlCenter4\\BrCcBoot.exe" %cd% -icons)

	REM rename default .ico name from extraction to match project reference (ApplicationIcon in .csproj or Properties > Application)
	IF EXIST "BrCcBoot_101.ico" (ren "BrCcBoot_101.ico" "CC4.ico")
)

REM exit gracefully even if this didn't work out, allow project to continue build attempt
EXIT 0