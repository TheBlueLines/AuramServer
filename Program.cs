using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Auram;

namespace AuramTest
{
    class Program
    {
        private static string url = "http://*:65500/";
        private static HttpListener listener;
        private static byte[] buffer;
        private static void InsertDate()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] ");
        }
        private static string CreateRandomPassword(int length = 100)
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new();
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
        static void Main(string[] args)
        {
            Console.Clear();
            Console.Title = "Auram Server";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("████████████████████████████████████████████████████████████████████████\n██▀▄─██▄─██─▄█▄─▄▄▀██▀▄─██▄─▀█▀─▄███─▄▄▄▄█▄─▄▄─█▄─▄▄▀█▄─█─▄█▄─▄▄─█▄─▄▄▀█\n██─▀─███─██─███─▄─▄██─▀─███─█▄█─████▄▄▄▄─██─▄█▀██─▄─▄██▄▀▄███─▄█▀██─▄─▄█\n▀▄▄▀▄▄▀▀▄▄▄▄▀▀▄▄▀▄▄▀▄▄▀▄▄▀▄▄▄▀▄▄▄▀▀▀▄▄▄▄▄▀▄▄▄▄▄▀▄▄▀▄▄▀▀▀▄▀▀▀▄▄▄▄▄▀▄▄▀▄▄▀\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Auram Server");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" by TTMC Corporation ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("TheBlueLines");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Version: v0.1 BEAM\n");
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Default server started on port: 65500");
            Console.ForegroundColor = ConsoleColor.Gray;
            Task listenTask = HandleIncomingConnections();
            AuramTcp();
            listenTask.GetAwaiter().GetResult();
            listener.Close();
        }
        private static void AuramTcp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Console server started on port: 13000");
            TcpListener server = null;
            int port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, port);
            server.Start();
            byte[] bytes = new byte[256];
            string data = null;
            while (true)
            {
                try
                {
                    while (true)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("Waiting for a connection... ");
                        TcpClient client = server.AcceptTcpClient();
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("Connected!");
                        data = null;
                        NetworkStream stream = client.GetStream();
                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = Encoding.UTF8.GetString(bytes, 0, i);
                            Console.WriteLine("Received: {0}", data);
                            string[] command = { };
                            try
                            {
                                command = data.Split(' ');
                                if (command[0].ToUpper() == "SET" || command[0].ToUpper() == "ADD" || command[0].ToUpper() == "CREATE" || command[0].ToUpper() == "APPEND" || command[0].ToUpper() == "+")
                                {
                                    if (command.Length == 3)
                                    {
                                        Database.AddToDatabase("Database/"+command[1], command[2]);
                                        data = command[1] + " + " + command[2];
                                    }
                                    else
                                    {
                                        data = "Usage: SET <KEY> <VALUE>";
                                    }
                                }
                                else if (command[0].ToUpper() == "GET" || command[0].ToUpper() == "SEE" || command[0].ToUpper() == "LOOK" || command[0].ToUpper() == "READ" || command[0].ToUpper() == "=")
                                {
                                    if (command.Length == 2)
                                    {
                                        data = command[1] + " = " + Database.GetFromDatabase("Database/"+command[1]);
                                    }
                                    else
                                    {
                                        data = "Usage: GET <KEY>";
                                    }
                                }
                                else if (command[0].ToUpper() == "REMOVE" || command[0].ToUpper() == "DELETE" || command[0].ToUpper() == "DEL" || command[0].ToUpper() == "RM" || command[0].ToUpper() == "-")
                                {
                                    if (command.Length == 2)
                                    {
                                        Database.RemoveFromDatabase("Database/"+command[1]);
                                        data = command[1] + " - REMOVED";
                                    }
                                    else
                                    {
                                        data = "Usage: REMOVE <KEY>";
                                    }
                                }
                                else
                                {
                                    data = "Unknown command: " + command[0];
                                }
                            }
                            catch
                            {
                                data = "ERROR";
                            }
                            byte[] msg = Encoding.UTF8.GetBytes(data);
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine("Sent: {0}", data);
                            Database.SaveDatabase("Database.auram");
                        }
                        client.Close();
                    }
                }
                catch
                {
                    server.Stop();
                    server.Start();
                }
            }
        }
        private static async Task HandleIncomingConnections()
        {
            if (File.Exists("Database.auram"))
            {
                Database.LoadDatabase("Database.auram");
            }
            bool runServer = true;
            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                string text;
                var reader = new StreamReader(req.InputStream, req.ContentEncoding);
                text = reader.ReadToEnd();
                try
                {
                    if (req.HttpMethod == "POST")
                    {
                        Database.AddToDatabase("Database" + req.Url.AbsolutePath, text);
                        InsertDate();
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("Value ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(req.Url.AbsolutePath[1..]);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine(" set!");
                        buffer = Encoding.UTF8.GetBytes("DONE");
                        resp.ContentLength64 = buffer.Length;
                        Stream output = resp.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                    if (req.HttpMethod == "DELETE")
                    {
                        Database.RemoveFromDatabase("Database" + req.Url.AbsolutePath);
                        InsertDate();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Value ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(req.Url.AbsolutePath[1..]);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" deleted!");
                        buffer = Encoding.UTF8.GetBytes("DONE");
                        resp.ContentLength64 = buffer.Length;
                        Stream output = resp.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                    if (req.HttpMethod == "GET")
                    {
                        buffer = Encoding.UTF8.GetBytes(Database.GetFromDatabase("Database" + req.Url.AbsolutePath));
                        InsertDate();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("Value ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(req.Url.AbsolutePath[1..]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(" requested!");
                        resp.ContentLength64 = buffer.Length;
                        Stream output = resp.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                    Database.SaveDatabase("Database.auram");
                }
                catch
                {
                    buffer = Encoding.UTF8.GetBytes("ERROR");
                    resp.ContentLength64 = buffer.Length;
                    Stream output = resp.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }
        }
    }
}