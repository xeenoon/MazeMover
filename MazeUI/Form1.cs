using MazeMover;
using System.Timers;
using System.Windows.Forms;

namespace MazeUI
{
    public partial class Form1 : Form
    {
        const int cellsize = 20;
        int xdraw = 0;
        int ydraw = 0;

        Maze maze;
        Point centre = new Point(0, 0);

        public Form1()
        {
            InitializeComponent();
            maze = new Maze(Width / 10, Height / 10, 0);
            maze.GenerateMaze();
            centre = new Point(Width / 2, Height / 2);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush black = new Pen(Color.Black).Brush;
            Brush white = new Pen(Color.White).Brush;
            for (int i = 0; i < maze.width * maze.height; ++i)
            {
                if (maze.GetCell(i))
                {
                    g.FillRectangle(black, (i % maze.width) * cellsize + xdraw, Height - ((i / maze.width) * cellsize + ydraw), cellsize, cellsize);
                }
                else
                {
                    g.FillRectangle(white, (i % maze.width) * cellsize + xdraw, Height - ((i / maze.width) * cellsize + ydraw), cellsize, cellsize);
                }
            }
        }

        Point cursorposition = new Point(0, 0);
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            cursorposition = e.Location;

        }
        System.Timers.Timer playermovetick = new System.Timers.Timer();
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            playermovetick.Enabled = true;
            playermovetick.AutoReset = true;
            playermovetick.Interval = 10;
            playermovetick.Elapsed += new System.Timers.ElapsedEventHandler(PlayerMove);
        }
        bool tickrunning = false;
        private void PlayerMove(object sender, ElapsedEventArgs e)
        {
            if (tickrunning)
            {
                return;
            }
            tickrunning = true;
            xdraw -= (int)Math.Ceiling((cursorposition.X - centre.X) / 100f);
            ydraw += (int)Math.Ceiling((cursorposition.Y - centre.Y) / 100f);
            pictureBox1.Invoke((MethodInvoker)delegate
            {
                // Running on the UI thread
                pictureBox1.Refresh();
            });
            tickrunning = false;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            playermovetick.Enabled = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            centre = new Point(Width / 2, Height / 2);
            pictureBox1.Refresh();
        }
    }
}