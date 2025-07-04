document.addEventListener('DOMContentLoaded', function () {
    // 1. Configuração inicial
    const timerElement = document.getElementById('timer');
    const voltarBtn = document.getElementById('voltarBtn');
    const avancarBtn = document.getElementById('avancarBtn');
    const formCard = document.getElementById('formCard');

    // 2. Countdown de redirecionamento
    let seconds = 5;

    const countdown = setInterval(() => {
        seconds--;
        timerElement.textContent = seconds;

        if (seconds <= 0) {
            clearInterval(countdown);
            window.location.href = "/Login"; // Altere para a URL desejada
        }
    }, 1000);

    // 3. Botão Voltar
    if (voltarBtn) {
        voltarBtn.addEventListener('click', function (e) {
            e.preventDefault();
            if (formCard) formCard.classList.add('card-exit');
            setTimeout(() => {
                window.location.href = "/";
            }, 600);
        });
    }

    // 4. Botão Avançar
    if (avancarBtn) {
        avancarBtn.addEventListener('click', function (e) {
            e.preventDefault();
            if (formCard) formCard.classList.add('card-exit');
            setTimeout(() => {
                window.location.href = "/Documentos/DocumentosPagina";
            }, 600);
        });
    }
});