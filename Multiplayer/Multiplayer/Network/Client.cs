using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Multiplayer.Network
{
    public class Client
    {
        private const int DISCONNECT_ID = -2094817;

        public Dictionary<int, Player> PlayerData = new Dictionary<int, Player>();
        public Server Server { get; set; }
        public NetworkClient nClient { get; set; }
        private Player Player { get; set; }
        public bool ConnectionMidState { get; set; }
        public bool Connected { get; set; }

        public Client(bool server)
        {
            if (server)
                Server = new Server();
        }

        /**
         * Connect to a given server or start a server/client host
         * <param name="ip">The IP of the server to connect to</param>
         * <param name="port">The port of the server to connect to</param>
        **/
        public Client Connect(string ip = null, int port = 4444)
        {
            if (!Connected && !ConnectionMidState)
            {
                ConnectionMidState = true;

                if (Server != null)
                {
                    Server.Start(port);
                    nClient = ClientScene.ConnectLocalServer();
                }
                else
                {
                    nClient = new NetworkClient();
                    nClient.Connect(ip, port);
                }

                nClient.RegisterHandler(MsgType.Error, OnError);
                nClient.RegisterHandler(PacketType.PlayerConnected, OnPlayerConnected);
                nClient.RegisterHandler(PacketType.PlayerDisconnected, OnPlayerDisconnected);
                nClient.RegisterHandler(PacketType.PlayerData, OnPlayerData);
                nClient.RegisterHandler(PacketType.ClientConnect, OnClientConnect);
                nClient.RegisterHandler(PacketType.ClientDisconnect, OnClientDisconnect);
                nClient.RegisterHandler(PacketType.ServerShutdown, OnServerShutdown);
            }

            return this;
        }

        /**
         * Update tick
        **/
        public void Update()
        {
            Player.Data.PlayerName = Plugin.Username;
            Player.Data.Position = PlayerMovement.Singleton.transform.position;
            Player.Data.Score = LocalGameManager.Singleton.ScoreThisRun + LocalGameManager.Singleton.ScoreThisCombo;

            nClient.Send(PacketType.PlayerData, Player.Data);
        }

        /**
         * Disconnect from the server or shut down if server/client host
        **/
        public void Disconnect()
        {
            if (Connected && !ConnectionMidState)
            {
                ConnectionMidState = true;

                nClient.Send(PacketType.PlayerDisconnected, Player.Data);

                Player.Destroy();
                foreach (Player player in PlayerData.Values)
                    player.Destroy();
            }
        }

        /**
         * Callback for when an error occurs
        **/
        private void OnError(NetworkMessage msg)
        {
            ErrorMessage message = msg.ReadMessage<ErrorMessage>();
            Console.WriteLine($"{message.errorCode}");
        }

        /**
         * Callback for when a player joins the server
        **/
        private void OnPlayerConnected(NetworkMessage msg)
        {
            PlayerData otherData = msg.ReadMessage<PlayerData>();

            if (otherData.Id == Player.Data.Id)
                return;

            Player other = LocalGameManager.Singleton.gameObject.AddComponent<Player>();
            other.Data = otherData;
            PlayerData[otherData.Id] = other;
        }

        /**
         * Callback for when a player disconnects from the server
        **/ 
        private void OnPlayerDisconnected(NetworkMessage msg)
        {
            int id = msg.ReadMessage<IntegerMessage>().value;

            if (id == Player.Data.Id)
                return;

            PlayerData[id].Disconnect();
            PlayerData.Remove(id);
        }

        /**
         * Callback for player data updates
        **/
        private void OnPlayerData(NetworkMessage msg)
        {
            PlayerData otherData = msg.ReadMessage<PlayerData>();
            otherData.PlayerName = Plugin.Username;

            if (otherData.Id == Player.Data.Id)
                return;

            PlayerData[otherData.Id].Data = otherData;
        }

        /**
         * Callback for when the client successfully connects to the server
         * Server generates new player data to be used for the current player
        **/
        private void OnClientConnect(NetworkMessage msg)
        {
            Connected = true;

            Player = new Player
            {
                Data = msg.ReadMessage<PlayerData>()
            };

            LevelGenerator.Singleton.GenerateLevel(new World(Player.Data.WorldSeed));

            ConnectionMidState = false;
        }

        /**
         * Callback for when the client successfully disconnects from the server
        **/
        private void OnClientDisconnect(NetworkMessage msg)
        {
            nClient.Disconnect();

            if (Server != null)
                Server.Stop();

            Connected = false;
            ConnectionMidState = false;
        }

        /**
         * Callback for when the server shuts down
        **/
        private void OnServerShutdown(NetworkMessage msg)
        {
            ConnectionMidState = true;

            foreach (Player player in PlayerData.Values)
                player.Disconnect();
            PlayerData.Clear();

            Connected = false;
            ConnectionMidState = false;
        }
    }
}
