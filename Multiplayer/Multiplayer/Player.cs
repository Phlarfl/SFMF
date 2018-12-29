using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer
{
    public class Player : MonoBehaviour
    {
        public PlayerData Data { get; set; }
        private Vector3 Lerp = Vector3.zero;
        public GameObject Primitive { get; set; }
        private Renderer Render { get; set; }

        public void Start()
        {
            Primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Primitive.transform.localScale = new Vector3(10, 10, 10);

            Render = Primitive.GetComponent<Renderer>();
            Render.material.SetColor("_Color", Color.HSVToRGB(Data.Col.x / 360f, Data.Col.y / 100f, Data.Col.z / 100f));

            Destroy(Primitive.GetComponent<Collider>());
        }

        public void Update()
        {
            if (Lerp == Vector3.zero)
                Lerp = new Vector3(Data.Position.x, Data.Position.y, Data.Position.z);
            Lerp = new Vector3(Lerp.x + ((Data.Position.x - Lerp.x) / 5f), Lerp.y + ((Data.Position.y - Lerp.y) / 5f), Lerp.z + ((Data.Position.z - Lerp.z) / 5f));
            Primitive.transform.position = Lerp;
        }

        public void Disconnect()
        {
            Destroy();
        }

        public void Destroy()
        {
            Destroy(Primitive);
        }
    }
}
