document.addEventListener('DOMContentLoaded', () => {
    cargarCartelera();
});

async function cargarCartelera() {
    try {
        const baseUrl = 'http://192.168.0.80:5000'; 
        const response = await fetch(`${baseUrl}/api/TicketPurchase/public`); 
        
        if (!response.ok) throw new Error(`Error: ${response.status}`);

        const eventos = await response.json();
        const grid = document.getElementById('events-grid');
        if (!grid) return; 
        
        grid.innerHTML = ''; 

        eventos.forEach(ev => {
            if (!ev.sysVisible) return; 

            // 1. LÓGICA DE DISPONIBILIDAD Y TEXTOS DINÁMICOS
            const disponibles = ev.capacity - (ev.soldTickets || 0);
            const isSoldOut = disponibles <= 0;
            
            let statusClass = '';
            let statusText = '';

            if (isSoldOut) {
                statusClass = 'status-danger';
                statusText = 'AGOTADO';
            } else if (disponibles <= 10) {
                statusClass = 'status-warning';
                statusText = `¡ÚLTIMOS ${disponibles} DISPONIBLES!`; 
            } else {
                statusClass = 'status-ok';
                statusText = 'DISPONIBLE';
            }

            // 2. CONSTRUCCIÓN DE LA URL DE LA IMAGEN
            let fotoUrl = '';
            if (ev.imageUrl && ev.imageUrl.trim() !== '') {
                const rutaLimpia = ev.imageUrl.replace(/\\/g, '/').replace(/^\//, '');
                fotoUrl = `${baseUrl}/${rutaLimpia}`;
            }

            const card = document.createElement('div');
            card.className = `event-card ${isSoldOut ? 'sold-out' : ''}`;
            
            card.style.position = 'relative';

            card.innerHTML = `
                <!-- Badge de disponibilidad (Siempre visible por CSS) -->
                <div class="status-badge ${statusClass}">${statusText}</div>
                
                <div class="event-image">
                    ${fotoUrl 
                        ? `<img src="${fotoUrl}" alt="${ev.name}" class="event-card-img">` 
                        : '<span class="no-image">PRÓXIMAMENTE</span>'
                    }
                </div>

                <div class="event-content">
                    <h3>${ev.name}</h3>
                    <p class="event-date">
                        📅 ${formatearFecha(ev.eventDate)}
                    </p>
                    
                    <div class="price-container">
                        <p class="event-price">
                        <span class="price-label">Precio :</span>
                            $${ev.fee.toLocaleString('es-MX', { minimumFractionDigits: 2 })}
                        </p>
                    </div>
                    
                    <button 
                        onclick="irAlCheckout('${ev.id}')" 
                        class="btn-buy" 
                        ${(!ev.sysEnabled || isSoldOut) ? 'disabled' : ''}>
                        ${!ev.sysEnabled ? 'VENTA PAUSADA' : (isSoldOut ? 'AGOTADO' : 'ADQUIRIR BOLETOS')}
                    </button>
                </div>
            `;
            grid.appendChild(card);
        });

    } catch (error) {
        console.error("Error al cargar la cartelera:", error);
        const grid = document.getElementById('events-grid');
        if (grid) {
            grid.innerHTML = `
                <div style="color:white; text-align:center; grid-column: 1/-1; padding: 2rem;">
                    <p>No se pudo conectar con el servidor de Partenón.</p>
                </div>`;
        }
    }
}


function formatearFecha(fechaStr) {
    const opciones = { weekday: 'long', day: 'numeric', month: 'long' };
    try {
        const fecha = new Date(fechaStr);
        const fechaFormateada = fecha.toLocaleDateString('es-MX', opciones);
        return fechaFormateada.charAt(0).toUpperCase() + fechaFormateada.slice(1);
    } catch (e) {
        return fechaStr;
    }
}

/**
 * Redirección al proceso de compra
 */
function irAlCheckout(id) {
    window.location.href = `checkout.html?eventId=${id}`;
}