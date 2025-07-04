# VoxDocs – Sistema de Gerenciamento de Documentos

## 1. Visão Geral
O **VoxDocs** é uma plataforma web desenvolvida em ASP.NET Core MVC que atua como um “cofre de documentos” seguro para empresas. Sua arquitetura é dividida em:
- **Interface MVC** (front‑end Razor + HTML/CSS/Bootstrap)
- **API REST** (endpoints que realizam operações CRUD e gerenciam autenticação)

## 2. Tecnologias Utilizadas
- **Back‑end:** ASP.NET Core MVC (.NET 9)
- **Banco de dados:** SQL Server
- **Armazenamento de arquivos:** Azure Blob Storage
- **Front‑end:** Razor Views com Bootstrap 5, AOS, Material Icons
- **Scripts:** JavaScript (validações, modais, AOS init)

## 3. Instalação e Execução
```bash
git clone https://github.com/Felipeysz/VoxDocs.git
cd VoxDocs/backend
dotnet restore
dotnet run
```
Acesse `https://localhost:5001` (ou porta exibida no console).

## 4. Configuração
No arquivo `appsettings.json`, ajuste a conexão ao Azure Blob:
```json
"AzureBlobStorage": {
  "ConnectionString": "<sua-chave>",
  "ContainerName": "documentos-container"
}
```
> **Importante:** Nunca compartilhe sua ConnectionString em repositórios públicos.

## 5. Funcionalidades Atuais

### 5.1. Sistema de Autenticação
- **Registro** de novos usuários (nome, e-mail, senha).
- **Login** via `/api/User/Login` retorna cookie de autenticação.
- **Logout** encerra a sessão via `/api/User/Logout`.
- Todas as páginas MVC protegidas usam `[Authorize]`.

### 5.2. Gerenciamento de Documentos
- **Listagem** de áreas e tipos de documentos.
- **Filtragem** de documentos por área/tipo.
- **Upload** de arquivos via `/api/UploadDocumentos/upload`:
  - Valida tipo e extensão.
  - Renomeia arquivo conforme padrão: `DOC[Tipo][MMyyyy]_[Área].[ext]`.
  - Armazena no Azure Blob e registra no banco.
- **Exibição** em Razor Views com partial views e scripts de passo a passo.

### 5.3. Perfil do Usuário (Meu Perfil)
- Página **Meu Perfil**: visualize e edite seus dados pessoais (nome, e-mail).
- **Alteração de senha** diretamente pelo perfil.
- Preferências de notificação salvas em `localStorage`.
- Histórico de acesso e atividades recentes exibidos em tabela.
- Visualização dos documentos salvos offline e opção para gerenciar esses arquivos.

### 5.4. Documentos Offline
- **Salvar documentos para acesso offline**: clique no botão <span class="material-symbols-outlined align-middle" style="font-size:1.1em;">save</span> ao lado de cada documento.
- Gerenciamento dos documentos salvos offline diretamente pelo perfil.
- Feedback visual de progresso e sucesso ao salvar offline.

### 5.5. Criptografia de Documentos
- Todos os documentos são **criptografados automaticamente** no upload e no armazenamento.
- Garantia de privacidade e segurança máxima dos dados da empresa.

### 5.6. Administração
- **Dashboard Administrativa**: painel para administradores com gráficos de uso, uploads, atividades recentes e relatórios.
- **CRUD de usuários** via `/api/User` (somente administradores).
- Controle de permissões (roles e claims).
- Mensagens de erro e feedback via TempData.

## 6. Endpoints Principais da API

| Endpoint                                    | Método | Descrição                                           |
|---------------------------------------------|--------|-----------------------------------------------------|
| `/api/AreasDocumento`                       | GET    | Lista todas as áreas de documento                   |
| `/api/AreasDocumento/{id}`                  | GET    | Detalhes de uma área                                |
| `/api/AreasDocumento`                       | POST   | Cria nova área                                      |
| `/api/TipoDocumento`                        | GET    | Lista tipos (PDF, DOCX, etc.)                       |
| `/api/TipoDocumento/{id}`                   | GET    | Detalhes de um tipo                                 |
| `/api/Documento`                            | GET    | Lista todos os documentos                           |
| `/api/Documento/{id}`                       | GET    | Detalhes de um documento                            |
| `/api/Documento/filter?areaId=&tipoId=`     | GET    | Filtra documentos por área & tipo                   |
| `/api/UploadDocumentos/upload`              | POST   | Upload de arquivo multipart/form-data               |
| `/api/User/Register`                        | POST   | Registra usuário                                    |
| `/api/User/Login`                           | POST   | Login e criação de sessão                           |
| `/api/User/Logout`                          | POST   | Logout                                              |
| `/api/User/GetUsers`                        | GET    | Lista usuários (admin)                              |
| `/api/User/UpdateUser/{id}`                 | PUT    | Atualiza dados de usuário                           |
| `/api/User/DeleteUser/{id}`                 | DELETE | Exclui usuário                                      |
| `/api/User/GeneratePasswordResetLink`       | POST   | Gera link para redefinição de senha via e‑mail      |
| `/api/User/ResetPasswordWithToken`          | POST   | Redefine senha usando token                         |
| `/api/User/GetTokenExpiration?token=xyz`    | GET    | Retorna data de expiração do token                  |
| `/api/User/GetUserByUsernameAsync?username=`| GET    | Retorna dados básicos do usuário autenticado        |

## 7. Próximas Atualizações (12–24 horas)
- **VoxChat:** chat inteligente integrado para perguntas sobre documentos.
- **Planos de Pagamento:** sistema de assinatura com seleção e gestão de planos.

---

_Desenvolvido por VoxDocs Team • 2025_