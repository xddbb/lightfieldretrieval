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
	public class Pair<TFirst, TSecond>
	{
		private TFirst first;
		public TFirst First
		{
			get { return first; }
			set { first = value; }
		}

		private TSecond second;
		public TSecond Second
		{
			get { return second; }
			set { second = value; }
		}

		public Pair(TFirst f, TSecond s)
		{
			first = f;
			second = s;
		}
	}
}
