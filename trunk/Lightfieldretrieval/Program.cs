using System;
using System.Threading;

namespace Lightfieldretrieval
{
    static class Program
    {		
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Renderer game = new Renderer())
            {                
                game.filename = args[0];
                game.Run();
            }
        }	
    }
}

