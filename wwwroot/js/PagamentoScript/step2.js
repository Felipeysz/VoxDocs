function initStep2() {
  const pastasContainer = document.getElementById('pastasPrincipaisContainer');
  const btnAddPasta = document.getElementById('btnAdicionarPastaPrincipal');
  const templatePasta = document.getElementById('templatePastaPrincipal');
  const templateSubpasta = document.getElementById('templateSubpasta');

  if (!btnAddPasta || !pastasContainer) {
    console.warn('Elementos de Step 2 não encontrados');
    return;
  }

  // Remover listeners antigos para evitar duplicação
  removeOldEventListeners();

  // Recarregar dados salvos
  const salvo = formData.PastasPrincipais || [];
  salvo.forEach(objPasta => {
    const pastaElement = createPastaElement(objPasta, templatePasta, templateSubpasta);
    pastasContainer.appendChild(pastaElement);
    configurarEventosPasta(pastaElement);
  });

  // Criar pasta inicial se não houver dados salvos
  if (salvo.length === 0) {
    const pastaElement = createPastaElement({ Nome: '', Subpastas: [] }, templatePasta, templateSubpasta);
    pastasContainer.appendChild(pastaElement);
    configurarEventosPasta(pastaElement);
  }

  // Configurar evento para adicionar nova pasta
  btnAddPasta.addEventListener('click', () => {
    clearStep2Messages();
    const pastaElement = createPastaElement({ Nome: '', Subpastas: [] }, templatePasta, templateSubpasta);
    pastasContainer.appendChild(pastaElement);
    configurarEventosPasta(pastaElement);
  });
}

function createPastaElement(objPasta, templatePasta, templateSubpasta) {
  const clone = templatePasta.content.cloneNode(true);
  const pastaElement = clone.querySelector('.pasta-principal');

  // Configurar nome da pasta
  const inputPasta = pastaElement.querySelector('.pasta-nome');
  inputPasta.value = objPasta.Nome || '';

  // Configurar subpastas
  const subContainer = pastaElement.querySelector('.subpastas-container');
  (objPasta.Subpastas || []).forEach(nomeSub => {
    const subpastaElement = createSubpastaElement(nomeSub, templateSubpasta);
    subContainer.appendChild(subpastaElement);
  });

  return pastaElement;
}

function createSubpastaElement(nomeSub, templateSubpasta) {
  const cloneSub = templateSubpasta.content.cloneNode(true);
  const subpastaElement = cloneSub.querySelector('.subpasta-item');
  const inputSub = subpastaElement.querySelector('.subpasta-nome');
  inputSub.value = nomeSub || '';

  // Configurar evento de remoção
  const btnRemSub = subpastaElement.querySelector('.btn-remover-subpasta');
  btnRemSub.addEventListener('click', () => subpastaElement.remove());

  // Configurar validação
  inputSub.addEventListener('blur', () => {
    const inputPasta = subpastaElement.closest('.pasta-principal').querySelector('.pasta-nome');
    validarSubpastasUnicas(inputPasta.value, inputSub);
  });

  return subpastaElement;
}

function configurarEventosPasta(pastaElement) {
  const btnAddSubpasta = pastaElement.querySelector('.btn-adicionar-subpasta');
  const btnRemoverPasta = pastaElement.querySelector('.btn-remover-pasta');
  const inputPasta = pastaElement.querySelector('.pasta-nome');

  // Adicionar subpasta
  btnAddSubpasta.addEventListener('click', () => {
    clearStep2Messages();
    const subpastaElement = createSubpastaElement('', document.getElementById('templateSubpasta'));
    pastaElement.querySelector('.subpastas-container').appendChild(subpastaElement);
  });

  // Remover pasta principal
  btnRemoverPasta.addEventListener('click', () => {
    const todasPastas = document.querySelectorAll('#pastasPrincipaisContainer .pasta-principal');
    if (todasPastas.length === 1) {
      showStep2Error('Deve existir ao menos uma pasta principal.');
      return;
    }
    clearStep2Messages();
    pastaElement.remove();
  });

  // Validar subpastas ao alterar nome da pasta
  inputPasta.addEventListener('blur', () => {
    validarTodasSubpastas(pastaElement);
  });
}

function validarSubpastasUnicas(nomePasta, inputSubpasta) {
  if (!nomePasta || !inputSubpasta.value) return;

  const pastaElement = inputSubpasta.closest('.pasta-principal');
  const subpastas = pastaElement.querySelectorAll('.subpasta-nome');
  let duplicado = false;

  subpastas.forEach(input => {
    if (input === inputSubpasta) return;
    if (input.value === inputSubpasta.value) duplicado = true;
  });

  if (duplicado) {
    showInvalidFeedback(inputSubpasta, 'Já existe uma subpasta com este nome nesta pasta.');
    showStep2Error('Existem subpastas duplicadas.');
  } else {
    clearInvalidFeedback(inputSubpasta);
    clearStep2Messages();
  }
}

function validarTodasSubpastas(pastaElement) {
  const subpastas = pastaElement.querySelectorAll('.subpasta-nome');
  subpastas.forEach(input => input.dispatchEvent(new Event('blur')));
}

function showInvalidFeedback(input, message) {
  input.classList.add('is-invalid');
  const errorDiv = document.createElement('div');
  errorDiv.className = 'invalid-feedback';
  errorDiv.textContent = message;
  input.parentNode.appendChild(errorDiv);
}

function clearInvalidFeedback(input) {
  input.classList.remove('is-invalid');
  input.nextElementSibling?.remove();
}

function showStep2Error(mensagem) {
  const errorDiv = document.getElementById('step2Error');
  const successDiv = document.getElementById('step2Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = mensagem;
  errorDiv.classList.remove('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

function showStep2Success(mensagem) {
  const errorDiv = document.getElementById('step2Error');
  const successDiv = document.getElementById('step2Success');
  if (!errorDiv || !successDiv) return;
  successDiv.textContent = mensagem;
  successDiv.classList.remove('d-none');
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
}

function clearStep2Messages() {
  const errorDiv = document.getElementById('step2Error');
  const successDiv = document.getElementById('step2Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

function validateStep2() {
  clearStep2Messages();

  const pastaCards = document.querySelectorAll('#pastasPrincipaisContainer .pasta-principal');
  if (pastaCards.length === 0) {
    showStep2Error('É obrigatório criar ao menos uma pasta principal.');
    return false;
  }

  for (let card of pastaCards) {
    const nomePasta = card.querySelector('.pasta-nome')?.value.trim() || '';
    if (!nomePasta) {
      showStep2Error('Cada pasta principal deve ter um nome preenchido.');
      return false;
    }

    const subEls = card.querySelectorAll('.subpasta-nome');
    const existentes = Array.from(subEls).filter(inp => inp.value.trim() !== '');
    if (existentes.length === 0) {
      showStep2Error(`A pasta "${nomePasta}" deve ter ao menos uma subpasta.`);
      return false;
    }
  }

  showStep2Success('Tudo certo em Gerenciamento de Pastas.');
  return true;
}

function removeOldEventListeners() {
  $(document).off('click', '#btnAdicionarPastaPrincipal');
  $(document).off('click', '.btn-remover-pasta');
  $(document).off('click', '.btn-remover-subpasta');
  $(document).off('click', '.btn-adicionar-subpasta');
  $(document).off('blur', '.subpasta-nome');
}