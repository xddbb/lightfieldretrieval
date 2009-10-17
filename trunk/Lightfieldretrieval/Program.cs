using System;
using System.Collections;
using System.IO;
using DescriptionExtractor;

namespace Lightfieldretrieval
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            // Process input directory
            String dirname = args[0];
            if(!Directory.Exists(dirname))
            {
                return;
            }

            // Check if file exists
            if (!File.Exists(dirname + "/basenames"))
            {
                return;
            }

            // Get list of all directory and model files
            BaseReader reader = new BaseReader(dirname, "basenames");

            int i = 0;
            foreach( DictionaryEntry de in reader.dirs ){
                //if (i == 0)
                {
                    using (Renderer game = new Renderer())
                    {
                        game.filename = de.Key + "/" + de.Value;
                        game.Run();
                    }
                }
                i++;
            }
        }
    }
}

