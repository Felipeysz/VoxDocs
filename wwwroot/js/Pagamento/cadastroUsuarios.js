document.addEventListener('DOMContentLoaded', function () {
    // Animação do step ativo ao carregar
    const activeStep = document.querySelector('.step.active .step-number');
    if (activeStep) {
        activeStep.style.animation = 'slowPulse 3s infinite ease-in-out';
    }

    // Validação do formulário
    const usuarioForm = document.getElementById('usuarioForm');
    if (usuarioForm) {
        usuarioForm.addEventListener('submit', function (e) {
            e.preventDefault();
            let isValid = true;

            // Validação dos campos
            const requiredFields = this.querySelectorAll('[required]');
            requiredFields.forEach(function (field) {
                const validationMsg = field.nextElementSibling;
                if (field.value.trim() === '') {
                    validationMsg.classList.remove('d-none');
                    isValid = false;
                } else {
                    validationMsg.classList.add('d-none');
                }
            });

            if (isValid) {
                this.submit();
            }
        });
    }

    // Desabilita o botão de finalizar se não houver admins
    const adminCadastrados = @adminCadastrados;
    if (adminCadastrados === 0) {
        const finalizarBtn = document.querySelector('form[action="FinalizarCadastroUsuarios"] button');
        if (finalizarBtn) {
            finalizarBtn.disabled = true;
        }
    }

    // Atualiza os options do select conforme os limites
    function atualizarOpcoesPermissao() {
        const adminRestantes = @adminRestantes;
        const userRestantes = @usuariosRestantes;

        const adminOption = document.querySelector('select[name="PermissaoConta"] option[value="Admin"]');
        const userOption = document.querySelector('select[name="PermissaoConta"] option[value="User"]');

        if (adminOption) adminOption.disabled = adminRestantes <= 0;
        if (userOption) userOption.disabled = userRestantes <= 0;
    }

    atualizarOpcoesPermissao();
});