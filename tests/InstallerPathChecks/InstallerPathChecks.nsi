; Developer-only Windows regression harness. No app is installed.
Unicode true
Name "Captovix installer path checks"
OutFile "InstallerPathChecks.exe"
RequestExecutionLevel user
SilentInstall silent
!include "LogicLib.nsh"
!include "../../installer/InstallPath.nsh"
Var Result
Var Failures
!macro ValidPath Input Expected
 StrCpy $INSTDIR "${Input}"
 Call NormalizeInstallDirectory
 IfErrors 0 +3
  FileWrite $Result "FAIL valid: ${Input}$\r$\n"
  IntOp $Failures $Failures + 1
 ${If} $INSTDIR != "${Expected}"
  FileWrite $Result "FAIL normalized: $INSTDIR; expected ${Expected}$\r$\n"
  IntOp $Failures $Failures + 1
 ${EndIf}
!macroend
!macro InvalidPath Input
 StrCpy $INSTDIR "${Input}"
 Call NormalizeInstallDirectory
 IfErrors +3 0
  FileWrite $Result "FAIL invalid: ${Input}$\r$\n"
  IntOp $Failures $Failures + 1
!macroend
Section
 StrCpy $Failures 0
 FileOpen $Result "$TEMP\Captovix-InstallerPathChecks-result.txt" w
 !insertmacro ValidPath "D:\Captovix\Captovix" "D:\Captovix\Captovix"
 ; Random per-run name has never been created by this harness.
 System::Call 'kernel32::GetTickCount() i.r4'
 !insertmacro ValidPath "D:\Captovix-path-test-$4\missing\nested" "D:\Captovix-path-test-$4\missing\nested"
 !insertmacro ValidPath "D:\Captovix\Captovix\" "D:\Captovix\Captovix"
 !insertmacro ValidPath "D:\Apps\..\Captovix\Captovix" "D:\Captovix\Captovix"
 !insertmacro ValidPath "D:\Ứng dụng\Captovix" "D:\Ứng dụng\Captovix"
 !insertmacro ValidPath "C:\Apps with spaces\Captovix" "C:\Apps with spaces\Captovix"
 !insertmacro InvalidPath "D:\"
 !insertmacro InvalidPath "D:"
 !insertmacro InvalidPath "D:\Captovix\.."
 !insertmacro InvalidPath "Captovix"
 !insertmacro InvalidPath "D:Captovix"
 !insertmacro InvalidPath "\\server\share\Captovix"
 FileWrite $Result "12 path cases; failures=$Failures$\r$\n"
 FileClose $Result
 SetErrorLevel $Failures
SectionEnd
