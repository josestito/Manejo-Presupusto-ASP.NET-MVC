using System.ComponentModel.DataAnnotations;
using ManejoPresupuesto.Validaciones;

namespace ManejoPresupuesto.Models
{
    public class Cuenta
    {
        public int id { get; set; }
        [Required(ErrorMessage ="El campo Nombre es requerido")]
        [StringLength(maximumLength: 50)]
        [PrimeraLetraMayusculaAtributte]
        public string Nombre { get; set; }
        [Display(Name ="Tipo Cuenta")]
        public int  TipoCuentaId { get; set; }
        public decimal Balance { get; set; }
        [StringLength(maximumLength: 1000)]
        public string Descripcion { get; set; }
        public string tipoCuenta { get; set; }
    }
}
