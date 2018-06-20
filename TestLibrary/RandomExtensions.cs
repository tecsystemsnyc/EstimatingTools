using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary
{
    public static class RandomExtensions
    {
        public static void RepeatAction(this Random rand, Action act, int maxTimes = 0)
        {
            int numTimes = maxTimes < 0 ? rand.Next() : rand.Next(maxTimes);
            for(int i = 0; i < numTimes; i++)
            {
                act.Invoke();
            }
        }
    }
}
