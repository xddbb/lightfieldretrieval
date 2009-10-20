using System;
using System.Threading;

// NOTE: Reverted single flie

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
				game.Disposed += new EventHandler(game_Disposed);
                game.Run();
				/*
				while (!quit)	// Wait for game to end
				{
					;// Thread.Sleep(20);
				}
				int g = 0;
				*/
				/*
				while (game.IsActive)
				{
					;// Thread.Sleep(20);
				}
				*/
            }
        }

		static bool quit;
		static void game_Disposed(object sender, EventArgs e)
		{
			quit = true;
		}
    }
}

