// Função para mostrar notificações personalizadas
function showNotification(message, type = 'error') {
    const existingAlerts = document.querySelectorAll('.custom-alert');
    existingAlerts.forEach(alert => alert.remove());

    const alertDiv = document.createElement('div');
    alertDiv.className = `custom-alert ${type}`;

    let icon;
    if (type === 'error') {
        icon = 'exclamation-circle';
    } else if (type === 'warning') {
        icon = 'exclamation-triangle';
    } else {
        icon = 'check-circle';
    }

    alertDiv.innerHTML = `
        <i class="fas fa-${icon}"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(alertDiv);

    setTimeout(() => {
        alertDiv.remove();
    }, type === 'success' ? 5000 : 3000);
}

// Funções para manipulação de pastas principais
function addPastaPrincipal() {
    const container = document.getElementById("pastas-principais-container");
    const existingItems = container.querySelectorAll('.pasta-principal-item');
    const newIndex = existingItems.length;

    if (newIndex >= 5) {
        showNotification("Limite máximo de 5 pastas principais atingido", "warning");
        return;
    }

    const html = `
        <div class="form-group pasta-principal-item">
            <label class="form-label">Nome da Pasta Principal <span class="text-danger">*</span></label>
            <input type="text" name="PastasPrincipais[${newIndex}].Nome" 
                class="form-control" placeholder="Digite o nome da pasta principal" 
                maxlength="100" data-touched="false" />
            <i class="fas fa-folder input-icon"></i>
            <div class="invalid-feedback d-none"></div>
            <button type="button" class="btn btn-outline btn-sm mt-2 remove-pasta" onclick="removePastaPrincipal(this)">
                <i class="fas fa-trash"></i> Remover
            </button>
        </div>`;

    container.insertAdjacentHTML("beforeend", html);
}

function removePastaPrincipal(button) {
    if (document.querySelectorAll('.pasta-principal-item').length <= 1) {
        showNotification('Você deve manter pelo menos uma pasta principal', 'warning');
        return;
    }

    const item = button.closest('.pasta-principal-item');
    item.remove();
    reindexPastasPrincipais();
}

function reindexPastasPrincipais() {
    const items = document.querySelectorAll('.pasta-principal-item');
    items.forEach((item, index) => {
        const nomeInput = item.querySelector('input[type="text"]');
        nomeInput.name = `PastasPrincipais[${index}].Nome`;

        const idInput = item.querySelector('input[type="hidden"]');
        if (idInput) {
            idInput.name = `PastasPrincipais[${index}].Id`;
        }
    });
}

// Validação completa de pastas principais
function validatePastasPrincipais(ignoreEmpty = false) {
    let isValid = true;
    const container = document.getElementById("pastas-principais-container");
    const inputs = container.querySelectorAll('input[type="text"][name^="PastasPrincipais"]');

    // Limpa erros anteriores
    inputs.forEach(input => {
        input.classList.remove('is-invalid');
        const feedback = input.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = '';
            feedback.classList.add('d-none');
        }
    });

    // Validação individual de cada campo
    inputs.forEach(input => {
        const value = input.value.trim();
        const feedback = input.nextElementSibling;

        // Validação de campo obrigatório (ignora se ignoreEmpty=true)
        if (!value && (!ignoreEmpty || input.dataset.touched === 'true')) {
            showError(input, feedback, 'O nome da pasta principal é obrigatório.');
            isValid = false;
            return;
        }

        // Validação de tamanho máximo
        if (value.length > 100) {
            showError(input, feedback, 'O nome não pode exceder 100 caracteres.');
            isValid = false;
            return;
        }

        // Validação de formato (opcional, se necessário)
        if (value && !/^[a-zA-Z0-9\s\-_]+$/.test(value)) {
            showError(input, feedback, 'Use apenas letras, números, espaços, hífens ou underscores.');
            isValid = false;
            return;
        }
    });

    // Validação de nomes duplicados (apenas se todos os campos individuais forem válidos)
    if (isValid && !ignoreEmpty) {
        const nomes = Array.from(inputs).map(input => input.value.trim().toLowerCase()).filter(n => n);
        const hasDuplicates = new Set(nomes).size !== nomes.length;

        if (hasDuplicates) {
            inputs.forEach(input => {
                const nome = input.value.trim().toLowerCase();
                if (nome && nomes.indexOf(nome) !== nomes.lastIndexOf(nome)) {
                    const feedback = input.nextElementSibling;
                    showError(input, feedback, 'Já existe uma pasta com este nome.');
                    isValid = false;
                }
            });
        }
    }

    return isValid;
}

function showError(input, feedback, message) {
    input.classList.add('is-invalid');
    if (feedback && feedback.classList.contains('invalid-feedback')) {
        feedback.textContent = message;
        feedback.classList.remove('d-none');
    }
}

// Event listeners e inicialização
document.addEventListener('DOMContentLoaded', function () {
    // Validação em tempo real
    const container = document.getElementById("pastas-principais-container");

    container.addEventListener('input', function (e) {
        if (e.target.matches('input[type="text"]')) {
            const input = e.target;
            const value = input.value.trim();
            const feedback = input.nextElementSibling;

            // Marca o campo como "tocado"
            input.dataset.touched = 'true';

            // Limpa erros enquanto digita
            input.classList.remove('is-invalid');
            if (feedback) {
                feedback.textContent = '';
                feedback.classList.add('d-none');
            }

            // Validação de tamanho máximo em tempo real
            if (value.length > 100) {
                showError(input, feedback, 'O nome não pode exceder 100 caracteres.');
            }

            // Validação de campo obrigatório só se o campo foi preenchido e depois esvaziado
            if (input.dataset.touched === 'true' && !value) {
                showError(input, feedback, 'O nome da pasta principal é obrigatório.');
            }
        }
    });

    // Validação no submit do formulário
    const form = document.getElementById("pastasPrincipaisForm");
    if (form) {
        form.addEventListener("submit", function (e) {
            // Valida sem ignorar campos vazios
            if (!validatePastasPrincipais(false)) {
                e.preventDefault();
                showNotification('Por favor, corrija os erros destacados no formulário.', 'error');

                // Rolagem para o primeiro erro
                const firstError = form.querySelector('.is-invalid');
                if (firstError) {
                    firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
                return false;
            }

            // Mostra loading no botão de submit
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                const submitText = submitButton.querySelector('.submit-text');
                const spinner = submitButton.querySelector('.loading-spinner');

                if (submitText) submitText.style.display = 'none';
                if (spinner) spinner.style.display = 'inline-block';
                submitButton.disabled = true;
            }

            return true;
        });
    }

    // Adiciona validação ao remover pastas
    document.addEventListener('click', function (e) {
        if (e.target.matches('.pasta-principal-item .remove-pasta, .pasta-principal-item .remove-pasta *')) {
            setTimeout(() => validatePastasPrincipais(true), 100);
        }
    });

    // Validação inicial - ignora campos vazios
    validatePastasPrincipais(true);
});