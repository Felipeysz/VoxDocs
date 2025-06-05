// step4.js

// Inicializa o Step 4 (Usuários Comuns)
window.initStep4 = function() {
  const container = document.getElementById('usersContainer');
  if (!container) return;

  // Remove event listeners antigos, para não duplicar
  $(document).off('click', '#btnAdicionarUser');
  $(document).off('click', '.remover-user');
  $(document).off('input', '#usersContainer input');

  // 1) Popula com dados já salvos (mesmo vazios)
  populateUsersFromSession();

  // 2) Seta os event listeners de clique e input
  setupUserEventListeners();

  // 3) Atualiza logo de cara o botão “Adicionar Usuário”
  atualizarVisibilidadeBotaoUser();

  // 4) Ao sair/atualizar página, salva automaticamente
  window.addEventListener('beforeunload', function() {
    currentStep = 4;
    saveStepData();
  });
};

// Configura os listeners para adicionar, remover e input de campos
function setupUserEventListeners() {
  // Ao clicar em “Adicionar Usuário”
  $(document).on('click', '#btnAdicionarUser', function() {
    const container = document.getElementById('usersContainer');
    const limite = parseInt(document.getElementById('userLimit')?.textContent) || 0;
    const atuaisCards = container.querySelectorAll('.user-card').length;

    if (atuaisCards + 1 > limite) {
      showStep4Error(`Você não pode adicionar mais de ${limite} usuários comuns.`);
      return;
    }

    clearStep4Messages();

    // Cria um novo user-card vazio e anexa
    const novoCard = criarUserCardVazio(atuaisCards);
    container.appendChild(novoCard);
    novoCard.scrollIntoView({ behavior: 'smooth' });

    // Reindexa todos, para ajustar título e name dos inputs
    reindexUsers();

    // Salva imediatamente
    currentStep = 4;
    saveStepData();
  });

  // Ao clicar em “Remover” em qualquer card
  $(document).on('click', '.remover-user', function() {
    const cardsAntes = document.querySelectorAll('#usersContainer .user-card').length;
    if (cardsAntes <= 1) {
      showStep4Error('Deve existir ao menos um usuário comum.');
      return;
    }

    clearStep4Messages();
    $(this).closest('.user-card').remove();

    // Reindexa após remoção
    reindexUsers();

    // Salva imediatamente
    currentStep = 4;
    saveStepData();
  });

  // Ao digitar/alterar qualquer input dentro de #usersContainer
  $(document).on('input', '#usersContainer input', function() {
    clearStep4Messages();
    currentStep = 4;
    saveStepData();
  });
}

// Mostra ou esconde o botão “Adicionar Usuário” conforme limite
function atualizarVisibilidadeBotaoUser() {
  const limite = parseInt(document.getElementById('userLimit')?.textContent) || 0;
  const atuaisCards = document.querySelectorAll('#usersContainer .user-card').length;
  const btnAdicionar = document.getElementById('btnAdicionarUser');

  if (btnAdicionar) {
    btnAdicionar.style.display = (atuaisCards < limite) ? 'inline-block' : 'none';
  }
}

// Reindexa todos os user-cards: ajusta título, name dos inputs e botão “Remover”
function reindexUsers() {
  const container = document.getElementById('usersContainer');
  const cards = container.querySelectorAll('.user-card');

  cards.forEach((card, idx) => {
    // 1) Atualiza o título “Usuário X”
    const heading = card.querySelector('h5');
    if (heading) heading.textContent = `Usuário ${idx + 1}`;

    // 2) Reatribui name mantendo data-field:
    const inpUsuario = card.querySelector('input[data-field="Usuario"]');
    if (inpUsuario) inpUsuario.name = `Users[${idx}].Usuario`;

    const inpEmail = card.querySelector('input[data-field="Email"]');
    if (inpEmail) inpEmail.name = `Users[${idx}].Email`;

    const inpSenha = card.querySelector('input[data-field="Senha"]');
    if (inpSenha) inpSenha.name = `Users[${idx}].Senha`;

    const inpPerm = card.querySelector('input[data-field="PermissionAccount"]');
    if (inpPerm) inpPerm.name = `Users[${idx}].PermissionAccount`;

    // 3) Botão “Remover” só aparece se idx > 0
    let btnRemover = card.querySelector('.remover-user');
    if (idx === 0) {
      if (btnRemover) btnRemover.remove();
    } else {
      if (!btnRemover) {
        btnRemover = document.createElement('button');
        btnRemover.type = 'button';
        btnRemover.className = 'btn btn-outline-danger btn-sm remover-user';
        btnRemover.innerHTML = `<span class="material-symbols-outlined">delete</span> Remover`;
        card.appendChild(btnRemover);
      }
    }
  });

  // 4) Atualiza o contador visível
  document.getElementById('userCount').textContent = cards.length;
  atualizarVisibilidadeBotaoUser();
}

