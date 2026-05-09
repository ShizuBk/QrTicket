document.addEventListener('DOMContentLoaded', async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const eventId = urlParams.get('eventId');

    if (!eventId) {
        window.location.href = 'events.html';
        return;
    }

    // 1. Obtener datos del evento para mostrar imagen y precio
    try {
        const response = await fetch(`http://192.168.0.80:5000/TicketPurchase/event/${eventId}`);
        const event = await response.json();

        document.getElementById('event-name').textContent = event.name;
        document.getElementById('unit-price').textContent = `$${event.fee}`;
        document.getElementById('event-description').textContent = event.details;
        
        // Si el admin cargó imagen, se muestra, si no, una por defecto
        if(event.imageUrl) {
            document.getElementById('event-image').src = event.imageUrl;
        }

        // Lógica para actualizar el total al cambiar cantidad
        const inputCount = document.getElementById('ticket-count');
        inputCount.addEventListener('input', () => {
            const total = inputCount.value * event.fee;
            document.getElementById('total-price').textContent = `$${total.toFixed(2)}`;
        });

    } catch (error) {
        console.error("Error al obtener detalles:", error);
    }
});