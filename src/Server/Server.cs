using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using LiteNetLib;
using LiteNetLib.Utils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MinecraftSharp.Server.Configuration;

namespace MinecraftSharp.Server;

internal sealed class Server : BackgroundService
{
    private readonly ServerOptions _options;
    private readonly ILogger<Server> _logger;
    private readonly EventBasedNetListener _listener;
    private readonly NetManager _server;
    private readonly Stopwatch _stopwatch = new();
    private readonly Dictionary<int, Player> _players = [];
    private readonly TimeSpan _tickRate;

    public Server(IOptions<ServerOptions> options, ILogger<Server> logger)
    {
        _options = options.Value;
        _logger = logger;
        _tickRate = TimeSpan.FromSeconds(1 / _options.TicksPerSecond);
        _listener = new();
        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnPeerConnected;
        _listener.PeerDisconnectedEvent += OnPeerDisconnected;
        _listener.NetworkReceiveEvent += OnNetworkReceive;
        _listener.NetworkErrorEvent += OnNetworkError;
        _server = new(_listener);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server starting on port {Port}", _options.Port);
        _server.Start(_options.Port);
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _stopwatch.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan deltaTime = _stopwatch.Elapsed;
                _stopwatch.Restart();

                // Poll LiteNetLib for incoming packets and connection events
                _server.PollEvents();

                // TODO: Do work

                TimeSpan elapsed = _stopwatch.Elapsed;
                TimeSpan remaining = _tickRate - elapsed;
                if (remaining > TimeSpan.Zero)
                {
                    await Task.Delay(remaining, stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Server tick took {Elapsed}, exceeding budget of {Budget}", elapsed, _tickRate);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown, not an error.
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Server encountered a fatal error");
            throw;
        }
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        _logger.LogInformation("Incoming connection request from {Endpoint}", request.RemoteEndPoint);
        if (_server.ConnectedPeersCount >= _options.MaxPlayers)
        {
            // TODO: Give reason for rejection
            request.Reject();
        }
        else
        {
            // TODO: Authentication
            request.Accept();
        }
    }

    private void OnPeerConnected(NetPeer peer)
    {
        _logger.LogInformation("Peer connected: {Peer}", peer);
        // TODO: instantiate player tracking and send initial game state
        //_players.Add(peer.Id, new Player());
        NetDataWriter writer = new();
        writer.Put("Hello client!");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        _logger.LogInformation("Peer disconnected: {Peer} ({Reason})", peer, info.Reason);
        // TODO: remove player tracking
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        // TODO
    }

    private void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        _logger.LogError("Network error from {EndPoint}: {SocketError}", endPoint, socketError);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server stopping");
        _server.Stop();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _listener.ConnectionRequestEvent -= OnConnectionRequest;
        _listener.PeerConnectedEvent -= OnPeerConnected;
        _listener.PeerDisconnectedEvent -= OnPeerDisconnected;
        _listener.NetworkReceiveEvent -= OnNetworkReceive;
        _listener.NetworkErrorEvent -= OnNetworkError;
        base.Dispose();
    }
}