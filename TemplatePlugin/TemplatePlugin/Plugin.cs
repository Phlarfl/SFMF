using SFMF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatePlugin
{
    public class Plugin : IPlugin
    {
        public void Start()
        {
            Console.WriteLine("Template plugin initialized");
        }
    }
}
