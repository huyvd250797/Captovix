; First-install/migration wrapper. Future updates are performed by Velopack.
Unicode true
!include "MUI2.nsh"
!include "x64.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!ifndef APPVERSION
 !define APPVERSION "1.0.3"
!endif
!ifndef CORESETUP
 !define CORESETUP "..\artifacts\CoreSetup.exe"
!endif
!ifndef VCREDIST
 !define VCREDIST "..\prerequisites\vc_redist.x64.exe"
!endif
!ifndef OUTPUT
 !define OUTPUT "..\artifacts\Captovix-v${APPVERSION}-Setup.exe"
!endif
Name "Captovix ${APPVERSION}"
Icon "..\src\Captovix\Assets\Captovix.ico"
OutFile "${OUTPUT}"
RequestExecutionLevel user
SetCompressor /SOLID lzma
SetCompressorDictSize 32
VIProductVersion "${APPVERSION}.0"
VIAddVersionKey "ProductName" "Captovix"
VIAddVersionKey "FileDescription" "Captovix ${APPVERSION} Setup"
VIAddVersionKey "FileVersion" "${APPVERSION}"
VIAddVersionKey "LegalCopyright" "Copyright 2026 HuyVo"
Var LegacyPath
Var LegacyVersion
Var LegacyPublisher
Var LegacyName
Var NewPath
Var CoreLog
!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_FUNCTION LaunchInstalled
!insertmacro MUI_PAGE_FINISH
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
 System::Call 'kernel32::OpenMutexW(i 0x100000, i 0, w "Local\HuyVo.Captovix") p.r0'
 ${If} $0 != 0
  System::Call 'kernel32::CloseHandle(p r0)'
  MessageBox MB_ICONSTOP "Hay dong Captovix truoc khi cai dat."
  Abort
 ${EndIf}
 System::Call 'kernel32::OpenMutexW(i 0x100000, i 0, w "Local\HuyVo.CaptureFoundation") p.r0'
 ${If} $0 != 0
  System::Call 'kernel32::CloseHandle(p r0)'
  MessageBox MB_ICONSTOP "Hay dong Capture Foundation cu truoc khi cai dat."
  Abort
 ${EndIf}
 ReadRegStr $LegacyPath HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Captovix" "InstallLocation"
 ReadRegStr $LegacyVersion HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Captovix" "DisplayVersion"
 ReadRegStr $LegacyPublisher HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Captovix" "Publisher"
 ReadRegStr $LegacyName HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Captovix" "DisplayName"
 StrCpy $NewPath "$LOCALAPPDATA\HuyVo.Captovix"
 StrCpy $CoreLog "$LOCALAPPDATA\HuyVo\Captovix\Logs\setup-core.log"
FunctionEnd

Function LaunchInstalled
 ; The visible Velopack setup normally launches the app itself. Do not launch
 ; a second copy and trigger the application's single-instance warning.
 System::Call 'kernel32::OpenMutexW(i 0x100000, i 0, w "Local\HuyVo.Captovix") p.r0'
 ${If} $0 != 0
  System::Call 'kernel32::CloseHandle(p r0)'
  Return
 ${EndIf}
 Exec '"$NewPath\Update.exe" start Captovix.exe'
FunctionEnd

