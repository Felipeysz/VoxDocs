//Variaveis Globais
let currentStep = 1;
const totalSteps = 5;
let formData = JSON.parse(sessionStorage.getItem('formData')) || {};

//Funções de salvamento de Dados:
function saveStepData() {
    const stepHandlers = {
        1: () => {
            formData.EmpresaContratante = document.querySelector('input[name="EmpresaContratante"]')?.value || '';
            formData.EmailEmpresa = document.querySelector('input[name="EmailEmpresa"]')?.value || '';
        },
        2: () => {
            const arrPastas = [];
            document.querySelectorAll('#pastasPrincipaisContainer .pasta-principal').forEach(card => {
                const nomePasta = card.querySelector('.pasta-nome').value.trim();
                const subEls = card.querySelectorAll('.subpasta-nome');
                const listaSub = Array.from(subEls).map(inpSub => inpSub.value.trim()).filter(Boolean);
                arrPastas.push({ Nome: nomePasta, Subpastas: listaSub });
            });
            formData.PastasPrincipais = arrPastas;
        },
        3: () => {
            const admins = [];
            document.querySelectorAll('#adminsContainer .admin-card').forEach(card => {
                const usuario = card.querySelector('input[data-field="Usuario"]')?.value || '';
                const email = card.querySelector('input[data-field="Email"]')?.value || '';
                const senha = card.querySelector('input[data-field="Senha"]')?.value || '';
                admins.push({ Usuario: usuario, Email: email, Senha: senha, PermissionAccount: "Admin" });
            });
            formData.Admins = admins;
        },
        4: () => {
            const users = [];
            document.querySelectorAll('#usersContainer .user-card').forEach(card => {
                const usuario = card.querySelector('input[data-field="Usuario"]')?.value || '';
                const email = card.querySelector('input[data-field="Email"]')?.value || '';
                const senha = card.querySelector('input[data-field="Senha"]')?.value || '';
                users.push({ Usuario: usuario, Email: email, Senha: senha, PermissionAccount: "User" });
            });
            formData.Users = users;
        }
    };

    stepHandlers[currentStep]();
    sessionStorage.setItem('formData', JSON.stringify(formData));
    updateHiddenFields();
}

//Funções de Atualização de Campos Ocultos
function updateHiddenFields() {
    const form = document.getElementById('formWizard');
    if (!form) return;

    const fields = [
        { name: 'EmpresaContratante', value: formData.EmpresaContratante },
        { name: 'EmailEmpresa', value: formData.EmailEmpresa },
        { name: 'PastasPrincipaisJson', value: JSON.stringify(formData.PastasPrincipais || []) },
        { name: 'AdminsJson', value: JSON.stringify(formData.Admins || []) },
        { name: 'UsersJson', value: JSON.stringify(formData.Users || []) }
    ];

    fields.forEach(({ name, value }) => {
        let input = form.querySelector(`input[name="${name}"][type="hidden"]`);
        if (!input) {
            input = document.createElement('input');
            input.type = 'hidden';
            input.name = name;
            form.appendChild(input);
        }
        input.value = value;
    });
}

//Funções de Carregamento de Dados
function loadStepData() {
    const savedData = sessionStorage.getItem('formData');
    if (savedData) {
        formData = JSON.parse(savedData);
    }
    return formData;
}

function loadStep(step) {
    saveStepData();

    const container = document.getElementById('stepContentContainer');
    container.innerHTML = '';

    fetch('/PagamentosMvc/GetCurrentPlan')
        .then(response => response.json())
        .then(data => {
            const planoNome = data.planoNome;
            if (!planoNome) {
                window.location.href = '/PagamentosMvc/SelecionarPlano';
                return;
            }

            fetch(`/PagamentosMvc/Step?step=${step}`)
                .then(response => response.text())
                .then(html => {
                    container.innerHTML = html;
                    updateStepper(step);
                    loadStepData();

                    const stepInitFunctions = {
                        1: () => import('/js/PagamentoScript/step1.js').then(module => module.initStep1(formData)),
                        2: () => loadScript('/js/PagamentoScript/step2.js', () => {
                            if (typeof initStep2 === 'function') initStep2();
                        }),
                        3: () => setTimeout(() => {
                            if (typeof window.initStep3 === 'function') window.initStep3();
                        }, 100),
                        4: () => setTimeout(() => {
                            if (typeof window.initStep4 === 'function') window.initStep4();
                        }, 100),
                        5: () => setTimeout(() => {
                            if (typeof window.initStep5 === 'function') window.initStep5();
                            else console.error('initStep5 não está definido!');
                        }, 100)
                    };

                    stepInitFunctions[step]();

                    currentStep = step;
                })
                .catch(error => {
                    container.innerHTML = `<div class="alert alert-danger"><strong>Erro:</strong> ${error.message}</div>`;
                    console.error('Erro ao carregar etapa:', error);
                });
        })
        .catch(error => {
            console.error('Erro ao recuperar plano:', error);
            window.location.href = '/PagamentosMvc/SelecionarPlano';
        });
}

//Funções de Navegação e UI
function updateStepper(step) {
    document.querySelectorAll('.stepper .step').forEach((el, idx) => {
        el.classList.toggle('active', idx + 1 === step);
    });
    document.getElementById('prevBtn').disabled = (step === 1);
    document.getElementById('nextBtn').textContent = (step === totalSteps ? 'Concluir' : 'Próximo »');

    const nextBtn = document.getElementById('nextBtn');
    nextBtn.style.display = step === 5 ? 'none' : '';
}

function loadScript(src, callback) {
    const script = document.createElement('script');
    script.src = src;
    script.onload = callback;
    document.head.appendChild(script);
}

//Eventos e Inicialização
document.addEventListener('DOMContentLoaded', function () {
    loadStepData();

    document.getElementById('nextBtn').addEventListener('click', () => {
        const stepValidators = {
            1: () => import('/js/PagamentoScript/step1.js').then(module => module.validateStep1().then(isValid => {
                if (isValid) loadStep(currentStep + 1);
            })),
            2: () => {
                if (typeof validateStep2 === 'function' && !validateStep2()) return;
                loadStep(currentStep + 1);
            },
            3: () => {
                if (typeof validateStep3 === 'function' && !validateStep3()) return;
                loadStep(currentStep + 1);
            },
            4: () => {
                if (typeof validateStep4 === 'function' && !validateStep4()) return;
                loadStep(currentStep + 1);
            },
            5: () => {
                if (typeof validateStep5 === 'function' && !validateStep5()) return;
                loadStep(currentStep + 1);
            }
        };

        stepValidators[currentStep]();
    });

    document.getElementById('prevBtn').addEventListener('click', () => {
        if (currentStep > 1) {
            saveStepData();
            loadStep(currentStep - 1);
        }
    });

    loadStep(currentStep);

    window.addEventListener('unload', () => {
        sessionStorage.removeItem('formData');
    });
});