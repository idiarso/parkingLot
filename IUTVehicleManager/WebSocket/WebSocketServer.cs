using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IUTVehicleManager.Services;

namespace IUTVehicleManager.WebSocket
{
    public class WebSocketServer
    {
        private HttpListener _listener;
        private readonly string _url;
        private bool _isRunning;
        private readonly List<System.Net.WebSockets.WebSocket> _clients = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly ILogger _logger;

        public WebSocketServer(string url, ILogger logger)
        {
            _url = url;
            _logger = logger;
        }

        public async Task Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);

            try
            {
                _listener.Start();
                _logger.Info($"WebSocket server started at {_url}");

                while (_isRunning)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        if (context.Request.IsWebSocketRequest)
                        {
                            _ = HandleWebSocketClient(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error accepting WebSocket client: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting WebSocket server: {ex.Message}");
                _isRunning = false;
            }
        }

        private async Task HandleWebSocketClient(HttpListenerContext context)
        {
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            var webSocket = webSocketContext.WebSocket;

            _lock.EnterWriteLock();
            try
            {
                _clients.Add(webSocket);
                _logger.Info("New WebSocket client connected");
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            try
            {
                // Keep client connection alive and handle messages
                var buffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Process incoming messages if needed
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.Debug($"Received WebSocket message: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"WebSocket client disconnected with error: {ex.Message}");
            }
            finally
            {
                _lock.EnterWriteLock();
                try
                {
                    _clients.Remove(webSocket);
                    _logger.Info("WebSocket client disconnected");
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                if (webSocket.State != WebSocketState.Closed)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.EndpointUnavailable,
                            "Server closing",
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error closing WebSocket: {ex.Message}");
                    }
                }

                webSocket.Dispose();
            }
        }

        public async Task BroadcastAsync(object data)
        {
            var jsonMessage = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);

            List<System.Net.WebSockets.WebSocket> clientsToRemove = new();

            _lock.EnterReadLock();
            try
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.SendAsync(
                                new ArraySegment<byte>(buffer),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                        }
                        else
                        {
                            clientsToRemove.Add(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error broadcasting to client: {ex.Message}");
                        clientsToRemove.Add(client);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Clean up dead clients
            if (clientsToRemove.Count > 0)
            {
                _lock.EnterWriteLock();
                try
                {
                    foreach (var client in clientsToRemove)
                    {
                        _clients.Remove(client);
                        client.Dispose();
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public async Task Stop()
        {
            _isRunning = false;

            // Close all client connections
            _lock.EnterWriteLock();
            try
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Server shutting down",
                                CancellationToken.None);
                        }
                        client.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error closing client WebSocket: {ex.Message}");
                    }
                }
                _clients.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            // Stop the HTTP listener
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
                _logger.Info("WebSocket server stopped");
            }
        }
    }
} 