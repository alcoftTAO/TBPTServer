using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace TBPTServer
{
    public static class Server
    {
        private static Socket ServerSocket;
        public readonly static List<Socket> Clients = new List<Socket>();
        public static string PackagesFolder = "TBPT/";
        private static byte[] receiveBuffer = new byte[512];

        public static void Start(IPEndPoint ep, int maxConnections)
        {
            if (ServerSocket != null)
            {
                ServerSocket.Close();
                ServerSocket.Dispose();

                ServerSocket = null;
            }

            if (!Directory.Exists(PackagesFolder))
            {
                Directory.CreateDirectory(PackagesFolder);
            }

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(maxConnections);

            ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), null);
        }

        public static TPackage? SearchPackage(string name)
        {
            List<TPackage> Packages = new List<TPackage>();
            DirectoryInfo dirInfo = new DirectoryInfo(PackagesFolder);

            for (int i = 0; i < dirInfo.GetFiles().Length; i++)
            {
                Packages.Add(JsonConvert.DeserializeObject<TPackage>(File.ReadAllText(dirInfo.GetFiles()[i].FullName)));

                if (Packages[i].Name == name)
                {
                    return Packages[i];
                }
            }

            return null;
        }

        private static void AcceptConnection(IAsyncResult ar)
        {
            Socket socket = ServerSocket.EndAccept(ar);

            Clients.Add(socket);

            socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), socket);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), null);
        }

        private static void ReceiveData(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int received = socket.EndReceive(ar);
            byte[] dataBuffer = new byte[received];
            Array.Copy(receiveBuffer, dataBuffer, received);
            string receivedText = Encoding.ASCII.GetString(dataBuffer);

            if (receivedText.StartsWith("GET_PACKAGE "))
            {
                TPackage? package = SearchPackage(receivedText.Substring("GET_PACKAGE ".Length));

                if (package != null)
                {
                    socket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(package.Value)));
                }
                else
                {
                    socket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new TPackage()
                    {
                        Name = "TNULL",
                        Version = "0.0.0",
                        Data = null
                    })));
                }
            }
        }
    }

    public struct TPackage
    {
        public string Name;
        public string Version;
        public string Repository;
        public byte[] Data;
    }
}
