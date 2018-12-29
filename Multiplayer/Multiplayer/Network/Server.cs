using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Multiplayer.Network
{
    public class Server
    {
        public Dictionary<int, PlayerData> PlayerData = new Dictionary<int, PlayerData>();

        /**
         * Start the server
        **/
        public void Start(int port)
        {
            NetworkServer.Listen(port);

            NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
            //NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);
            NetworkServer.RegisterHandler(PacketType.PlayerDisconnected, OnDisconnect);
            NetworkServer.RegisterHandler(PacketType.PlayerData, OnPlayerData);
        }

        /**
         * Stop the server
        **/
        public void Stop()
        {
            NetworkServer.SendToAll(PacketType.ServerShutdown, new IntegerMessage(-2094817));
            NetworkServer.DisconnectAll();
            NetworkServer.Shutdown();
        }

        /**
         * Callback for when a player joins the server
        **/
        private void OnConnect(NetworkMessage msg)
        {
            PlayerData Data = PlayerData[msg.conn.connectionId] = new PlayerData()
            {
                Id = msg.conn.connectionId,
                PlayerName = "",
                Position = new UnityEngine.Vector3(0, 1310, -1220),
                Col = new UnityEngine.Vector3(Util.MathUtil.Random.Next(360), 100, 100),
                Score = 0,
                WorldSeed = WorldManager.currentWorld.seed
            };

            msg.conn.Send(PacketType.ClientConnect, Data);

            foreach (PlayerData other in PlayerData.Values)
                if (other.Id != Data.Id)
                    msg.conn.Send(PacketType.PlayerConnected, other);

            NetworkServer.SendToAll(PacketType.PlayerConnected, Data);
        }

        /**
         * Callback for when a player disconnects from the server
        **/
        private void OnDisconnect(NetworkMessage msg)
        {
            msg.conn.Send(PacketType.ClientDisconnect, new IntegerMessage(1));
            NetworkServer.SendToAll(PacketType.PlayerDisconnected, new IntegerMessage(msg.conn.connectionId));
            PlayerData.Remove(msg.conn.connectionId);
        }

        /**
         * Callback for player data updates
        **/
        private void OnPlayerData(NetworkMessage msg)
        {
            PlayerData otherData = msg.ReadMessage<PlayerData>();
            foreach (PlayerData other in PlayerData.Values)
                if (other.Id != otherData.Id)
                    NetworkServer.SendToAll(msg.msgType, otherData);
        }
    }
}
