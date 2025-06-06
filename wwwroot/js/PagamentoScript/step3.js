function clearStep3Messages() {
  const error = document.getElementById('error3');
  const success = document.getElementById('success3');
  if (error) error.classList.add('d-none');
  if (success) success.classList.add('d-none');
}

function showStep3Error(message) {
  const error = document.getElementById('error3');
  if (error) {
    error.textContent = message;
    error.classList.remove('d-none');
  }
}

function showStep3Success(message) {
  const success = document.getElementById('success3');
  if (success) {
    success.textContent = message;
    success.classList.remove('d-none');
  }
}

window.initStep3 = function () {
  const container = document.getElementById('adminsContainer');
  if (!container) return;

  // Remover listeners antigos
  removeOldEventListeners();

  // Preencher dados salvos
  populateAdminsFromSession();

  // Configurar eventos
  setupAdminEventListeners();

  // Atualizar visibilidade do botão
  atualizarVisibilidadeBotaoAdmin();

  // Salvar ao sair da página
  window.addEventListener('beforeunload', () => {
    currentStep = 3;
    saveStepData();
  });
};

function criarAdminCardVazio(index, dados = {}) {
  const card = document.createElement('div');
  card.className = 'admin-card mb-3 p-3 border rounded';
  card.innerHTML = `
    <h5>Administrador ${index + 1}</h5>
    <div class="mb-3">
      <label class="form-label">Usuário (login)</label>
      <input data-field="Usuario" type="text" class="form-control" placeholder="Login do Admin" value="${dados.Usuario || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">E-mail do Administrador</label>
      <input data-field="Email" type="email" class="form-control" placeholder="Email do Admin" value="${dados.Email || ''}" required>
    </div>
    <div class="mb-3">
      <label class="form-label">Senha</label>
      <input data-field="Senha" type="text" class="form-control" placeholder="Digite Sua Senha" value="${dados.Senha || ''}" required>
    </div>
    <input data-field="PermissionAccount" type="hidden" value="Admin" />
    ${index > 0 ? `<button type="button" class="btn btn-outline-danger btn-sm remover-admin"><span class="material-symbols-outlined">delete</span> Remover</button>` : ''}
  `;
  return card;
}

function setupAdminEventListeners() {
  const container = document.getElementById('adminsContainer');

  // Adicionar administrador
  document.getElementById('btnAdicionarAdmin')?.addEventListener('click', () => {
    const limite = parseInt(document.getElementById('adminLimit')?.textContent) || 0;
    const atuais = container.querySelectorAll('.admin-card').length;

    if (atuais + 1 > limite) {
      showStep3Error(`Você não pode adicionar mais de ${limite} administradores.`);
      return;
    }

    const novoCard = criarAdminCardVazio(atuais);
    container.appendChild(novoCard);
    reindexAdmins();
    currentStep = 3;
    saveStepData();
  });

  // Remover administrador
  container.addEventListener('click', (e) => {
    if (e.target.closest('.remover-admin')) {
      const cards = container.querySelectorAll('.admin-card');
      if (cards.length <= 1) {
        showStep3Error('Deve existir ao menos um administrador.');
        return;
      }
      e.target.closest('.admin-card').remove();
      reindexAdmins();
      currentStep = 3;
      saveStepData();
    }
  });

  // Input em campos
  container.addEventListener('input', () => {
    currentStep = 3;
    saveStepData();
  });
}

function reindexAdmins() {
  const container = document.getElementById('adminsContainer');
  const cards = container.querySelectorAll('.admin-card');

  cards.forEach((card, idx) => {
    // Atualizar título
    card.querySelector('h5')?.textContent = `Administrador ${idx + 1}`;

    // Atualizar nomes dos inputs
    const inputs = card.querySelectorAll('input');
    inputs.forEach(input => {
      const field = input.getAttribute('data-field');
      input.name = `Admins[${idx}].${field}`;
    });

    // Botão "Remover"
    const btnRemover = card.querySelector('.remover-admin');
    if (idx === 0) {
      if (btnRemover) btnRemover.remove();
    } else {
      if (!btnRemover) {
        const btn = document.createElement('button');
        btn.className = 'btn btn-outline-danger btn-sm remover-admin';
        btn.innerHTML = `<span class="material-symbols-outlined">delete</span> Remover`;
        card.appendChild(btn);
      }
    }
  });

  // Atualizar contadores
  document.getElementById('adminCount')?.textContent(cards.length);
  atualizarVisibilidadeBotaoAdmin();
}

function populateAdminsFromSession() {
  const container = document.getElementById('adminsContainer');
  container.innerHTML = '';

  const admins = (formData.Admins || []).filter(u => u.PermissionAccount === "Admin");

  if (admins.length === 0) {
    container.appendChild(criarAdminCardVazio(0));
    reindexAdmins();
    return;
  }

  admins.forEach((adm, idx) => {
    const card = criarAdminCardVazio(idx, {
      Usuario: adm.Usuario,
      Email: adm.Email,
      Senha: adm.Senha
    });
    container.appendChild(card);
  });

  reindexAdmins();
}

function validateStep3() {
  clearStep3Messages();
  const cards = document.querySelectorAll('#adminsContainer .admin-card');

  if (cards.length === 0) {
    showStep3Error('É obrigatório criar ao menos um administrador.');
    return false;
  }

  for (let card of cards) {
    const usuario = card.querySelector('[data-field="Usuario"]')?.value.trim();
    const email = card.querySelector('[data-field="Email"]')?.value.trim();
    const senha = card.querySelector('[data-field="Senha"]')?.value.trim();

    if (!usuario || !email || !senha) {
      showStep3Error('Todos os campos devem ser preenchidos.');
      return false;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      showStep3Error('E-mail inválido.');
      return false;
    }

    if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$/.test(senha)) {
      showStep3Error('Senha deve ter 8+ caracteres, incluindo maiúscula, minúscula e caractere especial.');
      return false;
    }
  }

  showStep3Success('Administradores validados com sucesso.');
  return true;
}

function atualizarVisibilidadeBotaoAdmin() {
  const limite = parseInt(document.getElementById('adminLimit')?.textContent) || 0;
  const atuais = document.querySelectorAll('#adminsContainer .admin-card').length;
  const btn = document.getElementById('btnAdicionarAdmin');
  if (btn) btn.style.display = (atuais < limite) ? 'inline-block' : 'none';
}

function removeOldEventListeners() {
  $(document).off('click', '#btnAdicionarAdmin');
  $(document).off('click', '.remover-admin');
  $(document).off('input', '#adminsContainer input');
}