# Script para criar repositório no GitHub e fazer push
# Execute este script para automatizar tudo!

Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     CityMapStudio - GitHub Repository Creator             ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Verificar se GitHub CLI está instalado
Write-Host "[1/3] Verificando GitHub CLI..." -ForegroundColor Yellow
$ghPath = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghPath) {
    Write-Host "? GitHub CLI não encontrado!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Opção 1: Instale manualmente em https://github.com/cli/cli" -ForegroundColor Yellow
    Write-Host "Opção 2: Use o comando:" -ForegroundColor Yellow
    Write-Host "   winget install GitHub.cli" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}
Write-Host "? GitHub CLI encontrado: $(gh --version)" -ForegroundColor Green
Write-Host ""

# Fazer login no GitHub
Write-Host "[2/3] Fazendo login no GitHub..." -ForegroundColor Yellow
Write-Host "   Uma aba do navegador será aberta para autenticação" -ForegroundColor Gray
Write-Host "   Autentique e autorize o GitHub CLI" -ForegroundColor Gray
Write-Host ""

try {
    gh auth login
    Write-Host "? Login realizado com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "? Erro ao fazer login" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Criar repositório no GitHub
Write-Host "[3/3] Criando repositório CityMapStudio no GitHub..." -ForegroundColor Yellow
Write-Host ""

try {
    cd C:\Users\vinic\source\repos\CityMapStudio
    
    # Criar repositório público com descrição
    gh repo create CityMapStudio `
        --public `
        --source=. `
        --description="WPF application for terrain and map editing with Cities: Skylines 2 export" `
        --push `
        --remote=origin
    
    Write-Host ""
    Write-Host "? Repositório criado com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Resumo:" -ForegroundColor Cyan
    Write-Host "   • Repositório: https://github.com/vipgeralfoda-collab/CityMapStudio" -ForegroundColor Green
    Write-Host "   • Commits: 7" -ForegroundColor Green
    Write-Host "   • Arquivos: 50+" -ForegroundColor Green
    Write-Host "   • Branch: master" -ForegroundColor Green
    Write-Host ""
    Write-Host "? Código já foi enviado automaticamente!" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "? Erro ao criar repositório: $_" -ForegroundColor Red
    exit 1
}

Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?              SUCESSO! ??                                   ?" -ForegroundColor Green
Write-Host "?  Seu repositório está online no GitHub!                   ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green
