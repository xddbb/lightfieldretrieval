using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace DescriptionExtractor
{
    public class BaseReader
    {
        public SortedList[] batches;
        public SortedList<string, string> dirs;
        public SortedList<string, string> original;

        // Constructor

        public BaseReader(string basefilename) : this(basefilename, -1) {}

        public BaseReader(string basefilename, int batch)
        {
            dirs = new SortedList<string, string>();
            original = new SortedList<string, string>();
            String line;
            StreamReader reader;
            reader = File.OpenText(basefilename);
            line = "";
			FileInfo fi = new FileInfo(basefilename);

            // Walk through lines
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

                    string key = fi.DirectoryName + "/" + string.Join("/", result);
			  
                    dirs.Add(key, model);
                    original.Add(key, line);
                }
            }

            // Split into batches
            if (batch > 0)
            {
                int batch_size = (int)Math.Ceiling((double)dirs.Count / (double)batch);
                int batch_count = 0;
                batches = new SortedList[batch_size];

                foreach (KeyValuePair<string, string> de in dirs)
                {
                    int lookup = (int)((double)batch_count / (double)batch);

                    if (batches[lookup] == null)
                    {
                        batches[lookup] = new SortedList();
                        batches[lookup].Add(de.Key, de.Value);
                    }
                    else
                    {
                        batches[lookup].Add(de.Key, de.Value);
                    }

                    batch_count++;
                }
            }

            reader.Close();
        }
    }
}
