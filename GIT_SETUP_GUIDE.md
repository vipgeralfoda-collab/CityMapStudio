# ?? Guia de Configuração do Git - CityMapStudio

## ? Status Atual

O repositório Git local foi **inicializado com sucesso** e contém um commit inicial com todo o código.

```
Repositório: C:\Users\vinic\source\repos\CityMapStudio
Branch: master
Commits: 1 (inicial)
Status: Pronto para conectar a um repositório remoto
```

---

## ?? Próximos Passos: Configurar Repositório Remoto

### Opção 1: GitHub (Recomendado)

#### 1.1 Criar Repositório no GitHub

1. Acesse [https://github.com/new](https://github.com/new)
2. Preencha os campos:
   - **Repository name**: `CityMapStudio`
   - **Description**: `WPF application for terrain and map editing with CS2 export`
   - **Visibility**: Public ou Private (sua escolha)
   - **NÃO** inicialize com README, .gitignore ou LICENSE (já temos)

3. Clique em **Create repository**

#### 1.2 Conectar ao Repositório Local

```powershell
# Abra PowerShell na pasta do projeto
cd C:\Users\vinic\source\repos\CityMapStudio

# Adicione a URL do repositório remoto
git remote add origin https://github.com/SEU_USUARIO/CityMapStudio.git

# Ou se usar SSH (mais seguro):
git remote add origin git@github.com:SEU_USUARIO/CityMapStudio.git
```

#### 1.3 Fazer Push para o GitHub

```powershell
# Fazer push do branch master
git push -u origin master

# Para futuros pushes, basta:
git push
```

---

### Opção 2: GitLab

#### 2.1 Criar Repositório no GitLab

1. Acesse [https://gitlab.com/projects/new](https://gitlab.com/projects/new)
2. Preencha os dados similares
3. Clique em **Create project**

#### 2.2 Conectar

```powershell
git remote add origin https://gitlab.com/SEU_USUARIO/CityMapStudio.git
git push -u origin master
```

---

### Opção 3: Bitbucket

```powershell
git remote add origin https://bitbucket.org/SEU_USUARIO/CityMapStudio.git
git push -u origin master
```

---

## ?? Configurar Autenticação

### SSH (Recomendado para automação)

```powershell
# Gerar chave SSH (se não tiver)
ssh-keygen -t ed25519 -C "seu_email@example.com"

# Copiar a chave pública
Get-Content $env:USERPROFILE\.ssh\id_ed25519.pub

# Adicionar no GitHub/GitLab/Bitbucket em Settings > SSH Keys
```

### HTTPS com Token (GitHub)

1. Gere um token em: [GitHub Settings > Developer settings > Personal access tokens](https://github.com/settings/tokens)
2. Selecione escopos: `repo`, `workflow`
3. Use como senha quando solicitado:

```powershell
git remote set-url origin https://USUARIO:TOKEN@github.com/USUARIO/CityMapStudio.git
```

---

## ?? Comandos Git Essenciais

### Verificar Status

```powershell
git status
git log --oneline
git remote -v
```

### Criar Branch para Novas Features

```powershell
# Criar e mudar para nova branch
git checkout -b feature/minha-feature

# Fazer mudanças...

# Commit
git add .
git commit -m "feat: descrição da feature"

# Push
git push -u origin feature/minha-feature

# Depois criar Pull Request no GitHub/GitLab
```

### Atualizar do Repositório Remoto

```powershell
# Atualizar master
git pull origin master

# Resolver conflitos se necessário
git merge --continue
```

---

## ?? Automação: Build & Push Automático

### Windows PowerShell

```powershell
# Execute o script após fazer mudanças
.\build-and-push.ps1

# Ou crie um atalho no Visual Studio
# Tools > External Tools > Add
# Title: Build & Push
# Command: powershell
# Arguments: -ExecutionPolicy Bypass -File build-and-push.ps1
# Initial directory: $(SolutionDir)
```

### Git Hooks (Automático após commit)

```powershell
# Criar hook para fazer push automático
echo "git push" > .git/hooks/post-commit
chmod +x .git/hooks/post-commit
```

---

## ?? Estrutura de Commits Recomendada

Use o padrão **Conventional Commits**:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Exemplos:

```
feat(water): add water transparency to rendering
fix(camera): correct Z-axis positioning for frontal view
refactor(mesh): optimize normal calculation in TerrainMeshGenerator
docs(readme): update installation instructions
test(heightmap): add unit tests for smoothing service
perf(lod): improve LOD controller performance
```

### Types:
- `feat`: Nova feature
- `fix`: Correção de bug
- `refactor`: Refatoração de código
- `perf`: Melhorias de performance
- `docs`: Documentação
- `test`: Testes
- `style`: Formatação (sem lógica)
- `chore`: Dependências e configuração

---

## ?? Branches Recomendadas

```
master/main          - Código estável, pronto para produção
?? develop          - Branch de integração
?  ?? feature/*     - Novas features
?  ?? fix/*         - Correções de bugs
?  ?? refactor/*    - Refatorações
?? release/*        - Preparação de releases
```

---

## ?? Fluxo de Trabalho Recomendado

```powershell
# 1. Criar branch para feature
git checkout -b feature/nova-funcionalidade

# 2. Fazer mudanças e commits regulares
git add .
git commit -m "feat: implementar nova funcionalidade"

# 3. Compilar para validar
dotnet build

# 4. Fazer push
git push -u origin feature/nova-funcionalidade

# 5. Criar Pull Request no GitHub/GitLab

# 6. Após aprovação, fazer merge para develop/master
git checkout master
git pull origin master
git merge feature/nova-funcionalidade
git push origin master

# 7. Deletar branch de feature
git branch -d feature/nova-funcionalidade
git push origin --delete feature/nova-funcionalidade
```

---

## ? Status do Projeto

| Componente | Status |
|-----------|--------|
| Repositório Local | ? Inicializado |
| .gitignore | ? Criado |
| README.md | ? Criado |
| Primeiro Commit | ? Realizado |
| Repositório Remoto | ? Aguardando configuração |
| GitHub/GitLab | ? A conectar |
| CI/CD | ? Futuro |

---

## ?? Próximas Ações

1. **Hoje**: Configurar repositório remoto (GitHub/GitLab/Bitbucket)
2. **Depois**: Configurar GitHub Actions para CI/CD automático
3. **Futuro**: Adicionar testes automatizados e cobertura de código

---

## ?? Suporte

Se tiver dúvidas com Git:

```bash
git help <comando>
# Exemplo:
git help push
git help branch
```

Ou consulte: [Git Documentation](https://git-scm.com/doc)

---

**Criado em**: 2024
**Última atualização**: Agora
