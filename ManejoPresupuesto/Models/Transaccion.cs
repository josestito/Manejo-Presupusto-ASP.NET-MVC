using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha Transaccion")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Categoria")]
        [Display(Name ="Categorias")]
        public int CategoriaId { get; set; }
        [StringLength(maximumLength:1000,ErrorMessage ="la nota no puede pasar de 1000 caracteres")]
        public string Nota { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Cuenta")]
        [Display(Name = "Cuentas")]
        public int CuentaId { get; set; }
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;
        public string Cuenta { get; set; }
        public string Categoria { get; set; }
    }
}
