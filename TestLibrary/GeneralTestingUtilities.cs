﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary
{
    public static class GeneralTestingUtilities
    {
        /// <summary>
        /// Checks to see if two doubles are equal within a range. Used to account for floating point errors.
        /// </summary>
        /// <param name="first">"This" double</param>
        /// <param name="second">Double to compare against</param>
        /// <param name="maxDiff">Maximum acceptable difference</param>
        /// <returns></returns>
        public static bool Equals(this double first, double second, double maxDiff = (1.0 / 1000.0))
        {
            if (Math.Abs(first - second) > (maxDiff))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
