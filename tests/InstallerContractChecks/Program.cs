using System.Text.RegularExpressions;

var root = args.Length == 1 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
var script = File.ReadAllText(Path.Combine(root, "installer", "Captovix.nsi"));
var program = File.ReadAllText(Path.Combine(root, "src", "Captovix", "Program.cs"));
var passed = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("PASS: " + name); }

var calls = Regex.Matches(script, @"(?m)^\s*ExecWait\s+'([^'\r\n]*CoreSetup\.exe[^'\r\n]*)'\s+\$0\s*$");
Check(calls.Count == 1, "One explicit core installer invocation");
var command = calls[0].Groups[1].Value;
// Tokenize this restricted command contract (quoted paths and simple flags).
// This is a static regression check, not a Windows process execution test.
var tokens = Regex.Matches(command, "\"[^\"]*\"|[^\\s]+").Select(m => m.Value.Trim('"')).ToArray();
Check(!tokens.Contains("--"), "No EXE_ARGS passthrough separator: prevents Velopack #1039");
Check(!command.Contains("--captovix-install-only") && !program.Contains("--captovix-install-only"), "No obsolete app-only installer argument");
Check(!tokens.Contains("--silent"), "Core installer progress and errors remain visible");
Check(tokens.Length == 6 && tokens[1] == "--installto" && tokens[2] == "$NewPath" && tokens[3] == "--log" && tokens[4] == "$CoreLog" && tokens[5] == "--verbose", "Only supported named options; quoted install/log paths");
Check(script.Contains("Rename \"$CoreLog\" \"$CoreLog.previous\""), "Previous installation log is preserved on retry");
var core = calls[0].Index;
var legacy = script.IndexOf("ExecWait '\"$LegacyPath\\Uninstall.exe\"", StringComparison.Ordinal);
Check(core >= 0 && legacy > core && script.IndexOf("${If} $0 != 0", core, StringComparison.Ordinal) < legacy && script.IndexOf("IfFileExists \"$NewPath\\current\\Captovix.exe\"", core, StringComparison.Ordinal) < legacy, "Legacy uninstall stays behind core exit/existence checks");
Check(!Regex.IsMatch(script, @"(?im)^\s*RMDir\s+/r\b"), "Bootstrap never recursively removes user directories");
var launch = script.IndexOf("Function LaunchInstalled", StringComparison.Ordinal);
Check(script.IndexOf("OpenMutexW", launch, StringComparison.Ordinal) < script.IndexOf("Exec '\"$NewPath", launch, StringComparison.Ordinal), "Finish action checks already-running app before launching again");
Check(script.Contains("Buoc 1/3") && script.Contains("Buoc 2/3") && script.Contains("Buoc 3/3"), "Installer exposes each phase instead of an unexplained wait");
Check(script.Contains("!insertmacro MUI_PAGE_DIRECTORY"),"Installer includes a destination directory page");
Check(script.Contains("MUI_PAGE_CUSTOMFUNCTION_LEAVE ValidateDirectory")&&script.Contains("Call ValidateDirectory"),"Destination is validated for both wizard and silent install");
Check(script.Contains("StrCpy $NewPath \"$INSTDIR\""),"Chosen directory is passed to the core installer");
Check(script.Contains("GetTempFileName $0 \"$INSTDIR\""),"Destination must pass an actual write probe");
Check(script.Contains("FindFirst $0 $1 \"$INSTDIR\\*\""),"Fresh install never overwrites a nonempty directory");
Check(script.Contains("$INSTDIR != $ExistingPath"),"Changing an existing managed root requires explicit uninstall first");
Check(script.Contains("GetDriveTypeW")&&script.Contains("$LOCALAPPDATA\\HuyVo\\Captovix\\"),"Destination excludes unstable drives and persistent data overlap");
var validate = script[script.IndexOf("Function ValidateDirectory", StringComparison.Ordinal)..script.IndexOf("Function LaunchInstalled", StringComparison.Ordinal)];
var normalize = File.ReadAllText(Path.Combine(root, "installer", "InstallPath.nsh"));
Check(validate.Contains("Call NormalizeInstallDirectory") && !validate.Contains("GetFullPathName $INSTDIR"), "Directory validation avoids NSIS existing-path-only normalization");
Check(normalize.Contains("kernel32::GetFullPathNameW") && normalize.Contains("w .r2, p 0) i.r1"), "Normalization uses Windows lexical API with a separate output buffer");
Check(normalize.Contains("IntCmp $1 ${NSIS_MAX_STRLEN}") && normalize.Contains("IntCmp $1 0"), "Normalization rejects failed or truncated API output");
Check(normalize.Contains("IntCmp $1 3 normalize_invalid") && normalize.Contains("normalize_trim:"), "Drive roots remain rejected after normalization and trailing slash trimming");
Check(validate.IndexOf("IfErrors invalid_directory", StringComparison.Ordinal) < validate.IndexOf("GetDriveTypeW", StringComparison.Ordinal), "Failed normalization stops before drive inspection");
Check(!normalize.Contains("CreateDirectory") && !normalize.Contains("FileOpen"), "Lexical normalization does not create or overwrite files");
Check(validate.Contains("invalid_drive:") && validate.Contains("data_overlap:"), "Drive and data overlap failures have distinct explanations");
var pathCases = File.ReadAllText(Path.Combine(root, "tests", "InstallerPathChecks", "InstallerPathChecks.nsi"));
Check(pathCases.Contains("D:\\Captovix\\Captovix") && pathCases.Contains("missing\\nested"), "Windows NSIS harness covers requested and nonexistent nested destinations");
Console.WriteLine($"{passed} installer contract checks passed. Actual Windows install/restart still requires Windows.");
