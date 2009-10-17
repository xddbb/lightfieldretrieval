#region GPL EULA
// Copyright (c) 2009, Bojan Endrovski, http://furiouspixels.blogspot.com/
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DescriptionExtractor;

namespace FeatureComparator
{
	public class DodecahedronAlign
	{
		protected Graph ddc;		// Dodecahedron graph
		public Graph DodecahedronGraph
		{
			get
			{
				return ddc;
			}
		}

		public DodecahedronAlign()
		{
			ddc = new Graph(20);
			ddc.AddEdge(0, 1);
			ddc.AddEdge(1, 2);
			ddc.AddEdge(2, 3);
			ddc.AddEdge(3, 4);
			ddc.AddEdge(4, 5);
			ddc.AddEdge(5, 6);
			ddc.AddEdge(6, 7);
			ddc.AddEdge(7, 8);
			ddc.AddEdge(8, 9);
			ddc.AddEdge(9, 10);
			ddc.AddEdge(10, 11);
			ddc.AddEdge(11, 12);
			ddc.AddEdge(12, 13);
			ddc.AddEdge(13, 14);
			ddc.AddEdge(14, 15);
			ddc.AddEdge(15, 16);
			ddc.AddEdge(16, 17);
			ddc.AddEdge(17, 18);
			ddc.AddEdge(18, 19);
			ddc.AddEdge(0, 19);
			ddc.AddEdge(0, 13);
			ddc.AddEdge(1, 5);
			ddc.AddEdge(2, 12);
			ddc.AddEdge(3, 10);
			ddc.AddEdge(4, 8);
			ddc.AddEdge(6, 19);
			ddc.AddEdge(7, 17);
			ddc.AddEdge(9, 16);
			ddc.AddEdge(11, 15);
			ddc.AddEdge(14, 18);
		}

		public int[] GetAligment(int anchor, int node0, int node1, int node2)
		{
			ddc.Unpaint();
			int[] array = new int[20];
			Queue<int> queue = new Queue<int>();
			//
			array[0] = anchor;
			ddc.Paint(anchor);
			//
			queue.Enqueue(node0);
			queue.Enqueue(node1);
			queue.Enqueue(node2);
			ddc.Paint(node0);
			ddc.Paint(node1);
			ddc.Paint(node2);

			int i = 1;
			while (queue.Count > 0)
			{
				int n = queue.Dequeue();
				array[i++] = n;
				foreach (int node in ddc.GetAdjacent(n))
				{
					if (!ddc.IsPainted(node))
					{
						queue.Enqueue(node);
						ddc.Paint(node);
					}
				}
			}

			return array;
		}
	}
}
