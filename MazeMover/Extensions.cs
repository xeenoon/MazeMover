using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeMover
{
    internal static class Extensions
    {
        public static List<MazeConnection> Copy(this List<MazeConnection> connections)
        {
            List<MazeConnection> result = new List<MazeConnection> ();
            for (int i = 0; i < connections.Count(); ++i)
            {
                result.Add(new MazeConnection(connections[i].point1, connections[i].point2));
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
