# Compila una copia de consola (solo para pruebas) y ejecuta las pruebas del motor
$fw="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"; $here = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Get-ChildItem "$here\..\src\*.cs" | % FullName
& "$fw\csc.exe" /nologo /target:exe /codepage:65001 /out:$here\NeoCalcC.exe "/lib:$fw\WPF" /r:PresentationFramework.dll /r:PresentationCore.dll /r:WindowsBase.dll /r:System.Xaml.dll /r:System.Core.dll /r:System.Web.Extensions.dll "/resource:$here\..\src\Idiomas.txt,Idiomas.txt" @src
if ($LASTEXITCODE -ne 0) { exit 1 }
foreach ($a in $args) { $p = Start-Process "$here\NeoCalcC.exe" -ArgumentList $a -NoNewWindow -PassThru -Wait }
