@echo off
setlocal

%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
"$w=4096; $h=4096; ^
$lastPercent = -10; ^
[void][Reflection.Assembly]::LoadWithPartialName('System.Drawing'); ^
$b = New-Object System.Drawing.Bitmap($w,$h); ^
for($y=0; $y -lt $h; $y++){ ^
    $percent = [int](($y / $h) * 100); ^
    if($percent -ge ($lastPercent + 10)){ ^
        $lastPercent = ($percent / 10) * 10; ^
        Write-Host ('Progresso: ' + $lastPercent + '%'); ^
    } ^
    for($x=0; $x -lt $w; $x++){ ^
        $v = [Math]::Sin($x/$w*6.28)*0.5 + [Math]::Cos($y/$h*6.28)*0.5; ^
        $g = [int][Math]::Max(0,[Math]::Min(255,($v+1)*127.5)); ^
        $b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($g,$g,$g)); ^
    } ^
}; ^
$out = Join-Path (Get-Location) 'test_heightmap_4096.png'; ^
$b.Save($out,[System.Drawing.Imaging.ImageFormat]::Png); ^
$b.Dispose(); ^
Write-Host 'Progresso: 100%'; ^
Write-Host 'Concluído: test_heightmap_4096.png'"

pause