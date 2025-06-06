window.initStep4 = function () {
  const container = document.getElementById('usersContainer');
  if (!container) return;

  // Remover listeners antigos
  removeOldEventListeners();

  // Preencher dados salvos
  populateUsersFromSession();

  // Configurar eventos
  setupUserEventListeners();

  // Atualizar visibilidade do botão
  atualizarVisibilidadeBotaoUser();

  // Salvar ao sair da página
  window.addEventListener('beforeunload', () => {
    currentStep = 4;
    saveStepData();
  });
};

function criarUserCardVazio(index, dados = {}) {
  const card = document.createElement('div');
  card.className = 'user-card mb-3 p-3 border rounded';
  card.innerHTML = `
    <h5>Usuário ${index + 1}</h5>
    <div class="mb-3">
      <label class="form-label">Usuário (login)</label>
      <input data-field="Usuario" type="text" class="form-control" placeholder="Login do Usuario" value="${dados.Usuario || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">E-mail do Usuário</label>
      <input data-field="Email" type="email" class="form-control" placeholder="Email do Usuario" value="${dados.Email || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">Senha</label>
      <input data-field="Senha" type="text" class="form-control" placeholder="Digite Sua Senha" value="${dados.Senha || ''}" required>
    </div>
    <input data-field="PermissionAccount" type="hidden" value="User" />
    ${index > 0 ? `<button type="button" class="btn btn-outline-danger btn-sm remover-user"><span class="material-symbols-outlined">delete</span> Remover</button>` : ''}
  `;
  return card;
}

function setupUserEventListeners() {
  const container = document.getElementById('usersContainer');

  // Adicionar usuário
  document.getElementById('btnAdicionarUser')?.addEventListener('click', () => {
    const limite = parseInt(document.getElementById('userLimit')?.textContent) || 0;
    const atuais = container.querySelectorAll('.user-card').length;

    if (atuais + 1 > limite) {
      showStep4Error(`Você não pode adicionar mais de ${limite} usuários comuns.`);
      return;
    }

    const novoCard = criarUserCardVazio(atuais);
    container.appendChild(novoCard);
    reindexUsers();
    currentStep = 4;
    saveStepData();
  });

  // Remover usuário
  container.addEventListener('click', (e) => {
    if (e.target.closest('.remover-user')) {
      const cards = container.querySelectorAll('.user-card');
      if (cards.length <= 1) {
        showStep4Error('Deve existir ao menos um usuário comum.');
        return;
      }
      e.target.closest('.user-card').remove();
      reindexUsers();
      currentStep = 4;
      saveStepData();
    }
  });

  // Input em campos
  container.addEventListener('input', () => {
    currentStep = 4;
    saveStepData();
  });
}

function reindexUsers() {
  const container = document.getElementById('usersContainer');
  const cards = container.querySelectorAll('.user-card');

  cards.forEach((card, idx) => {
    // Atualizar título
    card.querySelector('h5')?.textContent(`Usuário ${idx + 1}`);

    // Atualizar nomes dos inputs
    const inputs = card.querySelectorAll('input');
    inputs.forEach(input => {
      const field = input.getAttribute('data-field');
      input.name = `Users[${idx}].${field}`;
    });

    // Botão "Remover"
    const btnRemover = card.querySelector('.remover-user');
    if (idx === 0) {
      if (btnRemover) btnRemover.remove();
    } else {
      if (!btnRemover) {
        const btn = document.createElement('button');
        btn.className = 'btn btn-outline-danger btn-sm remover-user';
        btn.innerHTML = `<span class="material-symbols-outlined">delete</span> Remover`;
        card.appendChild(btn);
      }
    }
  });

  // Atualizar contadores
  document.getElementById('userCount')?.textContent(cards.length);
  atualizarVisibilidadeBotaoUser();
}

function populateUsersFromSession() {
  const container = document.getElementById('usersContainer');
  container.innerHTML = '';

  const users = (formData.Users || []).filter(u => u.PermissionAccount === "User");

  if (users.length === 0) {
    container.appendChild(criarUserCardVazio(0));
    reindexUsers();
    return;
  }

  users.forEach((usr, idx) => {
    const card = criarUserCardVazio(idx, {
      Usuario: usr.Usuario,
      Email: usr.Email,
      Senha: usr.Senha
    });
    container.appendChild(card);
  });

  reindexUsers();
}

function validateStep4() {
  clearStep4Messages();
  const cards = document.querySelectorAll('#usersContainer .user-card');

  if (cards.length === 0) {
    showStep4Error('É obrigatório criar ao menos um usuário comum.');
    return false;
  }

  // Conjuntos de logins e e-mails dos administradores
  const adminLogins = new Set((formData.Admins || []).map(a => a.Usuario.trim().toLowerCase()));
  const adminEmails = new Set((formData.Admins || []).map(a => a.Email.trim().toLowerCase()));

  // E-mail da empresa (do Step 1)
  const emailEmpresa = formData.EmailEmpresa?.trim().toLowerCase();

  for (let card of cards) {
    const usuario = card.querySelector('[data-field="Usuario"]')?.value.trim();
    const email = card.querySelector('[data-field="Email"]')?.value.trim();
    const senha = card.querySelector('[data-field="Senha"]')?.value.trim();

    if (!usuario || !email || !senha) {
      showStep4Error('Todos os campos devem ser preenchidos.');
      return false;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      showStep4Error('E-mail inválido.');
      return false;
    }

    // Verificar se o e-mail já existe entre os administradores
    if (adminEmails.has(email.toLowerCase())) {
      showStep4Error(`O e-mail "${email}" já está sendo usado por um administrador.`);
      return false;
    }

    // Verificar se o e-mail é igual ao da empresa
    if (emailEmpresa && email.toLowerCase() === emailEmpresa) {
      showStep4Error(`O e-mail "${email}" já está sendo usado pela empresa.`);
      return false;
    }

    // Verificar complexidade da senha
    if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$/.test(senha)) {
      showStep4Error('Senha deve ter 8+ caracteres, incluindo maiúscula, minúscula e caractere especial.');
      return false;
    }
  }

  showStep4Success('Usuários comuns validados com sucesso.');
  return true;
}

function showStep4Error(mensagem) {
  const error = document.getElementById('step4Error');
  const success = document.getElementById('step4Success');
  if (error && success) {
    error.textContent = mensagem;
    error.classList.remove('d-none');
    success.textContent = '';
    success.classList.add('d-none');
  }
}

function showStep4Success(mensagem) {
  const error = document.getElementById('step4Error');
  const success = document.getElementById('step4Success');
  if (error && success) {
    success.textContent = mensagem;
    success.classList.remove('d-none');
    error.textContent = '';
    error.classList.add('d-none');
  }
}

function clearStep4Messages() {
  const error = document.getElementById('step4Error');
  const success = document.getElementById('step4Success');
  if (error && success) {
    error.textContent = '';
    error.classList.add('d-none');
    success.textContent = '';
    success.classList.add('d-none');
  }
}

function atualizarVisibilidadeBotaoUser() {
  const limite = parseInt(document.getElementById('userLimit')?.textContent) || 0;
  const atuais = document.querySelectorAll('#usersContainer .user-card').length;
  const btn = document.getElementById('btnAdicionarUser');
  if (btn) btn.style.display = (atuais < limite) ? 'inline-block' : 'none';
}

function removeOldEventListeners() {
  $(document).off('click', '#btnAdicionarUser');
  $(document).off('click', '.remover-user');
  $(document).off('input', '#usersContainer input');
}