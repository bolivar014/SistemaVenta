﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// 
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashBoardService _dashboardServicio;

        public DashboardController(IDashBoardService dashboardServicio)
        {
            _dashboardServicio = dashboardServicio;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerResumen()
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                VMDashBoard vmDashBoard = new VMDashBoard();

                vmDashBoard.TotalVentas = await _dashboardServicio.TotalVentasUltimaSemana();
                vmDashBoard.TotalIngresos = await _dashboardServicio.TotalIngresosUltimaSemana();
                vmDashBoard.TotalProductos = await _dashboardServicio.TotalProductos();
                vmDashBoard.TotalCategorias = await _dashboardServicio.TotalCategorias();

                List<VMVentasSemana> listaVentasSemana = new List<VMVentasSemana>();
                List<VMProductosSemana> listaProductosSemana = new List<VMProductosSemana>();

                // Generamos listas
                foreach (KeyValuePair<string, int> item in await _dashboardServicio.VentasUltimaSemana())
                {
                    listaVentasSemana.Add(new VMVentasSemana()
                    {
                        Fecha = item.Key,
                        Total = item.Value
                    });
                }

                // Productos TOP ultima semana
                foreach (KeyValuePair<string, int> item in await _dashboardServicio.ProductosTopUltimaSemana())
                {
                    listaProductosSemana.Add(new VMProductosSemana()
                    {
                        Producto = item.Key,
                        Cantidad = item.Value
                    });
                }

                // Obtenemos datos de modelo
                vmDashBoard.VentasUltimaSemana = listaVentasSemana;
                vmDashBoard.ProductosTopUltimaSemana = listaProductosSemana;

                // 
                gResponse.Estado = true;
                gResponse.Objeto = vmDashBoard;

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retornamos...
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
