import { Calendar } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import bootstrap5Plugin from '@fullcalendar/bootstrap5';
import interactionPlugin from '@fullcalendar/interaction';
import esLocale from '@fullcalendar/core/locales/es';

document.addEventListener('DOMContentLoaded', () => {
    // State variables
    let selectedDayElement = null;
    let selectedDateStr = '';
    let selectedTime = '';

    // DOM element references
    const calendarEl = document.getElementById('calendar');
    const timeSlotsContainer = document.getElementById('time-slots-container');
    const bookingButton = document.getElementById('agendar-cita-btn');

    if (!calendarEl) return;

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

    // Final booking button click (now simpler)
    bookingButton.addEventListener('click', async () => {

        if (!selectedDateStr || !selectedTime) {
            alert('Por favor, selecciona una fecha y una hora.');
            return;
        }

        const bookingData = {
            date: selectedDateStr,
            time: selectedTime
        };

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


    
    // --- Event Listener for Canceling Appointments (using Event Delegation) ---
    document.addEventListener('click', async (event) => {
        // Check if the clicked element is one of our cancel buttons
        if (event.target.classList.contains('btn-cancel-appointment')) {

            // 1. Get the Appointment ID from the button's data attribute
            const citaId = event.target.dataset.citaId;

            // 2. Ask the user for confirmation
            if (!confirm('¿Estás seguro de que quieres cancelar esta cita? Esta acción no se puede deshacer.')) {
                return; // Stop if the user clicks "Cancel"
            }

            // 3. Send the request to the backend API
            try {
                const response = await fetch(`/Appointments/CancelAppointment/${citaId}`, {
                    method: 'DELETE',
                    // We don't need headers or body, the ID is in the URL
                });

                const result = await response.json();

                if (response.ok && result.success) {
                    // Success!
                    alert(result.message);
                    // Reload the page to show the updated list (the simplest way)
                    window.location.reload();

                    // --- Optional: A slicker way (instead of reload) ---
                     /*const card = document.getElementById(`cita-card-${citaId}`);
                     card.querySelector('.badge').textContent = 'Cancelada';
                     card.querySelector('.badge').classList.replace('bg-primary', 'bg-danger');
                     event.target.remove(); // Remove the cancel button*/

                } else {
                    // Show error message from the server
                    alert('Error al cancelar: ' + (result.message || 'Error desconocido.'));
                }

            } catch (error) {
                console.error('Error canceling appointment:', error);
                alert('Hubo un error de conexión al intentar cancelar la cita.');
            }
        }
    });

});