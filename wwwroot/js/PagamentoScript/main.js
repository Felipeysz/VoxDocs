// main.js (fluxo de steps)

let currentStep = 1;
const totalSteps = 5;
let formData = JSON.parse(sessionStorage.getItem('formData')) || {};

function saveStepData() {
  if (currentStep === 1) {
    formData.EmpresaContratante = document.querySelector('input[name="EmpresaContratante"]')?.value || '';
    formData.EmailEmpresa       = document.querySelector('input[name="EmailEmpresa"]')?.value || '';
  }
  else if (currentStep === 2) {
    const arrPastas = [];
    const pastaCards = document.querySelectorAll('#pastasPrincipaisContainer .pasta-principal');

    pastaCards.forEach(card => {
      const nomePasta = card.querySelector('.pasta-nome').value.trim();
      const subEls    = card.querySelectorAll('.subpasta-nome');
      const listaSub  = [];
      subEls.forEach(inpSub => {
        const nomeSub = inpSub.value.trim();
        if (nomeSub) listaSub.push(nomeSub);
      });
      arrPastas.push({ Nome: nomePasta, Subpastas: listaSub });
    });
    formData.PastasPrincipais = arrPastas;
  }
  else if (currentStep === 3) {
    const admins = [];
    const adminCards = document.querySelectorAll('#adminsContainer .admin-card');

    adminCards.forEach(card => {
      const usuario = card.querySelector('input[data-field="Usuario"]')?.value || '';
      const email   = card.querySelector('input[data-field="Email"]')?.value || '';
      const senha   = card.querySelector('input[data-field="Senha"]')?.value || '';
      admins.push({
        Usuario: usuario,
        Email: email,
        Senha: senha,
        PermissionAccount: "Admin"
      });
    });
    formData.Admins = admins;
  }
  else if (currentStep === 4) {
    const users = [];
    const userCards = document.querySelectorAll('#usersContainer .user-card');

    userCards.forEach(card => {
      const usuario = card.querySelector('input[data-field="Usuario"]')?.value || '';
      const email   = card.querySelector('input[data-field="Email"]')?.value || '';
      const senha   = card.querySelector('input[data-field="Senha"]')?.value || '';
      users.push({
        Usuario: usuario,
        Email: email,
        Senha: senha,
        PermissionAccount: "User"
      });
    });
    formData.Users = users;
  }

  sessionStorage.setItem('formData', JSON.stringify(formData));
  updateHiddenFields();
}

function updateHiddenFields() {
  const form = document.getElementById('formWizard');
  if (!form) return;

  // Step 1
  let hiddenEmpresa = form.querySelector('input[name="EmpresaContratante"][type="hidden"]');
  if (!hiddenEmpresa) {
    hiddenEmpresa = document.createElement('input');
    hiddenEmpresa.type = 'hidden';
    hiddenEmpresa.name = 'EmpresaContratante';
    form.appendChild(hiddenEmpresa);
  }
  hiddenEmpresa.value = formData.EmpresaContratante || '';

  let hiddenEmail = form.querySelector('input[name="EmailEmpresa"][type="hidden"]');
  if (!hiddenEmail) {
    hiddenEmail = document.createElement('input');
    hiddenEmail.type = 'hidden';
    hiddenEmail.name = 'EmailEmpresa';
    form.appendChild(hiddenEmail);
  }
  hiddenEmail.value = formData.EmailEmpresa || '';

  // Step 2
  let hiddenPastas = form.querySelector('input[name="PastasPrincipaisJson"][type="hidden"]');
  if (!hiddenPastas) {
    hiddenPastas = document.createElement('input');
    hiddenPastas.type = 'hidden';
    hiddenPastas.name = 'PastasPrincipaisJson';
    form.appendChild(hiddenPastas);
  }
  hiddenPastas.value = JSON.stringify(formData.PastasPrincipais || []);

  // Step 3
  let hiddenAdmins = form.querySelector('input[name="AdminsJson"][type="hidden"]');
  if (!hiddenAdmins) {
    hiddenAdmins = document.createElement('input');
    hiddenAdmins.type = 'hidden';
    hiddenAdmins.name = 'AdminsJson';
    form.appendChild(hiddenAdmins);
  }
  hiddenAdmins.value = JSON.stringify(formData.Admins || []);

  // Step 4
  let hiddenUsers = form.querySelector('input[name="UsersJson"][type="hidden"]');
  if (!hiddenUsers) {
    hiddenUsers = document.createElement('input');
    hiddenUsers.type = 'hidden';
    hiddenUsers.name = 'UsersJson';
    form.appendChild(hiddenUsers);
  }
  hiddenUsers.value = JSON.stringify(formData.Users || []);
}

