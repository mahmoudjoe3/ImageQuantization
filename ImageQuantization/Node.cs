﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
    public class Node
    {
        public int vertix;
        public double key;
        public Node() { }
        public void set_key(double val)
        {
            this.key = val;
        }
    }
}
