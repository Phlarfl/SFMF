using Multiplayer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer
{
    public class Nametags : MonoBehaviour
    {
        private Dictionary<int, Vector3> W2S = new Dictionary<int, Vector3>();
        
        public void Update()
        {
            Camera cam = CameraManager.singelton.mainCamera.mainCameraReference;
            if (Plugin.Network != null)
                foreach (int id in Plugin.Network.PlayerData.Keys)
                    if (Plugin.Network.PlayerData[id].Primitive != null)
                        W2S[id] = cam.WorldToScreenPoint(Plugin.Network.PlayerData[id].Primitive.transform.position + new Vector3(0, 10, 0));
        }

        public void OnGUI()
        {
            GUIStyle white = new GUIStyle();
            white.normal.textColor = Color.white;
            white.fontSize = 16;

            GUIStyle black = new GUIStyle(white);
            black.normal.textColor = Color.black;

            if (Plugin.Network != null)
                foreach (int id in Plugin.Network.PlayerData.Keys)
                    if (W2S.ContainsKey(id) && W2S[id].z > 0)
                    {
                        GUIContent content = new GUIContent($"{Plugin.Network.PlayerData[id].Data.PlayerName} ({Plugin.Network.PlayerData[id].Data.Score})");
                        Vector2 size = GUI.skin.label.CalcSize(content);
                        GUI.Label(new Rect(W2S[id].x - (size.x / 2) + 1, Screen.height - W2S[id].y - size.y + 1, size.x, size.y), content, black);
                        GUI.Label(new Rect(W2S[id].x - (size.x / 2) - 1, Screen.height - W2S[id].y - size.y + 1, size.x, size.y), content, black);
                        GUI.Label(new Rect(W2S[id].x - (size.x / 2) - 1, Screen.height - W2S[id].y - size.y - 1, size.x, size.y), content, black);
                        GUI.Label(new Rect(W2S[id].x - (size.x / 2) + 1, Screen.height - W2S[id].y - size.y - 1, size.x, size.y), content, black);
                        GUI.Label(new Rect(W2S[id].x - (size.x / 2), Screen.height - W2S[id].y - size.y, size.x, size.y), content, white);
                    }
        }
    }
}
