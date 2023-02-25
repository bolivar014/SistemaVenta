
// Inicializamos tblData
let tablaData;
let valorImpuesto = 0;

// Cargamos cuando ya se hubiese inicializado el proyecto
$(document).ready(function () {
    // Fetch para obtener lista de Tipos de documentos
    fetch("/Venta/ListaTipoDocumentoVenta")
    .then(response => {
        // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        if (responseJSON.length > 0) {
            responseJSON.forEach((item) => {
                $("#cboTipoDocumentoVenta").append(
                    $("<option>").val(item.idTipoDocumentoVenta).text(item.descripcion)
                );
            });
        }
    });

    // Fetch para obtener lista de Tipos de documentos
    fetch("/Negocio/Obtener")
    .then(response => {
        // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        if (responseJSON.estado) {
            // Obtenemos el objeto de datos del negocio
            const d = responseJSON.objeto;

            // Modificamos dinamicamente la información del detalle de moneda
            $("#inputGroupSubTotal").text(`Sub total - ${d.simboloMoneda}`);
            $("#inputGroupIGV").text(`IGV(${d.porcentajeImpuesto}) - ${d.simboloMoneda}`);
            $("#inputGroupTotal").text(`Total - ${d.simboloMoneda}`);

            valorImpuesto = parseFloat(d.porcentajeImpuesto);
        }
    });

    // Integramos Select2 para filtros de busqueda de productos
    $("#cboBuscarProducto").select2({
        ajax: {
            url: "/Venta/ObtenerProductos",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    busqueda: params.term
                };
            },
            processResults: function (data) {
                
                return {
                    results: data.map((item) => (
                        {
                            id: item.idProducto,
                            text: item.descripcion,

                            marca: item.marca,
                            categoria: item.nombreCategoria,
                            urlImagen: item.urlImagen,
                            precio: parseFloat(item.precio)
                        }
                    ))
                };
            },
        },
        language: "es",
        placeholder: 'Buscar producto...',
        minimumInputLength: 1,
        templateResult: formatoResultados
    });
});

// Construimos plantilla template
function formatoResultados(data) {
    // Es por defecto, para mostrar evento de "Buscando..."
    if (data.loading)
        return data.text;

    // Creamos contenedor de plantilla
    var contenedor = $(
        `<table width="100%">
            <tr>
                <td style="width: 60px;">
                    <img style="height: 60px; width: 60px; margin-right: 10px;" src="${data.urlImagen}"/>
                </td>
                <td>
                    <p style="font-weight: bolder; margin: 2px;">${data.marca}</p>
                    <p style="margin: 2px;">${data.text}</p>
                </td>
            </tr>
        </table>`
    );

    // Retornamos contenedor...
    return contenedor;
}

$(document).on("select2:open", function () {
    // 
    document.querySelector(".select2-search__field").focus();

});

let ProductosParaVenta = [];

// Cuando posee un articulo seleccionado, hace algo...
$("#cboBuscarProducto").on("select2:select", function (e) {
    // Obtenemos el objeto del producto seleccionado
    const data = e.params.data;

    // Buscamos producto
    let producto_encontrado = ProductosParaVenta.filter(p => p.idProducto == data.id);

    // Verificamo si el producto existe
    if (producto_encontrado.length > 0) {
        $("#cboBuscarProducto").val("").trigger("change"); // Si el producto existe, limpiamos
        toastr.warning("", "El producto ya fue agregado");
        return false;
    }

    // 
    swal({
        title: data.marca,
        text: data.text,
        imageUrl: data.urlImagen,
        type: "input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceholder: "Ingrese Cantidad",

    }, function (valor) {
        // Verificamos
        if (valor === false) {
            return false;
        }

        if (valor === "") {
            toastr.warning("", "Necesita ingresar la cantidad");
            return false;
        }

        if(isNaN(parseInt(valor))){
            toastr.warning("", "Debe ingresar un valor numerico");
            return false;
        }

        // Calculamos producto
        let producto = {
            idProducto: data.id,
            marcaProducto: data.marca,
            descripcionProducto: data.text,
            categoriaProducto: data.categoria,
            cantidad: parseInt(valor),
            precio: data.precio.toString(),
            total: (parseFloat(valor) * data.precio).toString()
        }

        // Agregamos prodcuto
        ProductosParaVenta.push(producto);

        // Evento para calcular valor de productos...
        mostrarProducto_Precios();

        // 
        $("#cboBuscarProducto").val("").trigger("change");

        // 
        swal.close();
    })
});

function mostrarProducto_Precios() {
    let total = 0;
    let igv = 0;
    let subtotal = 0;
    let porcentaje = valorImpuesto / 100;

    $("#tbProducto tbody").html("");

    ProductosParaVenta.forEach((item) => {
        // Calculamos valor total
        total = total + parseFloat(item.total);

        // Insertamos campos a la tabla de productos
        $("#tbProducto tbody").append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-eliminar btn-sm").append(
                        $("<i>").addClass("fas fa-trash-alt")
                    ).data("idProducto", item.idProducto)
                ),
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total) 
            )
        )
    });

    subtotal = total / (1 + porcentaje);
    igv = total - subtotal;

    $("#txtSubTotal").val(subtotal.toFixed(2));
    $("#txtIGV").val(igv.toFixed(2));
    $("#txtTotal").val(total.toFixed(2));
}

$(document).on("click", "button.btn-eliminar", function () {
    const _idproducto = $(this).data("idProducto");

    // Omitimos el producto ya seleccionado
    ProductosParaVenta = ProductosParaVenta.filter(p => p.idProducto != _idproducto);

    // Refrescamos...
    mostrarProducto_Precios();
})

// Confirmar venta
$("#btnTerminarVenta").click(function () {
    if (ProductosParaVenta.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    const vmDetalleVenta = ProductosParaVenta;

    const venta = {
        idTipoDocumentoVenta: $("#cboTipoDocumentoVenta").val(),
        documentoCliente: $("#txtDocumentoCliente").val(),
        nombreCliente: $("#txtNombreCliente").val(),
        subTotal: $("#txtSubTotal").val(),
        impuestoTotal: $("#txtIGV").val(),
        total: $("#txtTotal").val(),
        DetalleVEnta: vmDetalleVenta
    };

    // ejecutamos Animación de procesando
    $("#btnTerminarVenta").LoadingOverlay("show");

    // Fetch para obtener lista de Tipos de documentos
    fetch("/Venta/RegistrarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json; chatset=utf-8" },
        body: JSON.stringify(venta)
    })
    .then(response => {

        // ejecutamos Animación de procesando
        $("#btnTerminarVenta").LoadingOverlay("hide");

        // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJSON => {
        if (responseJSON.estado) {
            ProductosParaVenta = [];
            mostrarProducto_Precios();

            $("#txtDocumentoCliente").val("");
            $("#txtNombreCliente").val("");
            $("#cboTipoDocumentoVenta").val($("#cboTipoDocumentoVenta option:first").val());

            // Evento de notificación
            swal("Registrado", `Numero de venta: ${responseJSON.objeto.numeroVenta}`, "success");
        } else {
            swal("Lo sentimos", "No se pudo registrar la venta", "error");
        }
    });
});