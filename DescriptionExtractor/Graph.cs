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
	/// Simple, undirected and unweighted graph utility class
	/// </summary>
	public class Graph
	{
		protected bool[,] adjacency;
		protected int count;

		public Graph(int nodesCount)
		{
			count = nodesCount;
			adjacency = new bool[count, count];
			// Init to false
			for (int i = 0; i < count; i++)
				for (int j = 0; j < count; j++)
				adjacency[i,j] = false;
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
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public int[] GetAdjacent(int i)
		{
		}
	}
}
