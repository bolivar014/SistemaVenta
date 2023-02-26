const VISTA_BUSQUEDA = {
    busquedaFecha: () => {
        $("#txtFechaInicio").val("");
        $("#txtFechaFin").val("");
        $("#txtNumeroVenta").val("");

        $(".busqueda-fecha").show();
        $(".busqueda-venta").hide();

    }, busquedaVenta: () => {
        $("#txtFechaInicio").val("");
        $("#txtFechaFin").val("");
        $("#txtNumeroVenta").val("");

        $(".busqueda-fecha").hide();
        $(".busqueda-venta").show();
    }
}

// 
$(document).ready(function () {
    VISTA_BUSQUEDA["busquedaFecha"]();

    $.datepicker.setDefaults($.datepicker.regional["es"]);

    $("#txtFechaInicio").datepicker({ dateFormat: "dd/mm/yy" });
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" });
});

// 
$("#cboBuscarPor").change(function () {
    if ($("#cboBuscarPor").val() == "fecha") {
        VISTA_BUSQUEDA["busquedaFecha"]()
    } else {
        VISTA_BUSQUEDA["busquedaVenta"]()
    }
});

$("#btnBuscar").click(function () {
    if ($("#cboBuscarPor").val() == "fecha") {
        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
            toastr.warning("", "Debe ingresar fechas de inicio y fin validas");
            return;
        }
    } else {
        if ($("#txtNumeroVenta").val().trim() == "") {
            toastr.warning("", "Debe ingresar numero de venta valida");
            return;
        }
    }

    // Recupero variables
    let numeroVenta = $("#txtNumeroVenta").val();
    let fechaInicio = $("#txtFechaInicio").val();
    let fechaFin = $("#txtFechaFin").val();

    // Hacemos visible Splash de carga
    $(".card-body").find("div.row").LoadingOverlay("show");
    
    // Fetch para obtener lista de roles
    fetch(`/Venta/Historial?numeroVenta=${numeroVenta}&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
    .then(response => {
        // Ocultamos Splash de carga
        $(".card-body").find("div.row").LoadingOverlay("hide");

        // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        // console.log(responseJSON);

        // Limpiamos
        $("#tbventa tbody").html("");

        // Verificamos
        if (responseJSON.length > 0) {

            responseJSON.forEach((venta) => {
                $("#tbventa tbody").append(
                    $("<tr>").append(
                        $("<td>").text(venta.fechaRegistro),
                        $("<td>").text(venta.numeroVenta),
                        $("<td>").text(venta.tipoDocumentoVenta),
                        $("<td>").text(venta.documentoCliente),
                        $("<td>").text(venta.nombreCliente),
                        $("<td>").text(venta.total),
                        $("<td>").append(
                            $("<button>").addClass("btn btn-info btn-sm").append(
                                $("<i>").addClass("fas fa-eye")
                            ).data("venta", venta)
                        ),
                    )
                )
            })
        }
    });
});

$("#tbventa tbody").on("click", ".btn-info", function () {
    let d = $(this).data("venta");

    console.log("objeto venta");
    console.log(d);

    $("#txtFechaRegistro").val(d.fechaRegistro);
    $("#txtNumVenta").val(d.numeroVenta);
    $("#txtUsuarioRegistro").val(d.usuario);
    $("#txtTipoDocumento").val(d.tipoDocumentoVenta);
    $("#txtDocumentoCliente").val(d.documentoCliente);
    $("#txtNombreCliente").val(d.nombreCliente);
    $("#txtSubTotal").val(d.subTotal);
    $("#txtIGV").val(d.impuestoTotal);
    $("#txtTotal").val(d.total);

    // Limpiamos
    $("#tbProductos tbody").html("");

    // accedemos al detalle del producto
    d.detalleVenta.forEach((item) => {
        $("#tbProductos tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total),
            )
        )
    })

    // Generamos link para descargar PDF
    $("#linkImprimir").attr("href", `/Venta/MostrarPDFVenta?numeroVenta=${d.numeroVenta}`);

    // Abrimos modal de datos
    $("#modalData").modal("show");
});

// 