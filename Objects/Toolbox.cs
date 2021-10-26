using System;
using System.Collections.Generic;
using System.Text;

namespace Objects
{
    static class Toolbox
    {
        public static T MergeObjects<T>(T o, T o2)
        {
            var properties = o.GetType().GetProperties();
            for(int i = 0; i < o.GetType().GetProperties().Length; i++)
                if(o[properties[i]] == "")
        }
    }
}
