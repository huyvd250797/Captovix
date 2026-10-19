Unicode true
!include "MUI2.nsh"
!include "x64.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!ifndef PAYLOAD
 !define PAYLOAD "..\artifacts\win-x64"
!endif
!ifndef VCREDIST
 !define VCREDIST "..\prerequisites\vc_redist.x64.exe"
!endif
!ifndef OUTPUT
 !define OUTPUT "..\artifacts\Capture-Foundation-v0.1.0-Setup.exe"
!endif
Name "Capture Foundation 0.1.0"
OutFile "${OUTPUT}"
InstallDir "$LOCALAPPDATA\Programs\CaptureFoundation"
RequestExecutionLevel user
SetCompressor /SOLID lzma
SetCompressorDictSize 32
VIProductVersion "0.1.0.0"
VIAddVersionKey "ProductName" "Capture Foundation"
VIAddVersionKey "FileDescription" "Capture Foundation 0.1.0 Setup"
VIAddVersionKey "FileVersion" "0.1.0"
VIAddVersionKey "LegalCopyright" "Copyright 2026 HuyVo"
!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\CaptureFoundation.exe"
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Vietnamese"

Function .onInit
 ${IfNot} ${RunningX64}
  MessageBox MB_ICONSTOP "Ban nay danh cho Windows x64."
  Abort
 ${EndIf}
 ${IfNot} ${AtLeastWin10}
  MessageBox MB_ICONSTOP "Can Windows 10 22H2 hoac Windows 11."
  Abort
 ${EndIf}
 SetRegView 64
 System::Call 'kernel32::OpenMutexW(i 0x100000, i 0, w "Local\HuyVo.CaptureFoundation") p.r0'
 ${If} $0 != 0
  System::Call 'kernel32::CloseHandle(p r0)'
  MessageBox MB_ICONSTOP "Hay dong Capture Foundation truoc khi cai dat hoac cap nhat."
  Abort
 ${EndIf}
FunctionEnd

Section "Capture Foundation"
 SetShellVarContext current
 SetRegView 64
 ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" "Installed"
 ${If} $0 != 1
  InitPluginsDir
  SetOutPath "$PLUGINSDIR"
  File /oname=vc_redist.x64.exe "${VCREDIST}"
  MessageBox MB_OKCANCEL|MB_ICONINFORMATION "App can Microsoft Visual C++ Runtime x64. Windows se hoi quyen quan tri de cai thanh phan nay. Ban co the huy cai dat." IDOK continue_runtime IDCANCEL abort_runtime
  continue_runtime:
  ClearErrors
  ExecShellWait "runas" "$PLUGINSDIR\vc_redist.x64.exe" "/install /passive /norestart" SW_SHOWNORMAL $1
  ${If} ${Errors}
   MessageBox MB_ICONSTOP "Chua cai duoc Visual C++ Runtime. Hay chay lai bo cai va chap nhan yeu cau quyen Windows."
   Abort
  ${EndIf}
  ${If} $1 != 0
  ${AndIf} $1 != 3010
  ${AndIf} $1 != 1638
   MessageBox MB_ICONSTOP "Visual C++ Runtime tra ve ma loi $1. Hay kiem tra truoc khi cai app."
   Abort
  ${EndIf}
  Goto runtime_done
  abort_runtime:
   Abort
  runtime_done:
 ${EndIf}
 SetOutPath "$INSTDIR"
 File /r "${PAYLOAD}\*.*"
 WriteUninstaller "$INSTDIR\Uninstall.exe"
 CreateDirectory "$SMPROGRAMS\Capture Foundation"
 CreateShortcut "$SMPROGRAMS\Capture Foundation\Capture Foundation.lnk" "$INSTDIR\CaptureFoundation.exe"
 CreateShortcut "$DESKTOP\Capture Foundation.lnk" "$INSTDIR\CaptureFoundation.exe"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "DisplayName" "Capture Foundation"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "DisplayVersion" "0.1.0"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "Publisher" "HuyVo"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "InstallLocation" "$INSTDIR"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "DisplayIcon" "$INSTDIR\CaptureFoundation.exe"
 WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "UninstallString" '$\"$INSTDIR\Uninstall.exe$\"'
 WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "NoModify" 1
 WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation" "NoRepair" 1
SectionEnd

Section "Uninstall"
 SetShellVarContext current
 SetRegView 64
 Delete "$DESKTOP\Capture Foundation.lnk"
 RMDir /r "$SMPROGRAMS\Capture Foundation"
 DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CaptureFoundation"
 ; User media/settings are outside the install directory and are intentionally retained.
 RMDir /r "$INSTDIR"
SectionEnd
