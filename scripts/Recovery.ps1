# Independent rescue UI: works without Captovix WPF/runtime DLLs. Never deletes user media.
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.IO.Compression.FileSystem
$dataRoot = Join-Path $env:LOCALAPPDATA 'HuyVo\Captovix'
$form = New-Object System.Windows.Forms.Form
$form.Text = 'Captovix - Cong cu cuu ho'; $form.Width=650; $form.Height=330; $form.StartPosition='CenterScreen'
$label = New-Object System.Windows.Forms.Label
$label.Text = "Cong cu nay khong xoa anh/video, KEY hay du an.`r`nNeu app khong mo: xuat bao cao, cai lai bang Full Setup hop le.`r`nTrong app: Tro giup > Cap nhat & khoi phuc du lieu de kiem tra/restore backup.`r`nKhong giai nen backup de ghi de thu cong khi chua co ban an toan."
$label.SetBounds(20,20,600,100); $form.Controls.Add($label)
function Add-Action($text,$top,$callback) { $button=New-Object System.Windows.Forms.Button; $button.Text=$text; $button.SetBounds(20,$top,590,36);$button.Add_Click($callback);$form.Controls.Add($button) }
Add-Action 'Mo thu muc sao luu va du lieu' 125 { if(!(Test-Path $dataRoot)){[System.Windows.Forms.MessageBox]::Show('Chua co thu muc du lieu.');return};Start-Process explorer.exe -ArgumentList ('"'+$dataRoot+'"') }
Add-Action 'Xuat bao cao chan doan (khong kem anh/KEY)' 166 {
 try { $save=New-Object System.Windows.Forms.SaveFileDialog; $save.Filter='Text|*.txt';$save.FileName='Captovix-Recovery.txt';if($save.ShowDialog() -ne 'OK'){return};$report=@('Captovix independent recovery',('UTC: '+[DateTime]::UtcNow.ToString('o')),('OS: '+[Environment]::OSVersion.VersionString));foreach($name in @('Captovix.exe','Captovix.dll','Captovix.runtimeconfig.json','Velopack.dll','release-quality.json')){$path=Join-Path $PSScriptRoot $name;if(Test-Path $path){$report+=($name+' bytes='+ (Get-Item $path).Length+' sha256='+ (Get-FileHash $path -Algorithm SHA256).Hash)}else{$report+=('MISSING '+$name)}};$errors=Join-Path $dataRoot 'errors.log';if(Test-Path $errors){$report+=(Get-Content $errors -Tail 100 | Where-Object { $_ -notmatch '(?i)key|token|certificate|authorization|password|recovery.?code' })};$report|Set-Content -Path $save.FileName -Encoding UTF8;[System.Windows.Forms.MessageBox]::Show('Da xuat bao cao. Xem lai duong dan/thong tin truoc khi chia se.') }catch{[System.Windows.Forms.MessageBox]::Show($_.Exception.Message)}
}
Add-Action 'Chon Full Setup de cai lai app' 207 { $pick=New-Object System.Windows.Forms.OpenFileDialog;$pick.Filter='Captovix Full Setup|Captovix*Setup.exe';if($pick.ShowDialog() -eq 'OK'){if([System.Windows.Forms.MessageBox]::Show(('Chay bo cai da chon? Chi dung file tu nguon phat hanh tin cay: '+$pick.FileName),'Captovix','YesNo') -eq 'Yes'){Start-Process -FilePath $pick.FileName}} }
[void]$form.ShowDialog()
