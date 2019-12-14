using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{

    public class MST
    {
        public double Weight;
        public List<Node> tree;
        public int[] parent;
        public MST(double Weight, List<Node> tree,int[]parent)
        {
            this.Weight = Weight;
            this.tree = tree;
            this.parent = parent;
        }

    }
}
