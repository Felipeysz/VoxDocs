// Função auxiliar para limpar mensagens de validação
function resetMessages(emailError, emailSuccess) {
  if (emailError) {
    emailError.classList.add('d-none');
    emailError.textContent = '';
  }
  if (emailSuccess) {
    emailSuccess.classList.add('d-none');
    emailSuccess.textContent = '';
  }
}

export function initStep1(stepData) {
  const empresaInput = document.querySelector('input[name="EmpresaContratante"]');
  const emailInput = document.querySelector('input[name="EmailEmpresa"]');
  const emailError = document.getElementById('emailError');
  const emailSuccess = document.getElementById('emailSuccess');

  if (!empresaInput || !emailInput || !emailError || !emailSuccess) return;

  // Preencher campos visíveis com dados do stepData
  if (empresaInput && stepData.EmpresaContratante) {
    empresaInput.value = stepData.EmpresaContratante;
  }
  if (emailInput && stepData.EmailEmpresa) {
    emailInput.value = stepData.EmailEmpresa;
  }

  // Função auxiliar para criar campos ocultos
  function createHiddenField(form, name, value) {
    let hiddenField = form.querySelector(`input[name="${name}"][type="hidden"]`);
    if (!hiddenField) {
      hiddenField = document.createElement('input');
      hiddenField.type = 'hidden';
      hiddenField.name = name;
      form.appendChild(hiddenField);
    }
    hiddenField.value = value;
    return hiddenField;
  }

  const form = document.getElementById('formWizard');
  createHiddenField(form, 'EmpresaContratante', empresaInput.value);
  createHiddenField(form, 'EmailEmpresa', emailInput.value);

  // Limpar mensagens de validação
  resetMessages(emailError, emailSuccess);

  // Evento para limpar mensagens ao digitar
  emailInput.addEventListener('input', () => resetMessages(emailError, emailSuccess));

  // Evento para validar e-mail ao sair do campo
  emailInput.addEventListener('blur', () => {
    const email = emailInput.value.trim();
    if (!email) return;

    if (!isValidEmail(email)) {
      emailError.textContent = 'Por favor, insira um e-mail válido.';
      emailError.classList.remove('d-none');
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
        } else {
          emailSuccess.textContent = 'Email Disponível';
          emailSuccess.classList.remove('d-none');
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

  // Limpar mensagens antes da validação
  resetMessages(emailError, document.getElementById('emailSuccess'));

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

function isValidEmail(email) {
  const regex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  return regex.test(email);
}