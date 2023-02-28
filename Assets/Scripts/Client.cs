using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using ThreeDNet.Engine;
using UnityEngine;

namespace ThreeDNet.Client
{
    public class Client
    {
        private static Client instance;

    
        public static Client getInstance()
        {
            if (instance == null)
                instance = new Client();
            return instance;
        }

        Engine.World world;
        //public IPAddress Localhost = IPAddress.Parse("127.0.0.1");

        public struct Response
        {
            public Response(byte[] data, Dictionary<string, string> headers, int code)
            {
                this._data = data;
                this.Headers = new ReadOnlyDictionary<string, string>(headers);
                this.Code = code;
            }

            public ReadOnlyDictionary<string, string> Headers;

            private byte[] _data;

            public string Text { get { return Encoding.UTF8.GetString(this._data); } }

            public byte[] Raw { get { return this._data; } }

            public int Code { get; }
        } 

        private Client()
        {
            Thread myThread = new Thread(new ThreadStart(PollServers));
            myThread.Start(); // запускаем поток

            this.world = Engine.World.getInstance();
        }

        public void AddSceneToLoad(IPAddress ip)
        {
            lock(serversQueueToPoll)
            {
                serversQueueToPoll.Enqueue(ip);
            }
        }

        public void LoadRecievedScenes()
        {
            SceneFromPoll sceneData;
            lock (serversQueueFromPoll)
                if (serversQueueFromPoll.Count > 0)
                    sceneData = serversQueueFromPoll.Dequeue();
                else
                    return;
            
            Utils.LogWrite(sceneData.data);

            //world.AddScene(Utils.AddrToDim(sceneData.ip), 
            //               Parser.CreateSceneFromJson(Utils.AddrToDim(sceneData.ip), sceneData.data));
        }

        private Queue<IPAddress> serversQueueToPoll = new Queue<IPAddress>();
        struct SceneFromPoll
        {
            public IPAddress ip;
            public string data;
        }
        private Queue<SceneFromPoll> serversQueueFromPoll = new Queue<SceneFromPoll>();
        void PollServers()
        {
            IPAddress server;

            while(true){
                while (true){
                    Thread.Sleep(500);
                    lock (serversQueueToPoll){
                        if (serversQueueToPoll.Count > 0){
                            server = serversQueueToPoll.Dequeue();
                            break;
                        }
                    }
                }
                //Debug.Log(server);
                //Ping ping = new Ping(server);
                //DateTime oldTime = new DateTime();
                //oldTime = DateTime.Now;
                //while(true){
                //    Thread.Sleep(500);
                //    if(((DateTime.Now - oldTime).Seconds >= 3) || ping.isDone){
                //        break;
                //    }
                //}
                //if (!ping.isDone)
                //continue;

                // IAsyncResult result = socket.BeginConnect(server, 53534, null, null);

                // bool success = result.AsyncWaitHandle.WaitOne(5000, true);

                // if (socket.Connected)
                // {
                //     socket.EndConnect(result);
                // }
                // else
                // {
                //     // NOTE, MUST CLOSE THE SOCKET
                //     Utils.LogWrite("Server "+server.ToString()+" not responding");
                //     socket.Disconnect(true);

                //     //Debug.Log(server + " not connected");
                //     continue;
                //     //Console.WriteLine("Exception: " + ex.ToString());
                // }

                // //Debug.Log("Huy1");
                // //NetworkStream stream = socket.GetStream();
                // byte[] sendBuffer = Encoding.UTF8.GetBytes(String.Format("GET {0} 3DNETP/1.0", server));
                // //stream.Write(buffer, 0, buffer.Length);
                // socket.Send(sendBuffer);
                // //DateTime oldTime = new DateTime();
                // //oldTime = DateTime.Now;
                // //while (true){
                // //    Thread.Sleep(500);
                // //    if (((DateTime.Now - oldTime).Seconds >= 3) || stream.DataAvailable){
                // //        break;
                // //    }
                // //}

                // //Debug.Log("Huy2");
                // int bytesRead = 0;
                // string responseData = "";
                // byte[] receiveBuffer = new byte[256];
                // do{
                //     bytesRead = socket.Receive(receiveBuffer);
                //     //Debug.Log(bytesRead);
                //     responseData = responseData + Encoding.UTF8.GetString(receiveBuffer, 0 , bytesRead);
                //     //Debug.Log(responseData);
                // } while (bytesRead >= 256);
                // socket.Shutdown(SocketShutdown.Both);
                // socket.Disconnect(true);

                Utils.LogWrite("Now load: "+server.ToString());
                
                Response resp = Client.Send("3dnet://" + server.ToString() + ":53534/main.3dml");
                if (resp.Code != 200)
                {
                    Utils.LogWrite("Scene "+server.ToString()+" not responding");
                    continue;
                }

                string responseData = resp.Text;

                // lock (serversQueueFromPoll)
                // {
                //     serversQueueFromPoll.Enqueue(sceneData);
                // }
                Parser.CreateSceneFromJson(Utils.AddrToDim(server), responseData);
            }
        }

