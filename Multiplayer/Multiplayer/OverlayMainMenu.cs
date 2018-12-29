using Multiplayer.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer
{
    public class OverlayMainMenu : MonoBehaviour
    {
        public string ServerIP { get; set; }
        public string ServerPort { get; set; }
        private Rect WindowRect { get; set; }
        private bool ShowWindow { get; set; }
        private Stopwatch Delay = new Stopwatch();

        public void Start()
        {
            ShowWindow = true;
            WindowRect = new Rect(10, 10, 400, 300);
            ServerIP = "127.0.0.1";
            ServerPort = "4444";
            Delay.Start();
        }

        public void OnGUI()
        {
            ShowWindow = GUI.Toggle(new Rect(10, Screen.height - 30, 100, 20), ShowWindow, $"{(ShowWindow ? "Hide" : "Show")} Window", "Button");
            if (ShowWindow)
                WindowRect = GUI.Window(0, WindowRect, CreateServerManagerWindow, "Multiplayer");
        }

        private void CreateServerManagerWindow(int id)
        {
            Vector2 lblUsernameSize = CreateLabel("Username:", 5, 60, 1, 0);
            Plugin.Username = GUI.TextField(new Rect(10 + lblUsernameSize.x, 49, 385 - lblUsernameSize.x, 24), Plugin.Username, 24);

            if (Plugin.Network == null || (!Plugin.Network.Connected && Plugin.Network.ConnectionMidState))
            {
                if ((Plugin.Network != null && !Plugin.Network.Connected && Plugin.Network.ConnectionMidState) || (Delay.IsRunning && Delay.ElapsedMilliseconds < 2000))
                    GUI.enabled = false;
                ServerIP = GUI.TextField(new Rect(110, 20, 130, 24), ServerIP, 15);
                ServerPort = GUI.TextField(new Rect(245, 20, 45, 24), ServerPort, 5);
                if (GUI.Button(new Rect(5, 20, 100, 24), "Host"))
                    Plugin.Network = new Client(true).Connect(null, int.Parse(ServerPort));

                if (GUI.Button(new Rect(295, 20, 100, 24), "Connect"))
                    Plugin.Network = new Client(false).Connect(ServerIP, int.Parse(ServerPort));
            } else
            {
                if (GUI.Button(new Rect(5, 20, 390, 24), "Disconnect"))
                {
                    Plugin.Network.Disconnect();
                    Plugin.Network = null;
                    Delay.Reset();
                }
                int i = 0;
                foreach (Player player in Plugin.Network.PlayerData.Values)
                    CreateLabel(player.Data.PlayerName, 5, 102 + (i++ * 24), 1, 1);
            }

            GUI.enabled = true;

            CreateLabel("Players:", 5, 78, 1, 1);

            GUI.DragWindow();
        }

        private Vector2 CreateLabel(string label, float x, float y, int xal, int yal)
        {
            GUIContent content = new GUIContent(label);
            Vector2 contentSize = GUI.skin.label.CalcSize(content);
            GUI.Label(new Rect(x - (contentSize.x / 2f) + ((contentSize.x / 2f) * xal), y - (contentSize.y / 2f) + ((contentSize.y / 2f) * yal), contentSize.x, contentSize.y), content);
            return contentSize;
        }
    }
}
