export function initStep1(stepData) {
  const empresaInput = document.querySelector('input[name="EmpresaContratante"]');
  const emailInput = document.querySelector('input[name="EmailEmpresa"]');
  const emailError = document.getElementById('emailError');
  const emailSuccess = document.getElementById('emailSuccess');

  if (!empresaInput || !emailInput || !emailError || !emailSuccess) return;

  // Preencher campos visíveis
  if (empresaInput && stepData.EmpresaContratante) empresaInput.value = stepData.EmpresaContratante;
  if (emailInput && stepData.EmailEmpresa) emailInput.value = stepData.EmailEmpresa;

  // Criar ou atualizar campos ocultos
  const form = document.getElementById('formWizard');

  let hiddenEmpresa = form.querySelector('input[name="EmpresaContratante"][type="hidden"]');
  if (!hiddenEmpresa) {
    hiddenEmpresa = document.createElement('input');
    hiddenEmpresa.type = 'hidden';
    hiddenEmpresa.name = 'EmpresaContratante';
    form.appendChild(hiddenEmpresa);
  }
  hiddenEmpresa.value = empresaInput.value;

  let hiddenEmail = form.querySelector('input[name="EmailEmpresa"][type="hidden"]');
  if (!hiddenEmail) {
    hiddenEmail = document.createElement('input');
    hiddenEmail.type = 'hidden';
    hiddenEmail.name = 'EmailEmpresa';
    form.appendChild(hiddenEmail);
  }
  hiddenEmail.value = emailInput.value;

  // Limpar mensagens
  emailError.classList.add('d-none');
  emailError.textContent = '';
  emailSuccess.classList.add('d-none');
  emailSuccess.textContent = '';

  // Eventos de input e blur
  emailInput.addEventListener('input', () => {
    emailError.classList.add('d-none');
    emailError.textContent = '';
    emailSuccess.classList.add('d-none');
    emailSuccess.textContent = '';
  });

  emailInput.addEventListener('blur', () => {
    const email = emailInput.value.trim();
    if (!email) return;

    // Validação de formato de e-mail
    if (!isValidEmail(email)) {
      emailError.textContent = 'Por favor, insira um e-mail válido.';
      emailError.classList.remove('d-none');
      emailSuccess.classList.add('d-none');
      return;
    }

    fetch('/PagamentosMvc/CheckEmailExists', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email })
    })
      .then(response => response.json())
      .then(exists => {
        if (exists) {
          emailError.textContent = 'Este e-mail já está cadastrado.';
          emailError.classList.remove('d-none');
          emailSuccess.classList.add('d-none');
        } else {
          emailSuccess.textContent = 'Email Disponível';
          emailSuccess.classList.remove('d-none');
          emailError.classList.add('d-none');
        }
      })
      .catch(() => {
        emailError.textContent = 'Erro ao verificar e-mail.';
        emailError.classList.remove('d-none');
      });
  });
}

export async function validateStep1() {
  const empresa = document.querySelector('input[name="EmpresaContratante"]')?.value.trim() || '';
  const email = document.querySelector('input[name="EmailEmpresa"]')?.value.trim() || '';
  const emailError = document.getElementById('emailError');

  if (!emailError) return false;

  emailError.classList.add('d-none');
  emailError.textContent = '';

  // Validação de campos obrigatórios
  if (!empresa || !email) {
    emailError.textContent = !empresa && !email
      ? 'Por favor, preencha ambos os campos.'
      : (!empresa ? 'Por favor, preencha o nome da empresa.' : 'Por favor, preencha o e-mail da empresa.');
    emailError.classList.remove('d-none');
    return false;
  }

  // Validação de formato de e-mail
  if (!isValidEmail(email)) {
    emailError.textContent = 'Por favor, insira um e-mail válido.';
    emailError.classList.remove('d-none');
    return false;
  }

  // Validação de e-mail duplicado
  try {
    const response = await fetch('/PagamentosMvc/CheckEmailExists', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email })
    });
    
    if (!response.ok) throw new Error('Erro na requisição');
    
    const exists = await response.json();
    if (exists) {
      emailError.textContent = 'Este e-mail já está cadastrado. Por favor, use outro.';
      emailError.classList.remove('d-none');
      return false;
    }
    return true;
  } catch (err) {
    emailError.textContent = 'Erro ao verificar o e-mail. Tente novamente.';
    emailError.classList.remove('d-none');
    return false;
  }
}

// Função auxiliar para validar formato de e-mail
function isValidEmail(email) {
  const regex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  return regex.test(email);
}