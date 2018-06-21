using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Does an action a random number of times.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="act"></param>
        /// <param name="maxTimes">Defines a maximum number of times the action should be allowed. A negative integer will use Random.Next()</param>
        public static void RepeatAction(this Random rand, Action act, int maxTimes = -1)
        {
            int numTimes = maxTimes < 0 ? rand.Next() : rand.Next(maxTimes);
            for(int i = 0; i < numTimes; i++)
            {
                act.Invoke();
            }
        }
    }
}
