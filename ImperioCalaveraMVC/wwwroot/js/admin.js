// wwwroot/js/admin.js

document.addEventListener('DOMContentLoaded', function () {
    // --- Referencias a elementos del DOM ---
    // Modal y formulario principal
    const editUserModal = new bootstrap.Modal(document.getElementById('editUserModal'));
    const editUserForm = document.getElementById('editUserForm');

    // Campos del formulario de edición
    const editUserId = document.getElementById('editUserId');
    const editUserNombre = document.getElementById('editUserNombre');
    const editUserEmail = document.getElementById('editUserEmail');
    const editUserPhoneNumber = document.getElementById('editUserPhoneNumber');
    const editUserTelefonoEmergencia = document.getElementById('editUserTelefonoEmergencia');
    const editUserNewPassword = document.getElementById('editUserNewPassword');
    const editUserConfirmNewPassword = document.getElementById('editUserConfirmNewPassword');
    const editUserLockoutStartDate = document.getElementById('editUserLockoutStartDate');
    const editUserLockoutEndDate = document.getElementById('editUserLockoutEndDate');
    const modalValidationSummary = document.getElementById('modalValidationSummary');

    // Event listener for "Edit User" buttons on cards
    document.querySelectorAll('.edit-user-btn').forEach(button => {
        button.addEventListener('click', async function () {
            const userId = this.dataset.userId;
            modalValidationSummary.innerHTML = ''; // Limpiar errores previos en el modal

            try {
                const response = await fetch(`/Admin/GetUserForEdit?id=${userId}`);

                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(`Error HTTP! Estado: ${response.status} - ${errorText}`);
                }

                const userData = await response.json();
                console.log('Datos de usuario recibidos para el modal:', userData); // ¡IMPORTANTE PARA DEPURAR!

                // Reset form first
                if (editUserForm) editUserForm.reset();
                editUserForm.querySelectorAll('.text-danger').forEach(span => span.textContent = '');


                // Poblar los campos del formulario del modal
                // Asegurarse de que el elemento exista antes de intentar asignar su valor
                if (editUserId) editUserId.value = userData.id || '';
                if (editUserNombre) editUserNombre.value = userData.nombre || '';
                if (editUserEmail) editUserEmail.value = userData.email || '';
                if (editUserPhoneNumber) editUserPhoneNumber.value = userData.phoneNumber || '';
                if (editUserTelefonoEmergencia) editUserTelefonoEmergencia.value = userData.telefonoEmergencia || '';

                // Limpiar campos de contraseña (estos son para NUEVA contraseña, no se rellenan)
                if (editUserNewPassword) editUserNewPassword.value = '';
                if (editUserConfirmNewPassword) editUserConfirmNewPassword.value = '';

                // Formatear fechas para input type="date" (YYYY-MM-DD)
                if (editUserLockoutStartDate) {
                    editUserLockoutStartDate.value = userData.lockoutStartDate ? new Date(userData.lockoutStartDate).toISOString().split('T')[0] : '';
                }
                if (editUserLockoutEndDate) {
                    editUserLockoutEndDate.value = userData.lockoutEndDate ? new Date(userData.lockoutEndDate).toISOString().split('T')[0] : '';
                }

                // Restablecer el formulario para limpiar estados de validación de jQuery Validation
               // if (editUserForm) editUserForm.reset();
                // Limpiar manualmente los mensajes de validación si persisten
                //editUserForm.querySelectorAll('.text-danger').forEach(span => span.textContent = '');

                editUserModal.show(); // Mostrar el modal
            } catch (error) {
                console.error('Error al cargar los datos del usuario para edición:', error);
                if (modalValidationSummary) {
                    modalValidationSummary.innerHTML = `<div class="alert alert-danger">Error al cargar los datos del usuario. Por favor, inténtalo de nuevo. Detalles: ${error.message}</div>`;
                }
            }
        });
    });

    // Manejar el envío del formulario de edición de usuario (modal)
    editUserForm.addEventListener('submit', async function (event) {
        event.preventDefault(); // Prevenir el envío de formulario por defecto
        if (modalValidationSummary) modalValidationSummary.innerHTML = ''; // Limpiar errores previos

        // Validar el formulario con jQuery Validation (si está configurado)
        if (typeof $ !== 'undefined' && $.validator && !$(this).valid()) {
            return; // Detener el envío si la validación del cliente falla
        }

        const formData = new FormData(this);
        const data = Object.fromEntries(formData.entries());

        // Solo convierte las fechas si el campo no está vacío.
        if (data.LockoutStartDate !== "") {
            data.LockoutStartDate = new Date(data.LockoutStartDate).toISOString();
        } else {
            // Asigna 'null' para que coincida con el tipo DateTime? en C#.
            data.LockoutStartDate = null;
        }

        if (data.LockoutEndDate !== "") {
            data.LockoutEndDate = new Date(data.LockoutEndDate).toISOString();
        } else {
            data.LockoutEndDate = null;
        }

        // Obtener el token de antiforja
        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

        try {
            const response = await fetch('/Admin/EditUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: JSON.stringify(data)
            });


            const result = await response.json();

            if (result.success) {
                editUserModal.hide();
                location.reload();
            } else {
                if (result.errors && result.errors.length > 0) {
                    let errorHtml = '<p class="fw-bold">Errores de validación:</p><ul>';
                    result.errors.forEach(err => {
                        errorHtml += `<li>${err}</li>`;
                    });
                    errorHtml += '</ul>';
                    if (modalValidationSummary) modalValidationSummary.innerHTML = `<div class="alert alert-danger">${errorHtml}</div>`;
                } else {
                    if (modalValidationSummary) modalValidationSummary.innerHTML = `<div class="alert alert-danger">${result.message || 'Error desconocido al actualizar el perfil.'}</div>`;
                }
            }
        } catch (error) {
            console.error('Error al enviar el formulario de edición:', error);
            if (modalValidationSummary) modalValidationSummary.innerHTML = '<div class="alert alert-danger">Hubo un error de conexión al guardar los cambios. Por favor, inténtalo de nuevo.</div>';
        }
    });

    // Manejar envíos de formularios para Bloquear/Desbloquear Usuario
    document.querySelectorAll('.toggle-lockout-form').forEach(form => {
        form.addEventListener('submit', async function (event) {
            event.preventDefault();
            const userId = this.querySelector('input[name="id"]').value;
            const antiForgeryToken = this.querySelector('input[name="__RequestVerificationToken"]').value;

            // Usamos un modal de confirmación de Bootstrap para mejor UX
            // En lugar de alert/confirm nativo, puedes implementar un modal personalizado aquí
            if (!confirm(`¿Estás seguro de que quieres ${this.querySelector('button').title.toLowerCase()}?`)) {
                return;
            }

            try {
                const response = await fetch(this.action, {
                    method: this.method,
                    headers: {
                        'RequestVerificationToken': antiForgeryToken,
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: new URLSearchParams({ id: userId })
                });
                const result = await response.json();

                if (result.success) {
                    location.reload();
                } else {
                    alert(result.message || 'Error al cambiar el estado de bloqueo.');
                }
            } catch (error) {
                console.error('Error al cambiar el estado de bloqueo:', error);
                alert('Error de conexión al cambiar el estado de bloqueo.');
            }
        });
    });

    // Manejar envíos de formularios para Eliminar Usuario
    document.querySelectorAll('.delete-user-form').forEach(form => {
        form.addEventListener('submit', async function (event) {
            event.preventDefault();
            const userId = this.querySelector('input[name="id"]').value;
            const antiForgeryToken = this.querySelector('input[name="__RequestVerificationToken"]').value;

            // Usamos un modal de confirmación de Bootstrap para mejor UX
            // En lugar de alert/confirm nativo, puedes implementar un modal personalizado aquí
            if (!confirm('¿Estás seguro de que quieres eliminar este usuario? Esta acción es irreversible.')) {
                return;
            }

            try {
                const response = await fetch(this.action, {
                    method: this.method,
                    headers: {
                        'RequestVerificationToken': antiForgeryToken,
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: new URLSearchParams({ id: userId })
                });
                const result = await response.json();

                if (result.success) {
                    location.reload();
                } else {
                    alert(result.message || 'Error al eliminar el usuario.');
                }
            } catch (error) {
                console.error('Error al eliminar el usuario:', error);
                alert('Error de conexión al eliminar el usuario.');
            }
        });
    });


    // --- Lógica para Crear Nuevo Usuario (Añadido) ---
    // Referencia al botón usando el nuevo ID
    const createUserButton = document.getElementById('createUserButton');
    const createUserModal = new bootstrap.Modal(document.getElementById('createUserModal'));
    const createUserForm = document.getElementById('createUserForm');
    const createValidationSummary = document.getElementById('createValidationSummary');

    if (createUserButton) {
        createUserButton.addEventListener('click', function (event) {
            event.preventDefault();
            if (createUserForm) createUserForm.reset();
            if (createValidationSummary) createValidationSummary.innerHTML = '';
            createUserModal.show();
        });
    }

    if (createUserForm) {
        createUserForm.addEventListener('submit', async function (event) {
            event.preventDefault();
            if (createValidationSummary) createValidationSummary.innerHTML = '';

            const formData = new FormData(this);
            const data = Object.fromEntries(formData.entries());

            data.SelectedRole = data.SelectedRole || '';

            const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

            try {
                const response = await fetch('/Admin/CreateUser', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': antiForgeryToken
                    },
                    body: JSON.stringify(data)
                });

                const result = await response.json();

                if (result.success) {
                    createUserModal.hide();
                    location.reload();
                } else {
                    if (result.errors && result.errors.length > 0) {
                        let errorHtml = '<p class="fw-bold">Errores de validación:</p><ul>';
                        result.errors.forEach(err => {
                            errorHtml += `<li>${err}</li>`;
                        });
                        errorHtml += '</ul>';
                        if (createValidationSummary) createValidationSummary.innerHTML = `<div class="alert alert-danger">${errorHtml}</div>`;
                    } else {
                        if (createValidationSummary) createValidationSummary.innerHTML = `<div class="alert alert-danger">${result.message || 'Error desconocido al crear el usuario.'}</div>`;
                    }
                }
            } catch (error) {
                console.error('Error al enviar el formulario de creación:', error);
                if (createValidationSummary) createValidationSummary.innerHTML = '<div class="alert alert-danger">Hubo un error de conexión al crear el usuario. Por favor, inténtalo de nuevo.</div>';
            }
        });
    }


});
