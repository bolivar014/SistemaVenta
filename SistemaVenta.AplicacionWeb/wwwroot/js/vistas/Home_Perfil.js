$(document).ready(function () {
    // Hacemos visible Splash de carga
    $(".container-fluid").LoadingOverlay("show");

    // Fetch para obtener lista de roles
    fetch("/Home/ObtenerUsuario")
        .then(response => {
        // ocultamos Splash de carga
            $(".container-fluid").LoadingOverlay("hide");

        // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        // console.log(responseJSON);

        // Verificamos
        if (responseJSON.estado) {

            const d = responseJSON.objeto;

            $("#imgFoto").attr("src", d.urlFoto);
            $("#txtNombre").val(d.nombre);
            $("#txtCorreo").val(d.correo);
            $("#txTelefono").val(d.telefono);
            $("#txtRol").val(d.nombreRol);
        } else {
            // En caso de error
            swal("Lo sentimos", responseJSON.mensaje, "error");
        }
    });
})


$("#btnGuardarCambios").click(function () {
    // Verificamos inputs
    if ($("#txtCorreo").val().trim() == "") {
        toastr.warning("", "Debe completar el campo : Correo...");
        $("#txtCorreo").focus();
        return;
    }
    if ($("#txTelefono").val().trim() == "") {
        toastr.warning("", "Debe completar el campo : Telefono...");
        $("#txTelefono").focus();
        return;
    }

    // Confirmamos cambios
    swal({
        title: "¿Deseas guardar los cambios?",
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-primary",
        confirmButtonText: "Si",
        cancelButtonText: "No",
        closeOnConfirm: false,
        closeOnCancel: true
    }, function (respuesta) {
        if (respuesta) {
            $(".showSweetAlert").LoadingOverlay("show");

            let modelo = {
                correo: $("#txtCorreo").val().trim(),
                telefono: $("#txTelefono").val().trim(),
            }

            // Evento que se ejecuta cuando es para "Editar"
            fetch("/Home/GuardarPerfil", {
                method: "POST",
                headers: { "Content-Type": "application/json; charset=utf-8" },
                body: JSON.stringify(modelo)
            })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJSON => {
                    if (responseJSON.estado) {

                        swal("Listo!", "Los cambios fueron guardados...", "success");
                    } else {
                        swal("Lo sentimos", responseJSON.mensaje, "error");
                    }
                })
        }
    })
})


$("#btnCambiarClave").click(function () {
    // Verificamos inputs
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");

    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();

        return;
    }

    // Validamos la contraseñas
    if ($("#txtClaveNueva").val().trim() != $("#txtConfirmarClave").val().trim()) {
        toastr.warning("", "Las contraseñas no coinciden...");
        return;
    }

    let modelo = {
        ClaveActual: $("#txtClaveActual").val().trim(),
        ClaveNueva: $("#txtClaveNueva").val().trim()
    }

    console.log("modelo");
    console.log(modelo);

    // Ejecutamos petición
    fetch("/Home/CambiarClave", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
    .then(response => {
        $(".showSweetAlert").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        if (responseJSON.estado) {

            swal("Listo!", "Su contraseña fue actualizada...", "success");

            // Limpiamos campos...
            $("input.input-validar").val("");
        } else {
            swal("Lo sentimos", responseJSON.mensaje, "error");
        }
    })


});