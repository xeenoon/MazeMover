using System.IO;
using System.Linq;
using System.Net;

namespace MazeMover
{
    [Flags]
    public enum Direction
    {
        Left = 1,
        Up = 2,
        Right = 4,
        Down = 8,
        None = 0,
    }
    class Program
    {
        public static int placements = 0;
        static List<ConsoleColor> consoleColors = new List<ConsoleColor>() 
        {
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.Gray,
            ConsoleColor.DarkGray,
            ConsoleColor.Blue,
            ConsoleColor.Green,
            ConsoleColor.Cyan,
            ConsoleColor.Red,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
        };
        static List<ConsoleColor> chosenColors = new List<ConsoleColor>();
        static void Main(string[] args)
        {
            int characterposition = 0;
            Console.ReadKey();
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            List<List<int>> setpaths = new List<List<int>>();
            while (true) 
            {
                int width = Console.WindowWidth / 2;
                int height = Console.WindowHeight - 2;
                Random r = new Random(1);
                int seed = r.Next();
                Maze maze = new Maze(width, height);
                maze.GenerateMaze(seed, false);
                Console.Clear();
                maze.Draw(false);
                Console.ForegroundColor = ConsoleColor.White;
                //Draw character position and readkey to find new position
                Console.CursorLeft = (characterposition % width) * 2; //Maze is double width
                Console.CursorTop = height - (characterposition / width) - 1;
                Console.Write("()");
                AI ai = new AI(0, maze);


                while (true) 
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = height; //Write moves at bottom of board
                    //ConsoleKey input = Console.ReadKey().Key;
                    Thread.Sleep(10);
                    Console.CursorLeft = 0;
                    Console.CursorTop = height; //Write moves at bottom of board
                    Console.Write(' ');

                    //Remove last one
                    Console.CursorLeft = (characterposition % width) * 2; //Maze is double width
                    Console.CursorTop = height - (characterposition / width) - 1;
                    Console.Write("  ");

                    //Player movement
                    /* 
                    switch (input)
                    {
                        case ConsoleKey.W:
                            if (characterposition / width < height-1 && maze.GetCell(characterposition + width)) //Can we move up?
                            {
                                characterposition += width;
                            }
                            break;
                        case ConsoleKey.A:
                            if (characterposition % width >= 1 && maze.GetCell(characterposition - 1)) //Can we move left?
                            {
                                characterposition--;
                            }
                            break;
                        case ConsoleKey.S:
                            if (characterposition >= width && maze.GetCell(characterposition - width)) //Can we move down?
                            {
                                characterposition -= width;
                            }
                            break;
                        case ConsoleKey.D:
                            if (characterposition % width < width-1 && maze.GetCell(characterposition + 1)) //Can we move right?
                            {
                                characterposition++;
                            }
                            break;
                    }
                    */

                    //AI movement
                    ai.Move();
                    characterposition = ai.position;

                    Console.CursorLeft = (characterposition % width) * 2; //Maze is double width
                    Console.CursorTop = height - (characterposition / width) - 1;
                    Console.Write("()");
                    if (ai.position == maze.mazeendidx)
                    {
                        while (true)
                        {
                            //Stop the program
                        }
                    }

                    continue;
                    //Find plausible paths from player position
                    //Remove old ones
                    Console.BackgroundColor = ConsoleColor.Black;
                    foreach (var path in setpaths)
                    {
                        foreach (var square in path)
                        {
                            if (square != characterposition)
                            {
                                Console.CursorLeft = (square % width) * 2;
                                Console.CursorTop = (height) - (square / width) - 1;
                                Console.Write("  ");
                            }
                        }
                    }
                    //Add new ones
                    setpaths.Clear();
                    maze.recursions = 0;
                    List<int> fixedpaths = null; //Optional paramater to force include a path
                    maze.FindPlausiblePaths(true, characterposition, Direction.None, ref setpaths, ref fixedpaths);
                    chosenColors.Clear(); //Used to ensure every path has a unique color
                    foreach (var path in setpaths)
                    {
                        ConsoleColor chosencolor;
                        do
                        {
                            chosencolor = consoleColors[r.Next(0, consoleColors.Count())];
                        } while (chosenColors.Contains(chosencolor));
                        chosenColors.Add(chosencolor);
                        Console.BackgroundColor = chosencolor;
                        foreach (var square in path)
                        {
                            if (square != characterposition)
                            {
                                Console.CursorLeft = (square % width) * 2;
                                Console.CursorTop = (height) - (square / width) - 1;
                                Console.Write("  ");
                            }
                        }
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }
        }
    }
    public enum WallType
    {
        Floor,
        DoubleFloor,
        Wall,
        DebugDot,
        Space,
    }
}