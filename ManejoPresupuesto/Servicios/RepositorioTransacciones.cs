using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks.Dataflow;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string ConnectionString;
        public RepositorioTransacciones()
        {
            ConnectionString = "Server=DESKTOP-HCTNQ2D;Database=ManejoPresupuesto;Integrated Security=True; Encrypt=False";
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(ConnectionString);
            //aca utilizamos un procedimiento de almacenado PROCEDURE
            var id = await connection.QuerySingleAsync<int>(
                                                            "Transacciones_Insertar",
                                                             new
                                                            {
                                                                transaccion.UsuarioId,
                                                                transaccion.FechaTransaccion,
                                                                transaccion.Monto,
                                                                transaccion.CategoriaId,
                                                                transaccion.CuentaId,
                                                                transaccion.Nota
                                                            },
                                                            //aca le indicamos a dapper que estamos usando un procedimiento de almacenado, en este caso llamado "Transacciones_Insertar"
                                                            commandType: CommandType.StoredProcedure);
            transaccion.Id = id;

            /* ESTE ES EL PROCEDIMIENTO DE ALMACENADO PL/SQL QUE SE UTILIZO EN ESTA QUERY
             * 
             * 
           SET ANSI_NULLS ON
           GO
           SET QUOTED_IDENTIFIER ON
           GO

           CREATE PROCEDURE [dbo].[Transacciones_Insertar]
               @UsuarioId int,
               @FechaTransaccion date,
               @Monto decimal(18,2),
               @CategoriaId int,
               @CuentaId int,
               @Nota nvarchar(1000) = NULL
           AS
           BEGIN

               SET NOCOUNT ON;

               INSERT INTO Transacciones(UsuarioId,FechaTransaccion,Monto,CategoriaId,CuentaId,Nota)
               VALUES (@UsuarioId,@FechaTransaccion,ABS(@Monto),@CategoriaId,@CuentaId,@Nota);

               UPDATE Cuentas 
               SET Balance += @Monto
               WHERE Id = @CuentaId;

               SELECT SCOPE_IDENTITY();
           END
           GO*/


        }

        public async Task Actualizar(Transaccion transaccion,decimal montoAnterior,int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                                                                        new 
                                                                        {
                                                                            transaccion.Id,
                                                                            transaccion.FechaTransaccion,
                                                                            transaccion.Monto,
                                                                            transaccion.CategoriaId,
                                                                            transaccion.CuentaId,
                                                                            transaccion.Nota,
                                                                            montoAnterior,
                                                                            cuentaAnteriorId
                                                                        },commandType: System.Data.CommandType.StoredProcedure);
    /*ALTER PROCEDURE [dbo].[Transacciones_Actualizar]
    @Id int,
    @FechaTransaccion datetime,
    @Monto decimal(18,2),
    @MontoAnterior decimal (18,2),
    @CuentaId int,
    @CuentaAnteriorId int,
    @CategoriaId int,
    @nota nvarchar(1000) = Null
    AS
    BEGIN
        --REVERTIR TRANSACCION ANTERIOR
        UPDATE Cuentas
        SET Balance -= @MontoAnterior
        WHERE Id = @CuentaAnteriorId;
        --REALIZA LA SIGUIENTE TRANSACCION
        UPDATE Cuentas
        SET Balance += @Monto
        WHERE Id = @CuentaId;

        UPDATE  Transacciones
        SET Monto = ABS(@Monto),FechaTransaccion = @FechaTransaccion,
        CategoriaId = @CategoriaId,CuentaId = @CuentaId, Nota = @nota
        WHERE Id = @Id;

    END*/
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);

            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT Transacciones.*, cat.TipoOperacionId
                                                                            FROM Transacciones
                                                                            INNER JOIN Categorias cat
                                                                            ON cat.Id = Transacciones.CategoriaId
                                                                            WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId;", new {id,usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync("Transacciones_Borrar", new { id },commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo) 
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id,t.Monto,t.FechaTransaccion,c.Nombre as Categoria,
                                                                    cu.Nombre as Cuenta,c.TipoOperacionId
                                                                    FROM Transacciones t
                                                                    INNER JOIN Categorias c
                                                                    ON C.Id = t.CategoriaId
                                                                    INNER JOIN Cuentas cu
                                                                    ON cu.Id = t.CuentaId
                                                                    WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
                                                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;",modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id,t.Monto,t.FechaTransaccion,c.Nombre as Categoria,
                                                                    cu.Nombre as Cuenta,c.TipoOperacionId
                                                                    FROM Transacciones t
                                                                    INNER JOIN Categorias c
                                                                    ON C.Id = t.CategoriaId
                                                                    INNER JOIN Cuentas cu
                                                                    ON cu.Id = t.CuentaId
                                                                    WHERE t.UsuarioId = @UsuarioId
                                                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin ORDER BY t.fechaTransaccion DESC;", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"SELECT DATEDIFF(d,@FechaInicio,FechaTransaccion) /7 + 1 Semana,
                                                                            SUM(Monto) as Monto,cat.TipoOperacionId
                                                                            FROM Transacciones
                                                                            INNER JOIN Categorias cat
                                                                            on cat.Id = Transacciones.CategoriaId
                                                                            WHERE Transacciones.UsuarioId = @usuarioId AND
                                                                            FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                                                            GROUP BY DATEDIFF(d,@FechaInicio,FechaTransaccion)/7 , cat.TipoOperacionId;",modelo);
        }
        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"SELECT MONTH(FechaTransaccion) AS Mes, SUM(Monto) AS Monto , cat.TipoOperacionId
                                                                        FROM Transacciones
                                                                        INNER JOIN Categorias cat
                                                                        ON cat.Id = Transacciones.CategoriaId
                                                                        WHERE Transacciones.UsuarioId = @UsuarioId AND YEAR(FechaTransaccion) = @Año
                                                                        GROUP BY MONTH(FechaTransaccion),cat.TipoOperacionId;", new {usuarioId,año});
        }
    }
}
