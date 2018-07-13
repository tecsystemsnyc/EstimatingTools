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
        /// <param name="maxTimes">Defines a maximum number of times the action should be allowed.</param>
        public static void RepeatAction(this Random rand, Action act, int maxTimes, int minTimes = 1)
        {
            int numTimes = rand.Next(minTimes, maxTimes);
            for(int i = 0; i <= numTimes; i++)
            {
                act.Invoke();
            }
        }

        /// <summary>
        /// Returns a random bool.
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static bool NextBool(this Random rand)
        {
            return (rand.NextDouble() < 0.5);
        }
    }
}
