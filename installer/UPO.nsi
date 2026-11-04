Unicode true
; UPO · Ultimate Performance Optimizer — kurulum betiği (NSIS 3)
; Derleme: makensis installer/UPO.nsi   (önce: dotnet publish -c Release -o publish)

!include "MUI2.nsh"
!include "x64.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"
!include "StrFunc.nsh"
${StrStr}
${UnStrStr}

!define APPNAME   "UPO"
!define FULLNAME  "UPO - Ultimate Performance Optimizer"
!define VERSION   "1.5.0"
!define PUBLISHER "Eren Ağalar"
!define UNINSTKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\UPO"

Name "${FULLNAME}"
!ifndef SRC
  !define SRC "..\publish"
!endif
OutFile "${SRC}\UPO-Setup-${VERSION}.exe"
InstallDir "$PROGRAMFILES64\UPO"
InstallDirRegKey HKLM "${UNINSTKEY}" "InstallLocation"
RequestExecutionLevel admin
SetCompressor /SOLID lzma
BrandingText "${FULLNAME} · ${PUBLISHER}"
ShowInstDetails show
ShowUninstDetails show

!define MUI_ICON "..\app.ico"
!define MUI_UNICON "..\app.ico"
!define MUI_ABORTWARNING
; Kurulumda seçilen dili hatırla (kaldırıcı tekrar sormasın)
!define MUI_LANGDLL_REGISTRY_ROOT "HKLM"
!define MUI_LANGDLL_REGISTRY_KEY "${UNINSTKEY}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "InstallerLanguage"
!define MUI_WELCOMEFINISHPAGE_BITMAP "welcome.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "welcome.bmp"
!define MUI_COMPONENTSPAGE_NODESC
!define MUI_WELCOMEPAGE_TITLE "$(WelcomeTitle)"
!define MUI_WELCOMEPAGE_TEXT "$(WelcomeText)"
!define MUI_FINISHPAGE_RUN "$INSTDIR\UPO.exe"
!define MUI_FINISHPAGE_RUN_TEXT "$(RunUpo)"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "Turkish"
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

