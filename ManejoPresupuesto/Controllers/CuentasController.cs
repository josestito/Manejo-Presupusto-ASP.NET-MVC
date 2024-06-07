using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly RepositorioTiposCuentas repositorioTiposCuentas;
        private readonly ServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;

        public CuentasController(RepositorioTiposCuentas repositorioTiposCuentas, ServicioUsuarios servicioUsuarios,
                                IRepositorioCuentas repositorioCuentas,IMapper mapper, IRepositorioTransacciones repositorioTransacciones
                                ,IServicioReportes servicioReportes)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);
            var modelo = cuentasConTipoCuenta.GroupBy(x => x.tipoCuenta).Select(grupo => new IndiceCuentasViewModel
            {
                TipoCuenta = grupo.Key,
                Cuentas = grupo.AsEnumerable()
            }).ToList();
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }
        //metodo post para crear cuentas enlazado al boton del formulario (este metodo recibe una cuenta como argumento).
        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            //obtenemos el id del usuario(en este caso es 2 por como esta definido el metodo)
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            //obtenemos un tipocuenta por el id de la cuenta que llegó como argumento, y el id del usuario que es 2.
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId,usuarioId);
            //validamos que efectivamente venga un tipo cuenta, en caso contrario nos redirige a la vista de error.
            if (tipoCuenta is null)
            {
                RedirectToAction("NoEncontrado", "Home");
            }
            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");


        }
        //este es un metodo para obtener tipos cuentas por id del usuario (esto deberia estar en un servicio XD)
        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId) {

            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        //el parametro id se lo estamos entregando desde la vista Index.cs especificamente en la linea 77 (<a asp-action="Editar" asp-route-id="@cuenta.id" class="btn btn-primary">)
        public async Task<IActionResult> Editar(int id)
        {
            //obtenemos el id del usuario
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            //obtenemos la cuenta por el id que enviamos desde Index.cshtml y el usuario id (en este caso el usuario id es 2 por como esta definido el metodo)
            var cuenta = await repositorioCuentas.ObetenerPorId(id, usuarioId);
            //validamos que venga un objeto cuenta con todos sus atributos, de no ser asi, nos redirige hacia la vista de error.
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //creamos un nuevo objeto de tipo CuentaCreacionViewModel y le damos valores a sus atributos utilizando la cuenta que nos hemos traido anteriormente
            var modelo = new CuentaCreacionViewModel()
            {
                id = cuenta.id,
                Nombre = cuenta.Nombre,
                TipoCuentaId = cuenta.TipoCuentaId,
                Descripcion = cuenta.Descripcion,
                Balance = cuenta.Balance,
                //utilizamos el metodo ObtenerTiposCuentas con el usuarioId para darle valor a este atributo
                TiposCuentas = await ObtenerTiposCuentas(usuarioId)
            };
            //enviamos el objeto hacia la vista con la variable modelo que creamos anteriormente.
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            var cuenta = await repositorioCuentas.ObetenerPorId(cuentaEditar.id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId,usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");


        }
        //metodo get para la vista de borrar cuenta
        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            //obtenemos el usuario ID
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            //Obtenemos las cuentas que le pertenecen al usuario por su id
            var cuenta = await repositorioCuentas.ObetenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(cuenta);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            //obtenemos el usuario ID
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            //Obtenemos las cuentas que le pertenecen al usuario por su id
            var cuenta = await repositorioCuentas.ObetenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detalle(int id,int mes,int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsiarioId();
            var cuenta = await repositorioCuentas.ObetenerPorId(id,usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            


            ViewBag.Cuenta = cuenta.Nombre;
             
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId,id,mes,año,ViewBag);

            return View(modelo);


        }
    }
}

