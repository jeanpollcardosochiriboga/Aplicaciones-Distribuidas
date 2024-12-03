// ************************************************************************ 
// Practica 07 
// Jean Poll Cardoso Chririboga
// Fecha de realización: 02/12/2024
// Fecha de entrega: 04/12/2024 
//-----------------------------------------------
// Resultados: 
// * El codigo del servidor permite la comunicación con el cliente a través de un protocolo de comunicación
// que permite enviar y recibir mensajes de manera ordenada y eficiente. El servidor recibe los datos del cliente,
// los procesa y envía una respuesta al cliente.
// 
//----------------------------------------------
// Conclusiones: 
// * Jean Poll Cardoso
// - La incorporación de la clase Protocolos en el servidor simplifica la implementación de nuevos comandos o solicitudes,
// ya que toda la lógica de negocio está centralizada y bien encapsulada.
//  
// - La separación de responsabilidades entre la gestión de clientes y el procesamiento de pedidos mejora la
// escalabilidad del sistema, facilitando futuras modificaciones y extensiones.

//---------------------------------------------
// Recomendaciones: 
// * Jean Poll Cardoso
// - Se recomienda optimizar el manejo de excepciones en el servidor para registrar de manera detallada
// los errores ocurridos y así facilitar la resolución de problemas en tiempo de ejecución.
// 
// - Es importante incluir un sistema de autenticación más sólido y proteger los datos enviados
// mediante protocolos seguros (por ejemplo, TLS) para evitar posibles vulnerabilidades en la
// comunicación entre cliente y servidor.
// 
// ************************************************************************
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor iniciado en el puerto 8080...");

                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado desde: " + cliente.Client.RemoteEndPoint);
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket: " + ex.Message);
            }
            finally
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream();
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();

                    Respuesta respuesta = Protocolos.ResolverPedido(mensaje, direccionCliente, ref listadoClientes);
                    Console.WriteLine($"Pedido: {mensaje} | Respuesta: {respuesta}");

                    byte[] bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }
    }
}