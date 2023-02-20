// Inicializamos modelo base
const MODELO_BASE = {
    idUsuario: 0,
    nombre: "",
    correo: "",
    telefono: "",
    idRol: 0,
    esActivo: 1,
    urlFoto: ""
}

// Inicializamos tblData
let tablaData;

// Cargamos cuando ya se hubiese inicializado el proyecto
$(document).ready(function () {
    fetch("/Usuario/ListaRoles")
    .then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        if (responseJSON.length > 0) {
            responseJSON.forEach((item) => {
                $("#cboRol").append(
                    $("<option>").val(item.idRol).text(item.descripcion)
                );
            });
        }
    });

    tablaData = $('#tbdata').DataTable({
        responsive: true,
         "ajax": {
             "url": '/Usuario/Lista',
             "type": "GET",
             "datatype": "json"
         },
        "columns": [
            {
                "data": "idUsuario",
                "visible": false,
                "searchable": false
            },
            {
                "data": "urlFoto",
                render: function (data) {
                    return `<img style="height: 60px;" src=${data} class="rounded mx-auto d-block"/>`
                }
            },
            { "data": "nombre" },
            { "data": "correo" },
            { "data": "telefono" },
            { "data": "nombreRol" },
            {
                "data": "esActivo",
                render: function (data) {
                    if (data == 1) {
                        return '<span class="badge badge-info">Activo</span>';
                    }
                    else
                    {
                        return '<span class="badge badge-danger">No Activo</span>';
                    }
                }
            },
             {
                 "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                     '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                 "orderable": false,
                 "searchable": false,
                 "width": "80px"
             }
         ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Usuarios',
                exportOptions: {
                    columns: [2,3,4,5,6]
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});

// 
function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idUsuario);
    $("#txtNombre").val(modelo.nombre);
    $("#txtCorreo").val(modelo.correo);
    $("#txtTelefono").val(modelo.telefono);
    $("#cboRol").val(modelo.idRol == 0 ? $("#cboRol option:first").val() : modelo.idRol);
    $("#cboEstado").val(modelo.esActivo);
    $("#txtFoto").val("");
    $("#imgUsuario").attr("src", modelo.urlFoto);

    $("#modalData").modal("show");
}

// Evento click para abrir modal
$("#btnNuevo").click(function () {
    // Ejecutamos el evento para abrir la modal de crear usuario
    mostrarModal();
});


// Evento click para guardar formulario de crear
$("#btnGuardar").click(function () {
    // debugger;
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");

    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();

        return;
    }

    // Inicializamos modelo de datos base
    const modelo = structuredClone(MODELO_BASE);

    // Recuperamos argumentos en el modelo de datos...
    modelo["idUsuario"] = parseInt($("#txtId").val());
    modelo["nombre"] = $("#txtNombre").val();
    modelo["correo"] = $("#txtCorreo").val();
    modelo["telefono"] = $("#txtTelefono").val();
    modelo["idRol"] = $("#cboRol").val();
    modelo["esActivo"] = $("#cboEstado").val();

    // Accedemos al input de la foto
    const inputFoto = document.getElementById("txtFoto");

    const formData = new FormData();

    // Agregamos objeto de datos al formData
    formData.append("foto", inputFoto.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    // Generamos animación de procesando...
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idUsuario == 0) {
        // Evento que se ejecuta cuando es para "Crear"
        fetch("/Usuario/Crear", {
            method: "POST",
            body: formData
        })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJSON => {
            if (responseJSON.estado) {
                tablaData.row.add(responseJSON.objeto).draw(false);
                $("#modalData").modal("hide");
                swal("Listo!", "Usuario creado", "success");
            } else {
                swal("Lo sentimos", responseJSON.mensaje, "error");
            }
        })
    }
    else
    {
        // Evento que se ejecuta cuando es para "Editar"
        fetch("/Usuario/Editar", {
            method: "PUT",
            body: formData
        })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJSON => {
            if (responseJSON.estado) {
                tablaData.row(filaSeleccionada).data(responseJSON.objeto).draw(false);
                // Liberamos variable
                filaSeleccionada = null;

                $("#modalData").modal("hide");
                swal("Listo!", "Usuario Actualizado", "success");
            } else {
                swal("Lo sentimos", responseJSON.mensaje, "error");
            }
        })
    }
});

// inicializo variable auxiliar
let filaSeleccionada;

// Evento callback para editar usuario
$("#tbdata tbody").on("click", ".btn-editar", function () {
    // Accedemos al tr que recibio el evento onclick
    if ($(this).closest("tr").hasClass("child")) {
        filaSeleccionada = $(this).closest("tr").prev();
    } else {
        filaSeleccionada = $(this).closest("tr");
    }

    // Accedemos al objeto de datos del registro seleccionado...
    const data = tablaData.row(filaSeleccionada).data();

    // Abrimos modal con su respectiva información
    mostrarModal(data);
});



// Evento callback para eliminar usuario
$("#tbdata tbody").on("click", ".btn-eliminar", function () {
    let fila;

    // Accedemos al tr que recibio el evento onclick
    if ($(this).closest("tr").hasClass("child")) {
        fila = $(this).closest("tr").prev();
    } else {
        fila = $(this).closest("tr");
    }

    // Accedemos al objeto de datos del registro seleccionado...
    const data = tablaData.row(fila).data();

    swal({
        title: "¿Estas seguro?",
        text: `Eliminar al usuario "${data.nombre}"?`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, Eliminar",
        cancelButtonText: "No, Cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    }, function (respuesta) {
        if (respuesta) {
            $(".showSweetAlert").LoadingOverlay("show");

            // Evento que se ejecuta cuando es para "Editar"
            fetch(`/Usuario/Eliminar?IdUsuario=${data.idUsuario}`, {
                method: "DELETE"
            })
            .then(response => {
                $(".showSweetAlert").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJSON => {
                if (responseJSON.estado) {
                    tablaData.row(fila).remove().draw();
   
                    swal("Listo!", "El usuario fue eliminado", "success");
                } else {
                    swal("Lo sentimos", responseJSON.mensaje, "error");
                }
            })
        }
    })
});
