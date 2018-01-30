using Microsoft.Extensions.Logging;
using PipeConnection;
using System;
using System.Threading.Tasks;

namespace SampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerTest();
        }

        static void ServerTest()
        {
            var loggerFactory = new LoggerFactory().AddConsole(LogLevel.Debug, true);

            var server = new PipeServer<string, int>("sample_pipe", s => s.Length, loggerFactory);

            Task.Run(() =>
            {
                try
                {
                    server.WhenAll().Wait();
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.Message);
                }
            });

            Console.WriteLine("Any key to cancel pipe server");
            Console.ReadKey();

            server.CancelAll();

            Console.WriteLine("Any key to end application");
            Console.ReadKey();
        }
    }
}
