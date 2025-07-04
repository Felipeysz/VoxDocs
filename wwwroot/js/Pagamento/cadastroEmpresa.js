$(document).ready(function () {
    // Configuração inicial
    setupValidation();

    // Verificação em tempo real para nome da empresa
    $('#EmpresaNome').on('blur', function () {
        verificarNomeEmpresaExistente($(this).val());
    });

    // Verificação em tempo real para email da empresa
    $('#EmailEmpresa').on('blur', function () {
        if ($(this).val().trim() !== '') {
            verificarEmailEmpresaExistente($(this).val());
        }
    });

    // Desabilita o botão de submit se o formulário estiver inválido
    $('form').on('submit', function () {
        if (!$(this).valid()) {
            return false;
        }
    });

    // Adiciona classes de validação quando o campo perde o foco
    $('.form-control').on('blur', function () {
        if ($(this).val().trim() !== '') {
            if ($(this).hasClass('input-validation-error')) {
                $(this).addClass('is-invalid');
            } else {
                $(this).addClass('is-valid');
            }
        }
    });
});

function setupValidation() {
    // Adiciona classes de validação personalizadas
    $.validator.setDefaults({
        errorClass: 'is-invalid',
        validClass: 'is-valid',
        errorElement: 'div',
        errorPlacement: function (error, element) {
            error.addClass('invalid-feedback');
            element.after(error);
        },
        highlight: function (element, errorClass, validClass) {
            $(element).addClass(errorClass).removeClass(validClass);
            $(element).nextAll('.invalid-feedback').show();
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).removeClass(errorClass).addClass(validClass);
            $(element).nextAll('.invalid-feedback').hide();
        }
    });

    // Adiciona validação customizada para verificar se o nome/email já existe
    $.validator.addMethod("empresaUnica", function (value, element) {
        return $(element).data('unico') === true;
    }, "Este valor já está em uso.");
}

function verificarNomeEmpresaExistente(nome) {
    if (nome.trim() === '') return;

    $.ajax({
        url: '/Pagamento/VerificarNomeEmpresa',
        type: 'GET',
        data: { nome: nome },
        success: function (response) {
            if (response.existe) {
                $('#nomeExistenteError').show();
                $('#EmpresaNome').addClass('is-invalid');
                $('#EmpresaNome').data('unico', false);
            } else {
                $('#nomeExistenteError').hide();
                $('#EmpresaNome').removeClass('is-invalid');
                $('#EmpresaNome').addClass('is-valid');
                $('#EmpresaNome').data('unico', true);
            }
        }
    });
}

function verificarEmailEmpresaExistente(email) {
    if (email.trim() === '') return;

    $.ajax({
        url: '/Pagamento/VerificarEmailEmpresa',
        type: 'GET',
        data: { email: email },
        success: function (response) {
            if (response.existe) {
                $('#emailExistenteError').show();
                $('#EmailEmpresa').addClass('is-invalid');
                $('#EmailEmpresa').data('unico', false);
            } else {
                $('#emailExistenteError').hide();
                $('#EmailEmpresa').removeClass('is-invalid');
                $('#EmailEmpresa').addClass('is-valid');
                $('#EmailEmpresa').data('unico', true);
            }
        }
    });
}