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
using System.Threading;
using System.Threading.Tasks;

namespace TimeDateService
{
    class Server
    {
        Socket s;
        bool connected = true;

        public Server(Socket socket)
        {
            s = socket;
            Thread t = new Thread(threadServer);
            t.IsBackground = true;
            t.Start();
        }

        protected void threadServer()
        {
            string msg = null;
            while (connected)
            {
                Socket sClient = s.Accept();
                IPEndPoint ieClient = (IPEndPoint)sClient.RemoteEndPoint;
                Console.WriteLine("Client connected: {0} at port {1}", ieClient.Address, ieClient.Port);

                try
                {
                    using (NetworkStream ns = new NetworkStream(sClient))
                    using (StreamReader sr = new StreamReader(ns))
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        msg = sr.ReadLine();
                        Console.WriteLine(msg);
                        if (msg != null)
                        {
                            string a = actions(msg);
                            Console.WriteLine(a);
                            sw.WriteLine(a);
                            sw.Flush();
                        }
                    }
                }                
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    connected = false;
                }
                //Console.WriteLine("Client disconnected");                
                //sClient.Close();
            }
        }

        protected string actions(string msg)
        {
            switch (msg)
            {
                case "DATE":
                    return DateTime.Now.Date.ToString("dd/MM/yyyy");
                case "TIME":
                    return DateTime.Now.ToString("HH:mm");
                case "DATETIME":
                    return DateTime.Now.ToString();
                case "CLOSE":
                    connected = false;
                    return "CLOSE";
                default:
                    return "COMMAND NOT VALID";
            }
        }
    }
}
