/**
 * Partenón Zihuatanejo
 */

window.onload = () => {
    actualizarPrecios();
    inicializarMascaras(); 
};

const PRECIOS_BOLETOS = {
    "General": 100.00,
    "Local": 50.00,
    "Estudiante": 20.00,
    "INAPAM": 0.00 
};


function actualizarPrecios() {
    let subtotal = 0;
    const inputs = document.querySelectorAll('.qty-input');
    
    inputs.forEach(input => {
        const cantidad = parseInt(input.value) || 0;
        const tipo = input.getAttribute('data-tipo');
        const precio = PRECIOS_BOLETOS[tipo] || 0;
        subtotal += (cantidad * precio);
    });

    const subtotalEl = document.getElementById('subtotal');
    const totalEl = document.getElementById('totalPrice');
    
    if(subtotalEl) subtotalEl.innerText = `$${subtotal.toFixed(2)}`;
    if(totalEl) totalEl.innerText = `$${subtotal.toFixed(2)}`;
}


function obtenerDetalleAsistentes() {
    let listaParaAPI = [];
    const inputs = document.querySelectorAll('.qty-input');
    inputs.forEach(input => {
        const cantidad = parseInt(input.value) || 0;
        const tipo = input.getAttribute('data-tipo');
        for (let i = 0; i < cantidad; i++) {
            listaParaAPI.push(tipo);
        }
    });
    return listaParaAPI;
}


async function simularCobro() {
    const btn = document.getElementById('btnPagar');
    const listaAsistentes = obtenerDetalleAsistentes();

    const tName = document.getElementById('titularName').value.trim();
    const tEmail = document.getElementById('email').value.trim();
    const cardNum = document.getElementById('cardNumber').value.trim();
    const cardExp = document.getElementById('exp').value.trim();
    const cardCvv = document.getElementById('cvv').value.trim();

    const emailRegex = /^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}$/;
    if (!tName || !tEmail || !emailRegex.test(tEmail)) {
        alert("Por favor, ingrese un nombre y correo electrónico válido.");
        return;
    }

    if (listaAsistentes.length === 0) {
        alert("Por favor, seleccione al menos un boleto.");
        return;
    }

    if (!cardNum || !cardExp || !cardCvv) {
        alert("Por favor, ingrese los datos de su tarjeta.");
        return;
    }

    if (cardNum.length < 16) {
        alert("El número de tarjeta debe tener 16 dígitos.");
        return;
    }

    if (cardExp.length < 5) {
        alert("La fecha de expiración debe tener el formato MM/YY.");
        return;
    }

    btn.disabled = true;
    btn.innerHTML = `⏳ Procesando pago...`;

    setTimeout(async () => {
        try {
            await procesarCompra(listaAsistentes);
            btn.innerHTML = `✅ Compra Exitosa`;
            btn.style.backgroundColor = "#27ae60";
        } catch (error) {
            alert(error.message);
            btn.disabled = false;
            btn.innerHTML = "Confirmar Pago y Descargar Ticket";
            btn.style.backgroundColor = "";
        }
    }, 2000);
}


async function procesarCompra(listaAsistentes) {
    const tName = document.getElementById('titularName').value;
    const tLast = document.getElementById('titularLastName').value;
    const tSur = document.getElementById('titularSurname').value;
    const tEmail = document.getElementById('email').value;

    let montoTotal = 0;
    document.querySelectorAll('.qty-input').forEach(input => {
        const cantidad = parseInt(input.value) || 0;
        const tipo = input.getAttribute('data-tipo');
        if (cantidad > 0) {
            montoTotal += cantidad * PRECIOS_BOLETOS[tipo];
        }
    });

    const pedidoFinal = {
        TitularName: tName,
        TitularLastName: tLast,
        TitularSurname: tSur,
        Email: tEmail,
        PurchaseDate: new Date().toISOString(), 
        AssistantNumber: listaAsistentes.length,
        AssistantDetails: listaAsistentes,
        TotalAmount: montoTotal, 
        purchaseDetails: `Compra por volumen: ${listaAsistentes.length} boletos. Total: $${montoTotal.toFixed(2)}`
    };

    const response = await fetch('http://localhost:5193/checkout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(pedidoFinal)
    });

    if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const nombreArchivo = tName.trim().replace(/\s+/g, '_');
        a.download = `Ticket_Partenon_${nombreArchivo}.pdf`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    } else {
        const errorText = await response.text();
        throw new Error(`Error: ${errorText}`);
    }
}


function inicializarMascaras() {
    const inputExp = document.getElementById('exp');
    const inputCard = document.getElementById('cardNumber');
    const inputCvv = document.getElementById('cvv');

    
    inputExp.addEventListener('input', (e) => {
        let val = e.target.value.replace(/\D/g, ''); 
        if (val.length >= 2) {
            e.target.value = val.slice(0, 2) + '/' + val.slice(2, 4);
        } else {
            e.target.value = val;
        }
    });

    
    [inputCard, inputCvv].forEach(input => {
        input.addEventListener('input', (e) => {
            e.target.value = e.target.value.replace(/\D/g, '');
        });
    });
}