Section "Captovix"
 SetShellVarContext current
 SetRegView 64
 DetailPrint "Buoc 1/3: Kiem tra Microsoft Visual C++ Runtime."
 ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" "Installed"
 InitPluginsDir
 SetOutPath "$PLUGINSDIR"
 ${If} $0 != 1
  File /oname=vc_redist.x64.exe "${VCREDIST}"
  MessageBox MB_OKCANCEL|MB_ICONINFORMATION "App can Microsoft Visual C++ Runtime x64. Windows se hoi quyen quan tri de cai thanh phan nay." IDOK continue_runtime IDCANCEL abort_runtime
  continue_runtime:
  ClearErrors
  ExecShellWait "runas" "$PLUGINSDIR\vc_redist.x64.exe" "/install /passive /norestart" SW_SHOWNORMAL $1
  ${If} ${Errors}
   MessageBox MB_ICONSTOP "Chua cai duoc Visual C++ Runtime."
   Abort
  ${EndIf}
  ${If} $1 != 0
  ${AndIf} $1 != 3010
  ${AndIf} $1 != 1638
   MessageBox MB_ICONSTOP "Visual C++ Runtime tra ve ma loi $1."
   Abort
  ${EndIf}
  Goto runtime_done
  abort_runtime:
   Abort
  runtime_done:
 ${EndIf}
 File /oname=CoreSetup.exe "${CORESETUP}"
 CreateDirectory "$LOCALAPPDATA\HuyVo\Captovix\Logs"
 ; Keep the previous attempt's log when retrying installation.
 Delete "$CoreLog.previous"
 Rename "$CoreLog" "$CoreLog.previous"
 DetailPrint "Buoc 2/3: Cai Captovix. Theo doi tien trinh o cua so cai dat Captovix vua mo."
 DetailPrint "Log cai dat: $CoreLog"
 ; Complete new installation before touching the registered V1.0 copy.
 ; Velopack 1.2.161 issue #1039: NEVER pass app arguments after '--'.
 ; Use its visible setup so progress, prerequisite prompts and errors are shown.
 ClearErrors
 ExecWait '"$PLUGINSDIR\CoreSetup.exe" --installto "$NewPath" --log "$CoreLog" --verbose' $0
 ${If} ${Errors}
  MessageBox MB_ICONSTOP "Khong khoi chay duoc bo cai Captovix. Ban cu van duoc giu. Log: $CoreLog"
  SetErrorLevel 1
  Abort
 ${EndIf}
 ${If} $0 != 0
  MessageBox MB_ICONSTOP "Bo cai tra ve ma $0. Ban cu chua bi go. Log: $CoreLog"
  SetErrorLevel 1
  Abort
 ${EndIf}
 IfFileExists "$NewPath\Update.exe" 0 invalid_install
 IfFileExists "$NewPath\current\Captovix.exe" 0 invalid_install
 DetailPrint "Buoc 3/3: Hoan tat chuyen doi va tao shortcut."
 ; Only the known registered NSIS V1.0 copy is eligible for migration.
 ${If} $LegacyVersion == "1.0.0"
 ${AndIf} $LegacyPublisher == "HuyVo"
 ${AndIf} $LegacyName == "Captovix"
 ${AndIf} $LegacyPath != ""
 ${AndIf} $LegacyPath != $NewPath
  IfFileExists "$LegacyPath\Uninstall.exe" 0 legacy_done
  IfFileExists "$LegacyPath\Captovix.exe" 0 legacy_done
  ; _?= prevents a detached uninstaller process so ExecWait waits for completion.
  ClearErrors
  ExecWait '"$LegacyPath\Uninstall.exe" /S _?=$LegacyPath' $0
  ${If} ${Errors}
   MessageBox MB_ICONEXCLAMATION "Ban moi da cai xong. Chua go duoc ban NSIS V1.0; hay go muc Captovix 1.0.0 trong Windows Settings. Du lieu van giu nguyen."
  ${ElseIf} $0 != 0
   MessageBox MB_ICONEXCLAMATION "Ban moi da cai xong. Ban cu tra ve ma $0 khi go. Kiem tra Windows Settings."
  ${Else}
   ; Old uninstaller only deletes its generated owned-file list, no recursion.
   Delete "$LegacyPath\Uninstall.exe"
   RMDir "$LegacyPath"
  ${EndIf}
 ${EndIf}
 legacy_done:
 ; Old uninstall may remove the new Desktop shortcut. Recreate stable launchers.
 CreateShortcut "$DESKTOP\Captovix.lnk" "$NewPath\Update.exe" "start Captovix.exe" "$NewPath\current\Captovix.exe"
 CreateShortcut "$SMPROGRAMS\Captovix.lnk" "$NewPath\Update.exe" "start Captovix.exe" "$NewPath\current\Captovix.exe"
 Goto done
 invalid_install:
  MessageBox MB_ICONSTOP "Chua tim thay file da cai. Ban cu chua bi go. Log: $CoreLog"
  SetErrorLevel 1
  Abort
 done:
 DetailPrint "Da hoan tat cai Captovix ${APPVERSION}."
SectionEnd
