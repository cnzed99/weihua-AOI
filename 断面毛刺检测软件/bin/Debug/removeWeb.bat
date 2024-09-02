@echo off
setlocal
 
:: 设置网站名称
set SITE_NAME=WH_MetalBurr
 
:: 使用appcmd.exe删除网站
%windir%\system32\inetsrv\appcmd delete site %SITE_NAME%
 
endlocal
echo Done.