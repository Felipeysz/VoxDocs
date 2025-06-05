// step3.js

// Inicializa o Step 3 (Administradores)
window.initStep3 = function() {
  const container = document.getElementById('adminsContainer');
  if (!container) return;

  // 1) Remove event listeners antigos para evitar duplicação
  $(document).off('click', '#btnAdicionarAdmin');
  $(document).off('click', '.remover-admin');
  $(document).off('input', '#adminsContainer input');

  // 2) Preenche com os dados já salvos (mesmo vazios)
  populateAdminsFromSession();

  // 3) Seta os event listeners de clique e input
  setupAdminEventListeners();

  // 4) Atualiza logo de cara o botão “Adicionar Admin”
  atualizarVisibilidadeBotaoAdmin();

  // 5) Ao sair/atualizar a página, salva automaticamente
  window.addEventListener('beforeunload', function() {
    currentStep = 3;
    saveStepData();
  });
};

// Configura os listeners para adicionar, remover e input de campos
function setupAdminEventListeners() {
  // Ao clicar em “Adicionar Administrador”
  $(document).on('click', '#btnAdicionarAdmin', function() {
    const container = document.getElementById('adminsContainer');
    const limite = parseInt(document.getElementById('adminLimit')?.textContent) || 0;
    const atuaisCards = container.querySelectorAll('.admin-card').length;

    if (atuaisCards + 1 > limite) {
      showStep3Error(`Você não pode adicionar mais de ${limite} administradores.`);
      return;
    }

    clearStep3Messages();

    // Cria um novo card (vazio) e anexa
    const novoCard = criarAdminCardVazio(atuaisCards);
    container.appendChild(novoCard);
    novoCard.scrollIntoView({ behavior: 'smooth' });

    // Reindexa todos para ajustar título, names e botão do primeiro
    reindexAdmins();

    // Salva imediatamente
    currentStep = 3;
    saveStepData();
  });

  // Ao clicar em “Remover” em qualquer card
  $(document).on('click', '.remover-admin', function() {
    const cardsAntes = document.querySelectorAll('#adminsContainer .admin-card').length;
    if (cardsAntes <= 1) {
      showStep3Error('Deve existir ao menos um administrador.');
      return;
    }

    clearStep3Messages();
    $(this).closest('.admin-card').remove();

    // Reindexa após remoção
    reindexAdmins();

    // Salva imediatamente
    currentStep = 3;
    saveStepData();
  });

  // Ao digitar/alterar qualquer input dentro de #adminsContainer
  $(document).on('input', '#adminsContainer input', function() {
    clearStep3Messages();
    currentStep = 3;
    saveStepData();
  });
}

// Mostra ou esconde o botão “Adicionar Administrador” conforme limite
function atualizarVisibilidadeBotaoAdmin() {
  const limite = parseInt(document.getElementById('adminLimit')?.textContent) || 0;
  const atuaisCards = document.querySelectorAll('#adminsContainer .admin-card').length;
  const btnAdicionar = document.getElementById('btnAdicionarAdmin');

  if (btnAdicionar) {
    btnAdicionar.style.display = (atuaisCards < limite) ? 'inline-block' : 'none';
  }
}

// Reindexa todos os cards: ajusta título, name dos inputs e botão “Remover”
function reindexAdmins() {
  const container = document.getElementById('adminsContainer');
  const cards = container.querySelectorAll('.admin-card');

  cards.forEach((card, idx) => {
    // 1) Atualiza o título “Administrador X”
    const heading = card.querySelector('h5');
    if (heading) heading.textContent = `Administrador ${idx + 1}`;

    // 2) Reatribui name mantendo data-field:
    const inpUsuario = card.querySelector('input[data-field="Usuario"]');
    if (inpUsuario) inpUsuario.name = `Admins[${idx}].Usuario`;

    const inpEmail = card.querySelector('input[data-field="Email"]');
    if (inpEmail) inpEmail.name = `Admins[${idx}].Email`;

    const inpSenha = card.querySelector('input[data-field="Senha"]');
    if (inpSenha) inpSenha.name = `Admins[${idx}].Senha`;

    const inpPerm = card.querySelector('input[data-field="PermissionAccount"]');
    if (inpPerm) inpPerm.name = `Admins[${idx}].PermissionAccount`;

    // 3) Botão “Remover” só aparece se idx > 0
    let btnRemover = card.querySelector('.remover-admin');
    if (idx === 0) {
      if (btnRemover) btnRemover.remove();
    } else {
      if (!btnRemover) {
        btnRemover = document.createElement('button');
        btnRemover.type = 'button';
        btnRemover.className = 'btn btn-outline-danger btn-sm remover-admin';
        btnRemover.innerHTML = `<span class="material-symbols-outlined">delete</span> Remover`;
        card.appendChild(btnRemover);
      }
    }
  });

  // 4) Atualiza o contador visível
  document.getElementById('adminCount').textContent = cards.length;
  atualizarVisibilidadeBotaoAdmin();
}

