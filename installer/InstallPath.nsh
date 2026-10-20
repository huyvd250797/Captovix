; Lexical normalization must work before the selected directory exists.
; Input/output: $INSTDIR. Error flag set for invalid/non-absolute/root paths.
; No filesystem changes. Registers are preserved.
!ifndef CAPTOVIX_INSTALL_PATH_NSH
!define CAPTOVIX_INSTALL_PATH_NSH
Function NormalizeInstallDirectory
 Push $0
 Push $1
 Push $2
 Push $3
 ClearErrors
 StrCpy $0 $INSTDIR 1 1
 StrCmp $0 ":" 0 normalize_invalid
 StrCpy $0 $INSTDIR 1 2
 StrCmp $0 "\" 0 normalize_invalid
 ; Unlike the NSIS instruction, this Windows API does not require existence.
 System::Call 'kernel32::GetFullPathNameW(w "$INSTDIR", i ${NSIS_MAX_STRLEN}, w .r2, p 0) i.r1'
 IntCmp $1 0 normalize_invalid normalize_invalid 0
 IntCmp $1 ${NSIS_MAX_STRLEN} normalize_invalid 0 normalize_invalid
 normalize_trim:
 StrLen $1 $2
 IntCmp $1 3 normalize_invalid normalize_invalid 0
 StrCpy $3 $2 1 -1
 StrCmp $3 "\" 0 normalize_ready
 StrCpy $2 $2 -1
 Goto normalize_trim
 normalize_ready:
 StrCpy $INSTDIR $2
 ClearErrors
 Goto normalize_done
 normalize_invalid:
 SetErrors
 normalize_done:
 Pop $3
 Pop $2
 Pop $1
 Pop $0
FunctionEnd
!endif
