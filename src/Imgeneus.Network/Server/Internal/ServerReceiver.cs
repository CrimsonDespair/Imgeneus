﻿using Imgeneus.Network.Data;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Imgeneus.Network.Server.Internal
{
    internal sealed class ServerReceiver : IDisposable
    {
        private bool disposedValue;
        private readonly IServer server;

        /// <summary>
        /// Gets the receive <see cref="SocketAsyncEventArgs"/> pool
        /// </summary>
        public ConcurrentStack<SocketAsyncEventArgs> ReadPool { get; }

        /// <summary>
        /// Creates a new <see cref="ServerReceiver"/> instance.
        /// </summary>
        /// <param name="server">The parent server.</param>
        public ServerReceiver(IServer server)
        {
            this.server = server;
            this.ReadPool = new ConcurrentStack<SocketAsyncEventArgs>();
        }

        /// <summary>
        /// Receives incoming data.
        /// </summary>
        /// <param name="e">Current socket async event args.</param>
        public void Receive(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("Cannot receive data from a null socket event.", nameof(e));
            }

            if (e.SocketError == SocketError.Success && e.BytesTransferred >= 4)
            {
                if (!(e.UserToken is ServerClient client))
                {
                    return;
                }

                var receivedBuffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, receivedBuffer, 0, e.BytesTransferred);

                ServerReceiver.DispatchPacket(client, receivedBuffer);

                if (!client.Socket.ReceiveAsync(e))
                {
                    this.Receive(e);
                }
            }
            else
            {
                this.CloseConnection(e);
            }
        }

        /// <summary>
        /// Closes the current socket event connection.
        /// </summary>
        /// <param name="e"></param>
        private void CloseConnection(SocketAsyncEventArgs e)
        {
            Array.Clear(e.Buffer, 0, e.Buffer.Length);
            this.ReadPool.Push(e);

            if (e.UserToken is ServerClient client)
            {
                this.server.DisconnectClient(client.Id);
            }
        }

        /// <summary>
        /// Dispatch an incoming packets to a client.
        /// </summary>
        /// <param name="client">the client.</param>
        /// <param name="packetData">Received packet data.</param>
        private static void DispatchPacket(ServerClient client, byte[] packetData)
        {
            Task.Run(() =>
            {
                using IPacketStream packet = new PacketStream(packetData);
                client.HandlePacket(packet);
            });
        }

        /// <summary>
        /// Disposes the <see cref="ServerReceiver"/> resources.
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                foreach (var socket in this.ReadPool)
                {
                    socket.Dispose();
                }
                this.ReadPool.Clear();
                this.disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose() => this.Dispose(true);
    }
}
