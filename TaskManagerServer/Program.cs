using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    private static TcpListener server;

    static void Main(string[] args)
    {
        StartServer();
    }

    private static void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Server Started...");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Someone is joined.");
            ThreadPool.QueueUserWorkItem(HandleClient, client);
        }
    }

    private static void HandleClient(object obj)
    {
        using (var client = (TcpClient)obj)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            string command = reader.ReadLine();
            Console.WriteLine("New command: " + command);

            if (command == "get")
            {
                var processes = Process.GetProcesses();
                var processList = new JArray();

                foreach (var process in processes)
                {
                    var processInfo = new JObject
                    {
                        ["Id"] = process.Id,
                        ["Name"] = process.ProcessName
                    };
                    processList.Add(processInfo);
                }

                string jsonResponse = JsonConvert.SerializeObject(processList);
                writer.WriteLine(jsonResponse);
            }
            else if (command == "delete")
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"All proccess ended: {process.ProcessName}, Error:{ex.Message}");
                    }
                }

                writer.WriteLine("All proccess ended.");
            }
        }
    }
}
