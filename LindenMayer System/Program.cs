using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

namespace LindenMayer_System
{
    class Program
    {
        static void Main(string[] args)
        {
            GameWindow window = new GameWindow(1366, 768, GraphicsMode.Default, "L-System", GameWindowFlags.Fullscreen);
            //GameWindow window = new GameWindow(800, 600);
            Game game = new Game(window);

            window.Run();
        }
    }
}
