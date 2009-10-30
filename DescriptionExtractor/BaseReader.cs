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
        public SortedList<string, string> reversed;
        public Dictionary<string, string> categories;
        public string directoryname;

        // Constructor

        public BaseReader(string basefilename) : this(basefilename, -1) {}

        public BaseReader(string basefilename, int batch)
        {
            dirs = new SortedList<string, string>();
            original = new SortedList<string, string>();
            reversed = new SortedList<string, string>();
            categories = new Dictionary<string, string>();
            String line;
            StreamReader reader;
            reader = File.OpenText(basefilename);
            line = "";
			FileInfo fi = new FileInfo(basefilename);
            directoryname = fi.DirectoryName;

            // Walk through lines
            while(line!=null)
            {
                line = reader.ReadLine();

                if (line != null)
                {
                    string model = "";
                    string[] parts = line.Split('/');

                    if (parts.Length > 2)
                    {
                        string[] result = new string[parts.Length - 1];
                        string category = parts[0];

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

                        string key = directoryname + "/" + string.Join("/", result);

                        dirs.Add(key, model);
                        original.Add(key, line);
                        reversed.Add(line, key);
                        categories.Add(key, category);
                    }
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
