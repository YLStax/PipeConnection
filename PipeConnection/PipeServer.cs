using PipeConnection.ObjectIO;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PipeConnection
{
    public class PipeServer<TIn, TOut>
    {
        public string PipeName { get; }

        public Func<TIn, TOut> Func { get; }

        public IObjectReaderWriter ObjectReaderWriter { get; }

        public int NumberOfServerInstances { get; }

        public IEnumerable<Task> Servers { get; }

        public IEnumerable<CancellationTokenSource> CancellationTokenSources { get; }

        public ILoggerFactory LoggerFactory { get; }

        private ILogger Logger { get; }

        public PipeServer(string pipeName, Func<TIn, TOut> func, int numberOfServerInstances = 1)
            : this(pipeName, func, new ObjectJsonBinaryReaderWriter(), new LoggerFactory(), numberOfServerInstances) { }

        public PipeServer(string pipeName, Func<TIn, TOut> func, IObjectReaderWriter objectReaderWriter, int numberOfServerInstances = 1)
            : this(pipeName, func, objectReaderWriter, new LoggerFactory(), numberOfServerInstances) { }

        public PipeServer(string pipeName, Func<TIn, TOut> func, ILoggerFactory loggerFactory, int numberOfServerInstances = 1)
            : this(pipeName, func, new ObjectJsonBinaryReaderWriter(loggerFactory), loggerFactory, numberOfServerInstances) { }

        public PipeServer(string pipeName, Func<TIn, TOut> func, IObjectReaderWriter objectReaderWriter, ILoggerFactory loggerFactory, int numberOfServerInstances = 1)
        {
            PipeName = pipeName;
            Func = func;
            ObjectReaderWriter = objectReaderWriter;
            NumberOfServerInstances = numberOfServerInstances;

            CancellationTokenSources = Enumerable.Repeat(new CancellationTokenSource(), NumberOfServerInstances);
            Servers = CancellationTokenSources.Select(n => ServerAsync(n.Token));

            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<PipeServer<TIn, TOut>>();

            Logger.LogInformation($"Generate pipe server instances.\r\n{ServerInfoLogString}");
        }

        public Task WhenAll()
        {
            return Task.WhenAll(Servers);
        }

        public void CancelAll()
        {
            foreach (var n in CancellationTokenSources)
            {
                n.Cancel();
            }
        }

        private async Task ServerAsync(CancellationToken cancellationToken)
        {
            using (Logger.BeginScope($"{nameof(ServerAsync)}"))
            {
                Logger.LogInformation($"Begin pipe server.\r\n{PipeNameLogString}");
                while (true)
                {
                    using (var stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NumberOfServerInstances))
                    {
                        Logger.LogDebug($"Wait for connection.");
                        await stream.WaitForConnectionAsync(cancellationToken);
                        Logger.LogDebug($"Connected to pipe client.");

                        var request = ObjectReaderWriter.ReadObject<TIn>(stream);

                        var response = Func(request);

                        ObjectReaderWriter.WriteObject(stream, response);

                        Logger.LogInformation($"End connection.\r\n{PipeNameLogString}");
                    }
                }
            }
        }

        private string PipeNameLogString =>
            $"Pipe name : {PipeName}";
        private string RequestTypeLogString =>
            $"Request type : {typeof(TIn)}";
        private string ResponseTypeLogString =>
            $"Response type : {typeof(TOut)}";
        private string NumberOfServerInstancesLogString =>
            $"Number of server instances : {NumberOfServerInstances}";

        private string ServerInfoLogString =>
            $"{PipeNameLogString}\r\n{RequestTypeLogString}\r\n{ResponseTypeLogString}\r\n{NumberOfServerInstancesLogString}";
    }
}
