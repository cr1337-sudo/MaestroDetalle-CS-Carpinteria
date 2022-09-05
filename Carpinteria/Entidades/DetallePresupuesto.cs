using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria
{
    internal class DetallePresupuesto
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }

        public DetallePresupuesto(Producto producto, int cantidad)
        {
            Producto = producto;
            Cantidad = cantidad;
        }

        public double calcularSubtotal()
        {
            return Producto.Precio * Cantidad;
        }
    }
}
