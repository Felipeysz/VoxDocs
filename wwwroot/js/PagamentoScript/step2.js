// step2.js

function initStep2() {
  const pastasContainer = document.getElementById('pastasPrincipaisContainer');
  const btnAddPasta     = document.getElementById('btnAdicionarPastaPrincipal');
  const templatePasta   = document.getElementById('templatePastaPrincipal');
  const templateSubpasta= document.getElementById('templateSubpasta');

  if (!btnAddPasta || !pastasContainer) {
    console.warn('Elementos de Step 2 não encontrados');
    return;
  }

  // Remove listeners antigos para evitar duplicação
  $(document).off('click', '#btnAdicionarPastaPrincipal');
  $(document).off('click', '.btn-remover-pasta');
  $(document).off('click', '.btn-remover-subpasta');
  $(document).off('click', '.btn-adicionar-subpasta');
  $(document).off('blur', '.subpasta-nome');

  // 1) Recarrega do formData, se já existir algo salvo
  const salvo = formData.PastasPrincipais || [];
  salvo.forEach((objPasta, idx) => {
    const clone = templatePasta.content.cloneNode(true);
    const pastaElement = clone.querySelector('.pasta-principal');
    pastasContainer.appendChild(pastaElement);

    // Escreve o nome da pasta principal
    const inputPasta = pastaElement.querySelector('.pasta-nome');
    inputPasta.value = objPasta.Nome || '';

    // Recria subpastas salvas
    const subContainer = pastaElement.querySelector('.subpastas-container');
    (objPasta.Subpastas || []).forEach(nomeSub => {
      const cloneSub = templateSubpasta.content.cloneNode(true);
      const subEl    = cloneSub.querySelector('.subpasta-item');
      const inputSub = subEl.querySelector('.subpasta-nome');
      inputSub.value = nomeSub;
      subContainer.appendChild(subEl);

      // Configura o botão “Remover” de cada subpasta recarregada
      const btnRemSub = subEl.querySelector('.btn-remover-subpasta');
      btnRemSub.addEventListener('click', () => {
        subEl.remove();
      });

      // Validação no blur
      inputSub.addEventListener('blur', () => {
        validarSubpastasUnicas(inputPasta.value, inputSub);
      });
    });

    // Configura eventos para essa pasta principal recarregada
    configurarEventosPasta(pastaElement);
  });

  // 2) Se não havia nenhuma pasta no formData, já criamos uma pasta + subpasta em branco
  if (salvo.length === 0) {
    const clone = templatePasta.content.cloneNode(true);
    const pastaElement = clone.querySelector('.pasta-principal');
    pastasContainer.appendChild(pastaElement);
    configurarEventosPasta(pastaElement);

    // Adiciona automaticamente uma subpasta em branco
    const subContainer = pastaElement.querySelector('.subpastas-container');
    const cloneSub = templateSubpasta.content.cloneNode(true);
    const subEl    = cloneSub.querySelector('.subpasta-item');
    subContainer.appendChild(subEl);

    const inputSub = subEl.querySelector('.subpasta-nome');
    inputSub.addEventListener('blur', () => {
      validarSubpastasUnicas(pastaElement.querySelector('.pasta-nome').value, inputSub);
    });
    const btnRemSub = subEl.querySelector('.btn-remover-subpasta');
    btnRemSub.addEventListener('click', () => {
      subEl.remove();
    });
  }

  // 3) Atualiza aparência se necessário (nenhuma contagem aqui)

  // 4) Seta listener para “Adicionar Pasta Principal”
  btnAddPasta.addEventListener('click', function() {
    clearStep2Messages();

    const clone = templatePasta.content.cloneNode(true);
    const pastaElement = clone.querySelector('.pasta-principal');
    pastasContainer.appendChild(pastaElement);

    configurarEventosPasta(pastaElement);

    // Ao criar nova pasta, adiciona já uma subpasta vazia
    const subContainer = pastaElement.querySelector('.subpastas-container');
    const cloneSub = templateSubpasta.content.cloneNode(true);
    const subEl    = cloneSub.querySelector('.subpasta-item');
    subContainer.appendChild(subEl);

    const inputSub = subEl.querySelector('.subpasta-nome');
    inputSub.addEventListener('blur', () => {
      validarSubpastasUnicas(pastaElement.querySelector('.pasta-nome').value, inputSub);
    });
    const btnRemSub = subEl.querySelector('.btn-remover-subpasta');
    btnRemSub.addEventListener('click', () => {
      subEl.remove();
    });
  });
}

