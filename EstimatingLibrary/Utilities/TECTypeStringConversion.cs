using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public static class TECTypeStringConversion
    {
        const string TEC_TYPICAL = "Typical Sys";

        const string TEC_SYSTEM = "System";
        const string TEC_EQUIPMENT = "Equipment";
        const string TEC_SUB_SCOPE = "Point";

        const string TEC_CONTROLLER = "Controller";

        public static string ToTECTypeString(this ITECObject obj)
        {
            var dict = stringDictionary;

            Type objType = obj.GetType();
            if (dict.ContainsKey(objType))
            {
                return dict[objType];
            }
            else
            {
                return "";
            }
        }

        private static Dictionary<Type, string> stringDictionary
        {
            get
            {
                Dictionary<Type, string> dict = new Dictionary<Type, string>();
                dict.Add(typeof(TECTypical), TEC_TYPICAL);
                dict.Add(typeof(TECSystem), TEC_SYSTEM);
                dict.Add(typeof(TECEquipment), TEC_EQUIPMENT);
                dict.Add(typeof(TECSubScope), TEC_SUB_SCOPE);
                dict.Add(typeof(TECController), TEC_CONTROLLER);
                return dict;
            }
        }
    }
}
