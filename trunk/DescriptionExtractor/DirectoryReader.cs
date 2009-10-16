using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace DescriptionExtractor
{
    class BaseReader
    {
        public Hashtable dirs;

        // Constructor

        public BaseReader(string dirname, string lookup)
        {
            dirs = new Hashtable();
            String line;
            StreamReader reader;
            reader = File.OpenText(dirname + "/" + lookup);
            line = "";

            while(line!=null)
            {
                line = reader.ReadLine();

                if (line != null)
                {
                    string model = "";
                    string[] parts = line.Split('/');
                    string[] result = new string[parts.Length - 1];

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (i < parts.Length - 1)
                        {
                            result[i] = parts[i];
                        }
                        else
                        {
                            model = parts[i];
                        }
                    }

                    dirs.Add(dirname + "/" + string.Join("/", result), model);
                }
            }

            reader.Close();
        }
    }
}
