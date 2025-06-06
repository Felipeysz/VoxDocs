// Validação antes de avançar para próxima etapa
window.validateStep5 = function() {
  clearStep5Messages();
  const activeTab = $('.tab.active').data('target');

  // Recupera o plano automaticamente de um campo oculto
  const tipoPlano = document.getElementById('PlanoPago').value.trim();
  if (!tipoPlano) {
    showStep5Error('Plano não foi selecionado ou não está disponível.');
    return false;
  }

  if (activeTab === 'card') {
    const numero = $('#cartaoNumber').val().replace(/\s/g, '');
    const validade = $('#validadeCartao').val();
    const cvv = $('#cvvCartao').val();

    if (!numero || numero.length < 13) {
      showStep5Error('Informe um número de cartão válido.');
      return false;
    }
    if (!validExpiry(validade)) {
      showStep5Error('Informe uma validade válida (MM/AA).');
      return false;
    }
    if (!/^\d{3,4}$/.test(cvv)) {
      showStep5Error('Informe um CVV válido.');
      return false;
    }
  }

  return true;
};

// Configura comportamento das abas (Cartão/Pix)
function setupTabs() {
  const tabs = document.querySelectorAll('.tab');
  const tabContents = document.querySelectorAll('.tab-content');

  tabs.forEach(t => t.classList.remove('active'));
  tabContents.forEach(c => c.classList.remove('active'));
  if (tabs.length && tabContents.length) {
    tabs[0].classList.add('active');
    tabContents[0].classList.add('active');
  }

  $(document).on('click', '.tab', function() {
    const target = $(this).data('target');
    tabs.forEach(t => t.classList.remove('active'));
    tabContents.forEach(c => c.classList.remove('active'));
    $(this).addClass('active');
    $(`#${target}`).addClass('active');
    clearStep5Messages();
  });
}

// Configura formatação e validações do cartão de crédito
function setupCardListeners() {
  $(document).on('input', '#cartaoNumber', function() {
    const vRaw = $(this).val();
    const nums = vRaw.replace(/\D/g, '').slice(0, 16);
    const formatted = nums.replace(/(.{4})/g, '$1 ').trim();
    $(this).val(formatted);

    const brand = detectBrand(nums);
    if (brand) {
      $('#cc-flag').css('background-image',
        `url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/${brand}.svg)`);
    } else {
      $('#cc-flag').css('background-image', '');
    }
  });

  $(document).on('input', '#validadeCartao', function() {
    let value = $(this).val().replace(/\D/g, '');
    if (value.length > 2) {
      value = value.slice(0, 2) + '/' + value.slice(2, 4);
    }
    $(this).val(value);
  });

  $(document).on('blur', '#validadeCartao', function() {
    const value = $(this).val();
    if (!/^\d{2}\/\d{2}$/.test(value)) {
      $(this).val('');
    }
  });

  $(document).on('blur', '#cvvCartao', function() {
    const value = $(this).val();
    if (!/^\d{3,4}$/.test(value)) {
      $(this).val('');
    }
  });

  $('#pagarCartao').off('click').on('click', handleCardPayment);
}

// Funções auxiliares para mensagens e validações
function validExpiry(v) {
  if (!/^\d{2}\/\d{2}$/.test(v)) return false;
  const [mm, yy] = v.split('/').map(n => parseInt(n, 10));
  if (mm < 1 || mm > 12) return false;
  const now = new Date();
  const exp = new Date(2000 + yy, mm - 1, 1);
  return exp > now;
}

function detectBrand(num) {
  if (/^4/.test(num)) return 'visa';
  if (/^5[1-5]/.test(num)) return 'mastercard';
  if (/^3[47]/.test(num)) return 'amex';
  return '';
}

function showStep5Error(msg) {
  if ($('.tab.active').data('target') === 'card') {
    $('#resCartao').removeClass('d-none').removeClass('alert-success').addClass('alert alert-danger').text(msg);
  } else {
    $('#resPix').removeClass('d-none').removeClass('alert-success').addClass('alert alert-danger').text(msg);
  }
}

function showStep5Success(msg) {
  if ($('.tab.active').data('target') === 'card') {
    $('#resCartao').removeClass('d-none').removeClass('alert-danger').addClass('alert alert-success').text(msg);
  } else {
    $('#resPix').removeClass('d-none').removeClass('alert-danger').addClass('alert alert-success').text(msg);
  }
}

function clearStep5Messages() {
  $('#resCartao').addClass('d-none').text('').removeClass('alert-danger alert-success');
  $('#resPix').addClass('d-none').text('').removeClass('alert-danger alert-success');
}







// Inicializa o Step 5 (Pagamento)
window.initStep5 = function() {
  const savedData = JSON.parse(sessionStorage.getItem('formData')) || {};
  const planoPago = savedData.PlanoPago || '';

  // Define o plano automaticamente em um campo oculto
  document.getElementById('PlanoPago').value = planoPago;

  $(document).off('click', '.tab');
  $(document).off('input', '#cartaoNumber');
  $(document).off('input', '#validadeCartao');
  $(document).off('blur', '#validadeCartao');
  $(document).off('blur', '#cvvCartao');
  $(document).off('click', '#pagarCartao');
  $(document).off('click', '#btnGerarPix');

  setupTabs();
  setupCardListeners();
  setupPixListeners();

  window.addEventListener('beforeunload', function() {
    currentStep = 5;
    saveStepData();
  });
};

