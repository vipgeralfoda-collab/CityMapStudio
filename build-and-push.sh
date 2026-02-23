#!/bin/bash
# Script para compilar e fazer push automático para o repositório Git

set -e  # Exit on error

echo "=========================================="
echo "CityMapStudio - Build & Git Push"
echo "=========================================="
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Compilar o projeto
echo -e "${YELLOW}[1/4]${NC} Compilando projeto..."
dotnet build --configuration Release
if [ $? -eq 0 ]; then
    echo -e "${GREEN}? Compilação bem-sucedida${NC}"
else
    echo -e "${RED}? Erro na compilação${NC}"
    exit 1
fi
echo ""

# 2. Adicionar arquivos modificados
echo -e "${YELLOW}[2/4]${NC} Adicionando arquivos ao Git..."
git add -A
echo -e "${GREEN}? Arquivos adicionados${NC}"
echo ""

# 3. Fazer commit
COMMIT_MESSAGE="Build: Compilação bem-sucedida - $(date '+%Y-%m-%d %H:%M:%S')"
echo -e "${YELLOW}[3/4]${NC} Fazendo commit..."
git commit -m "$COMMIT_MESSAGE" || echo -e "${YELLOW}! Nenhuma mudança para commitar${NC}"
echo ""

# 4. Fazer push (se houver repositório remoto)
echo -e "${YELLOW}[4/4]${NC} Fazendo push para repositório remoto..."
if git remote -v | grep -q "origin"; then
    git push -u origin master 2>/dev/null || echo -e "${YELLOW}! Não foi possível fazer push (repositório remoto não configurado)${NC}"
    echo -e "${GREEN}? Push realizado${NC}"
else
    echo -e "${YELLOW}! Repositório remoto não configurado. Execute:${NC}"
    echo "   git remote add origin <URL_DO_REPOSITORIO>"
    echo "   git push -u origin master"
fi
echo ""

echo -e "${GREEN}=========================================="
echo "Processo concluído com sucesso!"
echo "==========================================${NC}"
