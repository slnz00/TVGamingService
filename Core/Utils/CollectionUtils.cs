using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public static class CollectionUtils
    {
        public static List<T> CloneList<T>(List<T> list)
        {
            if (list == null) {
                return null;
            }

            List<T> clonedList = new List<T>(list.Count);

            foreach (T item in list) {
                clonedList.Add(item);
            }

            return clonedList;
        }
    }
}
