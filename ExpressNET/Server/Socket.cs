using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Debouncehouse.ExpressNET.Server
{

    public interface ISocketHost
    {
        void Listen();
        Task ListenTask();

        void Stop();
        Task StopTask();

        void Broadcast(byte[] message);
        void OnDataRecieved(EventHandler<SocketEventArgs> handler);
        void OnClientConnected(EventHandler<SocketEventArgs> handler);
    }

    public class SocketHost : ISocketHost, IDisposable
    {

        #region Public Data

        public IPAddress Host { get; private set; }

        public int Port { get; private set; }

        public List<TcpClient> Clients { get; private set; }

        #endregion

        #region Private Data

        bool isListening = false;

        bool isStopPending = false;

        Queue<byte[]> sendQueue = new Queue<byte[]>();

        BackgroundWorker bgwSend;

        EventHandler<SocketEventArgs> dataRecievedHandlers;

        EventHandler<SocketEventArgs> clientConnectedHandlers;

        #endregion

        #region Constructor

        /// <summary>
        /// Builds a new instance of the [SocketHost] type
        /// : Listens on all interfaces on a random port
        /// </summary>
        public SocketHost() : this(IPAddress.Any, 0) { }

        /// <summary>
        /// Builds a new instance of the [SocketHost] type
        /// : Listens on all interfaces on the supplied port {portnumber}
        /// </summary>
        public SocketHost(int portnumber) : this(IPAddress.Any, portnumber) { }

        /// <summary>
        /// Builds a new instance of the [SocketHost] type
        /// : Listens on the supplied interface {interfaceaddress} on a random available port
        /// </summary>
        public SocketHost(IPAddress interfaceaddress) : this(interfaceaddress, 0) { }

        /// <summary>
        /// Builds a new instance of the [SocketHost] type
        /// : Listens on the supplied interface {interfaceaddress} on the supplied port {portnumber}
        /// </summary>
        public SocketHost(IPAddress interfaceaddress, int portnumber)
        {
            Host = interfaceaddress;
            Port = portnumber;

            Clients = new List<TcpClient>();

            bgwSend = new BackgroundWorker();
            bgwSend.DoWork += processSendQueue;
        }

        #endregion

        #region Private Members

        void processSendQueue(object sender, EventArgs e)
        {
            while (sendQueue.Count() > 0)
            {
                var message = sendQueue.Dequeue();

                for (int i = 0; i < Clients.Count; i++)
                {
                    var client = Clients[i];

                    try
                    {
                        client.GetStream().Write(message, 0, message.Length);
                    }
                    catch (Exception)
                    {
                        Clients.Remove(client);
                    }
                }
            }
        }

        Task absorbClientTask(TcpClient client)
        {
            Clients.Add(client);

            callClientConnected(new SocketEventArgs(new ClientData(client)));

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        var buff = new byte[client.ReceiveBufferSize];
                        int bytesread = 0;
                        string data = "";

                        bytesread = client.GetStream().Read(buff, 0, buff.Length);

                        if (bytesread == 0)
                            break;

                        data = Encoding.Default.GetString(buff, 0, bytesread);

                        callDataRecieved(new SocketEventArgs(new ClientData(client, data)));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
                }

                Clients.Remove(client);
            });
        }

        void callDataRecieved(SocketEventArgs e)
        {
            try
            {
                if (dataRecievedHandlers == null)
                    return;

                dataRecievedHandlers.Invoke(this, e);
            }
            catch (Exception) { }
        }

        void callClientConnected(SocketEventArgs e)
        {
            try
            {
                if (clientConnectedHandlers == null)
                    return;

                clientConnectedHandlers.Invoke(this, e);
            }
            catch (Exception) { }
        }

        void internalBroadcast(byte[] message)
        {
            sendQueue.Enqueue(message);

            if (bgwSend.IsBusy)
                return;

            bgwSend.RunWorkerAsync();
        }

        #endregion

        #region ISocketHost Members

        public void Listen()
        {
            if (isListening)
                throw new InvalidOperationException("Server is already in a listening state");

            var server = new TcpListener(Host, Port);

            try
            {
                server.Start(32);
                
                isListening = true;

                while (isListening && !isStopPending)
                {
                    while (server.Pending())
                    {
                        var client = server.AcceptTcpClient();

                        absorbClientTask(client);
                    }
                     
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }

            isListening = isStopPending = false;
        }

        public Task ListenTask()
        {
            if (isListening)
                throw new InvalidOperationException("Server is already in a listening state");

            return Task.Factory.StartNew(() =>
            {
                Listen();
            });
        }

        public void Stop()
        {
            isStopPending = true;

            while (isListening | isStopPending) { System.Threading.Thread.Sleep(500); }
        }

        public Task StopTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Stop();
            });
        }

        public void Broadcast(byte[] message)
        {
            internalBroadcast(message);
        }

        public void Broadcast(string message)
        {
            if (message.EndsWith("\r\n") == false)
            {
                message = message + "\r\n";
            }

            var msgbytes = Encoding.Default.GetBytes(message);

            internalBroadcast(msgbytes);
        }

        public void OnDataRecieved(EventHandler<SocketEventArgs> handler)
        {
            if(dataRecievedHandlers != null && dataRecievedHandlers.GetInvocationList().Contains(handler))
                throw new InvalidOperationException("Invocation list already contains the supplied delegate");

            dataRecievedHandlers += handler;
        }

        public void OnClientConnected(EventHandler<SocketEventArgs> handler)
        {
            if (clientConnectedHandlers != null && clientConnectedHandlers.GetInvocationList().Contains(handler))
                throw new InvalidOperationException("Invocation list already contains the supplied delegate");

            clientConnectedHandlers += handler;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion

    }

    public class SocketEventArgs : EventArgs
    {

        public ClientData Data { get; private set; }

        public Exception SocketException { get; private set; }

        public SocketEventArgs() : this(null, null) { }

        public SocketEventArgs(ClientData data) : this(data, null) { }

        public SocketEventArgs(Exception ex) : this(null, ex) { }

        public SocketEventArgs(ClientData data, Exception ex)
        {
            Data = data;
            SocketException = ex;
        }

    }

    public class ClientData
    {

        #region Public Data

        public TcpClient Client { get; private set; }

        public string Data { get; private set; }

        #endregion

        #region Constructor

        public ClientData() : this(null, null) { }

        public ClientData(TcpClient client) : this(client, null) { }
        
        public ClientData(string data) : this(null, data) { }

        public ClientData(TcpClient client, string data)
        {
            Client = client;
            Data = data;
        }

        #endregion

    }

}
