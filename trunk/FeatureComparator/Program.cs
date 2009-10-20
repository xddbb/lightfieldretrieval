using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DescriptionExtractor;
using System.Xml.Serialization;

namespace FeatureComparator
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("No inupt files provided!");
				return;
			}

			String file0 = args[0];
			if (!File.Exists(file0))
			{
				Console.Write("File " + file0 + " not found!");
				return;
			}

			String file1 = args[1];
			if (!File.Exists(file1))
			{
				Console.Write("File " + file1 + " not found!");
				return;
			}

			//////////////////////////////////////////////////////////////////////
			// Deserialization
			//////////////////////////////////////////////////////////////////////
			XmlSerializer s = new XmlSerializer(typeof(LightFieldDescriptor));
			TextReader r = new StreamReader(file0);
			LightFieldDescriptor lfd0 = (LightFieldDescriptor)s.Deserialize(r);		
			r.Close();
			//
			r = new StreamReader(file1);
			LightFieldDescriptor lfd1 = (LightFieldDescriptor)s.Deserialize(r);
			r.Close();

			//////////////////////////////////////////////////////////////////////
			// Actual compare
			//////////////////////////////////////////////////////////////////////
			Comparator cmp = new Comparator(lfd0, lfd1, 1.0f, 1.0f);
			double dist = cmp.Compare();

			Console.Write(dist);	// Output to console is enough for PowerShell script
		}
	}
}
