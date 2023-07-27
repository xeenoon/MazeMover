using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeMover
{
    internal static class Extensions
    {
        public static List<bool> Copy(this List<bool> list)
        {
            List<bool> result = new List<bool> ();
            for (int i = 0; i < list.Count(); ++i)
            {
                result.Add(list[i]);
            }
            return result;
        }
        public static int GetSetBitCount(long lValue)
        {
            int iCount = 0;

            //Loop the value while there are still bits
            while (lValue != 0)
            {
                //Remove the end bit
                lValue = lValue & (lValue - 1);

                //Increment the count
                iCount++;
            }

            //Return the count
            return iCount;
        }
    }
}
