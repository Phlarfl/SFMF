using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Multiplayer
{
    public class PlayerData : MessageBase
    {
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public Vector3 Position { get; set; }
        public int Score { get; set; }
        public Vector3 Col { get; set; }
        public int WorldSeed { get; set; }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)Id);
            writer.Write(PlayerName);
            writer.WritePackedUInt64((uint)((int)Position.x * 10000f));
            writer.WritePackedUInt64((uint)((int)Position.y * 10000f));
            writer.WritePackedUInt64((uint)((int)Position.z * 10000f));
            writer.WritePackedUInt32((uint)Score);
            writer.WritePackedUInt64((uint)((int)Col.x));
            writer.WritePackedUInt64((uint)((int)Col.y));
            writer.WritePackedUInt64((uint)((int)Col.z));
            writer.WritePackedUInt32((uint)WorldSeed);
        }

        public override void Deserialize(NetworkReader reader)
        {
            Id = (int)reader.ReadPackedUInt32();
            PlayerName = reader.ReadString();
            Position = new Vector3((int)reader.ReadPackedUInt64() / 10000f, (int)reader.ReadPackedUInt64() / 10000f, (int)reader.ReadPackedUInt64() / 10000f);
            Score = (int)reader.ReadPackedUInt32();
            Col = new Vector3((int)reader.ReadPackedUInt64(), (int)reader.ReadPackedUInt64(), (int)reader.ReadPackedUInt64());
            WorldSeed = (int)reader.ReadPackedUInt32();
        }

    }
}
