using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ParkingLotApp.Models;

namespace ParkingLotApp.Services.WebSocket
{
    public class WebSocketServer : IDisposable
    {
        private readonly ILogger<WebSocketServer> _logger;
        private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _clients;
        private readonly IWebHost _webHost;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
            _clients = new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();
            _cancellationTokenSource = new CancellationTokenSource();

            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://0.0.0.0:5000")
                .ConfigureServices(services =>
                {
                    services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(builder =>
                        {
                            builder.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
                        });
                    });
                })
                .Configure(app =>
                {
                    app.UseCors();
                    app.UseWebSockets();
                    app.Use(async (context, next) =>
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var clientId = Guid.NewGuid().ToString();
                            _clients.TryAdd(clientId, webSocket);

                            try
                            {
                                await HandleWebSocketConnection(clientId, webSocket);
                            }
                            finally
                            {
                                _clients.TryRemove(clientId, out _);
                                if (webSocket.State != WebSocketState.Closed)
                                {
                                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                                        "Connection closed by the server", 
                                        CancellationToken.None);
                                }
                            }
                        }
                        else
                        {
                            await next();
                        }
                    });
                })
                .Build();
        }

        public async Task StartAsync()
        {
            try
            {
                await _webHost.StartAsync(_cancellationTokenSource.Token);
                _logger.LogInformation("WebSocket server started on http://0.0.0.0:5000");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start WebSocket server");
                throw;
            }
        }

        private async Task HandleWebSocketConnection(string clientId, System.Net.WebSockets.WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

            while (!receiveResult.CloseStatus.HasValue)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    _logger.LogInformation($"Received message from client {clientId}: {receivedMessage}");

                    // Echo the message back to the client
                    var serverMsg = Encoding.UTF8.GetBytes($"Server received: {receivedMessage}");
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(serverMsg),
                        WebSocketMessageType.Text,
                        true,
                        _cancellationTokenSource.Token);
                }

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
            }
        }

        public async Task BroadcastMessage(WebSocketMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(json);
            var deadSockets = new List<string>();

            foreach (var client in _clients)
            {
                try
                {
                    if (client.Value.State == WebSocketState.Open)
                    {
                        await client.Value.SendAsync(
                            new ArraySegment<byte>(messageBytes),
                            WebSocketMessageType.Text,
                            true,
                            _cancellationTokenSource.Token);
                    }
                    else
                    {
                        deadSockets.Add(client.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error broadcasting message to client {client.Key}");
                    deadSockets.Add(client.Key);
                }
            }

            foreach (var deadSocket in deadSockets)
            {
                _clients.TryRemove(deadSocket, out _);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _webHost.StopAsync().Wait();
            _webHost.Dispose();
            _cancellationTokenSource.Dispose();

            foreach (var client in _clients)
            {
                client.Value.Dispose();
            }
            _clients.Clear();
        }
    }
} 