
using Multiplayer.Network;
using SFMF;
using System;
using System.Net;
using UnityEngine;

namespace Multiplayer
{
    public class Plugin : IPlugin
    {
        public static Client Network { get; set; }
        public static string Username { get; set; }
        private Vector3 LastMousePos { get; set; }
        private Vector3 Torque = Vector3.zero;

        public void Start()
        {
            Application.runInBackground = true;
            Cursor.visible = true;

            Console.WriteLine("Multiplayer Initialized");
            
            Username = SteamManager.Initialized ? Steamworks.SteamFriends.GetPersonaName() : (Util.StringUtil.GenerateName(5 + Util.MathUtil.Random.Next(3)));

            LocalGameManager.Singleton.gameObject.AddComponent<OverlayMainMenu>();
            LocalGameManager.Singleton.gameObject.AddComponent<Nametags>();
        }

        public void Update()
        {
            if (Network != null)
                Network.Update();
        }
    }
}
