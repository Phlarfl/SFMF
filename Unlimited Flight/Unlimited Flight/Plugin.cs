using SFMF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unlimited_Flight
{
    public class Plugin : IPlugin
    {
        public void Update()
        {
            PlayerMovement.Singleton.currentSpeed = Math.Max(800, PlayerMovement.Singleton.currentSpeed);
        }
    }
}
