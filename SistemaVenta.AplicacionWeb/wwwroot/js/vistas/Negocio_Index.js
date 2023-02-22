$(document).ready(function () {
    // Hacemos visible Splash de carga
    $(".card-body").LoadingOverlay("show");

    // Fetch para obtener lista de roles
    fetch("/Negocio/Obtener")
        .then(response => {
            // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJSON => {
            // console.log(responseJSON);

            // Ocultamos Splash de carga
            $(".card-body").LoadingOverlay("hide");
            // Verificamos
            if (responseJSON.estado) {

                const d = responseJSON.objeto;

                $("#txtNumeroDocumento").val(d.numeroDocumento);
                $("#txtRazonSocial").val(d.nombre);
                $("#txtCorreo").val(d.correo);
                $("#txtDireccion").val(d.direccion);
                $("#txTelefono").val(d.telefono);
                $("#txtImpuesto").val(d.porcentajeImpuesto);
                $("#txtSimboloMoneda").val(d.simboloMoneda);
                $("#imgLogo").attr("src", d.urlLogo);
            } else {
                // En caso de error
                swal("Lo sentimos", responseJSON.mensaje, "error");
            }
        });
})

// Evento click
$("#btnGuardarCambios").click(function () {
    // Obtenemos inputs de formulario a validar
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");

    // Verificamos que todos los campos esten llenos
    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();

        return;
    }

    // Generamos modelo de datos
    const modelo = {
        numeroDocumento: $("#txtNumeroDocumento").val(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txTelefono").val(),
        porcentajeImpuesto: $("#txtImpuesto").val(),
        simboloMoneda: $("#txtSimboloMoneda").val()
    }

    // 
    const inputLogo = document.getElementById("txtLogo");

    // Inicializamos
    const formData = new FormData();

    // Agregamos objeto de datos
    formData.append("logo", inputLogo.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    // Hacemos visible Splash de carga
    $(".card-body").LoadingOverlay("show");

    // Fetch para obtener lista de roles
    fetch("/Negocio/GuardarCambios", {
        method: "POST",
        body: formData
    })
        .then(response => {
            // Ocultamos Splash de carga
            $(".card-body").LoadingOverlay("hide");

            // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJSON => {
            // console.log(responseJSON);

            // Verificamos
            if (responseJSON.estado) {

                const d = responseJSON.objeto;

                // Inyectamos imagen
                $("#imgLogo").attr("src", d.urlLogo);

            } else {
                // En caso de error
                swal("Lo sentimos", responseJSON.mensaje, "error");
            }
        });
})
