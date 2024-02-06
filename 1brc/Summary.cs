using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1brc
{
    public class Summary
    {
        public float min;
        public float max;
        public float sum;
        public int count;

        public Summary(float initial)
        {
            min = initial;
            max = initial;
            sum = initial;
            count = 1;
        }

        public void Add(float value)
        {
            if (value < min) { min = value; }
            if (value > max) { max = value; }

            sum += value;
            count++;
        }        
    }
}