// Popula a lista de user-cards a partir de formData.Users
function populateUsersFromSession() {
  const container = document.getElementById('usersContainer');
  container.innerHTML = '';

  // Extraí do formData todos que têm PermissionAccount === "User"
  const todosUsers = (formData.Users || []).filter(u => u.PermissionAccount === "User");

  // Se não há nenhum registrado, criamos 1 card vazio
  if (todosUsers.length === 0) {
    const primeiro = criarUserCardVazio(0);
    container.appendChild(primeiro);
    reindexUsers();
    return;
  }

  // Recria o card para cada objeto
  todosUsers.forEach((usr, idx) => {
    const card = criarUserCardVazio(idx, {
      Usuario: usr.Usuario,
      Email: usr.Email,
      Senha: usr.Senha
    });
    container.appendChild(card);
  });

  // Após anexar todos, reindexa
  reindexUsers();
}

// Cria um elemento .user-card (vazio ou com dados) sem atribuir name,
// apenas com data-field, deixando reindexar cuidar do name correto depois.
function criarUserCardVazio(index, dados = {}) {
  const card = document.createElement('div');
  card.className = 'user-card mb-3 p-3 border rounded';
  card.innerHTML = `
    <h5>Usuário ${index + 1}</h5>
    <div class="mb-3">
      <label class="form-label">Usuário (login)</label>
      <input data-field="Usuario" type="text" class="form-control"
             placeholder="user01" value="${dados.Usuario || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">E-mail do Usuário</label>
      <input data-field="Email" type="email" class="form-control"
             placeholder="user@empresa.com" value="${dados.Email || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">Senha</label>
      <input data-field="Senha" type="password" class="form-control"
             placeholder="••••••••" value="${dados.Senha || ''}" required>
    </div>
    <input data-field="PermissionAccount" type="hidden" value="User" />
    ${index > 0
      ? `<button type="button" class="btn btn-outline-danger btn-sm remover-user">
           <span class="material-symbols-outlined">delete</span> Remover
         </button>`
      : ''}
  `;
  return card;
}

// Exibe mensagem de erro na view (id="step4Error")
function showStep4Error(mensagem) {
  const errorDiv   = document.getElementById('step4Error');
  const successDiv = document.getElementById('step4Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = mensagem;
  errorDiv.classList.remove('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

// Exibe mensagem de sucesso na view (id="step4Success")
function showStep4Success(mensagem) {
  const errorDiv   = document.getElementById('step4Error');
  const successDiv = document.getElementById('step4Success');
  if (!errorDiv || !successDiv) return;
  successDiv.textContent = mensagem;
  successDiv.classList.remove('d-none');
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
}

// Limpa mensagens de erro ou sucesso do Step 4
function clearStep4Messages() {
  const errorDiv   = document.getElementById('step4Error');
  const successDiv = document.getElementById('step4Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

// Validação antes de avançar: ao menos 1 usuário com login, e-mail e senha preenchidos
// e senha com complexidade mínima (8 caracteres, 1 maiúscula, 1 minúscula, 1 especial).
function validateStep4() {
  clearStep4Messages();
  const cards = document.querySelectorAll('#usersContainer .user-card');

  if (cards.length === 0) {
    showStep4Error('É obrigatório criar ao menos um usuário comum.');
    return false;
  }

  // Conjuntos contendo todos os logins e e-mails dos administradores
  const adminLogins = new Set((formData.Admins || []).map(a => a.Usuario.trim().toLowerCase()));
  const adminEmails = new Set((formData.Admins || []).map(a => a.Email.trim().toLowerCase()));

  for (let card of cards) {
    const nomeUsuario = card.querySelector('input[data-field="Usuario"]')?.value.trim() || '';
    const email       = card.querySelector('input[data-field="Email"]')?.value.trim() || '';
    const senha       = card.querySelector('input[data-field="Senha"]')?.value.trim() || '';

    if (!nomeUsuario) {
      showStep4Error('Cada usuário comum deve ter um login preenchido.');
      return false;
    }
    if (!email) {
      showStep4Error('Cada usuário comum deve ter um e-mail válido.');
      return false;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      showStep4Error('Informe um e-mail válido para cada usuário comum.');
      return false;
    }
    if (!senha) {
      showStep4Error('Cada usuário comum deve ter uma senha preenchida.');
      return false;
    }

    // Verifica se o login ou e-mail já existem entre os administradores
    if (adminLogins.has(nomeUsuario.toLowerCase())) {
      showStep4Error(`O login "${nomeUsuario}" já está sendo usado por um administrador.`);
      return false;
    }
    if (adminEmails.has(email.toLowerCase())) {
      showStep4Error(`O e-mail "${email}" já está sendo usado por um administrador.`);
      return false;
    }

    // ======== validação de complexidade de senha ========
    const senhaComplexaRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$/;
    if (!senhaComplexaRegex.test(senha)) {
      showStep4Error(
        'A senha deve ter no mínimo 8 caracteres, ' +
        'incluindo uma letra maiúscula, uma letra minúscula e um caractere especial.'
      );
      return false;
    }
    // ====================================================
  }

  showStep4Success('Usuários comuns validados com sucesso.');
  return true;
}
