// ShowPlay - Show what song is playing
//
// Copyright (C) 2020-2021 VacuityBox
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
// SPDX-License-Identifier: GPL-3.0-only 

using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ShowPlay
{
    public class Server
    {
        #region WebSocket Helper Class

        class WebSocketClient
        {
            public uint             Id        { get; set; } = 0;
            public WebSocketContext Context   { get; set; } = null;
            public IPAddress        IpAddress { get; set; } = null;
            public int              Port      { get; set; } = 0;
            public bool             IsSecure  { get; set; } = false;

            public WebSocketClient(uint id, WebSocketContext ctx, IPAddress ip, int port, bool secure)
            {
                Id = id;
                Context = ctx;
                IpAddress = ip;
                Port = port;
                IsSecure = secure;
            }
        }

        #endregion

        #region Private Proporties

        private IPAddress        mIpAddress  { get; set; } = null;
        private int              mPort       { get; set; } = 0;
        private bool             mIsSecure   { get; set; } = false;
        private uint             mNextFreeId { get; set; } = 0;

        #endregion

        #region Event Declaration

        public event EventHandler<ClientEventArgs> ConnectionAccepted;
        public event EventHandler<ClientEventArgs> ConnectionClosed;
        public event EventHandler<ClientEventArgs> DataReceived;

        #endregion

        #region Constructor

        public Server(IPAddress ip, UInt16 port, bool wss)
        {
            mIpAddress = ip;
            mPort = port;
            mIsSecure = wss;
        }

        #endregion

        #region Public Methods

        public async void Start()
        {
            // Create http listener.
            var listener = new HttpListener();
            var prefix = string.Format(
                "{0}://{1}:{2}/",
                mIsSecure ? "https" : "http", // TODO: https doesn't work
                mIpAddress.ToString(),
                mPort
            );
            listener.Prefixes.Add(prefix);

            // Start server.
            try
            {
                listener.Start();
            }
            catch (HttpListenerException e)
            {
                Log.Error("Can't start server: {0}", e);
                return;
            }

            Log.Info("ShowPlay server started");
            await Task.Run(() => StartListening(listener));
        }

        #endregion

        #region Events Calls

        private void OnConnectionAccepted(WebSocketClient client)
        {
            ConnectionAccepted?.Invoke(this, new ClientEventArgs(client.Id));
        }

        private void OnConnectionClosed(WebSocketClient client)
        {
            ConnectionClosed?.Invoke(this, new ClientEventArgs(client.Id));
        }

        private void OnDataReceived(WebSocketClient client, byte[] buffer)
        {
            DataReceived?.Invoke(this, new ClientEventArgs(client.Id, buffer));
        }

        #endregion

        #region Private Methods

        private uint GenerateId()
        {
            return mNextFreeId++;
        }

        private async void StartListening(HttpListener listener)
        {
            Log.Info("Server listening on {0}:{1}", mIpAddress, mPort);

            // Listen for requests.
            while (true)
            {
                var ctx = await listener.GetContextAsync();
                if (ctx.Request.IsWebSocketRequest)
                {
                    Log.Info("Received WebSocket request from {0}", ctx.Request.RemoteEndPoint.Address);
                    var client = await AcceptConnection(ctx);
                    if (client is not null)
                    {   
                        OnConnectionAccepted(client);
                        ProcessRequest(client);
                    }
                }
                else
                {
                    Log.Info("Received HTTP request from {0} is not WebSocket request", ctx.Request.RemoteEndPoint.Address);
                    ctx.Response.StatusCode = 400;
                    ctx.Response.Close();
                }
            }
        }

        private async Task<WebSocketClient> AcceptConnection(HttpListenerContext httpContext)
        {
            var webSocketContext = (WebSocketContext)null;
            var ipAddress = httpContext.Request.RemoteEndPoint.Address;
            var port = httpContext.Request.RemoteEndPoint.Port;
            var id = (uint)0;

            Log.Info("Trying to accept connection from {0}", ipAddress);
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
                id = GenerateId();
                Log.Success("Accepted connecton from {0}:{1} unique id #{2}", ipAddress, port, id);
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                Log.Error("Failed to accept connection from {0}:{1}, closing", ipAddress, port);
                Log.Debug("{0}", e);
            }

            return webSocketContext is null
                ? null
                : new WebSocketClient(id, webSocketContext, ipAddress, port, mIsSecure)
                ;
        }

        private async void ProcessRequest(WebSocketClient client)
        {
            // Receive data.
            var webSocket = client.Context.WebSocket;
            try
            {
                // While the connection is open. Receive data.
                var buffer = new byte[ServerConstants.BUFFER_SIZE];
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    Log.Info("Received message from client #{0} ({1})", client.Id, receiveResult.MessageType);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        Log.Info("Received close message from client #{0}", client.Id);
                        OnConnectionClosed(client);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }
                    else
                    {
                        Log.Debug("Received data from client #{0}: {1}", client.Id, System.Text.Encoding.UTF8.GetString(buffer));
                        OnDataReceived(client, buffer);
                        // TODO remove this after testing
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to receive data from client #{0}: {1}", client.Id, e);
            }
            finally
            {
                // Cleanup.
                if (webSocket is not null)
                {
                    Log.Info("Cleaning up connection of client #{0}", client.Id);
                    webSocket.Dispose();
                }
            }
        }

        #endregion
    }
}
