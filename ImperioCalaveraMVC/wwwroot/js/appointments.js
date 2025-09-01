
document.addEventListener('DOMContentLoaded', function () {
    // Validar que la librería exista
    if (typeof FullCalendar === 'undefined') {
        console.error('FullCalendar no está cargado. Verifica el orden de los <script>.');
        return;
    }

    const calendarEl = document.getElementById('calendar');
    if (!calendarEl) {
        console.error("No se encontró el elemento #calendar. Asegúrate de tener <div id='calendar'></div> en la vista.");
        return;
    }

    // Configuración básica 
    const calendar = new FullCalendar.Calendar(calendarEl, {
        
        themeSystem: 'bootstrap5',
        initialView: 'dayGridMonth',
        locale: 'es',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth'
        },
        selectable: true,
        // Cargar eventos desde el backend
        events: '/Appointments/GetAppointments', // endpoint que tú creas
        dateClick: function (info) {
            // info.dateStr: "YYYY-MM-DD"
            console.log('Fecha clickeada:', info.dateStr);
            // Llama a tu función para mostrar/llenar los horarios
            generateTimeSlots(info.dateStr);
        }
    });

    calendar.render();

    // Generar slots de ejemplo (9:00 - 21:00 cada 45 min)
    function generateTimeSlots(dateStr) {
        const container = document.getElementById('time-slots-container');
        if (!container) return;
        container.innerHTML = '';

        const start = new Date(`${dateStr}T09:00:00`);
        const end = new Date(`${dateStr}T21:00:00`);
        let cur = new Date(start);

        while (cur < end) {
            const hh = String(cur.getHours()).padStart(2, '0');
            const mm = String(cur.getMinutes()).padStart(2, '0');
            const label = `${hh}:${mm}`;
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn btn-outline-primary m-1';
            btn.textContent = label;

            // ejemplo: onClick selecciona el slot
            btn.addEventListener('click', () => {
                document.querySelectorAll('#time-slots-container .btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                // guarda selección en inputs ocultos o en un estado
            });

            container.appendChild(btn);

            cur.setMinutes(cur.getMinutes() + 45);
        }
    }
});
