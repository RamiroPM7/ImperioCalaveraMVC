// wwwroot/js/profile.js

let _verifiedPassword = null; // Variable JavaScript para almacenar la contraseña verificada

document.addEventListener('DOMContentLoaded', function () {
    const editProfileBtn = document.getElementById('editProfileBtn');
    const saveProfileBtn = document.getElementById('saveProfileBtn');
    const cancelEditBtn = document.getElementById('cancelEditBtn');
    const profileForm = document.getElementById('profileForm');
    const passwordConfirmationModal = new bootstrap.Modal(document.getElementById('passwordConfirmationModal'));
    const currentPasswordInput = document.getElementById('currentPasswordInput');
    const confirmPasswordBtn = document.getElementById('confirmPasswordBtn');
    const passwordError = document.getElementById('passwordError');
    const passwordChangeSection = document.getElementById('passwordChangeSection');

    // Elementos de visualización
    const displayNombre = document.getElementById('displayNombre');
    const displayEmail = document.getElementById('displayEmail');
    const displayPhoneNumber = document.getElementById('displayPhoneNumber');
    const displayTelefonoEmergencia = document.getElementById('displayTelefonoEmergencia');

    // Elementos de edición
    const editNombre = document.getElementById('editNombre');
    const editEmail = document.getElementById('editEmail');
    const editPhoneNumber = document.getElementById('editPhoneNumber');
    const editTelefonoEmergencia = document.getElementById('editTelefonoEmergencia');
    const newPasswordInput = document.getElementById('NewPassword'); // Referencia al campo de nueva contraseña

    let isEditing = false; // Estado para controlar si estamos en modo edición

    // Función para alternar entre modo de visualización y edición
    function toggleEditMode(enable) {
        isEditing = enable;

        displayNombre.classList.toggle('d-none', enable);
        displayEmail.classList.toggle('d-none', enable);
        displayPhoneNumber.classList.toggle('d-none', enable);
        displayTelefonoEmergencia.classList.toggle('d-none', enable);

        editNombre.classList.toggle('d-none', !enable);
        editEmail.classList.toggle('d-none', !enable);
        editPhoneNumber.classList.toggle('d-none', !enable);
        editTelefonoEmergencia.classList.toggle('d-none', !enable);

        if (enable) {
            // Copiar los valores actuales a los campos de edición
            editNombre.value = displayNombre.textContent.trim();
            editEmail.value = displayEmail.textContent.trim();
            editPhoneNumber.value = displayPhoneNumber.textContent.trim();
            editTelefonoEmergencia.value = displayTelefonoEmergencia.textContent.trim();
            passwordChangeSection.classList.remove('d-none'); // Mostrar sección de cambio de contraseña
        } else {
            passwordChangeSection.classList.add('d-none'); // Ocultar sección de cambio de contraseña
        }

        editProfileBtn.classList.toggle('d-none', enable);
        saveProfileBtn.classList.toggle('d-none', !enable);
        cancelEditBtn.classList.toggle('d-none', !enable);
    }

    // Evento al hacer clic en "Editar Perfil"
    editProfileBtn.addEventListener('click', function () {
        passwordConfirmationModal.show(); // Mostrar el modal de confirmación de contraseña
        currentPasswordInput.value = ''; // Limpiar el campo de contraseña del modal
        passwordError.classList.add('d-none'); // Ocultar cualquier mensaje de error previo
    });

    // Evento al hacer clic en "Confirmar" en el modal de contraseña
    confirmPasswordBtn.addEventListener('click', async function () {
        const password = currentPasswordInput.value;
        if (!password) {
            passwordError.textContent = 'Por favor, introduce tu contraseña.';
            passwordError.classList.remove('d-none');
            return;
        }

        // Obtener el token de antiforja para la solicitud AJAX
        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

        try {
            // Realizar la solicitud para verificar la contraseña
            const response = await fetch('/Profile/VerifyPassword', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: JSON.stringify({ password: password })
            });

            const data = await response.json();

            if (data.success) {
                passwordConfirmationModal.hide(); // Ocultar el modal
                toggleEditMode(true); // Entrar en modo edición
                _verifiedPassword = password; // ¡IMPORTANTE! Almacenar la contraseña en la variable JS
            } else {
                passwordError.textContent = data.message || 'Contraseña incorrecta.';
                passwordError.classList.remove('d-none');
            }
        } catch (error) {
            console.error('Error al verificar contraseña:', error);
            passwordError.textContent = 'Hubo un error al verificar la contraseña. Inténtalo de nuevo.';
            passwordError.classList.remove('d-none');
        }
    });

    // Evento al hacer clic en "Cancelar"
    cancelEditBtn.addEventListener('click', function () {
        toggleEditMode(false); // Salir del modo edición
        // Restaurar los valores originales en los campos de edición
        editNombre.value = displayNombre.textContent.trim();
        editEmail.value = displayEmail.textContent.trim();
        editPhoneNumber.value = displayPhoneNumber.textContent.trim();
        editTelefonoEmergencia.value = displayTelefonoEmergencia.textContent.trim();
        // Limpiar los campos de nueva contraseña
        document.getElementById('NewPassword').value = '';
        document.getElementById('ConfirmNewPassword').value = '';
        profileForm.reset(); // Restablecer el formulario (esto también limpia los campos)
        // Limpiar los mensajes de validación de jQuery
        const validationSpans = profileForm.querySelectorAll('.text-danger');
        validationSpans.forEach(span => span.textContent = '');

        _verifiedPassword = null; // ¡IMPORTANTE! Limpiar la variable al cancelar
    });

    // Evento al cerrar el modal de contraseña (por clic fuera o botón 'x')
    document.getElementById('passwordConfirmationModal').addEventListener('hidden.bs.modal', function () {
        currentPasswordInput.value = ''; // Limpiar el campo de contraseña del modal
        passwordError.classList.add('d-none'); // Ocultar cualquier mensaje de error
    });

    // Evento antes de enviar el formulario principal
    profileForm.addEventListener('submit', function (event) {
        // Solo si se va a cambiar la contraseña, inyectamos la contraseña actual
        // Comprobamos si el campo de nueva contraseña tiene un valor Y si tenemos una contraseña verificada
        if (newPasswordInput.value && _verifiedPassword) {
            // Creamos un campo oculto temporalmente
            const hiddenInput = document.createElement('input');
            hiddenInput.type = 'hidden';
            hiddenInput.name = 'CurrentPassword'; // Debe coincidir con el nombre de la propiedad en el ViewModel
            hiddenInput.value = _verifiedPassword;
            profileForm.appendChild(hiddenInput); // Añadir el campo oculto al formulario

            // Limpiamos la variable JS inmediatamente después de usarla
            _verifiedPassword = null;

            // Opcional: Eliminar el campo oculto después de un corto tiempo (la solicitud ya se envió)
            // Esto es más por si se inspecciona el DOM justo después del envío.
            setTimeout(() => {
                if (hiddenInput.parentNode) {
                    hiddenInput.parentNode.removeChild(hiddenInput);
                }
            }, 100);
        }
    });

    // Opcional: Ocultar el mensaje de éxito de TempData si el usuario interactúa con la página
    const successAlert = document.querySelector('.alert-success[role="alert"]');
    if (successAlert) {
        setTimeout(() => {
            successAlert.classList.remove('show');
            successAlert.classList.add('fade');
            setTimeout(() => successAlert.remove(), 150);
        }, 7000); // Ocultar después de 7 segundos
    }
});
