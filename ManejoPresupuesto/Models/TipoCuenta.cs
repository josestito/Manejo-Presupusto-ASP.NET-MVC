using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        /*ACA SE AGREGAN LAS VALIDACIONES*/
        public int Id { get; set; }
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [StringLength(maximumLength:50, MinimumLength =3, ErrorMessage = "La longitud del campo {0} debe ser entre {2} y {1}")]
        [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas")]
        public String Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }
    }
}
