﻿using Microsoft.AspNetCore.Mvc;

// 
using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using NuGet.Packaging.Signing;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProductoService _productoServicio;
        public ProductoController(
            IMapper mapper, 
            IProductoService productoServicio
        )
        {
            _mapper = mapper;
            _productoServicio = productoServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            // Generamos una lista tipo VMProducto
            List<VMProducto> vmProductoLista = _mapper.Map<List<VMProducto>>(await _productoServicio.Lista());

            // Retornamos
            return StatusCode(StatusCodes.Status200OK, vmProductoLista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                // Convertimos a modelo VMProducto
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                string nombreImagen = "";
                Stream imagenStream = null;

                // Validamos que llegue algún stream de imagen
                if (imagen != null)
                {
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombre_en_codigo, extension);
                    imagenStream = imagen.OpenReadStream();
                }

                // Generamos mapeo para la creación de producto
                Producto producto_creado = await _productoServicio.Crear(_mapper.Map<Producto>(vmProducto), imagenStream, nombreImagen);

                // Invertimos la creación del modelo producto
                vmProducto = _mapper.Map<VMProducto>(producto_creado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;
            }
            catch (Exception ex)
            {

                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retornamos
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                // Convertimos a modelo VMProducto
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                Stream imagenStream = null;

                // Validamos que llegue algún stream de imagen
                if (imagen != null)
                {
                    imagenStream = imagen.OpenReadStream();
                }

                // Generamos mapeo para la creación de producto
                Producto producto_editado = await _productoServicio.Crear(_mapper.Map<Producto>(vmProducto), imagenStream);

                // Invertimos la creación del modelo producto
                vmProducto = _mapper.Map<VMProducto>(producto_editado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retornamos
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idProducto)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();

            try
            {
                gResponse.Estado = await _productoServicio.Eliminar(idProducto);

            }
             catch(Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retornamos
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

    }
}
