using Microsoft.AspNetCore.Mvc;

// 
using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

// 
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly ITipoDocumentoVentaService _tipoDocumentoVentaServicio;
        private readonly IVentaService _ventaServicio;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;
        public VentaController(
            ITipoDocumentoVentaService documentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
            IConverter converter
        )
        {
            _tipoDocumentoVentaServicio = documentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
        }

        public IActionResult NuevaVenta()
        {
            return View();
        }
        
        public IActionResult HistorialVenta()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {
            List<VMTipoDocumentoVenta> vmTipoDocumentos = _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());
            
            // Retornamos...
            return StatusCode(StatusCodes.Status200OK, vmTipoDocumentos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            // 
            List<VMProducto> vmListaProductos = _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));

            // Retornamos...
            return StatusCode(StatusCodes.Status200OK, vmListaProductos);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VMVenta modelo)
        {
            // 
            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();

            try
            {
                modelo.IdUsuario = 1;

                Venta venta_creada = await _ventaServicio.Registrar(_mapper.Map<Venta>(modelo));

                // 
                modelo = _mapper.Map<VMVenta>(venta_creada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;

            }
            // Retornamos...
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            // 
            List<VMVenta> vmHistorialVeta = _mapper.Map<List<VMVenta>>(await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));

            // Retornamos...
            return StatusCode(StatusCodes.Status200OK, vmHistorialVeta);
        }

        public IActionResult MostrarPDFVenta(string numeroVenta)
        {
            // Ruta de la plantilla donde existe el PDF
            string urlPlantillaVista = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";

            // Generamos configuración para la creación del PDF
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = {
                    new ObjectSettings()
                    {
                        Page = urlPlantillaVista
                    }
                }
            };

            // Convertimos a PDF
            var archivoPDF = _converter.Convert(pdf);

            // Retornamos file
            return File(archivoPDF, "application/pdf");
        }
    }
}
