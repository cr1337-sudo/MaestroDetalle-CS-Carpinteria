using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace Carpinteria.Formularios
{
    public partial class FrmNuevoPresupuesto : Form
    {
        Presupuesto nuevoPresupuesto = new Presupuesto();
        public FrmNuevoPresupuesto()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void calcularTotales()
        {

            txtSubTotal.Text = nuevoPresupuesto.CalcularTotal().ToString();
            double desc = nuevoPresupuesto.CalcularTotal() * Convert.ToDouble(txtDescuento.Text) / 100;
            txtTotal.Text = (nuevoPresupuesto.CalcularTotal() - desc).ToString();
        }

        private void dgwDetalles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgwDetalles.CurrentCell.ColumnIndex == 4) {
                nuevoPresupuesto.QuitarDetalle(dgwDetalles.CurrentRow.Index);
                dgwDetalles.Rows.Remove(dgwDetalles.CurrentRow);
                calcularTotales();
            }
        }
            
        private void FrmNuevoPresupuesto_Load(object sender, EventArgs e)
        {
            lblPresupuesto.Text += ProximoPresupuesto();
            CargarProductos();
            txtCantidad.Text = "0";
            txtCliente.Text = "Consumidor Final";
            txtFecha.Text = DateTime.Now.Date.ToString("dd/MM/yyyy");
            txtTotal.Text = "0";
        }

        private int ProximoPresupuesto()
        {
            SqlConnection conexion = new SqlConnection();
            conexion.ConnectionString = @"Data Source=DESKTOP-N9BC4PL\SQLEXPRESS;Initial Catalog=carpinteria_db;Integrated Security=True";
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection = conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = "SP_PROXIMO_ID";
            SqlParameter param = new SqlParameter("@next", SqlDbType.Int);
            //Al ser un parámetro de salida hay que especificarlo
            param.Direction = ParameterDirection.Output;
            //Hay que asignar el parámetro al comando
            comando.Parameters.Add(param);
            //Luego de ejecutar la query, los datos quedan almacenados en el param (ya que es de salida)
            comando.ExecuteNonQuery();
            //Ciera conexion una vez terminadas las operaciones
            conexion.Close();
            //Asignar el número de presupuesto al label con el dato almacenado en param
            return (int)param.Value;
        }

        private void CargarProductos()
        {
            SqlConnection conexion = new SqlConnection();
            conexion.ConnectionString = @"Data Source=DESKTOP-N9BC4PL\SQLEXPRESS;Initial Catalog=carpinteria_db;Integrated Security=True";
            conexion.Open();
            SqlCommand comando = new SqlCommand();
            comando.Connection = conexion;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = "SP_CONSULTAR_PRODUCTOS";
            DataTable table = new DataTable();
            table.Load(comando.ExecuteReader());
            conexion.Close();

            cboProductos.DataSource = table;
            cboProductos.DisplayMember = "n_producto";
            cboProductos.ValueMember = "id_producto";
            cboProductos.SelectedIndex = -1;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (cboProductos.Text.Equals(string.Empty))
            {
                MessageBox.Show("Debe seleccionar un producto...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtCantidad.Text) || !int.TryParse(txtCantidad.Text, out _)) {
                MessageBox.Show("Debe ingresar una cantidad válida...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
            };

            foreach (DataGridViewRow row in dgwDetalles.Rows)
            {
                if (row.Cells["colProd"].Value.ToString().Equals(cboProductos.Text))
                {
                    MessageBox.Show("Este producto ya está presupuestado", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            DataRowView item = (DataRowView)cboProductos.SelectedItem;

            int prod = Convert.ToInt32(item.Row.ItemArray[0]);
            string nom = item.Row.ItemArray[1].ToString();
            double precio = Convert.ToDouble(item.Row.ItemArray[2]);
            Producto p = new Producto(prod, nom, precio);

            int cant = Convert.ToInt32(txtCantidad.Text);
            DetallePresupuesto detallePresupuesto = new DetallePresupuesto(p, cant);
            nuevoPresupuesto.AgregarDetalle(detallePresupuesto);
            dgwDetalles.Rows.Add(new object[] { prod, nom, precio, cant });
            calcularTotales();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCliente.Text))
            {
                MessageBox.Show("Debe ingresar un cliente válido...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtCliente.Focus();
                return;
            }
            if(dgwDetalles.Rows.Count == 0)
            {
                MessageBox.Show("Debe ingresar al menos un detalle...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //Grabas maestro y detalle
            GuardarPresupuesto();
        }

        private void GuardarPresupuesto()
        {
            nuevoPresupuesto.Fecha = Convert.ToDateTime(txtFecha.Text);
            nuevoPresupuesto.Client = txtCliente.Text;
            nuevoPresupuesto.Descuento = Convert.ToDouble(txtDescuento.Text);
            //   nuevoPresupuesto.AgregarDetalle(detallePresupuesto); Ya está agregado
            nuevoPresupuesto.Total = Convert.ToDouble(txtTotal.Text);

            if (nuevoPresupuesto.Confirmar())
            {
                MessageBox.Show("El presupuesto se grabó correctamente", "Notificación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Ciera el formulario y libera los recursos
                this.Dispose();
            }
            else
            {
                MessageBox.Show("El presupuesto  no se pudo grabar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 
            }

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