// Configura listeners para Pix
function setupPixListeners() {
  $('#btnGerarPix').off('click').on('click', handlePixPayment);

  const pendingPix = JSON.parse(localStorage.getItem('pendingPix'));
  if (pendingPix) {
    const now = new Date().getTime();
    const elapsed = now - pendingPix.generatedAt;
    const remaining = pendingPix.expiresIn - elapsed;

    if (remaining > 0) {
      loadPendingPix(pendingPix.token, remaining);
    } else {
      localStorage.removeItem('pendingPix');
    }
  }
}

// Funções auxiliares para Pix
function loadPendingPix(token, remainingTime) {
  $('#btnGerarPix').addClass('hidden');
  const urlPix = `${window.location.origin}/ConfirmandoPagamento?token=${token}`;
  new QRCode('qrCodeContainer', {
    text: urlPix,
    width: 180,
    height: 180
  });
  $('#qrCodeContainer').removeClass('hidden');

  $('#resPix').removeClass('d-none').addClass('alert alert-info').html(`Pagamento Pix pendente.<br><small>Tempo restante: <span id="pixCountdown"></span></small>`);

  startCountdown(remainingTime);
}

function startCountdown(remainingTime) {
  const endTime = Date.now() + remainingTime;
  const interval = setInterval(() => {
    const now = Date.now();
    const diff = endTime - now;
    const countdownEl = document.getElementById('pixCountdown');

    if (diff <= 0) {
      clearInterval(interval);
      countdownEl.textContent = 'Expirado!';
      localStorage.removeItem('pendingPix');
      return;
    }

    const minutes = Math.floor(diff / 60000);
    const seconds = Math.floor((diff % 60000) / 1000);
    countdownEl.textContent = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  }, 1000);
}







// Função para enviar o cadastro da empresa antes do pagamento
async function submitCompanyRegistration(formData) {
  console.log('🚀 Enviando dados da empresa para cadastro:', {
    ...formData,
    EmpresaContratante: formData.EmpresaContratante || 'N/A'
  });

  try {
    const res = await fetch('/Empresa/Cadastrar', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData)
    });

    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(errorText || 'Erro ao cadastrar a empresa.');
    }

    const result = await res.json();
    console.log('✅ Resposta do cadastro da empresa:', result);
    return result;
  } catch (error) {
    showStep5Error(`Falha no cadastro da empresa: ${error.message}`);
    throw error;
  }
}

// Função para pagamento com cartão
async function handleCardPayment() {
  clearStep5Messages();

  const formData = JSON.parse(sessionStorage.getItem('formData')) || {};
  const numero = $('#cartaoNumber').val().replace(/\s/g, '');
  const validade = $('#validadeCartao').val();
  const cvv = $('#cvvCartao').val();
  const tipoPlano = document.getElementById('PlanoPago').value.trim(); // Plano automático
  const empresa = formData.EmpresaContratante || '';

  if (!tipoPlano) {
    showStep5Error('Plano não foi selecionado ou não está disponível.');
    return;
  }

  try {
    const empresaResult = await submitCompanyRegistration(formData);
    const payload = {
      TipoPlano: tipoPlano,
      CartaoNumber: numero,
      ValidadeCartao: validade,
      CvvCartao: cvv,
      EmpresaContratante: empresa,
      PlanoPago: tipoPlano,
      EmpresaId: empresaResult.id
    };

    const res = await fetch('/PagamentosMvc/ProcessarPagamento', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(errorText || 'Erro ao processar pagamento com cartão.');
    }

    const result = await res.json();
    window.location.href = `/ConfirmarPagamentoCartao?token=${encodeURIComponent(result.pagamentoConcluidoId)}&plano=${encodeURIComponent(tipoPlano)}`;
  } catch (error) {
    $('#resCartao').removeClass('d-none').addClass('alert alert-danger').text(error.message);
    console.error('❌ Erro no pagamento com cartão:', error);
  }
}

// Função para pagamento com Pix
async function handlePixPayment() {
  clearStep5Messages();

  const tipoPlano = document.getElementById('PlanoPago').value.trim(); // Plano automático
  $('#qrCodeContainer').empty();

  if (!tipoPlano) {
    showStep5Error('Plano não foi selecionado ou não está disponível.');
    return;
  }

  try {
    const formData = JSON.parse(sessionStorage.getItem('formData')) || {};
    const empresaResult = await submitCompanyRegistration(formData);

    const payload = {
      tipoPlano
    };

    const res = await fetch('/api/PagamentoFalso/pix/gerar', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.Erro || 'Falha na geração do Pix.');
    }

    $('#qrPlaceholder').addClass('hidden');
    $('#resPix').removeClass('d-none').addClass('alert alert-success').html(`${data.mensagem}<br><small>ID: ${data.pagamentoPixId}</small>`);

    const token = data.qrCode.split('=')[1];
    const pixData = {
      token,
      generatedAt: new Date().getTime(),
      expiresIn: 600000,
      EmpresaId: empresaResult.id
    };
    localStorage.setItem('pendingPix', JSON.stringify(pixData));

    const urlPix = `${window.location.origin}/ConfirmandoPagamento?token=${token}`;
    new QRCode('qrCodeContainer', {
      text: urlPix,
      width: 180,
      height: 180
    });
    $('#qrCodeContainer').removeClass('hidden');
    $('#btnGerarPix').addClass('hidden');

    startCountdown(600000);
  } catch (err) {
    $('#qrPlaceholder').addClass('hidden');
    $('#resPix').removeClass('d-none').addClass('alert alert-danger').text(err.message);
    console.error('❌ Erro na geração de Pix:', err);
  }
}