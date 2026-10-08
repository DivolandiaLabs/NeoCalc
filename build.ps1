# Compila NeoCalc.exe con el csc de .NET Framework (viene con Windows). Sin instalar nada.
# El icono lo dibuja el propio programa (Icono.cs): 1) copia temporal sin icono  2) escribe app.ico  3) version final.
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$fw = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$csc = Join-Path $fw 'csc.exe'
$src = Get-ChildItem "$here\src\*.cs" | ForEach-Object { $_.FullName }
$common = @('/nologo', '/target:winexe', '/optimize+', '/platform:anycpu', '/codepage:65001',
            "/win32manifest:$here\app.manifest", "/lib:$fw\WPF",
            '/r:PresentationFramework.dll', '/r:PresentationCore.dll', '/r:WindowsBase.dll', '/r:System.Xaml.dll',
            '/r:System.Core.dll', '/r:System.Web.Extensions.dll',
            "/resource:$here\src\Idiomas.txt,Idiomas.txt")

$tmp = Join-Path $env:TEMP 'NeoCalc_icono.exe'
& $csc @common "/out:$tmp" @src
if ($LASTEXITCODE -ne 0) { throw "Error al compilar" }
Start-Process $tmp -ArgumentList '/icono', "`"$here\app.ico`"" -Wait
Remove-Item $tmp

& $csc @common "/out:$here\NeoCalc.exe" "/win32icon:$here\app.ico" @src
if ($LASTEXITCODE -ne 0) { throw "Error al compilar" }
Get-Item "$here\NeoCalc.exe" | Select-Object Name, Length
