using PipeConnection;
using System;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientTest();
        }

        static void ClientTest()
        {
            var client = new PipeClient<string, int>("sample_pipe");

            Console.WriteLine("Input \"exit\" to end application");
            Console.WriteLine("Input other string to response string length from pipe server");

            string request;
            while ((request = Console.ReadLine()) != "exit")
            {
                var response = client.Request(request);
                Console.WriteLine($"\"{request}\" length is {response}");
            }
        }
    }
}
