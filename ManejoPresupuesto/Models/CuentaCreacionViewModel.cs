using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    //este objeto esta heredando de cuenta, por lo que tiene su atributo, sumado a los atributos propios de cuenta
    public class CuentaCreacionViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
