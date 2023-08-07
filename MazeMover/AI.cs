using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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
        List<int> travelledsquares = new List<int>();
        bool justreachedend = false;
        public void Move()
        {
            List<List<int>> setpaths = new List<List<int>>();
            List<int> prevpaths = new List<int>();
            maze.recursions = 0;

            bool t = maze.GetCell(1194);
            bool f = maze.GetCell(1196);

            maze.FindPlausiblePaths(true, position, Direction.None, ref setpaths, ref prevpaths);

            //Check if we can immediatly solve the maze
            List<int> solvepath = setpaths.Where(p=>p.Contains(maze.mazeendidx)).FirstOrDefault();

            if (setpaths.Count >= 2 && justreachedend)
            {
                justreachedend = false;
            }

            if (solvepath != null)
            {
                chosenpath = solvepath;
            }
            else if (chosenpath.Count == 0 || setpaths.Where(p => p.Any(p2 => chosenpath.Contains(p2))).Count() == 0) //Do we have to generate a new path?
            {
                if (setpaths.Count == 1) //Only one path
                {
                    //Dont bother calculating probabilities, just add it
                    chosenpath = setpaths.First();
                    //We are obviously at a dead end, so make sure that we do not go here again
                    SetMazeSquare(position);
                    justreachedend = true;
                }
                else
                {
                    //Choose a random path
                    Random r = new Random();
                    //Random sample to make it so that paths visited more often will have a lower probability of being chosen

                    float[] sampleprobabilities = new float[setpaths.Count()];
                    for (int i = 0; i < setpaths.Count; i++)
                    {
                        sampleprobabilities[i] = 1; //Starting value of 1 to avoid divide by 0 errors
                        List<int> path = setpaths[i];

                        double mydistancetoend   = Math.Sqrt(Math.Pow(((position / maze.width) - (maze.mazeendidx / maze.width)), 2) + Math.Pow(((position % maze.width) - (maze.mazeendidx % maze.width)), 2));
                        double closestpathdistancetoend = double.MaxValue;
                        foreach (var idx in path)
                        {
                            closestpathdistancetoend = Math.Min(Math.Sqrt(Math.Pow(((idx / maze.width) - (maze.mazeendidx / maze.width)), 2) + Math.Pow(((idx % maze.width) - (maze.mazeendidx % maze.width)), 2)), closestpathdistancetoend);
                        }
                        sampleprobabilities[i] *= (float)(mydistancetoend / closestpathdistancetoend);


                        foreach (var square in travelledsquares) //Has duplicates so more visits = more iterations = lower probability
                        {
                            if (path.Contains(square))
                            {
                                sampleprobabilities[i] *= 0.8f; //Make it less likely to be picked
                            }
                        }
                    }

                    double rand = r.NextDouble();
                    float sum = 0;
                    for (int i = 0; i < sampleprobabilities.Length; i++)
                    {
                        var v = (sampleprobabilities[i] / sampleprobabilities.Sum());
                        if (rand > sum && rand < sum + v)
                        {
                            //Our probability range has been selected
                            chosenpath = setpaths[i];
                            break;
                        }
                        sum += v;

                    }
                }
            }
            else //Lengthen our current path to the max value
            {
                if (setpaths.Count() == 1 && justreachedend) //No intersections?
                {
                    //Increase the blockoff point
                    SetMazeSquare(position);
                }

                chosenpath = setpaths.Where(p => p.Any(p2 => chosenpath.Contains(p2))).First();
            }

            //Paths could be in wrong order
           foreach(var square in chosenpath)
           {
               if (square - maze.width == position || square + maze.width == position || square + 1 == position || square - 1 == position)
               {
                   position = square;
               }
           }

            chosenpath.Remove(position);

            travelledsquares.Add(position);
        }
        void SetMazeSquare(int position)
        {
            maze.claimedcells[position] = false;
            //return;
            Program.queue.Enqueue(new Program.ConsoleWriteInfo((position % maze.width) * 2, maze.height - (position / maze.width) - 1, "  ", ConsoleColor.Blue, ConsoleColor.Blue));
        }
        public void ChangeMaze(int removedwall, int placedwall, Maze newmaze, int endmazeidx)
        {
            maze.mazeendidx = endmazeidx;
            List<int> toremove = new List<int>();

            maze.claimedcells[removedwall] = true;
            maze.claimedcells[placedwall] = false;

            newmaze.SolveMaze(position, Direction.None, removedwall, ref toremove);
            newmaze.SolveMaze(position, Direction.None, placedwall, ref toremove);

            toremove.RemoveAll(p=>p==placedwall);
            foreach (var location in toremove)
            {
                maze.claimedcells[location] = true; //Remove previously placed flags
            }
            chosenpath.Clear();
            travelledsquares.RemoveAll(p=>toremove.Contains(p));
            return;
        }
    }
}
