using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceInvadersGV1
{
    // This is a custom control panel that we are using to customize our Game 
    public partial class GamePanel : Panel
    {

        //public enum GameState { Title, Playing, Paused, GameOver, Win }

        public GameWorld World { get; set; }

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
            if (World != null) World.Draw(pe.Graphics);
        }
    }


    public class GameWorld
    {

        // initialize the game state
        public Inputstate Input = new Inputstate();
        public int Score { get; private set; }
        public RectangleF Playfield;
        //public GameState state { get; private set; } = Gamestate.Title; 
        public Player Player = new Player();
        public List<Alien> Aleins = new List<Alien>();
        public List<bullet> Bullets = new List<bullet>();
        public List<Boulder> boulders = new List<Boulder>();
        private readonly List<ICollisionSystem> _collisionSystems = new List<ICollisionSystem>();

        // added lives label
        public Label Lives = new Label();

        // update the lives label
        public void LivesUpdate()
        {
            Lives.Text = $"Lives: {Player.LivesLeft}";

            // was trying to make color switch to red then fade to white, but not working

            //Timer ColorFadeTimer = new Timer();
            //ColorFadeTimer.Interval = 100; // Adjust the interval as needed
            //if (Player.LivesLeft < 3)
            //{
            //    Lives.ForeColor = Color.Red;
            //    ColorFadeTimer.Start();

            //    //ColorFadeTimer.Stop();
            //    //Lives.ForeColor = Color.White;

            //    //while (ColorFadeTimer.Interval > 1)
            //    //{
            //    //    ColorFadeTimer.Interval--;
            //    //}

            //    //if (ColorFadeTimer.Interval == 1)
            //    //{
            //    //    Lives.ForeColor = Color.White;
            //    //    ColorFadeTimer.Stop();
            //    //}

            //    //Lives.ForeColor = Color.FromArgb(Lives.ForeColor.A, Lives.ForeColor.R, Lives.ForeColor.G, Lives.ForeColor.B);
            //    //while (Lives.ForeColor == Color.Red)
            //    //{
            //    //    Lives.ForeColor = Color.FromArgb(Lives.ForeColor.A, Lives.ForeColor.R, Lives.ForeColor.G + 10, Lives.ForeColor.B + 10);
            //    //    if (Lives.ForeColor.B >= 255 || Lives.ForeColor.G >= 255)
            //    //    {
            //    //        Lives.ForeColor = Color.White;
            //    //        ColorFadeTimer.Stop();
            //    //    }
            //    //}
            //}
        }


        // need a random variable 
        private readonly Random rand = new Random();
        public IAlienModeController AlienMode { get; set; }
        private readonly Alienfleet alienfleet = new Alienfleet();
        public Alienfleet GetFleet() => alienfleet;

        private float spawnTimer = 0f;
   
        private float alienShottimer = 0f;

        private float alienshotInterval = 1.0f; // only one alien can shoot at a time per sec


        public float AlienStopLineY => Player.Bounds.Top - 40f;

        public GameWorld()
        {

            Playfield = new RectangleF(0, 0, 800, 600);
            Player.Bounds = new RectangleF(380, 540, 40, 20);

            // commented out loadsprite due to other stuff for it being commented out
            //Player.LoadSprite(@"C:\Users\hebba\Desktop\SICloneRepo2\Images\Player.png");
            Player.LivesLeft = 3;

            Lives.Location = new Point(715, 10);
            Lives.Size = new Size(100, 20);
            Lives.Font = new Font("Arial", 12, FontStyle.Bold);
            Lives.ForeColor = Color.White;
            Lives.BackColor = Color.Transparent;
            Lives.BringToFront();
            LivesUpdate();

            AlienMode = new AlienFleetWaveMode(GetFleet());

            // AlienMode = new AlienFleetWaveMode(alienfleet);

            _collisionSystems.Add(new PlayerBulletVsBoulderCollision());
            _collisionSystems.Add(new PlayerBulletVsBottomAlienCollision());
            _collisionSystems.Add(new AlienBulletVsPlayerCollision());


        }

        public void AddScore(int amount) => Score += amount;

        public void SetAlienMode(IAlienModeController mode)
        {
            AlienMode = mode;
            AlienMode.Start(this); // remake aliens to behave immediately for that mode
        }

        public void StartNewGame(Size playfieldSize)
        {

            Playfield = new RectangleF(0, 0, playfieldSize.Width, playfieldSize.Height);
            Player.Bounds = new RectangleF(playfieldSize.Width / 2f - 20, playfieldSize.Height - 40, 40, 20);
            //use if wanted to check dimesions if it fits better with the game

            //Playfield = new RectangleF(0, 0, 800, 600);
            //Player.Bounds = new RectangleF(380, 540, 40, 20);

            // for now hard code boulder dimentions 


            boulders.Clear();

           
            int boulderCount = 4;
            float boulderWidth = 60f;
            float boulderHeight = 30f;
            float spacing = playfieldSize.Width / (boulderCount + 1f);
            float boulderY = playfieldSize.Height - 100f;

            for (int i = 1; i <= boulderCount; i++)
            {

                float centerX = spacing * i;

                var bo = new Boulder
                {
                    Bounds = new RectangleF(centerX - boulderWidth / 2f, boulderY, boulderWidth, boulderHeight),
                    blockSize = 4,
                    Columns = (int)(boulderWidth / 4),
                    Rows = (int)(boulderHeight / 4),
                };

                bo.Build();
                boulders.Add(bo);
            }

            spawnTimer = 0f;
            AlienMode.Start(this);
        }

        public void Update(TimeSpan dt)
        {
            Input.PreUpdateInput();
           
            Player.Update(dt, Input, Playfield);
            // spawn bullets when firing ( holdtofire & cooldown player)
            if (Player.TryComsumeShot())
            {
                SpawnPlayerBullets();
            }

            // update the current number of lives
            LivesUpdate();

            AlienMode.Update(this, dt);
            //HandlePlayerBulletHits();

            alienShottimer = alienShottimer - (float)dt.TotalSeconds;

            if (alienShottimer <= 0f)
            {
                trySpawnAlienBullets();
                alienShottimer = alienshotInterval;

            }

            //update bullets
            foreach (var b in Bullets)
                b.Update(dt, Input, Playfield);

            // collisions
            foreach (var sys in _collisionSystems)
                sys.Handle(this);

            // clean up bullets 
            Bullets.RemoveAll(b => !b.IsAlive);
            Aleins.RemoveAll(a => !a.IsAlive);

            foreach (var B in boulders) B.Update(dt, Input, Playfield);

        }

        // make the aliens to be in a grid formation 
        public void SpawnAleinsGrid(int rows, int cols)
        {
            float alienW = 30, alienH = 20;

            float gapX = 10, gapY = 10;

            float startX = 60;
            float startY = 40;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = startX + c * (alienW + gapX);
                    float y = startY + r * (alienH + gapY);

                    Aleins.Add(new Alien { Bounds = new RectangleF(x, y, alienW, alienH) });

                }
            }
        }

        private void SpawnPlayerBullets()
        {

            float bulletW = 4f;
            float bulletH = 10f;

            float x = Player.Bounds.X + Player.Bounds.Width / 2f - bulletW / 2f;
            float y = Player.Bounds.Y - bulletH; // bullet will start above player 

            Bullets.Add(new bullet
            {

                Bounds = new RectangleF(x, y, bulletW, bulletH),
                directionY = -1,
                Speed = 100f

            });

            // test spawn marker ( output window) 
            System.Diagnostics.Debug.WriteLine($"Spawn bullet at X={x:0} Y={y:0}");

        }

        private void trySpawnAlienBullets ()
        {
            var alive = Aleins.Where(a => a.IsAlive).ToList();
            if (alive.Count == 0) return;

            var shooter = alive[rand.Next(alive.Count)];

            float w = 4f, h = 10f;
            float x = shooter.Bounds.X + shooter.Bounds.Width / 2f - w / 2f;
            float y = shooter.Bounds.Bottom;

            Bullets.Add(new bullet
            {
                Bounds = new RectangleF(x, y, w, h),
                directionY = +1,
                Speed = 100f,
                FromAlien = true

            });

        }

        public static float GetFleetBottom(List<Alien> aliens) {

            float bottom = float.MinValue;
            bool any = false;

            foreach (var a in aliens)
            {
                if (!a.IsAlive) continue;
                any = true;

                if (a.Bounds.Bottom > bottom)
                    bottom = a.Bounds.Bottom;
            }

            return any ? bottom : float.MinValue;



        }

        //public void Update(TimeSpan dt) => Player.Update(dt, Input, Playfield);
        public void Draw(Graphics g)
        {
            Player.Draw(g);
            foreach (var b in Bullets)
            {
                if (b.IsAlive) b.Draw(g);
            }

            foreach (var a in Aleins)
            {
                if (a.IsAlive) a.Draw(g);
            }

            foreach (var B in boulders) 
            {
                if (B.IsAlive) B.Draw(g);
            }
               
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

        // initialize lives = 3 (player can be damaged 3 times before death)
        public float LivesLeft = 3;
        // initialize IsInvulnerable = false (player can be damaged)
        private bool IsInvulnerable = false;
        // initialize invulnerability time = 0 (no invulnerability frames)
        private float InvulnerabilityTime = 0f;
        private float BlinkTimer = 0f; // timer for blinking effect when invulnerable
        private bool BlinkVisible = true; // controls whether the player is visible during blinking
        private const float BlinkInterval = 0.15f; // interval for blinking effect (e.g., 0.15 seconds)


        // player sprite stuff (likely will change)
        //private Image PlayerSprite;

        //public void LoadSprite(string path)
        //{
        //    PlayerSprite?.Dispose(); // dispose of the old sprite if it exists
        //    PlayerSprite = Image.FromFile(path);
        //}

        //public void DisposeSprite()
        //{
        //    PlayerSprite?.Dispose();
        //    PlayerSprite = null;
        //}

        // bool value showing if the player shooted
        private bool shotThisFrame = false;


        public override void Update(TimeSpan dt, Inputstate input, RectangleF Playfield)
        {

            if (IsAlive == false)
            {
                return; // skip update if the player is already dead
            }

            // initialize dx = 0 
            float dx = 0;

            // conditionals on how user input will change dx
            if (input.Left_held) dx -= 1;
            if (input.Right_held) dx += 1;

            float newX = Bounds.X + dx * Speed * (float)dt.TotalSeconds;

            float minX = Playfield.Left;
            float maxX = Playfield.Right - Bounds.Width;


            if (newX < minX) newX = minX;
            if (newX > maxX) newX = maxX;

            Bounds = new RectangleF(
                newX,
                Bounds.Y,
                Bounds.Width,
                Bounds.Height);


            //populate bounds with x,y, width and height 
            Firecooldown -= (float)dt.TotalSeconds;


            // firepressed update logic 
            if (input.Fire_held && Firecooldown <= 0f)
            {
                Firecooldown = 0.25f;
                shotThisFrame = true; // tell gameworld to spawn blet
            }

            if (IsInvulnerable == true)
            {
                InvulnerabilityTime -= (float)dt.TotalSeconds;
                BlinkTimer += (float)dt.TotalSeconds;

                // make player blink when invulnerable (for testing purposes, will be changed later)

                if (BlinkTimer >= BlinkInterval)
                {
                    BlinkVisible = !BlinkVisible; // toggle visibility for blinking effect
                    BlinkTimer = 0f; // reset blink timer
                }

                if (InvulnerabilityTime <= 0f)
                {
                    IsInvulnerable = false; // player is no longer invulnerable after invulnerability time runs out
                    InvulnerabilityTime = 0f; // reset invulnerability time
                    BlinkTimer = 0f; // reset blink timer
                    BlinkVisible = true; // ensure player is visible after invulnerability ends
                }
            }

            //// testing damage update logic using FirePressed, will be changed later
            //if (input.FirePressed)
            //    PlayerTakesDamage();

        }

        public bool TryComsumeShot()
        {

            if (!shotThisFrame)
                return false;


            shotThisFrame = false;  // consume it
            return true;

              
        }


        //public override void Draw(Graphics g) => g.FillRectangle(Brushes.Red, Bounds);

        // rewritten Draw method to make condition for player to blink when invulnerable
        public override void Draw(Graphics g)
        {
            if (IsInvulnerable == true && BlinkVisible == false)
            {
                return; // skip drawing the player to create a blinking effect when invulnerable
            }

            if (IsAlive == false)
            {
                return; // skip drawing to not draw the Player since they are dead
            }

            // commented out drawimage due to other stuff for loadsprite/player sprite being commented out
            //if (PlayerSprite != null)
            //{
            //    g.DrawImage(PlayerSprite, Bounds);
            //}

            else
            {
                g.FillRectangle(Brushes.Lime, Bounds);
            }

        }


        // damage update logic for the player (for testing purposes, will be changed later)
        public void PlayerTakesDamage()
        {
            if (IsInvulnerable == true || IsAlive == false || LivesLeft <= 0)
            {
                return; // player cannot take damage if they are invulnerable, already dead, or have no lives left
            }

            LivesLeft--;    // reduce lives by 1

            // for upgrade that reduces lives by half
            // LivesLeft -= 0.5f;    // reduce lives by 0.5


            // check if player is dead after taking damage
            if (LivesLeft <= 0)
            {
                LivesLeft = 0;  // prevent negative lives
                IsAlive = false; // player is now dead
                IsInvulnerable = true; // player becomes invulnerable because they're already dead
            }

            else
            {
                IsInvulnerable = true; // player becomes invulnerable after taking damage
                InvulnerabilityTime = 2.0f; // set invulnerability time (e.g., 2 seconds)
            }

        }

    }

    
    public interface IAlienModeController
    {
        void Start(GameWorld world); // called in startnew game
        void Update(GameWorld world, TimeSpan dt); // called every frame in gamworld update
    }


    public class Alien : GameObject
    {

        // variables for alien 

        // health 

        // points 




        // methods for alien 

        public override void Update(TimeSpan dt, Inputstate input, RectangleF Playfield)
        {
            // will add code to change the behavior of the alien in fleet 

        }

        public override void Draw(Graphics g) => g.FillRectangle(Brushes.Green, Bounds);





    }


    public sealed class Alienfleet
    {
        // fleet variables:

        public float speed = 40f; // how fast the fleet move together
        public float dropamount = 20f; // the rate at which fleet goes down
        public int direction = 1; // 1 = right  -1 = left 

        public void Update(TimeSpan dt, RectangleF playfield, List<Alien> aliens)
        {
            {
                // check if there are aliens present
                bool any = false;


                float left = 0;
                float right = 0;
                float top = 0;
                float bottom = 0;

                float dx = direction * speed * (float)dt.TotalSeconds;

                // check the location of each of the aliens and see if any have touched the left/right bounds 

                foreach (var alien in aliens)
                {

                    if (!alien.IsAlive) continue;
                    var btemp = alien.Bounds;

                    if (!any)
                    {

                        any = true;
                        left = btemp.Left; right = btemp.Right; top = btemp.Top; bottom = btemp.Bottom;

                    }
                    else
                    {
                        left = Math.Min(left, alien.Bounds.Left);
                        right = Math.Max(right, alien.Bounds.Right);

                        // this will be where an alien will have to move 

                        // getting the left right up down coordinates of the bounds of the alien,
                        // basically makes a picture as too how to bounds of the fleet looks like overall 
                        // row column wise
                    }


                }

                if (!any) return; // need to check what this means


                // logic on what to do if the fleet has touched a boundary side wall
                // 

                bool hitleftside = left + dx < playfield.Left;
                bool hitRightside = right + dx > playfield.Right;

                if (hitleftside || hitRightside)
                {
                    // snap idea
                    float snapDx = dx;
                    if (hitleftside) snapDx = playfield.Left - left;
                    if (hitRightside) snapDx = playfield.Right - right;

                    foreach (var alien in aliens)
                    {

                        if (!alien.IsAlive) continue;
                        alien.Bounds = new RectangleF(
                            alien.Bounds.X + snapDx,
                            alien.Bounds.Y + dropamount,
                            alien.Bounds.Width,
                            alien.Bounds.Height);

                    }

                    direction = direction * -1;

                }
                else
                {
                    foreach (var alien in aliens)
                    {

                        if (!alien.IsAlive) continue;
                        alien.Bounds = new RectangleF(
                            alien.Bounds.X + dx,
                            alien.Bounds.Y,
                            alien.Bounds.Width,
                            alien.Bounds.Height
                            );
                    }


                }
            }
        }

    }


    public sealed class AlienFleetWaveMode : IAlienModeController
    {

        private readonly Alienfleet fleet;

        public AlienFleetWaveMode(Alienfleet fleet)
        {
            this.fleet = fleet;
        }

        public void Start(GameWorld world)
        {
            world.Aleins.Clear();
            world.SpawnAleinsGrid(rows: 5, cols: 5);

            fleet.direction = 1;
            fleet.speed = 40f;
            fleet.dropamount = 20f;

        }

        public void Update(GameWorld world, TimeSpan dt)
        {
            fleet.Update(dt, world.Playfield, world.Aleins);

            // if we want to respawn a new wave 

            if (world.Aleins.All(a => !a.IsAlive))
            {
                world.Aleins.Clear();
                world.SpawnAleinsGrid(5, 5);
                fleet.speed *= 1.1f;

            }
        }

    }

    public sealed class AlienContinuousSpawnMode : IAlienModeController
    {

        private readonly Random random = new Random();

        public float spawnIntervalSpeed = 0.75f;

        public float fallSpeed = 60f;

        public int maxAliensOnScreen = 30;

        private float spawnTimer;

        public void Start ( GameWorld world)
        {
            world.Aleins.Clear();
            spawnTimer = 0f; // spawn right away

        }

        private void SpawnAliensAtTop(GameWorld world)
        {

            float w = 30f, h = 20f;
            float x = (float)random.NextDouble() * (world.Playfield.Width - w);
            float y = world.Playfield.Top;

            world.Aleins.Add(new Alien
            {
                Bounds = new RectangleF(x, y, w, h)

            });

            
        }
        public void Update(GameWorld world, TimeSpan dt)
        {
            float seconds = (float)dt.TotalSeconds;

            // spawn logic 

            spawnTimer = spawnTimer - seconds;

            if(spawnTimer <= 0f && world.Aleins.Count(a => a.IsAlive) < maxAliensOnScreen)
            {

                SpawnAliensAtTop (world);
                spawnTimer = spawnIntervalSpeed;

            }

            // fall logic

            foreach(var ac in world.Aleins)
            {

                if (!ac.IsAlive) continue;

                ac.Bounds = new RectangleF(

                    ac.Bounds.X,
                    ac.Bounds.Y + fallSpeed * seconds,
                    ac.Bounds.Width,
                    ac.Bounds.Height

                    );

                if (ac.Bounds.Top > world.Playfield.Bottom)
                {
                    ac.IsAlive = false;
                }

                
            }

            world.Aleins.RemoveAll(a => !a.IsAlive);

        }


    }

    public sealed class AlienStraightDownWaveMode : IAlienModeController
    {
        private int wave = 0;

        public float fallSpeed = 25f;



        public void Start(GameWorld world)
        {
            wave = wave + 1;

            world.Aleins.Clear();
            world.SpawnAleinsGrid(rows: 5, cols: 8);

            fallSpeed = 25f + 2f * (wave - 1);


        }

        public void Update(GameWorld world, TimeSpan dt)
        {


            float dy = fallSpeed * (float)dt.TotalSeconds;

            float bottom = GameWorld.GetFleetBottom(world.Aleins);
            if (bottom == float.MinValue)
            {
                Start(world);
                return;
            }

            float allowed = world.AlienStopLineY - bottom;
            if (allowed < 0) allowed = 0;
            if (dy > allowed) dy = allowed;

            foreach (var a in world.Aleins)
            {
                if (!a.IsAlive) continue;
                a.Bounds = new RectangleF(a.Bounds.X, a.Bounds.Y + dy, a.Bounds.Width, a.Bounds.Height);
            }

            if (world.Aleins.All(a => !a.IsAlive))
                Start(world);



        }




    }


        //public sealed class AlienFleetSlowDropMode : IAlienModeController
        //{
        //    private int dir = 1;
        //    private float speed = 40f;         // horizontal speed
        //    private float dropAmount = 10f;    // smaller drops feel “slow”
        //    private float dropTimer = 0f;
        //    private float dropInterval = 1.25f; // drop every 1.25s (tune)

        //    private int wave = 0;


        //    public void Start(GameWorld world)
        //    {
        //        wave = wave + 1;
        //        world.Aleins.Clear();
        //        world.SpawnAleinsGrid(rows: 5, cols: 8);

        //        dir = 1;
        //        speed = 35f + 3f * (wave - 1);
        //        dropTimer = 0f;

        //    }

        //    private static void MoveAliens(List<Alien> aliens, float dx, float dy)
        //    {
        //        foreach (var a in aliens)
        //        {
        //            if (!a.IsAlive) continue;
        //            a.Bounds = new RectangleF(a.Bounds.X + dx, a.Bounds.Y + dy, a.Bounds.Width, a.Bounds.Height);


        //        }

        //    }

        //    private static bool TryGetFleetBounds(List<Alien> aliens, out RectangleF bounds)
        //    {
        //        bool any = false;

        //        float left = 0, right = 0, top = 0, bottom = 0;

        //        foreach(var a in aliens)
        //        {
        //            if (!a.IsAlive) continue;

        //            var b = a.Bounds;

        //            if (!any)
        //            {
        //                any = true;

        //                left = b.Left; right = b.Right; top = b.Top; bottom = b.Bottom;


        //            }
        //            else
        //            {
        //                if (b.Left < left) left = b.Left;
        //                if (b.Right > right) right = b.Right;
        //                if (b.Top < top) top = b.Top;
        //                if (b.Bottom > bottom) bottom = b.Bottom;


        //            }


        //        }

        //        bounds = any ? RectangleF.FromLTRB(left, top, right, bottom) : RectangleF.Empty;
        //        return any;

        //    }

        //    public void Update (GameWorld world, TimeSpan dt)
        //    {

        //        //horizontal + timed drop logic 
        //    }

        //}


        public class bullet : GameObject
    {
        // initialize the speed = 60
        public float Speed = 60f;

        // firecooldown; is instaticiated 

        public int directionY = -1; // player shoots up 

        public bool FromAlien; // true = alien bullet , false = player bullet

        //private float Firecooldown;



        public int Health = 3;
        public void TakeHit()
        {
            Health--;
            if (Health <= 0)
                IsAlive = false;
        }

        public override void Update(TimeSpan dt, Inputstate Input, RectangleF Playfield)
        {

            float dy = directionY * Speed * (float)dt.TotalSeconds;

            Bounds = new RectangleF(Bounds.X, Bounds.Y + dy, Bounds.Width, Bounds.Height);


            // if the bullet is outside of the playfield, delete instance
            if (Bounds.Bottom < Playfield.Top || Bounds.Top > Playfield.Bottom)
                IsAlive = false;

            //// conditionals on how user input will change dx
            //if (input.Left_held) dx -= 1;
            //if (input.Right_held) dx += 1;

           

        }

       public override void Draw(Graphics g) => g.FillRectangle(Brushes.Pink, Bounds);

    }

    public class Boulder : GameObject
    {
      public List<RectangleF> Blocks = new List<RectangleF>();

        public int blockSize = 4;
        public int Columns = 15;
        public int Rows = 8;

        public void Build()
        {
            Blocks.Clear();

            for (int r = 0; r < Rows; r++)
            {

                for (int c = 0; c < Columns; c++)
                {

                    float x = Bounds.X + c * blockSize;
                    float y = Bounds.Y + r * blockSize;

                    Blocks.Add(new RectangleF(x, y, blockSize, blockSize));
                }
            }
        }
        
        // make health variable
        public int Health = 5;

        // make method for boulder to take damage
        public void TakeHit()
        {
            Health = Health - 1;

            if(Health <= 0) { IsAlive = false; }

        }


        public override void Update(TimeSpan dt, Inputstate input, RectangleF Playfield)
        {
            // stays in place
            IsAlive = Blocks.Count > 0;
        }

        public override void Draw(Graphics g)
        {

            foreach (var r in Blocks)
            {
                g.FillRectangle(Brushes.Green, r);

            }

        }

        public void DamageAt(PointF hitPoint, float radius)
        {

            float r2 = radius * radius;

            Blocks.RemoveAll(b =>
            {
                float cx = b.X + b.Width / 2f;
                float cy = b.Y + b.Height / 2f;
                float dx = cx - hitPoint.X;
                float dy = cy - hitPoint.Y;
                return (dx * dx + dy * dy) <= r2; // remove blocks inside circle
            });

        }


    }

    public interface ICollisionSystem
    {
        void Handle(GameWorld world);

    }


    public abstract class CollisionSystemBase: ICollisionSystem
    {
        protected static bool Hit(RectangleF a, RectangleF b) => a.IntersectsWith(b);
        public abstract void Handle(GameWorld world);

    }

    public sealed class PlayerBulletVsBottomAlienCollision: CollisionSystemBase
    {

        public override void Handle(GameWorld world)
        {
            foreach (var b in world.Bullets)
            {

                if (!b.IsAlive || b.FromAlien) continue;

                foreach (var target in GetBottomMostAliveRowPerColumn(world.Aleins))
                {

                    if (!target.IsAlive) continue;

                    if (Hit(b.Bounds, target.Bounds))
                    {

                        b.IsAlive = false;
                        target.IsAlive = false;
                        world.AddScore(10);
                        break;
                    }
                }



            }
        }


        
        private static IEnumerable<Alien> GetBottomMostAliveRowPerColumn(List<Alien> aliens)
        {

            var alive = aliens.Where(a => a.IsAlive);

            foreach (var col in alive.GroupBy(a => a.Bounds.X))
            {

                yield return col.OrderByDescending(a => a.Bounds.Y).First();

            }
        }
    }

    public sealed class AlienBulletVsPlayerCollision : CollisionSystemBase
    {
        public override void Handle(GameWorld world)
        {
            foreach (var b in world.Bullets)
            {
                if (!b.IsAlive || !b.FromAlien) continue;


               if(Hit(b.Bounds, world.Player.Bounds))
                {
                    b.IsAlive = false;
                    Debug.WriteLine("Player hit!");
                    world.Player.PlayerTakesDamage();
                    // later: world.Lives--, set GameOver, etc.

                }
            }
        }


    }


    public sealed class PlayerBulletVsBoulderCollision : CollisionSystemBase
    {
        public override void Handle(GameWorld world)
        {
            foreach(var b in world.Bullets)
            {
                if (!b.IsAlive || b.FromAlien) continue;

                var hitPoint = new PointF(b.Bounds.X + b.Bounds.Width / 2f, b.Bounds.Y + b.Bounds.Height / 2f);

                foreach (var bo in world.boulders)
                {
                    if (!bo.IsAlive) continue;

                    if (!Hit(b.Bounds, bo.Bounds)) continue;

                    b.IsAlive = false;
                    bo.DamageAt(hitPoint, radius: 10f);
 
                }


            }
        }
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

            // Lives.Text = $"Lives: {Player.LivesLeft}";   // not working, will be changed later

        }

    }


}