        public static string RestoreUri(string server, string uri)
        {
            Regex regex = new Regex(@"^([^:\/?#]+):?\/\/([^\/?#:]*)?:?(\d{0,5})(\/[^?#]*)?([^#]*)?#?(.*)?");
            Match match = regex.Match(uri);
            if (match.Success)
            {
                return uri;
            }
            else
            {
                if (uri.StartsWith("./"))
                    uri = String.Format("3dnet://{0}/{1}", server, uri.Substring(2, uri.Length-2));
                else if (uri.StartsWith("/"))
                    uri = String.Format("3dnet://{0}{0}", server, uri);
                else
                    uri = String.Format("3dnet://{0}/{1}", server, uri);
            }
            return uri;
        }

        public static Response Send(string uri)
        {
            Regex regex = new Regex(@"^([^:\/?#]+):?\/\/([^\/?#:]*)?:?(\d{0,5})(\/[^?#]*)??([^#]*)?#?(.*)?");
            Match match = regex.Match(uri);
            if (!match.Success)
            {
                Utils.LogWrite("Not match");
                return new Response(new byte[0], new Dictionary<string, string>{}, 0);
            }

            Utils.LogWrite(match.Groups[0].Value);
            IPAddress server = IPAddress.Parse(match.Groups[2].Value);
            //IPAddress server = IPAddress.Parse("127.0.0.1");

            Socket sender = new Socket(SocketType.Stream, ProtocolType.Tcp);
            sender.SendTimeout = 3000;
            sender.ReceiveTimeout = 3000;

            string responseData = "";
            try { 
                sender.Connect(server, 53534); 
    
                byte[] messageSent = Encoding.ASCII.GetBytes(String.Format("GET 3D.NET/0.2\n{0}\n\n", uri));
                // GET 3D.NET/0.2\n3dnet://79.164.55.74:53534/main.3dml\n\n
                int byteSent = sender.Send(messageSent); 

                byte[] receiveBuffer = new byte[1024];
                List<byte> receivedData = new List<byte>();

                int byteRecv = 0;
                do
                {
                    Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                    byteRecv = sender.Receive(receiveBuffer);
                    receivedData.AddRange(receiveBuffer);
                } while (byteRecv >= 1024);

                responseData = Encoding.UTF8.GetString(receivedData.ToArray()).TrimEnd('\0');;

                Utils.LogWrite("Message from Server -> "+responseData); 
    
                sender.Shutdown(SocketShutdown.Both); 
                sender.Close(); 
            } 
            catch (Exception e) { 
                Utils.LogWrite("Unexpected exception : "+e.ToString()); 
                return new Response(new byte[0], new Dictionary<string, string>{}, 0);
            }

            Utils.LogWrite(responseData);

            string body = "";
            Dictionary<string, string> headers = new Dictionary<string, string>{};
            bool isBody = false;
            World world = World.getInstance();
            string[] lines = responseData.Split('\n');
            for (int i=0; i<lines.Length; i++)
            {
                string line = lines[i];
                if (i == 0)
                {
                    if ((line.Split(' ')[1] != "200"))
                    {
                        Utils.LogWrite("Response status isn't OK");
                        throw new Exception();
                    }
                }
                else if (!isBody)
                {
                    string[] splited = line.Split(new string[] {": "}, StringSplitOptions.None);
                    if (splited.Length >= 2)
                        headers.Add(splited[0], splited[1]);
                }
                else
                {
                    body += line;
                    if (i > lines.Length)
                        body += '\n';
                }

                if (line == "")
                    isBody = true;
            }

            Utils.LogWrite(body);

            byte[] bytes = Encoding.UTF8.GetBytes(body);
            string hexString = BitConverter.ToString(bytes);
            Debug.Log(hexString);

            return new Response(Convert.FromBase64String(body), headers, 200);
        }
    }

}
