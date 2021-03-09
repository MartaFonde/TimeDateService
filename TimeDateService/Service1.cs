using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TimeDateService
{
    public partial class TimeDateService : ServiceBase
    {
        string nombreArchivo = "portDateTimeServer.txt";
        int PORT = 135;
        int port;
        bool connected = true;

        public TimeDateService()
        {
            InitializeComponent();
        }
        static void escribeEvento(string mensaje)
        {
            string nombre = "TimeDateService";
            string logDestino = "Application";
            if (!EventLog.SourceExists(nombre))
            {
                EventLog.CreateEventSource(nombre, logDestino);
            }
            EventLog.WriteEntry(nombre, mensaje);
        }

        protected override void OnStart(string[] args)
        {
            escribeEvento("DateTimeService START");
            bool puertoFichero = leerFicheroPuerto();
            bool rep = true;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, port);
            while (rep)
            {
                try
                {
                    rep = false;                    
                    s.Bind(ie);
                    escribeEvento("Port "+ie.Port);
                    s.Listen(5);
                }
                catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                {
                    if (puertoFichero)
                    {
                        port = PORT;
                        puertoFichero = false;
                        rep = true;
                    }
                    else
                    {
                        //rep = false;                        
                        connected = false;
                    }
                }
                catch (SocketException)
                {
                    connected = false;
                }
            }
            
            if (connected)
            {
                Server server = new Server(s);
            }
            else
            {
                escribeEvento($"Error: ports {port} & {PORT} in use");
                //OnStop();
            }
        }

        protected bool leerFicheroPuerto()
        {
            try
            {
                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("PROGRAMDATA")
                    + "\\" + nombreArchivo))
                {
                    string line = sr.ReadLine();                    
                    if (line != null && Int32.TryParse(line, out port))
                    {
                        return true;
                    }
                }
            }
            catch (IOException e)
            {
                escribeEvento("Error read file: "+e.Message);
            }
            port = PORT;    //se dá fallo de lectura -- non return true ---, port por defecto
            return false;
        }

        protected override void OnStop()
        {
            escribeEvento("TimeDate Service stoped");
        }        
    }
}
