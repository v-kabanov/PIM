echo off

rem script elevates permissions automatically

rem set WebSitePath=Pim
rem set sitedest=c:\inetpub\wwwroot\Pim
rem set WebSitePoolName=AspNetCore

set conf=%1

if not defined conf (
   set /p conf="Confirm deployment (Y/N)? " %=%
)

if /I "Y" neq "%conf%" (
  echo no package to deploy
  exit /b 9
)

set projectdir=%~dp0..\PimWeb
cd "%projectdir%"

dotnet publish -c Release

if %errorlevel% neq 0 exit /b 1

set publishdir="%projectdir%\bin\Release\net7.0\publish"

echo starting deployment in %~dp0
set transformdll=%~dp0\tools\Microsoft.Web.Xdt.2.1.2\lib\net40\Microsoft.Web.XmlTransform.dll

if not exist "%publishdir%" (
  echo no package to deploy
  exit /b 1
)

powershell start %SYSTEMROOT%\System32\inetsrv\appcmd -ArgumentList "stop","apppool", "/apppool.name:%WebSitePoolName%" -Verb Runas -Wait
if %errorlevel% neq 0 (
 echo failed stopping appPool
 exit /b 1
)

set srcdir=%publishdir%
set destdir=%sitedest%
rem set configdir=%~dp0config\%WebSitePath%

if not exist "%srcdir%" (
 echo "%srcdir%" does not exist
 exit /b 1
)

if exist "%srcdir%" (
  rem if exist "%destdir%" rmdir /S /Q "%destdir%"
  rem if %errorlevel% neq 0 exit /b 1

 robocopy "%srcdir%" "%destdir%" *.exe *.dll *.pdb /mir
 if %errorlevel% neq 0 (
    echo failed copying content
    exit /b 1
  )
)
  
rem powershell -File "%~dp0\ApplyXdtTransformations.ps1" -targetDir "%destdir%" -xdtDir "%configdir%" -xmlTransformDllPath "%transformdll%"
rem if %errorlevel% neq 0 exit /b 1

powershell start %SYSTEMROOT%\System32\inetsrv\appcmd -ArgumentList "start","apppool", "/apppool.name:%WebSitePoolName%" -Verb Runas -Wait
if %errorlevel% neq 0 (
 echo failed starting appPool
 exit /b 1
)
echo deployment completed successfully