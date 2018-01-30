using PipeConnection.ObjectIO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PipeConnection
{
    public class PipeClient<TOut, TIn>
    {
        public string ServerName { get; }

        public string PipeName { get; }

        public IObjectReaderWriter ObjectReaderWriter { get; }

        public ILoggerFactory LoggerFactory { get; }

        private ILogger Logger { get; }

        public PipeClient(string pipeName)
            : this(".", pipeName, new ObjectJsonBinaryReaderWriter(), new LoggerFactory()) { }

        public PipeClient(string pipeName, IObjectReaderWriter objectReaderWriter)
            : this(".", pipeName, objectReaderWriter, new LoggerFactory()) { }

        public PipeClient(string pipeName, ILoggerFactory loggerFactory)
            : this(".", pipeName, new ObjectJsonBinaryReaderWriter(), loggerFactory) { }

        public PipeClient(string serverName, string pipeName, IObjectReaderWriter objectReaderWriter)
            : this(serverName, pipeName, objectReaderWriter, new LoggerFactory()) { }

        public PipeClient(string serverName, string pipeName, ILoggerFactory loggerFactory)
            : this(serverName, pipeName, new ObjectJsonBinaryReaderWriter(loggerFactory), loggerFactory) { }

        public PipeClient(string pipeName, IObjectReaderWriter objectReaderWriter, ILoggerFactory loggerFactory)
            : this(".", pipeName, objectReaderWriter, loggerFactory) { }

        public PipeClient(string serverName, string pipeName, IObjectReaderWriter objectReaderWriter, ILoggerFactory loggerFactory)
        {
            ServerName = serverName;
            PipeName = pipeName;
            ObjectReaderWriter = objectReaderWriter;

            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<PipeClient<TOut, TIn>>();

            Logger.LogInformation($"Generate pipe client instance.\r\n{ClientInfoLogString}");
        }

        public TIn Request(TOut request)
        {
            using (Logger.BeginScope($"{nameof(Request)}"))
            {
                using (var stream = new NamedPipeClientStream(ServerName, PipeName))
                {
                    Logger.LogInformation($"Try connect to pipe server.\r\n{ConnectionLogString}");
                    stream.Connect();
                    Logger.LogDebug($"Connected to pipe server.");

                    return GetResponse(stream, request);
                }
            }
        }

        public TIn Request(TOut request, int timeout)
        {
            using (Logger.BeginScope($"{nameof(Request)}"))
            {
                using (var stream = new NamedPipeClientStream(ServerName, PipeName))
                {
                    Logger.LogInformation($"Try connect to pipe server.\r\n{ConnectionLogString}");
                    stream.Connect(timeout);
                    Logger.LogDebug($"Connected to pipe server.");

                    return GetResponse(stream, request);
                }
            }
        }

        public async Task<TIn> RequestAsync(TOut request)
        {
            using (Logger.BeginScope($"{nameof(RequestAsync)}"))
            {
                using (var stream = new NamedPipeClientStream(ServerName, PipeName))
                {
                    Logger.LogInformation($"Try connect to pipe server.\r\n{ConnectionLogString}");
                    await stream.ConnectAsync();
                    Logger.LogDebug($"Connected to pipe server.");

                    return GetResponse(stream, request);
                }
            }
        }

        public async Task<TIn> RequestAsync(TOut request, int timeout)
        {
            using (Logger.BeginScope($"{nameof(RequestAsync)}"))
            {
                using (var stream = new NamedPipeClientStream(ServerName, PipeName))
                {
                    Logger.LogInformation($"Try connect to pipe server.\r\n{ConnectionLogString}");
                    await stream.ConnectAsync(timeout);
                    Logger.LogDebug($"Connected to pipe server.");

                    return GetResponse(stream, request);
                }
            }
        }

        public async Task<TIn> RequestAsync(TOut request, CancellationToken cancellationToken)
        {
            using (Logger.BeginScope($"{nameof(RequestAsync)}"))
            {
                using (var stream = new NamedPipeClientStream(ServerName, PipeName))
                {
                    Logger.LogInformation($"Try connect to pipe server.\r\n{ConnectionLogString}");
                    await stream.ConnectAsync(cancellationToken);
                    Logger.LogDebug($"Connected to pipe server.");

                    return GetResponse(stream, request);
                }
            }
        }

        public async Task<TIn> RequestAsync(TOut request, int timeout, CancellationToken cancellationToken)
        {
            using (Logger.BeginScope($"{nameof(RequestAsync)}"))
            {
                using (var stream = new NamedPipeClientStream(ServerName, PipeName))
                {
                    Logger.LogInformation($"Try connect to pipe server.\r\n{ConnectionLogString}");
                    await stream.ConnectAsync(timeout, cancellationToken);
                    Logger.LogDebug($"Connected to pipe server.");

                    return GetResponse(stream, request);
                }
            }
        }

        private TIn GetResponse(NamedPipeClientStream stream, TOut request)
        {
            ObjectReaderWriter.WriteObject(stream, request);

            var response = ObjectReaderWriter.ReadObject<TIn>(stream);

            Logger.LogInformation($"End connection.\r\n{ConnectionLogString}");

            return response;
        }

        private string ServerNameLogString =>
            $"Server name : {ServerName}";
        private string PipeNameLogString =>
            $"Pipe name : {PipeName}";
        private string RequestTypeLogString =>
            $"Request type : {typeof(TOut)}";
        private string ResponseTypeLogString =>
            $"Response type : {typeof(TIn)}";

        private string ConnectionLogString =>
            $"{ServerNameLogString}\r\n{PipeNameLogString}";
        private string ClientInfoLogString =>
            $"{ServerNameLogString}\r\n{PipeNameLogString}\r\n{RequestTypeLogString}\r\n{ResponseTypeLogString}";
    }
}
