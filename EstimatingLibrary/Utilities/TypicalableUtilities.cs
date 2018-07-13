using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public class TypicalableUtilities
    {
        public static void MakeChildrenTypical<T>(T parent) where T : ITypicalable, IRelatable
        {
            foreach(ITypicalable child in parent.GetDirectChildren().Where(item => item is ITypicalable))
            {
                child.MakeTypical();
            }
        }
    }
}
