using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionExtractor
{
    public class Pair<TFist, TSecond>
    {
        private TFist first;
        public TFist First
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
    }
}
