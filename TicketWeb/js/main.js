/**
 * Partenón Zihuatanejo
 * Version: 20260415
 */

let compraFinalizada = false;

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

// --- 1. LÓGICA DE NAVEGACIÓN Y ADVERTENCIAS ---
async function intentarRegresarInicio() {
    if (compraFinalizada) {
        window.location.href = "index.html";
        return;
    }

    const tName = document.getElementById('titularName')?.value.trim();
    const tieneBoletos = obtenerDetalleAsistentes().length > 0;

    if (tName || tieneBoletos) {
        const resultado = await Swal.fire({
            title: '¿Estás seguro de salir?',
            text: "Se perderá todo el progreso de tu compra actual.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Sí, salir',
            cancelButtonText: 'Continuar con la compra'
        });

        if (resultado.isConfirmed) {
            window.location.href = "index.html";
        }
    } else {
        window.location.href = "index.html";
    }
}

// --- 2. PASAR A PAGAR (VALIDACIÓN Y RESUMEN) ---
async function irAPagar() {
    const tName = document.getElementById('titularName').value.trim();
    const tLast = document.getElementById('titularLastName').value.trim();
    const tEmail = document.getElementById('email').value.trim();
    
    const inputs = document.querySelectorAll('.qty-input');
    let detalleHTML = `<p><strong>Titular:</strong> ${tName} ${tLast}</p><hr style="border:0; border-top:1px solid #eee; margin:10px 0;">`;
    let totalActual = 0;
    let tieneBoletos = false;

    inputs.forEach(input => {
        const cantidad = parseInt(input.value) || 0;
        const tipo = input.getAttribute('data-tipo');
        if (cantidad > 0) {
            tieneBoletos = true;
            const sub = cantidad * (PRECIOS_BOLETOS[tipo] || 0);
            totalActual += sub;
            detalleHTML += `<p style="display:flex; justify-content:space-between; margin:5px 0;">
                                <span>${cantidad}x ${tipo}</span> 
                                <span>$${sub.toFixed(2)}</span>
                            </p>`;
        }
    });

    const emailRegex = /^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}$/;
    if (!tName || !tLast || !tEmail || !emailRegex.test(tEmail)) {
        Swal.fire('Datos Incompletos', 'Ingresa nombre completo y un correo válido.', 'warning');
        return;
    }

    if (!tieneBoletos) {
        Swal.fire('Sin Boletos', 'Selecciona al menos un boleto.', 'info');
        return;
    }

    const confirmacion = await Swal.fire({
        title: '¿Confirmar datos?',
        html: `Titular: ${tName} ${tLast}<br>Total: <b>$${totalActual.toFixed(2)} MXN</b>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, ir a pagar',
        cancelButtonText: 'Corregir'
    });

    if (confirmacion.isConfirmed) {
        document.getElementById('resumenCompraFinal').innerHTML = detalleHTML;
        document.getElementById('paso1').classList.add('hidden');
        document.getElementById('paso2').classList.remove('hidden');
        window.scrollTo(0, 0); 
    }
}

function volverADatos() {
    document.getElementById('paso2').classList.add('hidden');
    document.getElementById('paso1').classList.remove('hidden');
}

// --- 3. CÁLCULO DE PRECIOS CONFORME SE VA ELIGIENDO EL BOLETO ---
function actualizarPrecios() {
    let subtotal = 0;
    const inputs = document.querySelectorAll('.qty-input');
    
    inputs.forEach(input => {
        const cantidad = parseInt(input.value) || 0;
        const tipo = input.getAttribute('data-tipo');
        subtotal += (cantidad * (PRECIOS_BOLETOS[tipo] || 0));
    });

    const formato = `$${subtotal.toFixed(2)}`;
    const preliminar = document.getElementById('subtotal_preliminar');
    const sub = document.getElementById('subtotal');
    const tot = document.getElementById('totalPrice');
    
    if(preliminar) preliminar.innerText = formato;
    if(sub) sub.innerText = formato;
    if(tot) tot.innerText = formato;
}

function obtenerDetalleAsistentes() {
    let listaParaAPI = [];
    document.querySelectorAll('.qty-input').forEach(input => {
        const cantidad = parseInt(input.value) || 0;
        const tipo = input.getAttribute('data-tipo');
        for (let i = 0; i < cantidad; i++) { listaParaAPI.push(tipo); }
    });
    return listaParaAPI;
}

// --- 4. PROCESO DE PAGO ---
async function simularCobro() {
    const elCard = document.getElementById('cardNumber');
    const elExp = document.getElementById('exp');
    const elCcv = document.getElementById('ccv');
    const btn = document.getElementById('btnPagar');

    const cardNum = elCard ? elCard.value.replace(/\D/g, '') : "";
    const expDate = elExp ? elExp.value.replace(/\D/g, '') : ""; 
    const ccv = elCcv ? elCcv.value.replace(/\D/g, '') : "";

    // --- BLOQUE DE VALIDACIONES ---

    // Validar Tarjeta (16 dígitos)
    if (cardNum.length < 16) {
        return Swal.fire({
            title: 'Número de tarjeta incompleto',
            text: 'La tarjeta debe tener 16 dígitos.',
            icon: 'warning',
            confirmButtonColor: '#3498db'
        });
    }

    // Validar Expiración (Deben ser 4 números: 2 de mes y 2 de año)
    if (expDate.length < 4) {
        return Swal.fire({
            title: 'Fecha de expiración incompleta',
            text: 'Ingresa el mes y año (MM/AA).',
            icon: 'warning',
            confirmButtonColor: '#3498db'
        });
    }

    // Validar CCV (3 dígitos)
    if (ccv.length < 3) {
        return Swal.fire({
            title: 'Código CCV incompleto',
            text: 'Ingresa los 3 dígitos de seguridad al reverso.',
            icon: 'warning',
            confirmButtonColor: '#3498db'
        });
    }

    // --- CONFIRMACIÓN FINAL ---
    const confirmacion = await Swal.fire({
        title: '¿Confirmar Pago?',
        text: "Se procesará el cargo y se descargará tu ticket.",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#27ae60',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, pagar ahora',
        cancelButtonText: 'Cancelar'
    });

    if (!confirmacion.isConfirmed) return;

    // --- PROCESO DE PAGO ---
    btn.disabled = true;
    btn.innerHTML = `⏳ Procesando pago...`;

    try {
        await procesarCompra(obtenerDetalleAsistentes());
        
        await Swal.fire({
            title: '¡Pago Exitoso!',
            text: 'Tu ticket se ha generado correctamente.',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false
        });

        finalizarInterfaz();

    } catch (error) {
        console.error("Error en la compra:", error);
        Swal.fire('Error', 'No se pudo procesar: ' + error.message, 'error');
        
        // Reactiva el botón si hubo error para que puedan reintentar
        btn.disabled = false;
        btn.innerHTML = "Confirmar Pago y Descargar Ticket";
    }
}

async function procesarCompra(listaAsistentes) {
    const pedidoFinal = {
        TitularName: document.getElementById('titularName').value,
        TitularLastName: document.getElementById('titularLastName').value,
        TitularSurname: document.getElementById('titularSurname').value,
        AssistantNumber: listaAsistentes.length,
        AssistantDetails: listaAsistentes,
        Email: document.getElementById('email').value,
        PurchaseDate: new Date().toISOString(),
        TotalAmount: parseFloat(document.getElementById('totalPrice').innerText.replace('$', ''))
    };

    const response = await fetch('http://localhost:5193/checkout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/pdf' },
        body: JSON.stringify(pedidoFinal)
    });

    if (response.ok) {
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/pdf")) {
            const buffer = await response.arrayBuffer();
            const blob = new Blob([buffer], { type: 'application/pdf' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Ticket_${Date.now()}.pdf`;
            document.body.appendChild(a);
            a.click();
            setTimeout(() => window.URL.revokeObjectURL(url), 200);
        }
    } else {
        throw new Error("No se pudo generar el PDF.");
    }
}

