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
			XmlSerializer s = new XmlSerializer(typeof(LightFieldSet));
			TextReader r = new StreamReader(file0);
			LightFieldSet lfs0 = (LightFieldSet)s.Deserialize(r);		
			r.Close();
			//
			r = new StreamReader(file1);
			LightFieldSet lfs1 = (LightFieldSet)s.Deserialize(r);
			r.Close();

			//////////////////////////////////////////////////////////////////////
			// Actual compare. Search for the optimal roataion among the lightfields
			//////////////////////////////////////////////////////////////////////
			double min = Single.PositiveInfinity;
			for (int i = 0; i < lfs0.lightfields.Length; i++)
			{
				LightFieldDescriptor lfd0 = lfs0.lightfields[i];
				LightFieldDescriptor lfd1 = lfs1.lightfields[i];
				Comparator cmp = new Comparator(lfd0, lfd1, 1.0f, 20.0f);
				double dist = cmp.Compare();
				if (dist < min)
					min = dist;
			}						

			Console.Write(min);	// Output to console is enough for PowerShell script
		}
	}
}
