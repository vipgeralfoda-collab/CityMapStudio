# ?? Git Repository - CityMapStudio

## ? Repositório Inicializado

```
?? Localização: C:\Users\vinic\source\repos\CityMapStudio
?? Status: Inicializado e pronto
?? Remoto: Ainda não configurado
?? Commits: 2 (inicial + configuração)
```

---

## ?? Usar o Repositório

### Compilar e Fazer Push Automático

```powershell
# Windows PowerShell
.\build-and-push.ps1

# Ou direto no terminal
dotnet build
git add .
git commit -m "feat: descrição da alteração"
git push
```

---

## ?? Conectar a um Repositório Remoto

### Opção 1: GitHub (Recomendado)

```powershell
# 1. Criar repositório em: https://github.com/new

# 2. Conectar local ao remoto
git remote add origin https://github.com/SEU_USUARIO/CityMapStudio.git

# 3. Fazer push
git push -u origin master
```

### Opção 2: GitLab / Bitbucket

```powershell
git remote add origin https://gitlab.com/SEU_USUARIO/CityMapStudio.git
# ou
git remote add origin https://bitbucket.org/SEU_USUARIO/CityMapStudio.git

git push -u origin master
```

---

## ?? Comandos Rápidos

```powershell
# Ver status
git status

# Ver histórico
git log --oneline

# Criar branch
git checkout -b feature/nome-da-feature

# Trocar branch
git checkout master

# Fazer commit
git commit -m "tipo(escopo): descrição"

# Fazer push
git push

# Atualizar do remoto
git pull
```

---

## ?? Padrão de Commits

```
<tipo>(<escopo>): <descrição>

<corpo opcional>

<rodapé opcional>
```

### Tipos:
- `feat`: Nova feature
- `fix`: Correção de bug
- `refactor`: Refatoração
- `perf`: Performance
- `docs`: Documentação
- `test`: Testes
- `chore`: Configuração

### Exemplos:

```
feat(water): add water transparency and reflection
fix(camera): correct viewport positioning for frontal view
perf(mesh): optimize LOD calculation in TerrainMeshGenerator
docs(readme): update installation instructions
```

---

## ?? Autenticação

### SSH (Melhor para automação)

```powershell
# 1. Gerar chave (se não tiver)
ssh-keygen -t ed25519 -C "seu_email@example.com"

# 2. Adicionar chave em GitHub/GitLab > Settings > SSH Keys
Get-Content $env:USERPROFILE\.ssh\id_ed25519.pub | Set-Clipboard

# 3. Usar URL SSH
git remote set-url origin git@github.com:USUARIO/CityMapStudio.git
```

### HTTPS com Token

```powershell
# GitHub: Gerar token em https://github.com/settings/tokens
git remote set-url origin https://TOKEN@github.com/USUARIO/CityMapStudio.git
```

---

## ?? Estrutura de Branches

```
master          - Código estável ?
?? develop      - Integração
?  ?? feature/  - Novas features
?  ?? fix/      - Correções
?? release/     - Preparação de release
```

---

## ?? Fluxo de Trabalho

```powershell
# 1. Atualizar master
git pull origin master

# 2. Criar branch para feature
git checkout -b feature/nova-funcionalidade

# 3. Fazer mudanças e commits
git add .
git commit -m "feat(escopo): descrição"

# 4. Compilar para validar
dotnet build

# 5. Fazer push da branch
git push -u origin feature/nova-funcionalidade

# 6. Criar Pull Request no GitHub/GitLab

# 7. Após merge, atualizar local
git checkout master
git pull origin master
```

---

## ?? Status Atual

| Item | Status |
|------|--------|
| Repositório Local | ? Pronto |
| .gitignore | ? Configurado |
| README.md | ? Criado |
| Commits Iniciais | ? Realizados |
| Repositório Remoto | ? Configure (GitHub/GitLab) |
| Autenticação | ? Configure (SSH/Token) |

---

## ?? Referências

- [Git Documentation](https://git-scm.com/doc)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [GitHub Guides](https://guides.github.com/)
- [GitLab Documentation](https://docs.gitlab.com/)

---

## ?? Próximas Ações

1. **Escolha um repositório remoto** (GitHub recomendado)
2. **Configure autenticação** (SSH recomendado)
3. **Crie repositório remoto** com o mesmo nome: `CityMapStudio`
4. **Execute**: `git remote add origin <URL>` e `git push -u origin master`
5. **Agora**: Use os scripts para compilar e fazer push automático

---

**Última atualização**: 2024
**Versão Git**: 2.40+
