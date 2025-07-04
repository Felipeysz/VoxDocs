$(document).ready(function () {
    console.log('Documento pronto, iniciando scripts...');

    // 1. Animação do step ativo
    $('.step.active .step-number').css('animation', 'slowPulse 3s infinite ease-in-out');

    // 2. Formatação e validação do número do cartão
    $('#numeroCartao').on('input', function () {
        // Remove tudo que não é dígito
        let value = $(this).val().replace(/\D/g, '');

        // Adiciona espaço a cada 4 dígitos
        value = value.replace(/(\d{4})(?=\d)/g, '$1 ');

        // Atualiza o valor no campo
        $(this).val(value.trim());

        // Limita a 19 caracteres (16 dígitos + 3 espaços)
        if (value.length > 19) {
            $(this).val(value.substring(0, 19));
        }

        // Detecta a bandeira do cartão
        detectCardBrand(value.replace(/\s/g, ''));

        // Validação em tempo real
        validateCardNumber(value.replace(/\s/g, ''));
    });

    // 3. Formatação e validação da data de validade
    $('#validadeCartao').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');

        // Formata como MM/AA
        if (value.length > 2) {
            value = value.substring(0, 2) + '/' + value.substring(2, 4);
        }

        $(this).val(value);

        // Validação em tempo real
        validateExpiryDate(value);
    });

    // 4. Formatação e validação do CVV
    $('#cvvCartao').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');

        // Limita a 3 ou 4 dígitos dependendo da bandeira
        const cardNumber = $('#numeroCartao').val().replace(/\D/g, '');
        const isAmex = /^3[47]/.test(cardNumber);
        const maxLength = isAmex ? 4 : 3;

        if (value.length > maxLength) {
            value = value.substring(0, maxLength);
        }

        $(this).val(value);

        // Validação em tempo real
        validateCVV(value, isAmex);
    });

    // 5. Formatação e validação do nome no cartão
    $('#nomeCartao').on('input', function () {
        // Remove números e caracteres especiais
        let value = $(this).val().replace(/[^a-zA-ZÀ-ÿ\s]/g, '');
        $(this).val(value);

        // Validação em tempo real
        validateCardName(value);
    });

    // 6. Formatação e validação do CPF
    $('#cpfTitular').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');

        // Formatação: 000.000.000-00
        if (value.length > 3 && value.length <= 6) {
            value = value.replace(/(\d{3})(\d{0,3})/, '$1.$2');
        } else if (value.length > 6 && value.length <= 9) {
            value = value.replace(/(\d{3})(\d{3})(\d{0,3})/, '$1.$2.$3');
        } else if (value.length > 9) {
            value = value.replace(/(\d{3})(\d{3})(\d{3})(\d{0,2})/, '$1.$2.$3-$4');
        }

        $(this).val(value);

        // Validação em tempo real
        validateCPF(value);
    });

    // Função para detectar a bandeira do cartão
    function detectCardBrand(number) {
        const brand = $('#cardBrand');
        number = number.replace(/\D/g, '');

        if (/^4/.test(number)) {
            brand.attr('src', 'https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Visa_Inc._logo.svg/2560px-Visa_Inc._logo.svg.png')
                .attr('alt', 'Visa')
                .show();
        } else if (/^5[1-5]/.test(number)) {
            brand.attr('src', 'https://upload.wikimedia.org/wikipedia/commons/thumb/2/2a/Mastercard-logo.svg/1280px-Mastercard-logo.svg.png')
                .attr('alt', 'Mastercard')
                .show();
        } else if (/^3[47]/.test(number)) {
            brand.attr('src', 'https://upload.wikimedia.org/wikipedia/commons/thumb/f/fa/American_Express_logo_%282018%29.svg/1200px-American_Express_logo_%282018%29.svg.png')
                .attr('alt', 'American Express')
                .show();
        } else {
            brand.hide();
        }
    }

    // Função para validar número do cartão - CORRIGIDA
    function validateCardNumber(cardNumber) {
        const cleanedNumber = cardNumber.replace(/\D/g, '');
        const errorElement = $('#cartaoError');
        const inputElement = $('#numeroCartao');

        // Verifica se tem entre 13 e 19 dígitos (dependendo da bandeira)
        if (cleanedNumber.length < 13 || cleanedNumber.length > 19) {
            errorElement.text('Número de cartão inválido').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        // Verifica se é um número válido usando o algoritmo de Luhn
        if (!luhnCheck(cleanedNumber)) {
            errorElement.text('Número de cartão inválido').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        errorElement.hide();
        updateValidationStatus(inputElement, true);
        return true;
    }

    // Algoritmo de Luhn para validar número do cartão - CORRIGIDO
    function luhnCheck(cardNumber) {
        let sum = 0;
        let shouldDouble = false;

        // Percorre os dígitos da direita para a esquerda
        for (let i = cardNumber.length - 1; i >= 0; i--) {
            let digit = parseInt(cardNumber.charAt(i));

            if (shouldDouble) {
                digit *= 2;
                if (digit > 9) {
                    digit = (digit % 10) + 1; // ou digit -= 9
                }
            }

            sum += digit;
            shouldDouble = !shouldDouble;
        }

        return (sum % 10) === 0;
    }

    // Função para validar data de expiração
    function validateExpiryDate(expiryDate) {
        const errorElement = $('#validadeError');
        const inputElement = $('#validadeCartao');
        const [month, year] = expiryDate.split('/');

        // Verifica o formato básico
        if (!expiryDate.match(/^(0[1-9]|1[0-2])\/?([0-9]{2})$/)) {
            errorElement.text('Formato inválido (MM/AA)').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        const currentDate = new Date();
        const currentYear = currentDate.getFullYear() % 100;
        const currentMonth = currentDate.getMonth() + 1;

        // Verifica se o mês é válido (1-12)
        if (month < 1 || month > 12) {
            errorElement.text('Mês inválido').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        // Verifica se a data não está expirada
        if (year < currentYear || (year == currentYear && month < currentMonth)) {
            errorElement.text('Cartão expirado').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        errorElement.hide();
        updateValidationStatus(inputElement, true);
        return true;
    }

    // Função para validar CVV
    function validateCVV(cvv, isAmex) {
        const errorElement = $('#cvvError');
        const inputElement = $('#cvvCartao');
        const requiredLength = isAmex ? 4 : 3;

        if (cvv.length < requiredLength) {
            errorElement.text(`O CVV deve ter ${requiredLength} dígitos`).show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        if (!/^\d+$/.test(cvv)) {
            errorElement.text('O CVV deve conter apenas números').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        errorElement.hide();
        updateValidationStatus(inputElement, true);
        return true;
    }

    // Função para validar nome no cartão
    function validateCardName(name) {
        const errorElement = $('#nomeError');
        const inputElement = $('#nomeCartao');

        if (name.trim().length < 3) {
            errorElement.text('Nome muito curto').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        if (name.trim().split(' ').length < 2) {
            errorElement.text('Informe nome e sobrenome').show();
            updateValidationStatus(inputElement, false);
            return false;
        }

        errorElement.hide();
        updateValidationStatus(inputElement, true);
        return true;
    }

    // Função para atualizar o status visual da validação
    function updateValidationStatus(inputElement, isValid) {
        if (isValid) {
            inputElement.addClass('valid').removeClass('invalid');
        } else {
            inputElement.addClass('invalid').removeClass('valid');
        }
    }

    // Validação final no submit - ATUALIZADA
    $('#pagamentoForm').on('submit', function (e) {
        e.preventDefault();
        let isValid = true;

        // Resetar mensagens de erro e estados
        $('.validation-message').hide();
        $('.form-control').removeClass('invalid valid');

        // Validar cada campo
        isValid = validateCardNumber($('#numeroCartao').val().replace(/\s/g, '')) && isValid;
        isValid = validateExpiryDate($('#validadeCartao').val()) && isValid;

        const cardNumber = $('#numeroCartao').val().replace(/\D/g, '');
        const isAmex = /^3[47]/.test(cardNumber);
        isValid = validateCVV($('#cvvCartao').val(), isAmex) && isValid;

        isValid = validateCardName($('#nomeCartao').val()) && isValid;

        // Atualiza o estado do botão de submit
        $('button[type="submit"]').prop('disabled', !isValid);

        if (isValid) {
            console.log('Formulário válido, enviando...');
            this.submit();
        } else {
            console.log('Formulário inválido, corrija os erros');
            // Rola até o primeiro erro
            $('html, body').animate({
                scrollTop: $(".validation-message:visible").first().offset().top - 100
            }, 500);
        }
    });

    console.log('Configuração inicial concluída');
});