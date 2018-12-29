using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Multiplayer
{
    public class PacketType
    {
        public static readonly short PlayerConnected = MsgType.Highest + 1;
        public static readonly short PlayerDisconnected = MsgType.Highest + 2;
        public static readonly short ClientConnect = MsgType.Highest + 3;
        public static readonly short ClientDisconnect = MsgType.Highest + 4;
        public static readonly short PlayerData = MsgType.Highest + 5;
        public static readonly short ServerShutdown = MsgType.Highest + 6;
    }
}
