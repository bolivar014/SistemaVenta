// Inicializamos modelo base
const MODELO_BASE = {
    idCategoria: 0,
    descripcion: "",
    esActivo: 1
}

// Inicializamos tblData
let tablaData;

// Cargamos cuando ya se hubiese inicializado el proyecto
$(document).ready(function () {
    // 
    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Categoria/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idCategoria",
                "visible": false,
                "searchable": false
            },
            { "data": "descripcion" },
            {
                "data": "esActivo",
                render: function (data) {
                    if (data == 1) {
                        return '<span class="badge badge-info">Activo</span>';
                    }
                    else {
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
                filename: 'Reporte Categorias',
                exportOptions: {
                    columns: [1,2]
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });

});

// Evento click para abrir modal
// $(".btnNuevo").click(function () {
$("#btnNuevo").click(function () {
    console.log('event on click btnNuevo');
    // Ejecutamos el evento para abrir la modal de crear usuario
    mostrarModal();
});

// 
function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idCategoria);
    $("#txtDescripcion").val(modelo.descripcion);
    $("#cboEstado").val(modelo.esActivo);

    $("#modalData").modal("show");
}


// Evento click para guardar formulario de crear
$("#btnGuardar").click(function () {
    // debugger;
    if ($("#txtDescripcion").val().trim() == "") {
        toastr.warning("", "Debe completar el campo : Descripción...");
        $("#txtDescripcion").focus();
        return;
    }

    // Inicializamos modelo de datos base
    const modelo = structuredClone(MODELO_BASE);

    // Recuperamos argumentos en el modelo de datos...
    modelo["idCategoria"] = parseInt($("#txtId").val());
    modelo["descripcion"] = $("#txtDescripcion").val();
    modelo["esActivo"] = $("#cboEstado").val();

    // Generamos animación de procesando...
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idCategoria == 0) {
        // Evento que se ejecuta cuando es para "Crear"
        fetch("/Categoria/Crear", {
            method: "POST",
            headers: {
                "Content-Type":"application/json; charset=utf-8"
            },
            body: JSON.stringify(modelo)
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJSON => {
                if (responseJSON.estado) {
                    tablaData.row.add(responseJSON.objeto).draw(false);
                    $("#modalData").modal("hide");
                    swal("Listo!", "La categoria fue creada", "success");
                } else {
                    swal("Lo sentimos", responseJSON.mensaje, "error");
                }
            })
    }
    else {
        // Evento que se ejecuta cuando es para "Editar"
        fetch("/Categoria/Editar", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json;charset=utf-8"
            },
            body: JSON.stringify(modelo)
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
                    swal("Listo!", "La categoria fue Actualizada", "success");
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
        text: `Eliminar la categoria "${data.nombre}"?`,
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
            fetch(`/Categoria/Eliminar?IdCategoria=${data.idCategoria}`, {
                method: "DELETE"
            })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJSON => {
                    if (responseJSON.estado) {
                        tablaData.row(fila).remove().draw();

                        swal("Listo!", "La categoria fue eliminada", "success");
                    } else {
                        swal("Lo sentimos", responseJSON.mensaje, "error");
                    }
                })
        }
    })
});