// Popula a lista de cards de admin a partir de formData.Admins
function populateAdminsFromSession() {
  const container = document.getElementById('adminsContainer');
  container.innerHTML = '';

  // Extraí do formData todos que têm PermissionAccount === "Admin"
  const todosAdmins = (formData.Admins || []).filter(u => u.PermissionAccount === "Admin");

  // Se não há nenhum registrado, criamos 1 card vazio
  if (todosAdmins.length === 0) {
    const primeiro = criarAdminCardVazio(0);
    container.appendChild(primeiro);
    reindexAdmins();
    return;
  }

  // Recria o card para cada objeto
  todosAdmins.forEach((adm, idx) => {
    const card = criarAdminCardVazio(idx, {
      Usuario: adm.Usuario,
      Email: adm.Email,
      Senha: adm.Senha
    });
    container.appendChild(card);
  });

  // Após anexar todos, reindexa
  reindexAdmins();
}

// Cria um elemento .admin-card (vazio ou com dados) sem atribuir name,
// apenas com data-field, deixando reindexar cuidar do name correto depois.
function criarAdminCardVazio(index, dados = {}) {
  const card = document.createElement('div');
  card.className = 'admin-card mb-3 p-3 border rounded';
  card.innerHTML = `
    <h5>Administrador ${index + 1}</h5>
    <div class="mb-3">
      <label class="form-label">Usuário (login)</label>
      <input data-field="Usuario" type="text" class="form-control"
             placeholder="admin01" value="${dados.Usuario || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">E-mail do Administrador</label>
      <input data-field="Email" type="email" class="form-control"
             placeholder="admin@empresa.com" value="${dados.Email || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">Senha</label>
      <input data-field="Senha" type="password" class="form-control"
             placeholder="••••••••" value="${dados.Senha || ''}" required>
    </div>
    <input data-field="PermissionAccount" type="hidden" value="Admin" />
    ${index > 0
      ? `<button type="button" class="btn btn-outline-danger btn-sm remover-admin">
           <span class="material-symbols-outlined">delete</span> Remover
         </button>`
      : ''}
  `;
  return card;
}

// Mostra mensagem de erro (no estilo Step 1) na view
function showStep3Error(mensagem) {
  const errorDiv = document.getElementById('step3Error');
  const successDiv = document.getElementById('step3Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = mensagem;
  errorDiv.classList.remove('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

// Mostra mensagem de sucesso (no estilo Step 1) na view
function showStep3Success(mensagem) {
  const errorDiv = document.getElementById('step3Error');
  const successDiv = document.getElementById('step3Success');
  if (!errorDiv || !successDiv) return;
  successDiv.textContent = mensagem;
  successDiv.classList.remove('d-none');
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
}

// Limpa mensagens de erro ou sucesso
function clearStep3Messages() {
  const errorDiv = document.getElementById('step3Error');
  const successDiv = document.getElementById('step3Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

// Validação antes de avançar: ao menos 1 admin com login, e-mail e senha preenchidos
function validateStep3() {
  clearStep3Messages();
  const cards = document.querySelectorAll('#adminsContainer .admin-card');

  if (cards.length === 0) {
    showStep3Error('É obrigatório criar ao menos um administrador.');
    return false;
  }

  for (let card of cards) {
    const nomeUsuario = card.querySelector('input[data-field="Usuario"]')?.value.trim() || '';
    const email       = card.querySelector('input[data-field="Email"]')?.value.trim() || '';
    const senha       = card.querySelector('input[data-field="Senha"]')?.value.trim() || '';

    if (!nomeUsuario) {
      showStep3Error('Cada administrador deve ter um login preenchido.');
      return false;
    }
    if (!email) {
      showStep3Error('Cada administrador deve ter um e-mail válido.');
      return false;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      showStep3Error('Informe um e-mail vál ido para cada administrador.');
      return false;
    }
    if (!senha) {
      showStep3Error('Cada administrador deve ter uma senha preenchida.');
      return false;
    }
    // ======== validação de complexidade de senha ========
    const senhaComplexaRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$/;
    if (!senhaComplexaRegex.test(senha)) {
      showStep3Error(
        'A senha deve ter no mínimo 8 caracteres, ' +
        'incluindo uma letra maiúscula, uma letra minúscula e um caractere especial.'
      );
      return false;
    }
    // ====================================================
  }

  showStep3Success('Administradores validados com sucesso.');
  return true;
}