// Configura eventos de uma pasta principal (criada ou recarregada)
function configurarEventosPasta(pastaElement) {
  const btnAddSubpasta     = pastaElement.querySelector('.btn-adicionar-subpasta');
  const btnRemoverPasta    = pastaElement.querySelector('.btn-remover-pasta');
  const subpastasContainer = pastaElement.querySelector('.subpastas-container');
  const inputPasta         = pastaElement.querySelector('.pasta-nome');

  // 1) “Adicionar Subpasta”
  btnAddSubpasta.addEventListener('click', function() {
    clearStep2Messages();

    const cloneSub = document.getElementById('templateSubpasta').content.cloneNode(true);
    const subpastaElement = cloneSub.querySelector('.subpasta-item');
    subpastasContainer.appendChild(subpastaElement);

    // Configura exclusão para essa subpasta
    const btnRemSub = subpastaElement.querySelector('.btn-remover-subpasta');
    btnRemSub.addEventListener('click', function() {
      subpastaElement.remove();
    });

    // Validação de nome único
    const inputSubpasta = subpastaElement.querySelector('.subpasta-nome');
    inputSubpasta.addEventListener('blur', function() {
      validarSubpastasUnicas(inputPasta.value, inputSubpasta);
    });
  });

  // 2) “Remover Pasta Principal” (se só houver uma pasta, impede remoção)
  btnRemoverPasta.addEventListener('click', function() {
    const todasPastas = document.querySelectorAll('#pastasPrincipaisContainer .pasta-principal');
    if (todasPastas.length === 1) {
      showStep2Error('Deve existir ao menos uma pasta principal.');
      return;
    }
    clearStep2Messages();
    pastaElement.remove();
  });

  // 3) Validar subpastas ao alterar nome da pasta principal
  inputPasta.addEventListener('blur', function() {
    validarTodasSubpastas(pastaElement);
  });
}

// Valida se não há subpastas duplicadas dentro da mesma Pasta Principal
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
    inputSubpasta.classList.add('is-invalid');
    inputSubpasta.nextElementSibling?.remove();
    const errorDiv = document.createElement('div');
    errorDiv.className = 'invalid-feedback';
    errorDiv.textContent = 'Já existe uma subpasta com este nome nesta pasta.';
    inputSubpasta.parentNode.appendChild(errorDiv);
    showStep2Error('Existem subpastas duplicadas.');
  } else {
    inputSubpasta.classList.remove('is-invalid');
    inputSubpasta.nextElementSibling?.remove();
    clearStep2Messages();
  }
}

// Dispara blur em todas as subpastas de uma Pasta Principal
function validarTodasSubpastas(pastaElement) {
  const subpastas = pastaElement.querySelectorAll('.subpasta-nome');
  subpastas.forEach(input => input.dispatchEvent(new Event('blur')));
}

// Exibe mensagem de erro no placeholder da view, sem alert()
function showStep2Error(mensagem) {
  const errorDiv   = document.getElementById('step2Error');
  const successDiv = document.getElementById('step2Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = mensagem;
  errorDiv.classList.remove('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

// Exibe mensagem de sucesso no placeholder da view
function showStep2Success(mensagem) {
  const errorDiv   = document.getElementById('step2Error');
  const successDiv = document.getElementById('step2Success');
  if (!errorDiv || !successDiv) return;
  successDiv.textContent = mensagem;
  successDiv.classList.remove('d-none');
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
}

// Limpa qualquer mensagem de erro ou sucesso
function clearStep2Messages() {
  const errorDiv   = document.getElementById('step2Error');
  const successDiv = document.getElementById('step2Success');
  if (!errorDiv || !successDiv) return;
  errorDiv.textContent = '';
  errorDiv.classList.add('d-none');
  successDiv.textContent = '';
  successDiv.classList.add('d-none');
}

// Verifica se existe pelo menos 1 pasta e 1 subpasta
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