function loadStepData() {
  const savedData = sessionStorage.getItem('formData');
  if (savedData) {
    formData = JSON.parse(savedData);
  }
  return formData;
}

function updateStepper(step) {
  document.querySelectorAll('.stepper .step').forEach((el, idx) => {
    el.classList.toggle('active', idx + 1 === step);
  });
  document.getElementById('prevBtn').disabled = (step === 1);
  document.getElementById('nextBtn').textContent = (step === totalSteps ? 'Concluir' : 'Próximo »');
}

function loadScript(src, callback) {
  const script = document.createElement('script');
  script.src = src;
  script.onload = callback;
  document.head.appendChild(script);
}

function loadStep(step) {
  saveStepData();

  const container = document.getElementById('stepContentContainer');
  container.innerHTML = '';

  fetch(`/PagamentosMvc/Step${step}?planoNome=${planoNome}`)
    .then(response => response.text())
    .then(html => {
      container.innerHTML = html;
      updateStepper(step);

      loadStepData();

      if (step === 1) {
        import('/js/PagamentoScript/step1.js').then(module => module.initStep1(formData));
      }
      else if (step === 2) {
        loadScript('/js/PagamentoScript/step2.js', () => {
          if (typeof initStep2 === 'function') initStep2();
        });
      }
      else if (step === 3) {
        setTimeout(() => {
          if (typeof window.initStep3 === 'function') window.initStep3();
        }, 100);
      }
      else if (step === 4) {
        setTimeout(() => {
          if (typeof window.initStep4 === 'function') window.initStep4();
        }, 100);
      }

      currentStep = step;
    })
    .catch(error => {
      container.innerHTML = `<div class="alert alert-danger"><strong>Erro:</strong> ${error.message}</div>`;
      console.error('Erro ao carregar etapa:', error);
    });
}

document.addEventListener('DOMContentLoaded', function () {
  loadStepData();

  document.getElementById('nextBtn').addEventListener('click', () => {
    if (currentStep === 1) {
      import('/js/PagamentoScript/step1.js').then(module => {
        module.validateStep1().then(isValid => {
          if (isValid) {
            saveStepData();
            loadStep(currentStep + 1);
          }
        });
      });
    }
    else if (currentStep === 2) {
      if (typeof validateStep2 === 'function' && !validateStep2()) {
        return;
      }
      saveStepData();
      loadStep(currentStep + 1);
    }
    else if (currentStep === 3) {
      if (typeof validateStep3 === 'function' && !validateStep3()) {
        return;
      }
      saveStepData();
      loadStep(currentStep + 1);
    }
    else if (currentStep === 4) {
      if (typeof validateStep4 === 'function' && !validateStep4()) {
        return;
      }
      saveStepData();
      loadStep(currentStep + 1);
    }
    else if (currentStep < totalSteps) {
      saveStepData();
      loadStep(currentStep + 1);
    }
    else {
      document.getElementById('formWizard').submit();
    }
  });

  document.getElementById('prevBtn').addEventListener('click', () => {
    if (currentStep > 1) {
      saveStepData();
      loadStep(currentStep - 1);
    }
  });

  loadStep(currentStep);

  window.addEventListener('unload', function() {
    sessionStorage.removeItem('formData');
  });
});
