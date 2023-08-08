using MazeMover;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Timers;
using System.Windows.Forms;

namespace MazeUI
{
    public partial class Form1 : Form
    {
        const int cellsize = 20;
        int xdraw = 0;
        int ydraw = 1;

        Maze maze;
        Point centre = new Point(0, 0);

        Bitmap mazebitmap;
        Bitmap AI_bitmap;

        PointF AI_position = new PointF(0,0);

        const float originalspeed = 50;
        float AI_movespeed = originalspeed;
        static System.Timers.Timer incAIspeed = new System.Timers.Timer();

        public Form1()
        {
            InitializeComponent();
            maze = new Maze((Width / 21), (Height / 21) - 1, 0); //-1 accounts for borders
            maze.GenerateMaze();
            centre = new Point(Width / 2, Height / 2);
            GenerateMaze();

            AI_bitmap = new Bitmap((int)(cellsize), (int)(cellsize));
            Graphics g = Graphics.FromImage(AI_bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillEllipse(new Pen(Color.Aqua).Brush, cellsize * 0.1f, cellsize * 0.1f, cellsize * 0.8f, cellsize * 0.8f);
            StartAI();
        }

        private void GenerateMaze()
        {
            mazebitmap = new Bitmap(maze.width * cellsize, (maze.height + 2) * cellsize);
            Graphics g = Graphics.FromImage(mazebitmap);
            Brush black = new Pen(Color.Black).Brush;
            Brush white = new Pen(Color.White).Brush;
            for (int i = 0; i < maze.width * maze.height; ++i)
            {
                if (maze.GetCell(i))
                {
                    g.FillRectangle(black, (i % maze.width) * cellsize, (maze.height - (i / maze.width)) * cellsize, cellsize, cellsize);
                }
                else
                {
                    g.FillRectangle(white, (i % maze.width) * cellsize, (maze.height - (i / maze.width)) * cellsize, cellsize, cellsize);
                }
            }
        }

        bool stopAI;
        void StartAI()
        {
            AI ai = new AI(0, maze.Copy());
            Task.Factory.StartNew(() =>
            {
                while (stopAI)
                {
                    //If previous iteration was killed, wait for it to completely die
                }

                incAIspeed = new System.Timers.Timer(1000);
                incAIspeed.AutoReset = true;
                incAIspeed.Elapsed += new System.Timers.ElapsedEventHandler(IncAISpeed);
                incAIspeed.Start();

                Stopwatch turntimer = new Stopwatch();

                while (true)
                {
                    turntimer.Stop();
                    if (turntimer.ElapsedMilliseconds <= AI_movespeed)
                    {
                        for (int i = 0; i < (int)AI_movespeed - (int)turntimer.ElapsedMilliseconds; ++i)
                        {
                            if (stopAI)
                            {
                                stopAI = false;
                                return;
                            }
                            Thread.Sleep(1);
                        }
                    }

                    turntimer.Restart();

                    //AI movement
                    int oldposition = ai.position;
                    ai.Move();
                    MoveAI(ai.position, oldposition);
                    if (ai.position == maze.mazeendidx)
                    {
                        while (true)
                        {
                            //Stop the program
                            if (stopAI)
                            {
                                stopAI = false;
                                return;
                            }
                        }
                    }
                }
            });

        }
        bool moveairunning = false;
        private void MoveAI(int AI_position, int AI_oldposition)
        {
            while (moveairunning) { } //Wait for other threads to finish
            moveairunning = true;
            float xmove = AI_position % maze.width - AI_oldposition % maze.width;
            float ymove = AI_position / maze.width - AI_oldposition / maze.width;
            float distance = cellsize * (xmove + ymove);
            
            
            Point exactposition = new Point(AI_position%maze.width, AI_position/maze.width);
            //No pythag therom required as movements are only ever straight
            Stopwatch s = new Stopwatch();
            s.Start();
            while (s.ElapsedMilliseconds < (AI_movespeed - 10))
            {
                float timeleft = AI_movespeed - s.ElapsedMilliseconds;
                this.AI_position.X += (distance / timeleft) * xmove;
                this.AI_position.Y += (distance / timeleft) * ymove;

                pictureBox1.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    pictureBox1.Refresh();
                });
                Thread.Sleep(10);
                while (drawing) { } //Wait for other thread to finish
            }
            this.AI_position = new PointF(cellsize * (AI_position%maze.width), cellsize * (AI_position/maze.width));
            moveairunning = false;
        }

        private void IncAISpeed(object? sender, ElapsedEventArgs e)
        {
            AI_movespeed *= 0.99f;
        }
        bool drawing = false;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            drawing = true;
            e.Graphics.DrawImage(mazebitmap, xdraw, ydraw);
            e.Graphics.DrawImage(AI_bitmap, (AI_position.X) + xdraw, (Height - (AI_position.Y)) + ydraw);
            drawing = false;
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
            playermovetick.Interval = 5;
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
            ydraw -= (int)Math.Ceiling((cursorposition.Y - centre.Y) / 100f);
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

        private void button1_Click(object sender, EventArgs e)
        {
            xdraw = 0;
            ydraw = 0;
            maze = new Maze(Width / 20, Height / 20, 0);
            maze.GenerateMaze();
            centre = new Point(Width / 2, Height / 2);

            mazebitmap = new Bitmap(maze.width * cellsize, (maze.height + 1) * cellsize);
            GenerateMaze();

            AI_position = new PointF(0,0);
            stopAI = true;
            AI_movespeed = originalspeed;
            StartAI();
        }
    }
}