using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPReceive
{
    internal class Listener : IDisposable
    {
        private TcpListener _listener;
        private bool isListening = false;
        private bool alive = true;
        Action Callback;
        

        public Listener( Action callback, int port = 12344)
        {
            Callback = callback;
            _listener = new TcpListener(IPAddress.Any, 12344);
            _listener.Start();

            new Thread(async () =>
            {
                while (alive)
                {
                    if(isListening && _listener.Pending())
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        await ProcessClient(client);
                        client.Close();
                        client.Dispose();

                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

            }).Start();

        }

        private async Task ProcessClient(TcpClient client)
        {
            using NetworkStream ns = client.GetStream();
            using StreamReader sr = new StreamReader(ns);
            string? input = "";
            while(input!= null && input?.ToUpper() != "EXIT")
            {
                input = await sr.ReadLineAsync();
                if(input != null)
                {
                    Callback();
                }
            }


        }

        public void Kill()
        {
            alive = false;
        }

        public void Dispose()
        {
            alive = false;
        }

        public void Start() { 
            _listener.Start();
            isListening = true; 
        }
        public void Stop() { 
            _listener?.Stop();
            isListening = false;
        }



    }
}
