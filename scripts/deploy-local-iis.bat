echo off
rem script elevates permissions automatically

set WebSitePath=Pim
set sitedest=c:\inetpub\wwwroot\Pim
set WebSitePoolName=AspNetCore

call %~dp0deploy-web-apps.bat %1

powershell "wget http://localhost/Pim/ | Out-Null;"
