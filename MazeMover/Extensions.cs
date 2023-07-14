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
    }
}
