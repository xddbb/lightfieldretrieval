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
				return;
			}

			String file0 = args[0];
			if (!File.Exists(file0))
				return;

			String file1 = args[1];
			if (!File.Exists(file1))
				return;

			//////////////////////////////////////////////////////////////////////
			// Deserialization
			XmlSerializer s = new XmlSerializer(typeof(LightFieldDescriptor));
			TextReader r = new StreamReader(file0);
			LightFieldDescriptor lfd0 = (LightFieldDescriptor)s.Deserialize(r);		
			r.Close();
			//
			r = new StreamReader(file0);
			LightFieldDescriptor lfd1 = (LightFieldDescriptor)s.Deserialize(r);
			r.Close();

			Comparator cmp = new Comparator(lfd0, lfd1, 1.0f, 1.0f);
			cmp.Compare();


			Console.ReadKey();
		}
	}
}
