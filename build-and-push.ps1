# Script para compilar e fazer push automático para o repositório Git
# Para Windows PowerShell

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "CityMapStudio - Build & Git Push" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Compilar o projeto
Write-Host "[1/4] Compilando projeto..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Compilação bem-sucedida" -ForegroundColor Green
} else {
    Write-Host "? Erro na compilação" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 2. Adicionar arquivos modificados
Write-Host "[2/4] Adicionando arquivos ao Git..." -ForegroundColor Yellow
git add -A
Write-Host "? Arquivos adicionados" -ForegroundColor Green
Write-Host ""

# 3. Fazer commit
$commitMessage = "Build: Compilação bem-sucedida - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "[3/4] Fazendo commit..." -ForegroundColor Yellow
try {
    git commit -m $commitMessage
    Write-Host "? Commit realizado" -ForegroundColor Green
} catch {
    Write-Host "! Nenhuma mudança para commitar" -ForegroundColor Yellow
}
Write-Host ""

# 4. Fazer push (se houver repositório remoto)
Write-Host "[4/4] Fazendo push para repositório remoto..." -ForegroundColor Yellow
$remoteExists = git remote -v | Select-String "origin" -Quiet
if ($remoteExists) {
    try {
        git push -u origin master 2>$null
        Write-Host "? Push realizado" -ForegroundColor Green
    } catch {
        Write-Host "! Não foi possível fazer push (verifique suas credenciais)" -ForegroundColor Yellow
    }
} else {
    Write-Host "! Repositório remoto não configurado." -ForegroundColor Yellow
    Write-Host "   Execute os comandos abaixo:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   git remote add origin <URL_DO_REPOSITORIO>" -ForegroundColor Cyan
    Write-Host "   git push -u origin master" -ForegroundColor Cyan
}
Write-Host ""

Write-Host "==========================================" -ForegroundColor Green
Write-Host "Processo concluído com sucesso!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
