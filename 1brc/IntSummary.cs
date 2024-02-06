using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1brc
{
    public class IntSummary
    {
        public int min;
        public int max;
        public int sum;
        public int count;

        public IntSummary(int initial)
        {
            min = initial;
            max = initial;
            sum = initial;
            count = 1;
        }

        public void Add(int value)
        {
            if (value < min) { min = value; }
            if (value > max) { max = value; }

            sum += value;
            count++;
        }
    }

}
