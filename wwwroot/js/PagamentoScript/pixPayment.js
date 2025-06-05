const btnGerarPix = document.getElementById('btnGerarPix');

// --- GERAÇÃO DO PIX ---
if (btnGerarPix) {
  btnGerarPix.addEventListener('click', async () => {
    const tipoPlano = document.querySelector('input[name="PlanoPago"]').value;
    const resEl = document.getElementById('resPix');
    const qrPlaceholder = document.getElementById('qrPlaceholder');
    const qrCodeContainer = document.getElementById('qrCodeContainer');

    // Limpa estados anteriores
    qrCodeContainer.innerHTML = '';
    resEl.classList.add('d-none');
    
    if (!tipoPlano) {
      resEl.className = 'alert alert-warning';
      resEl.innerText = 'Selecione um plano antes de gerar o Pix!';
      resEl.classList.remove('d-none');
      return;
    }

    // Mostra placeholder
    qrPlaceholder.classList.remove('hidden');

    try {
      const res = await fetch('/api/PagamentoFalso/pix/gerar', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tipoPlano })
      });
      
      const data = await res.json();
      
      if (!res.ok) {
        throw new Error(data.Erro || 'Falha na requisição');
      }

      // Esconde placeholder e mostra mensagem
      qrPlaceholder.classList.add('hidden');
      
      resEl.className = 'alert alert-success';
      resEl.innerHTML = `
        ${data.mensagem}<br>
        <small>ID do pagamento: ${data.pagamentoPixId}</small>
      `;
      resEl.classList.remove('d-none');

      // Extrair token do QR Code
      const token = data.qrCode.split('=')[1];
      
      // Salvar no localStorage
      const pixData = {
        token: token,
        generatedAt: new Date().getTime(),
        expiresIn: 600000 // 10 minutos em milissegundos
      };
      localStorage.setItem('pendingPix', JSON.stringify(pixData));

      // Gera QR Code com URL de confirmação
      const urlPix = `${window.location.origin}/ConfirmandoPagamento?token=${token}`;
      new QRCode(qrCodeContainer, {
        text: urlPix,
        width: 180,
        height: 180,
      });
      qrCodeContainer.classList.remove('hidden');

      // Esconder botão de gerar
      btnGerarPix.classList.add('hidden');

      // Iniciar contador
      startCountdown(600000);

    } catch (error) {
      qrPlaceholder.classList.add('hidden');
      resEl.className = 'alert alert-danger';
      resEl.innerText = error.message;
      resEl.classList.remove('d-none');
    }
  });
}

// --- CARREGAR PIX PENDENTE AO INICIAR ---
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

// --- FUNÇÕES PARA GERENCIAR PIX PENDENTE ---
function loadPendingPix(token, remainingTime) {
  const btnGerarPix = document.getElementById('btnGerarPix');
  const qrCodeContainer = document.getElementById('qrCodeContainer');
  const resEl = document.getElementById('resPix');

  // Esconder botão de geração
  btnGerarPix.classList.add('hidden');
  
  // Exibir QR Code
  const urlPix = `${window.location.origin}/ConfirmandoPagamento?token=${token}`;
  new QRCode(qrCodeContainer, {
    text: urlPix,
    width: 180,
    height: 180,
  });
  qrCodeContainer.classList.remove('hidden');

  // Exibir mensagem
  resEl.className = 'alert alert-info';
  resEl.innerHTML = `Pagamento Pix pendente.<br><small>Tempo restante: <span id="pixCountdown"></span></small>`;
  resEl.classList.remove('d-none');

  // Iniciar contador regressivo
  startCountdown(remainingTime);
}

function startCountdown(remainingTime) {
  const countdownEl = document.getElementById('pixCountdown');
  const endTime = Date.now() + remainingTime;
  
  const interval = setInterval(() => {
    const now = Date.now();
    const diff = endTime - now;
    
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