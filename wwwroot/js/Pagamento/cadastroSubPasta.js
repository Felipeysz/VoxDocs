// Função para exibir notificações
function showNotification(message, type = 'error') {
    document.querySelectorAll('.notification').forEach(el => el.remove());

    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show notification`;
    notification.style.position = 'fixed';
    notification.style.top = '20px';
    notification.style.right = '20px';
    notification.style.zIndex = '9999';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        const bsAlert = new bootstrap.Alert(notification);
        bsAlert.close();
    }, 5000);
}

// Função para confirmar ações
async function confirmAction(message) {
    const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    const confirmMessage = document.getElementById('confirmMessage');
    const confirmButton = document.getElementById('confirmAction');

    confirmMessage.textContent = message;
    modal.show();

    return new Promise((resolve) => {
        confirmButton.onclick = () => {
            modal.hide();
            resolve(true);
        };

        modal._element.addEventListener('hidden.bs.modal', () => {
            confirmButton.onclick = null;
            resolve(false);
        }, { once: true });
    });
}

// Adiciona uma nova subpasta
function addSubPasta() {
    const container = document.getElementById("subpastas-container");
    const pastaSelect = document.getElementById("pastaPrincipalSelect");

    if (!pastaSelect.value) {
        showNotification('Selecione uma pasta principal primeiro', 'warning');
        pastaSelect.classList.add('is-invalid');
        pastaSelect.nextElementSibling.style.display = 'block';
        return;
    }

    const nextIndex = document.querySelectorAll('.subpasta-item').length;
    const pastaPrincipalNome = pastaSelect.options[pastaSelect.selectedIndex].text;

    const html = `
        <div class="form-group subpasta-item mt-3">
            <input type="hidden" name="SubPastas[${nextIndex}].Id" value="00000000-0000-0000-0000-000000000000" />
            <input type="hidden" name="SubPastas[${nextIndex}].PastaPrincipalId" value="${pastaSelect.value}" />
            <input type="hidden" name="SubPastas[${nextIndex}].PastaPrincipalNome" value="${pastaPrincipalNome}" />
            <label class="form-label">Nome da Subpasta <span class="text-danger">*</span></label>
            <input type="text" name="SubPastas[${nextIndex}].Nome" 
                   class="form-control"
                   placeholder="Digite o nome da subpasta"
                   maxlength="100"
                   required />
            <div class="invalid-feedback">
                <i class="fas fa-exclamation-circle"></i>
                <span>O nome da subpasta é obrigatório</span>
            </div>
            <div class="subpasta-actions">
                <button type="button" class="btn btn-outline btn-sm mt-2 btn-primary"
                        onclick="salvarSubpastaIndividual(this, ${nextIndex})">
                    <i class="fas fa-save"></i> Salvar
                </button>
                <button type="button" class="btn btn-outline btn-sm mt-2 btn-danger"
                        onclick="removerSubpasta(this)">
                    <i class="fas fa-trash"></i> Remover
                </button>
            </div>
        </div>`;

    const alert = container.querySelector('.alert-info');
    if (alert) {
        container.removeChild(alert);
    }

    container.insertAdjacentHTML("beforeend", html);
    pastaSelect.classList.remove('is-invalid');
    pastaSelect.nextElementSibling.style.display = 'none';
}

// Salva uma subpasta individual
async function salvarSubpastaIndividual(button, index) {
    const item = button.closest('.subpasta-item');

    document.querySelectorAll('.invalid-feedback').forEach(el => {
        el.style.display = 'none';
    });
    document.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });

    const pastaSelect = document.getElementById('pastaPrincipalSelect');
    const nomeInput = item.querySelector('input[name$=".Nome"]');
    const feedbackElement = item.querySelector('.invalid-feedback');

    const nome = nomeInput.value.trim();

    if (!nome) {
        nomeInput.classList.add('is-invalid');
        feedbackElement.style.display = 'block';
        showNotification('Informe o nome da subpasta', 'warning');
        return;
    }

    if (!pastaSelect.value) {
        pastaSelect.classList.add('is-invalid');
        pastaSelect.nextElementSibling.style.display = 'block';
        showNotification('Selecione uma pasta principal', 'warning');
        return;
    }

    const pastaPrincipalNome = pastaSelect.options[pastaSelect.selectedIndex].text;

    const formData = {
        Id: item.querySelector('input[name$=".Id"]').value,
        Nome: nome,
        PastaPrincipalId: pastaSelect.value,
        PastaPrincipalNome: pastaPrincipalNome
    };

    try {
        toggleLoading(button, true);

        const response = await fetch('/Pagamento/SalvarSubpastaIndividual', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(formData)
        });

        const result = await response.json();

        if (!response.ok || !result.success) {
            throw result;
        }

        updateSubPastaUI(item, button, result.subpastaId);
        showNotification(result.message, 'success');

    } catch (error) {
        console.error('Erro ao salvar subpasta:', error);
        handleSubPastaError(error, item);
    } finally {
        toggleLoading(button, false);
    }
}

// Atualiza a UI após salvar
function updateSubPastaUI(item, button, id) {
    item.querySelector('input[name$=".Id"]').value = id;
    item.classList.add('saved');

    const saveIcon = button.querySelector('.fa-save');
    const checkIcon = button.querySelector('.fa-check');

    if (saveIcon) saveIcon.style.display = 'none';
    if (checkIcon) checkIcon.style.display = 'inline-block';

    button.classList.remove('btn-primary');
    button.classList.add('btn-success');

    item.querySelector('input[name$=".Nome"]').classList.remove('is-invalid');
    item.querySelector('.invalid-feedback').style.display = 'none';
    document.getElementById('pastaPrincipalSelect').classList.remove('is-invalid');
    document.getElementById('pastaPrincipalSelect').nextElementSibling.style.display = 'none';
}

// Remove uma subpasta
async function removerSubpasta(button) {
    const container = document.getElementById("subpastas-container");
    const items = container.querySelectorAll('.subpasta-item');

    if (items.length <= 1) {
        showNotification('Mantenha pelo menos uma subpasta', 'warning');
        return;
    }

    const confirm = await confirmAction('Tem certeza que deseja remover esta subpasta?');
    if (!confirm) return;

    const item = button.closest('.subpasta-item');
    const idInput = item.querySelector('input[name$=".Id"]');

    if (idInput.value && idInput.value !== "00000000-0000-0000-0000-000000000000") {
        try {
            const response = await fetch('/Pagamento/RemoverSubpasta', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ id: idInput.value })
            });

            if (!response.ok) {
                throw new Error('Erro ao remover subpasta');
            }

            const data = await response.json();
            if (!data.success) {
                throw new Error(data.message || 'Erro ao remover subpasta');
            }

            showNotification('Subpasta removida com sucesso', 'success');
        } catch (error) {
            console.error('Erro:', error);
            showNotification(error.message || 'Erro ao remover subpasta', 'error');
            return;
        }
    }

    item.remove();
    reindexSubPastas();
}

// Reindexa as subpastas após remoção
function reindexSubPastas() {
    const items = document.querySelectorAll('.subpasta-item');
    items.forEach((item, index) => {
        const inputs = item.querySelectorAll('input');
        inputs[0].name = `SubPastas[${index}].Id`;
        inputs[1].name = `SubPastas[${index}].PastaPrincipalId`;
        inputs[2].name = `SubPastas[${index}].PastaPrincipalNome`;
        inputs[3].name = `SubPastas[${index}].Nome`;

        const saveButton = item.querySelector('button[onclick^="salvarSubpastaIndividual"]');
        if (saveButton) {
            saveButton.setAttribute('onclick', `salvarSubpastaIndividual(this, ${index})`);
        }
    });
}

// Mostra estado de carregamento
function toggleLoading(button, isLoading) {
    button.disabled = isLoading;
    button.innerHTML = isLoading
        ? '<i class="fas fa-spinner fa-spin"></i> Salvando...'
        : button.innerHTML.includes('fa-check')
            ? '<i class="fas fa-check"></i> Salvar'
            : '<i class="fas fa-save"></i> Salvar';
}

// Trata erros de subpasta
function handleSubPastaError(error, item) {
    let errorMessage = error.message || 'Ocorreu um erro ao processar a solicitação';

    document.querySelectorAll('.invalid-feedback').forEach(el => {
        el.style.display = 'none';
    });
    document.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });

    const isDuplicateError = errorMessage.toLowerCase().includes('já existe uma subpasta') ||
        (error.isDuplicateError !== undefined && error.isDuplicateError);

    if (isDuplicateError) {
        showNotification(errorMessage, 'error');
    }
    else if (errorMessage.toLowerCase().includes('nome')) {
        const nomeInput = item.querySelector('input[name$=".Nome"]');
        nomeInput.classList.add('is-invalid');
        item.querySelector('.invalid-feedback').style.display = 'block';
    }
    else if (errorMessage.toLowerCase().includes('pasta principal')) {
        const select = document.getElementById('pastaPrincipalSelect');
        select.classList.add('is-invalid');
        select.nextElementSibling.style.display = 'block';
    }
    else {
        showNotification(errorMessage, 'error');
    }
}

// Carrega as subpastas de uma pasta principal específica
async function carregarSubpastas(pastaPrincipalId) {
    try {
        const response = await fetch(`/Pagamento/GetSubpastasPorPasta?pastaPrincipalId=${pastaPrincipalId}`);
        if (!response.ok) {
            throw new Error('Erro ao carregar subpastas');
        }

        const subPastas = await response.json();
        const container = document.getElementById('subpastas-container');
        container.innerHTML = '';

        if (subPastas.length === 0) {
            addSubPasta();
            return;
        }

        subPastas.forEach((subPasta, index) => {
            const html = `
                <div class="form-group subpasta-item mt-3 saved">
                    <input type="hidden" name="SubPastas[${index}].Id" value="${subPasta.id}" />
                    <input type="hidden" name="SubPastas[${index}].PastaPrincipalId" value="${subPasta.pastaPrincipalId}" />
                    <input type="hidden" name="SubPastas[${index}].PastaPrincipalNome" value="${subPasta.pastaPrincipalNome}" />
                    <label class="form-label">Nome da Subpasta <span class="text-danger">*</span></label>
                    <input type="text" name="SubPastas[${index}].Nome" 
                           class="form-control"
                           value="${subPasta.nome}"
                           placeholder="Digite o nome da subpasta"
                           maxlength="100"
                           required />
                    <div class="invalid-feedback">
                        <i class="fas fa-exclamation-circle"></i>
                        <span>O nome da subpasta é obrigatório</span>
                    </div>
                    <div class="subpasta-actions">
                        <button type="button" class="btn btn-outline btn-sm mt-2 btn-success"
                                onclick="salvarSubpastaIndividual(this, ${index})">
                            <i class="fas fa-check"></i> Salvar
                        </button>
                        <button type="button" class="btn btn-outline btn-sm mt-2 btn-danger"
                                onclick="removerSubpasta(this)">
                            <i class="fas fa-trash"></i> Remover
                        </button>
                    </div>
                </div>`;

            container.insertAdjacentHTML('beforeend', html);
        });

        document.getElementById('addSubPastaBtn').disabled = false;

    } catch (error) {
        console.error('Erro ao carregar subpastas:', error);
        showNotification('Erro ao carregar subpastas', 'error');
    }
}

// Inicialização quando o DOM estiver carregado
document.addEventListener('DOMContentLoaded', function () {
    const select = document.getElementById('pastaPrincipalSelect');
    if (select) {
        const addBtn = document.getElementById('addSubPastaBtn');
        addBtn.disabled = !select.value;

        select.addEventListener('change', function () {
            document.getElementById('hiddenPastaPrincipalId').value = this.value;
            const addBtn = document.getElementById('addSubPastaBtn');
            addBtn.disabled = !this.value;

            carregarSubpastas(this.value);
        });

        const pastaPrincipalId = document.getElementById('hiddenPastaPrincipalId').value;
        if (pastaPrincipalId) {
            carregarSubpastas(pastaPrincipalId);
        }
    }

    const form = document.getElementById("subpastasForm");
    if (form) {
        form.addEventListener("submit", function (e) {
            let isValid = true;

            const pastaSelect = document.getElementById('pastaPrincipalSelect');
            if (!pastaSelect.value) {
                pastaSelect.classList.add('is-invalid');
                pastaSelect.nextElementSibling.style.display = 'block';
                isValid = false;
            }

            const nomeInputs = document.querySelectorAll('.subpasta-item input[name$=".Nome"]');
            nomeInputs.forEach(input => {
                if (!input.value.trim()) {
                    input.classList.add('is-invalid');
                    input.nextElementSibling.style.display = 'block';
                    isValid = false;
                }
            });

            if (!isValid) {
                e.preventDefault();
                showNotification('Preencha todos os campos obrigatórios', 'error');
                return;
            }

            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                const submitText = submitButton.querySelector('.submit-text');
                const spinner = submitButton.querySelector('.loading-spinner');

                if (submitText) submitText.style.display = 'none';
                if (spinner) spinner.style.display = 'inline-block';
                submitButton.disabled = true;
            }
        });
    }
});