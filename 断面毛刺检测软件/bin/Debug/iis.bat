@echo off
cd /d "%~dp0"
 
echo Installing IIS...
echo Wait a moment...
start /w pkgmgr /iu:IIS-WebServerRole;WAS-WindowsActivationService;WAS-ProcessModel;WAS-NetFxEnvironment;WAS-ConfigurationAPI
 
echo Done.