VIProductVersion "${VERSION}.0"
VIAddVersionKey /LANG=${LANG_TURKISH} "ProductName" "${FULLNAME}"
VIAddVersionKey /LANG=${LANG_TURKISH} "CompanyName" "${PUBLISHER}"
VIAddVersionKey /LANG=${LANG_TURKISH} "LegalCopyright" "© 2026 ${PUBLISHER}"
VIAddVersionKey /LANG=${LANG_TURKISH} "FileDescription" "${FULLNAME} Kurulum"
VIAddVersionKey /LANG=${LANG_TURKISH} "FileVersion" "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${FULLNAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "${PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "© 2026 ${PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${FULLNAME} Setup"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${VERSION}"

; ── Metinler ──
LangString WelcomeTitle ${LANG_TURKISH} "UPO Kurulumuna Hoş Geldiniz"
LangString WelcomeTitle ${LANG_ENGLISH} "Welcome to UPO Setup"
LangString WelcomeText  ${LANG_TURKISH} "Bu sihirbaz ${FULLNAME} ${VERSION} sürümünü bilgisayarınıza kuracak.$\r$\n$\r$\nTasarım: ${PUBLISHER}$\r$\n$\r$\nUPO hiçbir veri toplamaz; internete yalnızca güncelleme kontrolü için bağlanır. Yaptığı her ayar değişikliği yedeklenir ve tek tuşla geri alınabilir.$\r$\n$\r$\nDevam etmek için İleri'ye tıklayın."
LangString WelcomeText  ${LANG_ENGLISH} "This wizard will install ${FULLNAME} ${VERSION} on your computer.$\r$\n$\r$\nDesigner: ${PUBLISHER}$\r$\n$\r$\nUPO collects no data; it connects to the internet only to check for updates. Every settings change it makes is backed up and can be undone with one click.$\r$\n$\r$\nClick Next to continue."
LangString RunUpo       ${LANG_TURKISH} "UPO'yu şimdi çalıştır"
LangString RunUpo       ${LANG_ENGLISH} "Run UPO now"
LangString SecApp       ${LANG_TURKISH} "UPO (gerekli)"
LangString SecApp       ${LANG_ENGLISH} "UPO (required)"
LangString SecDesktop   ${LANG_TURKISH} "Masaüstü kısayolu"
LangString SecDesktop   ${LANG_ENGLISH} "Desktop shortcut"
LangString SecAssoc     ${LANG_TURKISH} ".upobackup yedek dosyalarını UPO ile aç"
LangString SecAssoc     ${LANG_ENGLISH} "Open .upobackup backup files with UPO"
LangString Need64       ${LANG_TURKISH} "UPO 64-bit Windows 10 veya 11 gerektirir."
LangString Need64       ${LANG_ENGLISH} "UPO requires 64-bit Windows 10 or 11."
LangString Running      ${LANG_TURKISH} "UPO şu anda açık. Devam etmek için UPO'yu kapatıp Yeniden Dene'ye tıklayın."
LangString Running      ${LANG_ENGLISH} "UPO is currently running. Close UPO and click Retry to continue."
LangString BackupType   ${LANG_TURKISH} "UPO Yedeği"
LangString BackupType   ${LANG_ENGLISH} "UPO Backup"
LangString UnPending    ${LANG_TURKISH} "UPO'nun yaptığı ve henüz geri alınmamış ayar değişiklikleri var.$\r$\n$\r$\nEvet: UPO'yu '08 · Tümünü Geri Al' sayfasında açar (geri aldıktan sonra kaldırmayı tekrar çalıştırın).$\r$\nHayır: Ayarları olduğu gibi bırakıp kaldırır. Yedekler silinmez; UPO'yu yeniden kurarak daha sonra geri alabilirsiniz.$\r$\nİptal: Kaldırmayı durdurur."
LangString UnPending    ${LANG_ENGLISH} "UPO made settings changes that haven't been undone yet.$\r$\n$\r$\nYes: Opens UPO on the '08 · Undo Everything' page (run the uninstaller again afterwards).$\r$\nNo: Uninstalls and leaves settings as they are. Backups are kept; you can reinstall UPO and undo later.$\r$\nCancel: Stops the uninstall."
LangString UnData       ${LANG_TURKISH} "UPO'nun ayarları ve skor geçmişi (C:\ProgramData\UPO) de silinsin mi?"
LangString UnData       ${LANG_ENGLISH} "Also delete UPO's settings and score history (C:\ProgramData\UPO)?"

Var KeepData

; ── UPO açık mı? ──
!macro CHECK_RUNNING UN STR
Function ${UN}CheckRunning
  StrCpy $3 0
  retry:
    nsExec::ExecToStack 'cmd /c tasklist /FI "IMAGENAME eq UPO.exe" /NH'
    Pop $0
    Pop $1
    ${${STR}StrStr} $2 $1 "UPO.exe"
    ${If} $2 != ""
      ; Sessiz güncellemede UPO kendini kapatıyor; en fazla 30 sn bekle.
      ${If} ${Silent}
        IntOp $3 $3 + 1
        ${If} $3 < 30
          Sleep 1000
          Goto retry
        ${EndIf}
        Abort
      ${EndIf}
      MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION "$(Running)" IDRETRY retry
      Abort
    ${EndIf}
FunctionEnd
!macroend
!insertmacro CHECK_RUNNING "" ""
!insertmacro CHECK_RUNNING "un." "Un"

; Uygulama içi güncelleme "/S /RELAUNCH" ile çalıştırır: kurulum bitince UPO yeniden açılır.
Function .onInstSuccess
  ${GetParameters} $R0
  ClearErrors
  ${GetOptions} $R0 "/RELAUNCH" $R1
  ${IfNot} ${Errors}
    Exec '"$INSTDIR\UPO.exe"'
  ${EndIf}
FunctionEnd

Function .onInit
  ${IfNot} ${RunningX64}
    MessageBox MB_OK|MB_ICONSTOP "$(Need64)"
    Abort
  ${EndIf}
  SetRegView 64
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd

Section "!$(SecApp)" SecApp
  SectionIn RO
  Call CheckRunning
  SetShellVarContext all
  SetOutPath "$INSTDIR"
  File "${SRC}\UPO.exe"
  File "..\README.md"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  CreateDirectory "$SMPROGRAMS\UPO"
  CreateShortcut "$SMPROGRAMS\UPO\UPO.lnk" "$INSTDIR\UPO.exe" "" "$INSTDIR\UPO.exe" 0
  CreateShortcut "$SMPROGRAMS\UPO\Uninstall UPO.lnk" "$INSTDIR\Uninstall.exe"

  WriteRegStr   HKLM "${UNINSTKEY}" "DisplayName" "${FULLNAME}"
  WriteRegStr   HKLM "${UNINSTKEY}" "DisplayVersion" "${VERSION}"
  WriteRegStr   HKLM "${UNINSTKEY}" "Publisher" "${PUBLISHER}"
  WriteRegStr   HKLM "${UNINSTKEY}" "DisplayIcon" "$INSTDIR\UPO.exe,0"
  WriteRegStr   HKLM "${UNINSTKEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr   HKLM "${UNINSTKEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr   HKLM "${UNINSTKEY}" "QuietUninstallString" '"$INSTDIR\Uninstall.exe" /S'
  WriteRegStr   HKLM "${UNINSTKEY}" "InstallerLanguage" "$LANGUAGE"
  WriteRegDWORD HKLM "${UNINSTKEY}" "NoModify" 1
  WriteRegDWORD HKLM "${UNINSTKEY}" "NoRepair" 1
  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKLM "${UNINSTKEY}" "EstimatedSize" "$0"
SectionEnd

Section "$(SecDesktop)" SecDesktop
  SetShellVarContext all
  CreateShortcut "$DESKTOP\UPO.lnk" "$INSTDIR\UPO.exe" "" "$INSTDIR\UPO.exe" 0
SectionEnd

Section "$(SecAssoc)" SecAssoc
  WriteRegStr HKCR ".upobackup" "" "UPO.Backup"
  WriteRegStr HKCR "UPO.Backup" "" "$(BackupType)"
  WriteRegStr HKCR "UPO.Backup\DefaultIcon" "" "$INSTDIR\UPO.exe,0"
  WriteRegStr HKCR "UPO.Backup\shell\open\command" "" '"$INSTDIR\UPO.exe" "%1"'
  System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, p 0, p 0)'
SectionEnd

; ── Kaldırma ──
Function un.onInit
  SetRegView 64
  !insertmacro MUI_UNGETLANGUAGE
  SetShellVarContext all   ; $APPDATA = C:\ProgramData
  StrCpy $KeepData "0"
  ; Geri alınmamış değişiklik var mı? (boş yedek dosyası "[]" kadar küçüktür)
  IfFileExists "$APPDATA\UPO\backup.json" 0 done
    FileOpen $0 "$APPDATA\UPO\backup.json" r
    FileSeek $0 0 END $1
    FileClose $0
    IntCmp $1 10 done done
    MessageBox MB_YESNOCANCEL|MB_ICONEXCLAMATION "$(UnPending)" IDYES openupo IDNO keep
    Abort
  openupo:
    Exec '"$INSTDIR\UPO.exe" --page 8'
    Abort
  keep:
    StrCpy $KeepData "1"
  done:
FunctionEnd

Section "Uninstall"
  Call un.CheckRunning
  SetShellVarContext all
  Delete "$INSTDIR\UPO.exe"
  Delete "$INSTDIR\README.md"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  Delete "$SMPROGRAMS\UPO\UPO.lnk"
  Delete "$SMPROGRAMS\UPO\Uninstall UPO.lnk"
  RMDir "$SMPROGRAMS\UPO"
  Delete "$DESKTOP\UPO.lnk"

  ReadRegStr $0 HKCR ".upobackup" ""
  ${If} $0 == "UPO.Backup"
    DeleteRegKey HKCR ".upobackup"
  ${EndIf}
  DeleteRegKey HKCR "UPO.Backup"
  System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, p 0, p 0)'
  DeleteRegKey HKLM "${UNINSTKEY}"

  ; Bekleyen yedek varsa veriler asla silinmez.
  ${If} $KeepData == "0"
  ${AndIf} ${FileExists} "$APPDATA\UPO\*.*"
    MessageBox MB_YESNO|MB_ICONQUESTION|MB_DEFBUTTON2 "$(UnData)" /SD IDNO IDNO skipdata
      RMDir /r "$APPDATA\UPO"
    skipdata:
  ${EndIf}
SectionEnd
