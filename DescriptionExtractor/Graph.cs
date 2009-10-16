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

namespace DescriptionExtractor
{
	/// <summary>
	/// Simple, undirected and unweighted graph utility class.
	/// Allows graph painting as well
	/// </summary>
	public class Graph : ICloneable
	{
		protected bool[,] adjacency;
		protected bool[] painted;
		protected int count;

		private Graph() { }

		public Graph(int nodesCount)
		{
			count = nodesCount;
			adjacency = new bool[count, count];
			painted = new bool[count];
			// Init to false
			for (int i = 0; i < count; i++)
			{
				painted[i] = false;
				for (int j = 0; j < count; j++)
					adjacency[i, j] = false;
			}
		}

		/// <summary>
		/// Adds connection in the graph between the i-th and j-th node.
		/// </summary>
		/// <param name="i">index i</param>
		/// <param name="j">index j</param>
		public void AddEdge(int i, int j)
		{
			if(i < 0 || j < 0 || i >= count || j >= count)
				throw new InvalidOperationException("Can't add pair!");

			// Matrix is simetrical;
			adjacency[i, j] = true;
			adjacency[j, i] = true;
		}

		/// <summary>
		/// Gets all the adjecent nodes to node i
		/// </summary>	
		public int[] GetAdjacent(int i)
		{
			// Count first
			int adjc = 0;
			for (int j = 0; j < count; j++)
				if (adjacency[i, j])
					adjc++;

			// Fill up then
			int[] adj = new int[adjc];
			adjc = 0;
			for (int j = 0; j < count; j++)
				if (adjacency[i, j])
					adj[adjc++] = j;

			// All fine
			return adj;
		}

		/// <summary>
		/// Gets all the adjecent nodes to node i, ignoring node n
		/// </summary>	
		public int[] GetAdjacentIgnore(int i, int n)
		{
			// Count first
			int adjc = 0;
			for (int j = 0; j < count; j++)
				if (j != n && adjacency[i, j])
					adjc++;

			// Fill up then
			int[] adj = new int[adjc];
			adjc = 0;
			for (int j = 0; j < count; j++)
				if (j != n && adjacency[i, j])
					adj[adjc++] = j;

			// All fine
			return adj;
		}

		/// <summary>
		/// Returns true of the i-th and j-th node are adjacent
		/// </summary>
		public bool IsAdjacent(int i, int j)
		{
			return adjacency[i, j] || adjacency[j, i];
		}

		/// <summary>
		/// Check if painted
		/// </summary>
		public bool IsPainted(int i)
		{
			return painted[i];
		}

		/// <summary>
		/// Paint i-th node
		/// </summary>
		public void Paint(int i)
		{
			painted[i] = true;
		}

		/// <summary>
		/// Unpaint i-th node
		/// </summary>
		public void Unpaint(int i)
		{
			painted[i] = false;
		}

		/// <summary>
		/// Unpaint i-th node
		/// </summary>
		public void Unpaint()
		{
			for(int i = 0; i < painted.Length; i++)
				painted[i] = false;
		}


		#region ICloneable Members

		public object Clone()
		{
			Graph g = new Graph(count);
			g.count = count;
			g.adjacency = new bool[count, count];
			g.painted = new bool[count];
			//
			// Init to false
			for (int i = 0; i < count; i++)
			{
				g.painted[i] = painted[i];
				for (int j = 0; j < count; j++)
					g.adjacency[i, j] = adjacency[i, j];
			}

			return g;
		}

		#endregion
	}
}


