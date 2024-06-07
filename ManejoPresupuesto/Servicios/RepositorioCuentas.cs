using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task<Cuenta> ObetenerPorId(int id, int usuarioId);
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
    }
    public class RepositorioCuentas : IRepositorioCuentas
    {
        public RepositorioCuentas()
        {
            
        }

        public async Task Crear(Cuenta cuenta) 
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas (Nombre,TipoCuentaId,Descripcion,Balance) 
                                                        VALUES (@Nombre,@TipoCuentaId,@Descripcion,@Balance);
                                                        SELECT SCOPE_IDENTITY();",cuenta);
            cuenta.id = id;
        
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId) 
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            return await connection.QueryAsync<Cuenta>(@"
                                                        SELECT Cuentas.Id,Cuentas.Nombre, Balance,tc.Nombre as TipoCuenta 
                                                        FROM Cuentas
                                                        INNER JOIN TiposCuentas tc
                                                        ON tc.Id = Cuentas.TipoCuentaId
                                                        WHERE tc.UsuarioId = @UsuarioId
                                                        ORDER BY tc.Orden;",new {usuarioId});

        }

        public async Task<Cuenta> ObetenerPorId(int id,int usuarioId) 
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");

            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"SELECT Cuentas.Id,Cuentas.Nombre, Balance,Descripcion, TipoCuentaId
                                                                        FROM Cuentas
                                                                        INNER JOIN TiposCuentas tc
                                                                        ON tc.Id = Cuentas.TipoCuentaId
                                                                        WHERE tc.UsuarioId = @UsuarioId AND Cuentas.Id = @Id", new {id,usuarioId} );
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta) 
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            await connection.ExecuteAsync(@"UPDATE Cuentas 
                                            SET Nombre = @nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
                                            WHERE Id = @Id;",cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection("Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False");
            await connection.ExecuteAsync(@"DELETE Cuentas WHERE id = @id", new {id});
        }
    }
}
