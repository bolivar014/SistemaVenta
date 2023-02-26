$(document).ready(function () {
    // Hacemos visible Splash de carga
    $("div.container-fluid").LoadingOverlay("show");

    // Fetch para obtener lista de roles
    fetch("/DashBoard/ObtenerResumen")
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide");
            // En caso de obtener un response correcto, retorna el JSON. de lo contrario, cancela
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJSON => {
            // console.log(responseJSON);

            if (responseJSON.estado) {
                // Mostrar datos para tarjetas
                let d = responseJSON.objeto;

                // Llevamos valor del objeto a la interfaz del formulario...
                $("#totalVenta").text(d.totalVentas);
                $("#totalIngresos").text(d.totalIngresos);
                $("#totalProductos").text(d.totalProductos);
                $("#totalCategorias").text(d.totalCategorias);

                // Obtener textos y valores para grafico de barras...
                let barchart_labels;
                let barchart_data;

                // Verificamos
                if (d.ventasUltimaSemana.length > 0) {
                    // Cuando existen datos
                    barchart_labels = d.ventasUltimaSemana.map((item) => { return item.fecha });
                    barchart_data = d.ventasUltimaSemana.map((item) => { return item.total });
                } else {
                    // Cuando no existen datos
                    barchart_labels = ["Sin resultados"];
                    barchart_data = [0];
                }

                // Obtener textos y valores para grafico de pie...
                let piechart_labels;
                let piechart_data;

                // Verificamos
                if (d.productosTopUltimaSemana.length > 0) {
                    // Cuando existen datos
                    piechart_labels = d.productosTopUltimaSemana.map((item) => { return item.producto });
                    piechart_data = d.productosTopUltimaSemana.map((item) => { return item.cantidad });
                } else {
                    // Cuando no existen datos
                    piechart_labels = ["Sin resultados"];
                    piechart_data = [0];
                }


                // BarChart Example
                let controlVenta = document.getElementById("chartVentas");
                let myBarChart = new Chart(controlVenta, {
                    type: 'bar',
                    data: {
                        // labels: ["06/07/2022", "07/07/2022", "08/07/2022", "09/07/2022", "10/07/2022", "11/07/2022", "12/07/2022"],
                        labels: barchart_labels,
                        datasets: [{
                            label: "Cantidad",
                            backgroundColor: "#4e73df",
                            hoverBackgroundColor: "#2e59d9",
                            borderColor: "#4e73df",
                            // data: [12, 10, 22, 11, 15, 10, 22],
                            data: barchart_data,
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        legend: {
                            display: false
                        },
                        scales: {
                            xAxes: [{
                                gridLines: {
                                    display: false,
                                    drawBorder: false
                                },
                                maxBarThickness: 50,
                            }],
                            yAxes: [{
                                ticks: {
                                    min: 0,
                                    maxTicksLimit: 5
                                }
                            }],
                        },
                    }
                });

                // Pie Chart Example
                let controlProducto = document.getElementById("chartProductos");
                let myPieChart = new Chart(controlProducto, {
                    type: 'doughnut',
                    data: {
                        // labels: ["Producto A", "Producto B", "Producto C", "Producto D"],
                        labels: piechart_labels,
                        datasets: [{
                            // data: [55, 30, 15, 10],
                            data: piechart_data,
                            backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', "#FF785B"],
                            hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', "#FF5733"],
                            hoverBorderColor: "rgba(234, 236, 244, 1)",
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        tooltips: {
                            backgroundColor: "rgb(255,255,255)",
                            bodyFontColor: "#858796",
                            borderColor: '#dddfeb',
                            borderWidth: 1,
                            xPadding: 15,
                            yPadding: 15,
                            displayColors: false,
                            caretPadding: 10,
                        },
                        legend: {
                            display: true
                        },
                        cutoutPercentage: 80,
                    },
                });

            }
        });
})
