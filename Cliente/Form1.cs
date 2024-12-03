// ************************************************************************ 
// Practica 07 
// Jean Poll Cardoso Chririboga
// Fecha de realización: 02/12/2024
// Fecha de entrega: 04/12/2024 
//-----------------------------------------------
// Resultados: 
// * Este Forms permite al usuario ingresar un usuario y contraseña para acceder a un sistema de
// validación de placas de vehículos. Utiliza una arquitecura cliente-servidor para la comunicación
// entre el cliente y el servidor. El servidor recibe los datos del cliente, los procesa y envía una
// respuesta al cliente.
// 
//----------------------------------------------
// Conclusiones: 
// * Jean Poll Cardoso
// - La integración de la clase Protocolos en el cliente centraliza la lógica de comunicación con el servidor,
// reduciendo la duplicación de código y promoviendo una arquitectura más limpia y modular.  
// - El uso de métodos como HazOperacion mejora la escalabilidad del sistema, ya que permite manejar
// nuevas operaciones o comandos de manera más sencilla y organizada.

//---------------------------------------------
// Recomendaciones: 
// * Jean Poll Cardoso
// - Se recomienda implementar validaciones más robustas en los datos ingresados por el usuario antes de enviarlos al
// servidor, para evitar posibles errores de formato o datos no válidos.
// 
// - Es importante manejar adecuadamente las excepciones en el cliente para garantizar que los errores en la comunicación
// con el servidor no afecten la experiencia del usuario y se notifiquen de manera clara.
// 
// ************************************************************************

using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        private TcpClient remoto;
        private NetworkStream flujo;
        private Protocolos protocolo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
                protocolo = new Protocolos(flujo);
                panPlaca.Enabled = false;

            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión: " + ex.Message, "ERROR");
            }
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña))
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA");
                return;
            }

            try
            {
                var respuesta = protocolo.HazOperacion("INGRESO", new[] { usuario, contraseña });

                if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
                {
                    panPlaca.Enabled = true;
                    panLogin.Enabled = false;
                    txtModelo.Focus();

                    MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                }
                else
                {
                    MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "ERROR");
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                var respuesta = protocolo.HazOperacion("CALCULO", new[] { txtModelo.Text, txtMarca.Text, txtPlaca.Text });
                MessageBox.Show("Respuesta recibida: " + respuesta.Mensaje, "INFORMACIÓN");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "ERROR");
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            try
            {
                var respuesta = protocolo.HazOperacion("CONTADOR", new string[0]);
                MessageBox.Show($"Número de consultas: {respuesta.Mensaje}", "INFORMACIÓN");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "ERROR");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            flujo?.Close();
            remoto?.Close();
        }
    }
}
