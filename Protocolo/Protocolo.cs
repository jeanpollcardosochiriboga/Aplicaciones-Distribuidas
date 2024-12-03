// ************************************************************************ 
// Practica 07 
// Jean Poll Cardoso Chririboga
// Fecha de realización: 02/12/2024
// Fecha de entrega: 04/12/2024 
//-----------------------------------------------
// Resultados: 
// * El código muestra la implementación de un protocolo de comunicación entre un cliente y un servidor,
// donde el cliente envía un mensaje al servidor y este responde con un mensaje de acuerdo al comando recibido.
// 
//----------------------------------------------
// Conclusiones: 
// * Jean Poll Cardoso
// - Se puede concluir que el protocolo de comunicación es una herramienta muy útil para la comunicación entre
// un cliente y un servidor, ya que permite establecer una comunicación de manera ordenada y eficiente.
// - El protocolo de comunicación permite establecer un conjunto de reglas y procedimientos que deben seguirse
// para que la comunicación sea exitosa.  
//---------------------------------------------
// Recomendaciones: 
// * Jean Poll Cardoso
// - Es importante tener en cuenta que el protocolo de comunicación debe ser claro y preciso, de manera que
// ambas partes puedan entenderse y comunicarse de manera efectiva.
// - Es recomendable establecer un conjunto de reglas y procedimientos claros para la comunicación, de manera
// que se eviten malentendidos y errores en la comunicación.
// 
// ************************************************************************

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Protocolo
{
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    public class Protocolos
    {
        private NetworkStream flujo;

        public Protocolos(NetworkStream flujo)
        {
            this.flujo = flujo;
        }

        public Respuesta HazOperacion(string comando, string[] parametros)
        {
            if (flujo == null)
                throw new InvalidOperationException("No hay conexión establecida.");

            try
            {
                // Crear y enviar pedido
                var pedido = new Pedido { Comando = comando, Parametros = parametros };
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Recibir respuesta
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                var partes = mensaje.Split(' ');
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException($"Error al intentar transmitir: {ex.Message}", ex);
            }
        }

        public static Respuesta ResolverPedido(string mensaje, string direccionCliente, ref Dictionary<string, int> listadoClientes)
        {
            Pedido pedido = Pedido.Procesar(mensaje);
            Respuesta respuesta = new Respuesta { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta { Estado = "OK", Mensaje = "ACCESO_CONCEDIDO" }
                            : new Respuesta { Estado = "NOK", Mensaje = "ACCESO_NEGADO" };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string placa = pedido.Parametros[2];
                        if (Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$"))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            { Estado = "OK", Mensaje = $"{placa} {indicadorDia}" };

                            if (!listadoClientes.ContainsKey(direccionCliente))
                                listadoClientes[direccionCliente] = 0;

                            listadoClientes[direccionCliente]++;
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    respuesta = listadoClientes.ContainsKey(direccionCliente)
                        ? new Respuesta
                        { Estado = "OK", Mensaje = listadoClientes[direccionCliente].ToString() }
                        : new Respuesta { Estado = "NOK", Mensaje = "No hay solicitudes previas" };
                    break;
            }

            return respuesta;
        }

        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }
    }
}
