import { Calendar } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import bootstrap5Plugin from '@fullcalendar/bootstrap5';
import interactionPlugin from '@fullcalendar/interaction';
import esLocale from '@fullcalendar/core/locales/es';

document.addEventListener('DOMContentLoaded', () => {



    //Obtenemos las referencias a los botones y contenedores
    const tableBody = document.getElementById('appointments-table-body');
    const btnViewCards = document.getElementById('btn-view-cards');

    // State variables
    let selectedDayElement = null;
    let selectedDateStr = '';
    let selectedTime = '';

    // DOM element references
    const calendarEl = document.getElementById('calendar');
    const timeSlotsContainer = document.getElementById('time-slots-container');
    const bookingButton = document.getElementById('agendar-cita-btn');

    if (!calendarEl) return;

   
    //Establish the connection to the hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/appointmentHub") // This must match the endpoint in Program.cs
        .build();

    //Define what happens when a "SlotBooked" message comes from the server
    connection.on("SlotBooked", (date, time) => {
        // Check if the update is for the day the user is currently viewing
        if (selectedDateStr === date) {
            const buttonToRemove = document.querySelector(`.time-slot-btn[data-time="${time}"]`);
            if (buttonToRemove) {
                buttonToRemove.remove();
            }
        }
    });


    // --- Helper para obtener la clase del badge de estado ---
    function getBadgeClass(estado) {
        if (estado === 'Iniciada') return 'bg-success';
        if (estado === 'Pendiente') return 'bg-primary';
        return 'bg-danger'; // Para Cancelada o Finalizada
    }

    function getActionButtons(cita) {
        if (cita.estado === 'Pendiente') {
            return `
                <button class="btn btn-sm btn-success">Iniciar Cita</button>
                <button class="btn btn-sm btn-outline-danger btn-cancel-appointment" data-cita-id="${cita.citaId}">Cancelar</button>
            `;
        }
        if (cita.estado === 'Iniciada') {
            return `<button class="btn btn-sm btn-primary">Finalizar Cita</button>`;
        }
        return ''; // No hay botones si está finalizada o cancelada
    }

    function getClientActionButton(cita) {
        if (cita.estado === 'Pendiente') {
            return `<button class="btn btn-sm btn-outline-danger btn-cancel-appointment" data-cita-id="${cita.citaId}">Cancelar Cita</button>`;
        }
        return '';
    }

    //Función para rellenar la tabla con los datos de las citas
    function populateTable(appointments) {
        if (!tableBody) return;
        tableBody.innerHTML = ''; // Limpiar la tabla

        if (appointments.length === 0) {
            const colCount = (window.currentUserRole === 'Barbero') ? 5 : 6;
            tableBody.innerHTML = `<tr><td colspan="${colCount}" class="text-center text-muted">No tienes citas agendadas.</td></tr>`;
            return;
        }

        appointments.forEach(cita => {
            const fecha = new Date(cita.fechaHora);
            const fechaStr = fecha.toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' });
            const horaStr = fecha.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });

            const badgeClass = getBadgeClass(cita.estado);
            const actionButtons = (window.currentUserRole === 'Admin' || window.currentUserRole === 'Barbero')
                ? getActionButtons(cita)
                : getClientActionButton(cita);

            let rowHtml = `
                <tr>
                    <td>${fechaStr}</td>
                    <td>${horaStr}</td>
            `;

            if (window.currentUserRole === 'Admin' || window.currentUserRole === 'Barbero') {
                const clientName = cita.walkInCustomerName ? `${cita.walkInCustomerName} (Walk-In)` : cita.clienteName;
                rowHtml += `<td>${clientName}</td>`;
            }

            if (window.currentUserRole !== 'Barbero') {
                rowHtml += `<td>${cita.barberoName}</td>`;
            }

            rowHtml += `
                    <td><span class="badge ${badgeClass}">${cita.estado}</span></td>
                    <td><div class="d-flex gap-2">${actionButtons}</div></td>
                </tr>
            `;

            tableBody.insertAdjacentHTML('beforeend', rowHtml);
        });
    }


    //Función para establecer la vista activa (tarjetas o tabla)
    function setView(viewToShow) {
        // Volvemos a encontrar los elementos CADA VEZ que se llama a la función.
        // Esto asegura que siempre trabajamos con los elementos que están en la página.
        const cardsContainer = document.getElementById('cards-container');
        const tableContainer = document.getElementById('table-container');
        const btnViewCards = document.getElementById('btn-view-cards');
        const btnViewTable = document.getElementById('btn-view-table');

        // Si los elementos no existen (por si acaso), no hacemos nada.
        if (!cardsContainer || !tableContainer || !btnViewCards || !btnViewTable) return;

        // El resto de la lógica es la misma que ya tenías
        if (viewToShow === 'table') {
            cardsContainer.style.display = 'none';
            tableContainer.style.display = 'block';
            btnViewTable.classList.replace('btn-outline-primary', 'btn-primary');
            btnViewCards.classList.replace('btn-primary', 'btn-outline-primary');
        } else {
            cardsContainer.style.display = 'block';
            tableContainer.style.display = 'none';
            btnViewCards.classList.replace('btn-outline-primary', 'btn-primary');
            btnViewTable.classList.replace('btn-primary', 'btn-outline-primary');
        }
        localStorage.setItem('appointmentView', viewToShow);
    }


    connection.on("AddNewAppointmentCard", (cita) => {
       
        const appointmentsContainer = document.getElementById('my-appointments-container');
        const noAppointmentsMessage = appointmentsContainer.querySelector('.alert');
        if (noAppointmentsMessage) {
            noAppointmentsMessage.remove();
        }

        // --- Helper function to format date/time ---
        const fecha = new Date(cita.fechaHora);
        const dateInfo = {
            month: fecha.toLocaleString('es-MX', { month: 'short' }),
            day: fecha.toLocaleString('es-MX', { day: '2-digit' }),
            year: fecha.getFullYear(),
            time: fecha.toLocaleString('es-MX', { hour: 'numeric', minute: '2-digit', hour12: true })
        };


        //helpers

        let cardHtml = '';

        // --- DECIDE WHICH CARD TO BUILD BASED ON THE ROLE ---
        if (window.currentUserRole === 'Admin' || window.currentUserRole === 'Barbero') {
            // === BUILD THE CARD FOR ADMINS AND BARBERS ===
            cardHtml = `
            <div class="card shadow-sm mb-3" id="cita-card-${cita.citaId}" style="max-width: 540px; margin: auto;">
                <div class="row g-0">
                    <div class="col-md-4 d-flex justify-content-center align-items-center bg-secondary text-white p-3 rounded-start">
                        <div class="text-center">
                            <h5 class="card-title mb-0 text-capitalize">${dateInfo.month}</h5>
                            <h1 class="display-4 mb-0" style="font-weight: 600;">${dateInfo.day}</h1>
                            <h6 class="card-title mb-0">${dateInfo.time}</h6>
                        </div>
                    </div>
                    <div class="col-md-8">
                        <div class="card-body">
                            <h5 class="card-title">Detalles de la Cita</h5>
                            <hr class="my-2">
                            <h6 class="card-subtitle mb-2 text-muted">Cliente</h6>
                            ${cita.walkInCustomerName
                    ? `<p class="card-text mb-1"><strong>Nombre:</strong> ${cita.walkInCustomerName} (Walk-In)</p>
                                   <p class="card-text mb-1"><strong>Teléfono:</strong> ${cita.walkInCustomerPhone}</p>`
                    : `<p class="card-text mb-1"><strong>Nombre:</strong> ${cita.clienteName}</p>
                                   <p class="card-text mb-1"><strong>Teléfono:</strong> ${cita.clientePhone}</p>
                                   ${cita.clienteTelEmergencia ? `<p class="card-text mb-1"><strong>Tel. Emergencia:</strong> ${cita.clienteTelEmergencia}</p>` : ''}`
                }
                            ${cita.walkInCustomerNotes ? `<p class="card-text mb-1"><strong>Notas:</strong> ${cita.walkInCustomerNotes}</p>` : ''}
                            <hr class="my-2">
                            ${window.currentUserRole === 'Admin'
                    ? `<h6 class="card-subtitle mb-2 text-muted">Barbero</h6>
                                   <p class="card-text mb-1"><strong>Nombre:</strong> ${cita.barberoName}</p>
                                   <hr class="my-2">`
                    : ''
                }
                            <p class="card-text mb-2">
                                <strong>Estado:</strong> <span class="badge ${getBadgeClass(cita.estado)}">${cita.estado}</span>
                            </p>
                            <div class="d-flex gap-2">
                                ${getActionButtons(cita)}
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;
        } else {
            // === BUILD THE CARD FOR REGULAR CLIENTS ===
            cardHtml = `
            <div class="card shadow-sm mb-3" id="cita-card-${cita.citaId}" style="max-width: 540px; margin: auto;">
                <div class="row g-0">
                    <div class="col-md-4 d-flex justify-content-center align-items-center bg-dark text-white p-3 rounded-start">
                        <div class="text-center">
                            <h5 class="card-title mb-0 text-capitalize">${dateInfo.month}</h5>
                            <h1 class="display-4 mb-0" style="font-weight: 600;">${dateInfo.day}</h1>
                            <h6 class="card-title mb-0">${dateInfo.year}</h6>
                        </div>
                    </div>
                    <div class="col-md-8">
                        <div class="card-body">
                            <h5 class="card-title">Tu Cita Agendada</h5>
                            <p class="card-text mb-1"><strong>Barbero:</strong> ${cita.barberoName}</p>
                            <p class="card-text mb-1"><strong>Hora:</strong> ${dateInfo.time}</p>
                            <hr class="my-2">
                            <h6 class="card-subtitle mb-2 text-muted">Detalles del Cliente</h6>
                            <p class="card-text mb-1"><strong>Nombre:</strong> ${cita.clienteName}</p>
                            <hr class="my-2">
                            <p class="card-text mb-2">
                                <strong>Estado:</strong> <span class="badge ${getBadgeClass(cita.estado)}">${cita.estado}</span>
                            </p>
                            ${cita.estado === 'Pendiente'
                    ? `<button class="btn btn-sm btn-outline-danger btn-cancel-appointment" data-cita-id="${cita.citaId}">
                                       Cancelar Cita
                                   </button>`
                    : ''
                }
                        </div>
                    </div>
                </div>
            </div>`;

        }

        // Add the newly created card to the top of the list
        appointmentsContainer.insertAdjacentHTML('afterbegin', cardHtml);
    });

    // === ESTE ES EL RECEPTOR COMPLETO Y CORREGIDO ===
    connection.on("ReceiveStatusUpdate", (citaId, newStatus) => {

        const card = document.getElementById(`cita-card-${citaId}`);
        if (!card) return;

        // 1. Actualiza la Insignia (Badge) - Texto Y COLOR
        const badge = card.querySelector('.badge');

        if (badge) {
            badge.textContent = newStatus;
            // --- LÍNEA CORREGIDA ---
            // Se usan comillas invertidas (`) para crear el string correctamente
            badge.className = `badge ${getBadgeClass(newStatus)}`;
        }

        // 2. Actualiza los Botones de Acción
        const actionsContainer = card.querySelector('.d-flex.gap-2'); // Para Staff
        const clientButtonContainer = card.querySelector('.card-body'); // Para Cliente

        if (window.currentUserRole === 'Admin' || window.currentUserRole === 'Barbero') {
            if (actionsContainer) {
                // Borra los botones viejos y reconstruye los nuevos
                actionsContainer.innerHTML = getActionButtons({ estado: newStatus, citaId: citaId });
            }
        } else {
            const oldButton = clientButtonContainer.querySelector('.btn-cancel-appointment');
            if (oldButton) oldButton.remove(); // Elimina el botón 
        }
    });


    //Start the connection
    connection.start().then(() => {
        console.log("SignalR Connected.");
    }).catch(err => console.error("SignalR Connection Error: ", err.toString()));
    

    // Function to fetch and display time slots
    async function fetchAndDisplaySlots(dateStr) {
        selectedDateStr = dateStr;
        timeSlotsContainer.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div>';

        try {
            const response = await fetch(`/Appointments/GetAvailableSlots?date=${dateStr}`);
            const slots = await response.json();

            timeSlotsContainer.innerHTML = '';
            if (slots.length === 0) {
                timeSlotsContainer.innerHTML = '<p class="text-muted">No hay horas disponibles para este día.</p>';
            } else {
                slots.forEach(slot => {
                    const button = document.createElement('button');
                    button.classList.add('btn', 'btn-outline-primary', 'time-slot-btn');
                    button.textContent = slot;
                    button.dataset.time = slot;
                    timeSlotsContainer.appendChild(button);
                });
            }
        } catch (error) {
            console.error('Error fetching time slots:', error);
            timeSlotsContainer.innerHTML = '<p class="text-danger">No se pudieron cargar las horas.</p>';
        }
    }

    // FullCalendar Initialization
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const calendar = new Calendar(calendarEl, {
        themeSystem: 'bootstrap5',
        plugins: [dayGridPlugin, bootstrap5Plugin, interactionPlugin],
        initialView: 'dayGridMonth',
        locale: esLocale,
        headerToolbar: { left: 'prev,next today', center: 'title', right: '' },
        validRange: { start: today },
        dayCellClassNames: (arg) => (arg.date.getDay() === 0 ? ['fc-day-disabled'] : []),
        dateClick: (info) => {
            if (info.dayEl.classList.contains('fc-day-disabled')) return;

            if (selectedDayElement) {
                selectedDayElement.classList.remove('selected-day');
            }
            info.dayEl.classList.add('selected-day');
            selectedDayElement = info.dayEl;

            fetchAndDisplaySlots(info.dateStr);
        }
    });

    calendar.render();

    // Event Listeners
    timeSlotsContainer.addEventListener('click', (event) => {
        if (event.target.classList.contains('time-slot-btn')) {
            const currentlyActive = timeSlotsContainer.querySelector('.btn.btn-primary');
            if (currentlyActive) {
                currentlyActive.classList.replace('btn-primary', 'btn-outline-primary');
            }
            event.target.classList.replace('btn-outline-primary', 'btn-primary');
            selectedTime = event.target.dataset.time;
        }
    });


    // Este bloque ahora se encarga de la inicialización Y de los botones estáticos.
    if (btnViewCards) {
        const btnViewTable = document.getElementById('btn-view-table');

        // Asignamos la funcionalidad directamente a los botones
        btnViewCards.addEventListener('click', () => setView('cards'));
        btnViewTable.addEventListener('click', () => setView('table'));

        // Rellena la tabla con los datos iniciales
        populateTable(window.appointmentsData);

        // Comprueba si hay una vista guardada y la aplica
        const savedView = localStorage.getItem('appointmentView');
        if (savedView) {
            setView(savedView);
        }
    }


    // Final booking button click (now simpler)
    bookingButton.addEventListener('click', async () => {

        if (!selectedDateStr || !selectedTime) {
            alert('Por favor, selecciona una fecha y una hora.');
            return;
        }

        const bookingData = {
            Date: selectedDateStr,
            Time: selectedTime
        };

        // ADD THIS LINE TO DEBUG
        console.log("Sending booking data:", bookingData); 

        const walkInNameInput = document.getElementById('walk-in-customer-name');
        const walkInPhoneInput = document.getElementById('walk-in-customer-phone'); // <-- Get the phone input
        const walkInNotesInput = document.getElementById('walk-in-customer-notes');

        if (walkInNameInput && !walkInNameInput.value.trim()) {
            alert('Por favor, ingresa el nombre del cliente para registrar la cita.');
            return; // Stop the function here
        }

        if (walkInPhoneInput && !walkInPhoneInput.value.trim()) {
            alert('Por favor, ingresa el número de tel. para registrar la cita.');
            return; // Stop the function here
        }



        if (walkInNameInput && walkInPhoneInput) {
            bookingData.walkInCustomerName = walkInNameInput.value;
            bookingData.walkInCustomerPhone = walkInPhoneInput.value; // <-- Add the phone value
            bookingData.walkInCustomerNotes = walkInNotesInput.value;
        }

        try {
            const response = await fetch('/Appointments/BookAppointment', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(bookingData)
            });

            const result = await response.json();

            if (response.ok && result.success) {
                alert(result.message);
                window.location.reload();
            } else {
                const errorMessage = result.message || (await response.text());
                alert('Hubo un error al agendar la cita: ' + errorMessage);
            }
        } catch (error) {
            console.error('Error booking appointment:', error);
            alert('Hubo un error de conexión al agendar la cita.');
        }
    });


    // AÑADE ESTE BLOQUE PARA LA INICIALIZACIÓN DE LA VISTA
    // Este código se ejecuta solo una vez, cuando la página carga.
    const myAppointmentsContainer = document.getElementById('my-appointments-container');
    if (myAppointmentsContainer) {
        // Rellena la tabla con los datos iniciales que vienen del servidor
        populateTable(window.appointmentsData);

        // Comprueba si hay una vista guardada y aplícala
        const savedView = localStorage.getItem('appointmentView');
        if (savedView) {
            setView(savedView);
        }
    }


    // 2. AÑADE (O UNIFICA) ESTE "SUPER-ESCUCHA" PARA TODOS LOS CLICS
    document.addEventListener('click', async (event) => {

        // --- Lógica para la paginación con AJAX ---
        const paginationLink = event.target.closest('.pagination a');
        if (paginationLink) {
            // 1. Previene la recarga completa de la página
            event.preventDefault();

            const url = paginationLink.getAttribute('href');
            if (!url) return;

            // El contenedor principal que vamos a actualizar
            const container = document.getElementById('my-appointments-container');

            // 2. Muestra un indicador de carga para que el usuario sepa que algo está pasando
            container.innerHTML = '<div class="text-center p-5"><div class="spinner-border" role="status"><span class="visually-hidden">Cargando...</span></div></div>';

            // 3. Usa fetch para pedir solo el nuevo contenido al servidor
            fetch(url)
                .then(response => {
                    if (!response.ok) {
                        // Maneja errores si el servidor responde con un error (ej. 404, 500)
                        throw new Error(`Error en la red: ${response.statusText}`);
                    }
                    return response.text();
                })
                .then(html => {
                    // Reemplaza el contenido viejo con el nuevo HTML que recibimos
                    container.innerHTML = html;

                    // Actualiza la URL en la barra del navegador sin recargar la página
                    window.history.pushState({}, '', url);

                    populateTable(window.appointmentsData); 

                    // Vuelve a aplicar la vista de tabla/tarjetas que el usuario tenía guardada
                    const savedView = localStorage.getItem('appointmentView');
                    if (savedView) {
                        // La función setView se encargará de mostrar la vista correcta
                        // y de ajustar los botones, ya que el HTML es nuevo.
                        setView(savedView);
                    }
                })
                .catch(error => {
                    console.error('Error al cargar la nueva página:', error);
                    container.innerHTML = '<div class="alert alert-danger">Error al cargar la página de citas.</div>';
                });
        }

        // --- Lógica para cancelar una cita ---
        const cancelButton = event.target.closest('.btn-cancel-appointment');
        if (cancelButton) {
            // 1. Prevenimos cualquier otro comportamiento por defecto del botón
            event.preventDefault();

            // 2. Obtenemos el ID de la cita desde el atributo data del botón
            const citaId = cancelButton.dataset.citaId;
            if (!citaId) return;

            // 3. Pedimos confirmación al usuario antes de proceder
            if (!confirm('¿Estás seguro de que quieres cancelar esta cita?')) {
                return;
            }

            // 4. Usamos fetch para enviar la solicitud de cancelación al servidor
            try {
                const response = await fetch(`/Appointments/CancelAppointment/${citaId}`, {
                    method: 'DELETE',
                });

                if (response.ok) {
                    // Éxito. La solicitud se procesó correctamente.
                    // No necesitamos hacer nada para actualizar la vista aquí.
                    // SignalR transmitirá el cambio y nuestro receptor "ReceiveStatusUpdate"
                    // se encargará de actualizar la tarjeta para todos, incluyéndonos.
                    const result = await response.json();
                    console.log(result.message); // Opcional: muestra el mensaje de éxito en la consola
                } else {
                    // Maneja errores si el servidor responde con un error (ej. 403, 404)
                    alert(`Error al cancelar la cita: ${response.status} ${response.statusText}`);
                }
            } catch (error) {
                console.error('Error de red al cancelar la cita:', error);
                alert('Hubo un error de conexión al intentar cancelar la cita.');
            }
        }




    });


});