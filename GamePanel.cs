using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceInvadersGV1
{
    // This is a custom control panel that we are using to customize our Game 
    public partial class GamePanel : Panel
    {

        //public enum GameState { Title, Playing, Paused, GameOver, Win }

        public GameWorld World {  get; set; }

        public GamePanel()
        {
            InitializeComponent();
            // reduce flickering ( before adding this it was really flickery)
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            pe.Graphics.Clear(Color.Black);
            if(World != null) World.Draw(pe.Graphics);
        }
    }


    // parent class for all sprites, includes varaibles and abstract methods
    public abstract class GameObject
    {
        public RectangleF Bounds; // hold the position of the sprite/ "Bounds" (location, size) 
        public bool IsAlive = true;

        public abstract void Update(TimeSpan dt, Inputstate input, RectangleF Playfield);
        public abstract void Draw(Graphics g);
    }

    

    public class Player : GameObject
    {


        // variables of player

        // initialize the speed = 60
        public float Speed = 60f;
        // firecooldown; is instaticiated 
        private float Firecooldown; 

        public override void Update(TimeSpan dt, Inputstate input, RectangleF Playfield)
        {

            // initialize dx = 0 
            float dx = 0;

            // conditionals on how user input will change dx
            if (input.Left_held) dx -= 1;
            if(input.Right_held) dx += 1;

            float newX = Bounds.X + dx * Speed * (float)dt.TotalSeconds;

            float minX = Playfield.Left;
            float maxX = Playfield.Right - Bounds.Width;


            if (newX < minX)  newX = minX;
            if (newX > maxX)  newX = maxX;

            Bounds = new RectangleF(
                newX,
                Bounds.Y,
                Bounds.Width,
                Bounds.Height);


            //populate bounds with x,y, width and height 
            Firecooldown -= (float)dt.TotalSeconds;

           
            // firepressed update logic 
            if(input.Fire_held && Firecooldown <= 0f)
            {
                Firecooldown = 0.25f;
            }


        }

        public override void Draw(Graphics g) => g.FillRectangle(Brushes.Red, Bounds);


    }


    public class GameWorld
    {
     
        // initialize the game state
        public Inputstate Input = new Inputstate();
        public int Score {  get; private set; }
        public RectangleF Playfield;
        //public GameState state { get; private set; } = Gamestate.Title; 
        public Player Player = new Player();

        public GameWorld() {

            Playfield = new RectangleF(0, 0, 800, 600);
            Player.Bounds = new RectangleF(380, 540, 40, 20);

        }



        public void StartNewGame (Size playfieldSize)
        {

            Playfield = new RectangleF (0,0,playfieldSize.Width,playfieldSize.Height);
            Player.Bounds = new RectangleF(playfieldSize.Width / 2f - 20, playfieldSize.Height - 40, 40, 20);

            //use if wanted to check dimesions if it fits better with the game

            //Playfield = new RectangleF(0, 0, 800, 600);
            //Player.Bounds = new RectangleF(380, 540, 40, 20);


        }

        //public void Update(TimeSpan dt) => Player.Update(dt, Input, Playfield);
        public void Draw(Graphics g) => Player.Draw(g);

        public void Update(TimeSpan dt)
        {
            Input.PreUpdateInput();
            Player.Update(dt, Input, Playfield);

        }

        //public void Draw(Graphics g)
        //{
        //    Player.Draw(g);
        //}


        ////***** keep CalculateDeltaTime() might need later//////

        // each ticks (how often the game updates) 
        //private TimeSpan LastFrameTime;

        //Stopwatch timer = new Stopwatch();

        //(calculating the change in time) ( the change in intervals of time)


        //public TimeSpan CalculateDeltaTime()
        //{
        //    TimeSpan currentTime = timer.Elapsed;

        //    TimeSpan dt = currentTime - LastFrameTime;

        //    LastFrameTime = currentTime;

        //    return dt;
        //}

    }



    // Inputstate holds the user's inputs (Shows what the user is trying to do) 
    public sealed class Inputstate
    {
        //private readonly GameWorld _world = new GameWorld();
        public bool Left_held; // A & < 
        public bool Right_held; // D & > 
        public bool Fire_held; // E & Spacebar

        public bool FirePressed; // user tapped key
        internal bool Fire_was_held_last_frame; // book keeping 


        public void PreUpdateInput()
        {
            // pressed = held now and not held last frame
            FirePressed = Fire_held && !Fire_was_held_last_frame;
            // implament pause 

            Fire_was_held_last_frame = Fire_held;
            // implament pause


        }

    }

}
