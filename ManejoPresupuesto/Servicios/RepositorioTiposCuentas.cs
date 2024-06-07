using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
    
{
    
    public class RepositorioTiposCuentas
    {
        

       
        //METODO ASINCRONO
        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
                                                            new {usuarioId = tipoCuenta.UsuarioId,
                                                            nombre = tipoCuenta.Nombre},
                                                            commandType: System.Data.CommandType.StoredProcedure);

            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;", new {nombre, usuarioId});
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id,Nombre,Orden from TiposCuentas WHERE UsuarioId = @usuarioId ORDER BY Orden;", new { usuarioId});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            await connection.ExecuteAsync(@"UPDATE TiposCuentas set Nombre = @Nombre where Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Id,Nombre,Orden  FROM TiposCuentas WHERE Id = @Id and UsuarioId = @UsuarioId", new { Id, UsuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            await connection.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new {id});
        }
        public async Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados) 
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden Where Id = @Id;";
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            await connection.ExecuteAsync(query, tiposCuentasOrdenados);
            
        }
    }
}
