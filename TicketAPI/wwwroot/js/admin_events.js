{
    const API_BASE_URL = 'http://192.168.0.80:5000';

    const inicializar = () => {
        console.log("🚀 Panel iniciado");
        cargarEventos();
        
        const eventForm = document.getElementById('eventForm');
        if (eventForm) {
            const nuevoForm = eventForm.cloneNode(true);
            eventForm.parentNode.replaceChild(nuevoForm, eventForm);
            nuevoForm.addEventListener('submit', guardarEvento);
        }
    };

    async function cargarEventos() {
        console.log("Intentando cargar eventos desde:", `${API_BASE_URL}/api/management/events`);
        try {
            const response = await fetch(`${API_BASE_URL}/api/management/events`);
            
            if (!response.ok) {
                console.error("Error del servidor:", response.status);
                return;
            }

            const eventos = await response.json();
            console.log("Eventos recibidos:", eventos);
            renderizarTabla(eventos);

        } catch (error) {
            console.error("Error de red o CORS:", error);
        }
    }

    function renderizarTabla(eventos) {
        const tbody = document.getElementById('eventTableBody');
        if (!tbody) return;
        
        tbody.innerHTML = '';

        if (!eventos || eventos.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center">No se encontraron eventos.</td></tr>';
            return;
        }

        eventos.forEach(ev => {
            const id = ev.id || ev.Id;
            const nombre = (ev.name || ev.Name || "").replace("[ELIMINADO] ", "");
            const esVisible = (ev.sysVisible === true || ev.SysVisible === true);
            
            // IMAGEN: Construcción de ruta segura
            const imgPath = ev.imageUrl || ev.ImageUrl || "";
            let imgSrc = "https://via.placeholder.com/50x35?text=No+Foto";
            
            if (imgPath && imgPath.trim() !== "") {
                const rutaLimpia = imgPath.startsWith('/') ? imgPath.substring(1) : imgPath;
                imgSrc = `${API_BASE_URL}/${rutaLimpia}?t=${new Date().getTime()}`;
            }

            const fila = document.createElement('tr');
            fila.innerHTML = `
                <td><strong>${nombre}</strong></td>
                <td>${new Date(ev.eventDate || ev.EventDate).toLocaleDateString()}</td>
                <td>$${(ev.fee || ev.Fee || 0).toFixed(2)}</td>
                <td>${ev.soldTickets || ev.SoldTickets || 0} / ${ev.capacity || ev.Capacity || 0}</td>
                <td class="text-center">
                    <span style="
                        display: inline-block; 
                        padding: 4px 8px; 
                        border-radius: 4px; 
                        font-weight: bold; 
                        background-color: ${esVisible ? '#d4edda' : '#f8d7da'}; 
                        color: ${esVisible ? '#155724' : '#721c24'};
                        border: 1px solid ${esVisible ? '#c3e6cb' : '#f5c6cb'};
                    ">
                        ${esVisible ? 'Público' : 'Oculto'}
                    </span>
                </td>
                <td class="text-center">
                    <div style="position: relative; display: inline-block;">
                        <img src="${imgSrc}" 
                            style="width: 45px; height: 35px; object-fit: cover; border-radius: 4px; border: 1px solid #ddd;"
                            onerror="this.src='https://via.placeholder.com/45x35?text=Error'">
                        <button type="button" 
                                onclick="window.abrirSelectorImagen('${id}')" 
                                style="position: absolute; bottom: -5px; right: -5px; border: none; background: white; border-radius: 50%; cursor: pointer; box-shadow: 0 1px 3px rgba(0,0,0,0.3); font-size: 10px;">
                            📷
                        </button>
                    </div>
                </td>
                <td>
                    <div style="display: flex; gap: 4px;">
                        <button type="button" class="btn-edit" onclick="window.prepararEdicion('${id}')" title="Editar">✏️</button>
                        <button type="button" class="btn-delete" onclick="window.eliminarEvento('${id}')" title="Eliminar">🗑️</button>
                    </div>
                </td>
            `;
            tbody.appendChild(fila);
        });
    }

    window.clearFilters = function() {
        console.log("🧹 Reseteando filtros y cargando lista completa...");
        
        const term = document.getElementById('searchTerm');
        const vis = document.getElementById('filterVisible');

        if (term) term.value = '';
        if (vis) vis.value = '';
        if (typeof cargarEventos === 'function') {
            cargarEventos();
        } else {
            performSearch();
        }
    };

    document.addEventListener('click', function(e) {
        if (e.target && e.target.textContent.toLowerCase().includes('limpiar')) {
            e.preventDefault();
            window.clearFilters();
        }
    });

    window.actualizarEstado = async (id, valor, tipo) => {
        const url = `${API_BASE_URL}/api/management/update_status`;
        const bodyData = { 
            Id: id, 
            [tipo === 'visible' ? 'SysVisible' : 'SysEnabled']: valor === "true" 
        };

        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(bodyData)
            });

            if (res.ok) {
                const Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, timer: 1000 });
                Toast.fire({ icon: 'success', title: 'Estado actualizado' });
            }
        } catch (e) {
            console.error("Error:", e);
        }
    };

    window.eliminarEvento = async (id) => {
        const result = await Swal.fire({ 
            title: '¿Borrar evento?', 
            text: "Esta acción no se puede deshacer",
            icon: 'warning', 
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Sí, borrar'
        });

        if (!result.isConfirmed) return;

        try {
                const res = await fetch(`${API_BASE_URL}/api/management/delete_event`, { 
                    method: 'DELETE',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Id: id }) 
                });
                
                if (res.ok) {
                    Swal.fire('Eliminado', 'El evento ha sido borrado.', 'success');
                    cargarEventos(); 
                } else {
                    console.error("Error al borrar. Status:", res.status);
                    Swal.fire('Error', 'No se pudo eliminar el evento.', 'error');
                }
            } catch (e) { 
                console.error("Error de red:", e); 
            }
    };

    window.abrirSelectorImagen = function(id) {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';
        
        input.onchange = async () => {
            const file = input.files[0];
            if (!file) return;

            const formData = new FormData();
            formData.append('file', file);

            try {
                const response = await fetch(`${API_BASE_URL}/api/management/upload-image/${id}`, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    Swal.fire('Éxito', 'Imagen actualizada correctamente', 'success');
                    cargarEventos(); 
                } else {
                    console.error("Error en respuesta:", response.status);
                    Swal.fire('Error', 'No se pudo subir la imagen (404)', 'error');
                }
            } catch (error) {
                console.error("Error en fetch:", error);
            }
        };
        
        input.click();
    };

    let isEditing = false;

    async function prepararEdicion(id) {
        try {
            const response = await fetch(`${API_BASE_URL}/api/management/events`);
            const eventos = await response.json();
            const ev = eventos.find(x => (x.id || x.Id) === id);

            if (!ev) throw new Error("No se encontró el evento");

            // 1. Llenamos el formulario
            document.getElementById('editingEventId').value = id;
            document.getElementById('title').value = (ev.name || ev.Name || "").replace("[ELIMINADO] ", "");
            document.getElementById('price').value = ev.fee || ev.Fee;
            document.getElementById('capacity').value = ev.capacity || ev.Capacity;
            document.getElementById('description').value = ev.details || ev.Details || ''; 
            document.getElementById('sysVisible').value = (ev.sysVisible !== undefined ? ev.sysVisible : ev.SysVisible).toString();
            
            if (ev.eventDate || ev.EventDate) {
                const fecha = ev.eventDate || ev.EventDate;
                document.getElementById('eventDate').value = fecha.substring(0, 16);
            }

            // 2. Cambiamos el botón
            const btnSave = document.querySelector('.btn-save-main'); 
            const btnActual = document.querySelector('.btn-save'); 
            
            btnActual.textContent = "Actualizar Cambios";
            btnActual.classList.add('mode-edit'); 
            
            isEditing = true;

            const formSection = document.querySelector('.form-section');
            if (formSection) formSection.scrollTo({ top: 0, behavior: 'smooth' });

        } catch (error) {
            console.error("Error en preparación:", error);
            Swal.fire("Error", "No se pudieron cargar los datos", "error");
        }
    }

    window.prepararEdicion = async (id) => {
        // Llama a la función prepararEdicion
        await prepararEdicion(id); 
    };

    async function guardarEvento(e) {
        e.preventDefault();

        const inputFile = document.getElementById('eventImageInput');
        const editingId = document.getElementById('editingEventId').value;
        const btnSubmit = document.querySelector('.btn-save');

        const dto = {
            id: isEditing ? editingId : "00000000-0000-0000-0000-000000000000", 
            name: document.getElementById('title').value,
            details: document.getElementById('description').value,
            eventDate: document.getElementById('eventDate').value,
            fee: parseFloat(document.getElementById('price').value) || 0,
            capacity: parseInt(document.getElementById('capacity').value) || 0,
            sysVisible: document.getElementById('sysVisible').value === 'true'
        };

        const url = isEditing 
            ? `${API_BASE_URL}/api/management/update_event` 
            : `${API_BASE_URL}/api/management/new_event`;
        
        const method = isEditing ? 'PUT' : 'POST';

        try {
            btnSubmit.disabled = true;
            btnSubmit.textContent = "Procesando...";

            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });

            if (response.ok) {
                let finalId = editingId;

                if (!isEditing) {
                    await new Promise(r => setTimeout(r, 500));
                    
                    const resList = await fetch(`${API_BASE_URL}/api/management/events`);
                    const eventos = await resList.json();
                    const creado = eventos.find(x => (x.name || x.Name) === dto.name);
                    if (creado) finalId = creado.id || creado.Id;
                }

                if (finalId && inputFile.files && inputFile.files.length > 0) {
                    const formData = new FormData();
                    formData.append('file', inputFile.files[0]);

                    await fetch(`${API_BASE_URL}/api/management/upload-image/${finalId}`, {
                        method: 'POST',
                        body: formData
                    });
                }

                Swal.fire({
                    icon: 'success',
                    title: isEditing ? '¡Actualizado!' : '¡Creado!',
                    text: 'El evento se guardó correctamente.',
                    timer: 2000
                });

                isEditing = false;
                document.getElementById('editingEventId').value = "";
                document.getElementById('eventForm').reset();
                
                btnSubmit.textContent = "Guardar y Publicar Evento";
                btnSubmit.classList.remove('mode-edit');
                btnSubmit.disabled = false;

                cargarEventos(); 

            } else {
                const errorText = await response.text();
                throw new Error(errorText || "Error en el servidor");
            }

        } catch (error) {
            console.error("Error en guardarEvento:", error);
            Swal.fire('Error', 'No se pudo guardar: ' + error.message, 'error');
            btnSubmit.disabled = false;
            btnSubmit.textContent = isEditing ? "Actualizar Cambios" : "Guardar y Publicar Evento";
        }
    }

    let debounceTimer;

    document.addEventListener('DOMContentLoaded', () => {
        console.log("Panel administrativo cargado");
        
        performSearch();

        const searchInput = document.getElementById('searchTerm');
        if (searchInput) {
            searchInput.addEventListener('input', () => {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    console.log("Buscando término:", searchInput.value);
                    performSearch();
                }, 500);
            });
        }

        const filterSelect = document.getElementById('filterVisible');
        if (filterSelect) {
            filterSelect.addEventListener('change', () => {
                console.log("Filtrando por visibilidad");
                performSearch();
            });
        }
    });


    async function performSearch() {
        console.log("🔍 Iniciando búsqueda...");

        const termInput = document.getElementById('searchTerm');
        const visibleInput = document.getElementById('filterVisible');
        const tableBody = document.getElementById('eventTableBody');

        const termValue = termInput ? termInput.value.trim() : "";
        const visibleValue = visibleInput ? visibleInput.value : "";

        const filters = {
            term: termValue === "" ? null : termValue,
            isVisible: visibleValue === "" ? null : (visibleValue === "true")
        };

        try {
            const response = await fetch(`${API_BASE_URL}/api/Management/search`, {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json',
                    'Accept': 'application/json' 
                },
                body: JSON.stringify(filters)
            });

            if (!response.ok) {
                throw new Error(`Error servidor: ${response.status}`);
            }

            const datosRecibidos = await response.json();
            
            console.log("¿Qué campos trae el primer evento?");
            console.dir(datosRecibidos[0]); 

            if (datosRecibidos && datosRecibidos.length > 0) {
                renderizarTabla(datosRecibidos); 
            } else {
                tableBody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">No se encontraron resultados.</td></tr>`;
            }

        } catch (error) {
            console.error("Error en búsqueda:", error);
            if (tableBody) {
                tableBody.innerHTML = `<tr><td colspan="7" class="text-center text-warning">Error en la petición: ${error.message}</td></tr>`;
            }
        }
    }

    function getVisibilityValue() {
        const val = document.getElementById('filterVisible').value;
        if (val === "true") return true;
        if (val === "false") return false;
        return null;
    }
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', inicializar);
        else inicializar();
    }