function finalizarInterfaz() {
    compraFinalizada = true;
    const mainContainer = document.querySelector('.checkout-container') || document.getElementById('paso2');
    
    if (mainContainer) {
        mainContainer.innerHTML = `
            <div style="text-align: center; padding: 40px 20px; background: #ffffff; border-radius: 20px; box-shadow: 0 15px 35px rgba(0,0,0,0.1); max-width: 400px; margin: 40px auto; font-family: sans-serif; border: 1px solid #f0f0f0;">
                <div style="font-size: 60px; margin-bottom: 20px;">✅</div>
                <h2 style="color: #2c3e50; margin: 0 0 10px 0; font-size: 24px;">¡Pago Confirmado!</h2>
                <p style="color: #7f8c8d; font-size: 16px; margin-bottom: 25px; line-height: 1.5;">
                    Tu ticket para el <strong>Partenón Zihuatanejo</strong> se ha descargado correctamente.
                </p>
                
                <div style="border-top: 2px dashed #eee; margin: 20px 0;"></div>
                
                <button onclick="window.location.href='index.html'" 
                    style="background: #3498db; color: white; border: none; padding: 15px 30px; border-radius: 50px; font-size: 16px; font-weight: bold; cursor: pointer; width: 100%; box-shadow: 0 4px 15px rgba(52, 152, 219, 0.4); transition: transform 0.2s; outline: none;">
                    Volver al Inicio
                </button>
                
            </div>
        `;
    }
}

function inicializarMascaras() {
    const cardInput = document.getElementById('cardNumber');
    const expInput = document.getElementById('exp');
    const ccvInput = document.getElementById('ccv');

    // Limitar Tarjeta a 16 números
    if (cardInput) {
        cardInput.addEventListener('input', (e) => {
            e.target.value = e.target.value.replace(/\D/g, '').slice(0, 16);
        });
    }

    // Formato MM/AA automático
    if (expInput) {
        expInput.addEventListener('input', (e) => {
            let val = e.target.value.replace(/\D/g, ''); 
            if (val.length > 2) {
                e.target.value = val.slice(0, 2) + '/' + val.slice(2, 4);
            } else {
                e.target.value = val;
            }
        });
    }

    // Limitar CCV a 3 números
    if (ccvInput) {
        ccvInput.addEventListener('input', (e) => {
            e.target.value = e.target.value.replace(/\D/g, '').slice(0, 3);
        });
    }
}