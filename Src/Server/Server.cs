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
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShowPlay
{
    public class Server
    {
        #region WebSocket Helper Class

        class WebSocketClient
        {
            public int              Id        { get; set; } = 0;
            public WebSocketContext Context   { get; set; } = null;
            public IPAddress        IpAddress { get; set; } = null;
            public int              Port      { get; set; } = 0;
            public bool             IsSecure  { get; set; } = false;

            public WebSocketClient(int id, WebSocketContext ctx, IPAddress ip, int port, bool secure)
            {
                Id = id;
                Context = ctx;
                IpAddress = ip;
                Port = port;
                IsSecure = secure;
            }
        }

        #endregion

        #region ReceiveResult Helper Class

        class ReceiveResult : WebSocketReceiveResult
        {
            public MemoryStream Stream { get; set; }

            public ReceiveResult(WebSocketReceiveResult result, MemoryStream stream)
                : base
                    ( result.Count
                    , result.MessageType
                    , result.EndOfMessage
                    , result.CloseStatus
                    , result.CloseStatusDescription
                    )
            {
                Stream = stream;
            }
        }

        #endregion

        #region Private Proporties

        private HttpListener mHttpListener  { get; set; } = null;
        private IPAddress    mIpAddress     { get; set; } = null;
        private int          mPort          { get; set; } = 0;
        private bool         mIsSecure      { get; set; } = false;
        private int          mNextFreeId    { get; set; } = 0;
        private object       mIdLock        { get; set; } = new object();
        private bool         mIsRunning     { get; set; } = false;
        private int?         mActiveClient  { get; set; } = null;
        private string       mToken         { get; set; } = null;

        private ConcurrentDictionary<int, WebSocketClient>
                             mClients       { get; set; } = new ConcurrentDictionary<int, WebSocketClient>();

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

        #region Start/Stop Server Public Methods

        public void Start()
        {
            if (mIsRunning)
            {
                return;
            }

            // Create http listener.
            mHttpListener = new HttpListener();
            var prefix = string.Format(
                "{0}://{1}:{2}/",
                mIsSecure ? "https" : "http", // TODO: https doesn't work
                mIpAddress.ToString(),
                mPort
            );
            mHttpListener.Prefixes.Add(prefix);
            
            // Start server.
            try
            {
                mHttpListener.Start();
            }
            catch (HttpListenerException e)
            {
                Log.Error("Can't start server: {0}", e);
                return;
            }
            finally
            {
                mIsRunning = true;
            }


            Log.Info("ShowPlay server started");
            Task.Run(() => StartListening());
        }

        public async Task Stop()
        {
            if (mHttpListener?.IsListening ?? false && mIsRunning)
            {
                mIsRunning = false;
                await CloseAllConnections();
                mHttpListener.Stop();
                mHttpListener.Close();
                Log.Info("ShowPlay server stopped");
            }
        }

        public async void Restart()
        {
            Log.Info("Restarting server...");
            await Stop();
            Start();
        }

        public bool IsRunning()
        {
            return mIsRunning;
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

        private void OnDataReceived(WebSocketClient client, MemoryStream stream)
        {
            Log.Info("Parsing payload");

            // Deserialize json.
            var payload = (Payload)null;
            try
            {
                var json = stream.ToArray();
                payload = (Payload)JsonSerializer.Deserialize(json, typeof(Payload));
            }
            catch (Exception e)
            {
                Log.Error("Failed to deserialize json");
                Log.Debug("{0}", e);
                return;
            }
            finally
            {
                Log.Success("Succesfully deserialized json");
            }

            DataReceived?.Invoke(this, new ClientEventArgs(client.Id, payload));
        }

        #endregion

        #region Client Id/Token Generation/Setters

        private int GenerateId()
        {
            lock(mIdLock)
            {
                return mNextFreeId++;
            }
        }

        public int? GetActiveClientId()
        {
            return mActiveClient;
        }

        public void SetActiveClientId(int? id)
        {
            // If id is null and there is active client, deactivate.
            if (id is null && mActiveClient != null)
            {
                DeactivateClient(id);
                return;
            }

            // Invalid id.
            if (id < 0 && id >= mClients.Count)
            {
                return;
            }

            // Ignore if client id is active.
            if (mActiveClient == id)
            {
                return;
            }

            // First deactivate curent active client.
            if (mActiveClient != null)
            {
                DeactivateClient(mActiveClient);
            }

            // Activate new client.
            ActivateClient(id);
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        private void ActivateClient(int? id)
        {
            var client = mClients[(int)id];

            // Generate new Token.
            mToken = GenerateToken();

            // Create token payload.
            var token = "{ \"Token\": \"" + mToken + "\" }";
            var data = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(token));
                
            // Send message to client.
            client.Context.WebSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            Log.Success("Send activation token to client #{0}", id);

            mActiveClient = id;
        }

        private void DeactivateClient(int? id)
        {
            var client = mClients[(int)id];

            // Create token payload.
            var token = "{ \"Token\": null }";
            var data = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(token));
                
            // Send message to client.
            client.Context.WebSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            Log.Success("Send deactivation token to client #{0}", id);

            mActiveClient = null;
        }

        #endregion

        #region Private Methods

        private async Task StartListening()
        {
            Log.Info("Server listening on {0}:{1}", mIpAddress, mPort);

            // Listen for requests.
            try
            {
                while (mIsRunning)
                {
                    var ctx = await mHttpListener.GetContextAsync();
                    if (mClients.Count + 1 > ServerConstants.MAX_CONNECTED_CLIENTS)
                    {
                        Log.Warning
                            ( "Maximum number of clients reached, ignoring request from {0}:{1}"
                            , ctx.Request.RemoteEndPoint.Address
                            , ctx.Request.RemoteEndPoint.Port
                            );
                        ctx.Response.StatusCode = 503;
                        ctx.Response.Close();
                    }
                    else
                    {
                        if (ctx.Request.IsWebSocketRequest)
                        {
                            Log.Info("Received WebSocket request from {0}", ctx.Request.RemoteEndPoint.Address);
                            var client = await AcceptConnection(ctx);
                            if (client is not null)
                            {
                                _ = ProcessRequest(client);
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
            }
            catch (HttpListenerException e) when (mIsRunning)
            {
                Log.Error("Failed to listen for requests {0}", e);
            }
        }

        private async Task<WebSocketClient> AcceptConnection(HttpListenerContext httpContext)
        {
            var webSocketContext = (WebSocketContext)null;
            var ipAddress = httpContext.Request.RemoteEndPoint.Address;
            var port = httpContext.Request.RemoteEndPoint.Port;
            var id = 0;

            Log.Info("Trying to accept connection from {0}:{1}", ipAddress, port);
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
                id = GenerateId();
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                Log.Error("Failed to accept connection from {0}:{1}, closing", ipAddress, port);
                Log.Debug("{0}", e);
            }
            
            if (webSocketContext is not null)
            {
                // Create client and notify about accepted connection.
                var client = new WebSocketClient(id, webSocketContext, ipAddress, port, mIsSecure);

                // Add new client.
                mClients.TryAdd(id, client);
                Log.Success("Accepted connecton from {0}:{1} assiging id #{2}", ipAddress, port, id);

                OnConnectionAccepted(client);
                return client;
            }

            return null;
        }

        private async Task CloseConnection
            ( WebSocketClient      client
            , WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure
            )
        {
            if (!mClients.ContainsKey(client.Id))
                return;

            // Deactivate if client is active.
            if (client.Id == mActiveClient)
            {
                DeactivateClient(mActiveClient);
            }

            // Remove client.
            mClients.TryRemove(client.Id, out _);

            // Remove client and notify that there is no longer a connection.
            var webSocket = client.Context.WebSocket;
            if (webSocket is not null)
            {
                Log.Info("Trying to close connection with client #{0}", client.Id);
                await webSocket.CloseAsync(closeStatus, "", CancellationToken.None);
                
                OnConnectionClosed(client);

                webSocket.Dispose();

                Log.Info("Closed connection with client #{0}", client.Id);
            }
        }

        private async Task ProcessRequest(WebSocketClient client)
        {
            // Receive data.
            var webSocket = client.Context.WebSocket;
            var closeStatus = WebSocketCloseStatus.NormalClosure;
            try
            {
                // While the connection is open. Receive data.
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await ReceiveData(client);
                    if (result is null)
                    {
                        closeStatus = WebSocketCloseStatus.MessageTooBig;
                        await CloseConnection(client, closeStatus);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log.Info("Received close message from client #{0}", client.Id);
                        await CloseConnection(client, closeStatus);
                        break;
                    }
                    else
                    {
                        Log.Info("Received data from client #{0} ({1} b)", client.Id, result.Stream.Length);

                        // Process data further only if data come from active client.
                        if (client.Id == mActiveClient)
                        {
                            OnDataReceived(client, result.Stream);                        
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to receive data from client #{0}: {1}", client.Id, e);
            }
            finally
            {
                if (webSocket.State != WebSocketState.Closed)
                    webSocket.Abort();
            }
        }

        private async Task<ReceiveResult> ReceiveData(WebSocketClient client)
        {
            var webSocket = client.Context.WebSocket;
            var result = (WebSocketReceiveResult)null;
            var buffer = new ArraySegment<byte>(new byte[ServerConstants.BUFFER_SIZE]);
            var stream = new MemoryStream();

            // Read the buffer until end of message is set.
            do
            {
                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                // Too much data.
                if (stream.Length + result.Count > ServerConstants.MAX_DATA_SIZE)
                {
                    Log.Error("Message size from client #{0} is too big to process", client.Id);
                    return null;
                }
                else
                {
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                }
            } while (!result.EndOfMessage);

            stream.Seek(0, SeekOrigin.Begin);

            return new ReceiveResult(result, stream);
        }

        private async Task CloseAllConnections()
        {
            foreach (var client in mClients.Values)
            {
                await CloseConnection(client);
            }

            mClients.Clear();
        }

        #endregion
    }
}
