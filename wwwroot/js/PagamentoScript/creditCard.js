const ccInput = document.getElementById('cartaoNumber');
const flagEl = document.getElementById('cc-flag');
const expInput = document.getElementById('validadeCartao');
const cvvInput = document.getElementById('cvvCartao');

// formata número em blocos de 4
function formatCard(v) {
  const nums = v.replace(/\D/g, '').slice(0, 16);
  return nums.replace(/(.{4})/g, '$1 ').trim();
}

// detecta bandeira pelo início do número
function detectBrand(num) {
  if (/^4/.test(num)) return 'visa';
  if (/^5[1-5]/.test(num)) return 'mastercard';
  if (/^3[47]/.test(num)) return 'amex';
  return '';
}

if (ccInput) {
  ccInput.addEventListener('input', e => {
    e.target.value = formatCard(e.target.value);
    const brand = detectBrand(e.target.value.replace(/\s/g, ''));
    flagEl.style.backgroundImage = brand
      ? `url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/${brand}.svg)`
      : '';
  });
}

if (expInput) {
  // auto-inserção de "/" em MM/AA
  expInput.addEventListener('input', e => {
    let v = e.target.value.replace(/\D/g, '').slice(0, 4);
    if (v.length > 2) {
      v = v.slice(0, 2) + '/' + v.slice(2);
    }
    e.target.value = v;
  });

  // valida expiração e destaca campo
  function validExpiry(v) {
    if (!/^\d{2}\/\d{2}$/.test(v)) return false;
    const [mm, yy] = v.split('/').map(n => parseInt(n, 10));
    if (mm < 1 || mm > 12) return false;
    const now = new Date();
    const exp = new Date(2000 + yy, mm - 1);
    return exp > now;
  }
  
  expInput.addEventListener('blur', e => {
    e.target.style.borderColor = validExpiry(e.target.value) ? '' : 'red';
  });
}

if (cvvInput) {
  // valida CVV e destaca campo
  function validCVV(v) {
    const brand = detectBrand(ccInput.value.replace(/\s/g, ''));
    const len = brand === 'amex' ? 4 : 3;
    return new RegExp(`^\\d{${len}}$`).test(v);
  }
  
  cvvInput.addEventListener('blur', e => {
    e.target.style.borderColor = validCVV(e.target.value) ? '' : 'red';
  });
}