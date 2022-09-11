using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace Carpinteria
{
    internal class Presupuesto
    {
        public int PresupuestoNro { get; set; }
        public DateTime Fecha { get; set; }
        public string Client { get; set; }
        public double Total { get; set; }
        public double CostoMO { get; set; }
        public double Descuento { get; set; }
        public DateTime FechaBaja { get; set; }
        public List<DetallePresupuesto> Detalles { get; set; }

        public Presupuesto()
        {
            Detalles = new List<DetallePresupuesto>();
                
        }

        public void AgregarDetalle( DetallePresupuesto detalle)
        {
            Detalles.Add(detalle);
        }

        public void QuitarDetalle( int indice)
        {
            Detalles.RemoveAt(indice);
        }

        public double CalcularTotal()
        {
            double total = 0;
            foreach (DetallePresupuesto item in Detalles)
            {
                total += item.calcularSubtotal();
            }
            return total;
        }

        public bool Confirmar()
            
        {
                SqlConnection conexion = new SqlConnection();
            SqlTransaction transaccion = null;
            try {
                conexion.ConnectionString = @"Data Source=DESKTOP-N9BC4PL\SQLEXPRESS;Initial Catalog=carpinteria_db;Integrated Security=True";
                conexion.Open();
                //EMPIEZA LA TRANSACCIÓN
                transaccion = conexion.BeginTransaction();
                SqlCommand comando = new SqlCommand ();
                comando.Connection = conexion;
                //Asignar la transacción al comando
                comando.Transaction = transaccion;
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = "SP_INSERTAR_MAESTRO";
                //Parametros de entreda
                comando.Parameters.AddWithValue("@cliente", this.Client);
                comando.Parameters.AddWithValue("@dto", this.Descuento);
                comando.Parameters.AddWithValue("@total", this.Total);
                //Parametro de salida
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@presupuesto_nro";
                param.SqlDbType = SqlDbType.Int;
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                comando.ExecuteNonQuery();

                this.PresupuestoNro = Convert.ToInt32(param.Value);

                int detalleNro = 1;

                foreach (DetallePresupuesto item in Detalles)
                {
                    SqlCommand comandoDetalle = new SqlCommand();
                    //La creación de detalles también va a depender de la misma transacción
                    comandoDetalle.Connection = conexion;
                    comandoDetalle.Transaction = transaccion;
                    comandoDetalle.CommandType = CommandType.StoredProcedure;
                    comandoDetalle.CommandText = "SP_INSERTAR_DETALLE";
                    comandoDetalle.Parameters.AddWithValue("@presupuesto_nro", this.PresupuestoNro);
                    comandoDetalle.Parameters.AddWithValue("@detalle", detalleNro);
                    comandoDetalle.Parameters.AddWithValue("@id_producto", item.Producto.ProductoNro);
                    comandoDetalle.Parameters.AddWithValue("@cantidad", item.Cantidad);
                    comandoDetalle.ExecuteNonQuery();
                    detalleNro++;
                }

                transaccion.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                transaccion.Rollback();
                return false;
            }
            finally
            {
                if(conexion.State == ConnectionState.Open)
                {
                    conexion.Close();
                }
            }
        }
    }
}
