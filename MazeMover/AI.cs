using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeMover
{
    public class AI
    {
        public int position;
        public Maze maze;

        public AI(int position, Maze maze)
        {
            this.position = position;
            this.maze = maze;
        }
        List<int> chosenpath = new List<int>();
        public void Move()
        {
            List<List<int>> setpaths = new List<List<int>>();
            List<int> prevpaths = null;
            maze.recursions = 0;
            maze.FindPlausiblePaths(true, position, Direction.None, ref setpaths, ref prevpaths);

            //Check if we can immediatly solve the maze
            List<int> solvepath = setpaths.Where(p=>p.Contains(maze.mazeendidx)).FirstOrDefault();
            if (solvepath != null)
            {
                chosenpath = solvepath;
                chosenpath.Reverse();
            }
            else if (chosenpath.Count() == 0 || setpaths.Where(p => p.Last() == chosenpath[0]).Count() == 0) //Do we have to generate a new path?
            {
                //Choose a random path
                Random r = new Random();
                chosenpath = setpaths[r.Next(0, setpaths.Count())];
                chosenpath.Reverse();
            }
            else //Lengthen our current path to the max value
            {
                chosenpath = setpaths.Where(p => p.Last() == chosenpath[0]).First();
                chosenpath.Reverse();
            }
            position = chosenpath[0];
            chosenpath.RemoveAt(0);
        }
    }
}
