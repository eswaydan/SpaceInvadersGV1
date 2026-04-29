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

    public enum ModeChoice { FleetWaves, Continuous }
    public enum GameState { Title, Playing, Paused, GameOver }

    public enum AlienType { Octopus, Crab, Squid }

    public partial class GamePanel : Panel
    {

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


    // Sprites for the Player and 3 Aliens (so that we load them once)
    public static class Sprites
    {
        public static readonly Image PlayerSprite = Properties.Resources.Player;
        public static readonly Image Alien1Sprite = Properties.Resources.Octopus1;
        public static readonly Image Alien2Sprite = Properties.Resources.Crab1;
        public static readonly Image Alien3Sprite = Properties.Resources.Squid1;
    }
  
    public class GameWorld
    {

        // initialize the game state
        public GameState State { get; private set; } = GameState.Title;
        public ModeChoice SelectedMode { get; set; } = ModeChoice.FleetWaves;

        public Inputstate Input = new Inputstate();
        public List<int> SessionScores { get; } = new List<int>();
        public int Score { get; private set; }
        public RectangleF Playfield;
      
        public Player Player = new Player();
        public List<Alien> Aleins = new List<Alien>();
        public List<bullet> Bullets = new List<bullet>();
        public List<Boulder> boulders = new List<Boulder>();
        private readonly List<ICollisionSystem> _collisionSystems = new List<ICollisionSystem>();


        // added lives label
        //Score.Location = new Point(100, 30);
        //Score.Size = new Size(100, 30);
        //Score.Font = new Font("Arial", 12, FontStyle.Bold);
        //Score.ForeColor = Color.White;
        //Score.BackColor = Color.Transparent;
        //Score.BringToFront();
        // update the lives label

        // need a random variable 
        private readonly Random rand = new Random();
        public IAlienModeController AlienMode { get; set; }
        private readonly Alienfleet alienfleet = new Alienfleet();
        public Alienfleet GetFleet() => alienfleet;
        public int AlienKillScoreMultiplier => (AlienMode is AlienContinuousSpawnMode) ? 2 : 1;
   
        private float alienShottimer = 0f;

        private float alienshotInterval = 1.0f; // only one alien can shoot at a time per sec


        public float AlienStopLineY => Player.Bounds.Top - 40f;

        public GameWorld()
        {

            Playfield = new RectangleF(0, 0, 800, 600);
            Player.Bounds = new RectangleF(380, 540, 40, 20);

          
            Player.LivesLeft = 3;

            AlienMode = new AlienFleetWaveMode(GetFleet());

            _collisionSystems.Add(new PlayerBulletVsBoulderCollision());
            _collisionSystems.Add(new AlienBulletVsBoulderCollision());
            _collisionSystems.Add(new PlayerBulletVsBottomAlienCollision());
            _collisionSystems.Add(new AlienBulletVsPlayerCollision());
            _collisionSystems.Add(new AlienVsBoulderCollision());


        }

        public void AddScore(int amount)
        {
            if (amount == 0) return;
            Score = Score + amount;
            ApplyUpgradesIfNeeded();
        }

        public void SetAlienMode(IAlienModeController mode)
        {
            AlienMode = mode;
            AlienMode.Start(this); // remake aliens to behave immediately for that mode
        }

        public void StartNewGame(Size playfieldSize)
        {
           
            
            if (SelectedMode == ModeChoice.FleetWaves)
                SetAlienMode(new AlienFleetWaveMode(GetFleet()));
            else
                SetAlienMode(new AlienContinuousSpawnMode());


            Playfield = new RectangleF(0, 0, playfieldSize.Width, playfieldSize.Height);
            Player.Bounds = new RectangleF(playfieldSize.Width / 2f - 20, playfieldSize.Height - 40, 40, 20);

            Score = 0;
            Player.IsAlive = true;
            Player.LivesLeft = 3f;

            Bullets.Clear();
            Aleins.Clear();

            boulders.Clear();

            Player.Upg_StrongerFaster = false;
            Player.Upg_Multishot = false;
            Player.Upg_Defense = false;

            Player.BulletDamage = 5;
            Player.BulletSpeed = 100f;
            Player.DamageMultiplierTaken = 1.0f;
         
           
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

            State = GameState.Playing;


        }

        public void DebugAddScore(int amount) => AddScore(amount);

        public void DebugSetScore(int score)
        {
            Score = Math.Max(0, score);
            ApplyUpgradesIfNeeded(); // important so thresholds trigger
        }

        public void TogglePause()
        {
            if (State == GameState.Playing) State = GameState.Paused;
            else if (State == GameState.Paused) State = GameState.Playing;
        }

        public void GameOverNow()
        {
            SessionScores.Add(Score);
            SessionScores.Sort((a, b) => b.CompareTo(a));
            State = GameState.GameOver;
            Debug.WriteLine($"GAME OVER triggered. Score={Score}, Lives={Player.LivesLeft}, IsAlive={Player.IsAlive}");
        }

        public void GoToTitle()
        {
            State = GameState.Title;
            // optional cleanup so the screen is empty:
            Bullets.Clear();
            Aleins.Clear();
            boulders.Clear();
        }
        public void Update(TimeSpan dt)
        {

            if (State != GameState.Playing) return;

            Input.PreUpdateInput();
           
            Player.Update(dt, Input, Playfield);
            // spawn bullets when firing ( holdtofire & cooldown player)
            if (Player.TryComsumeShot())
            {
                SpawnPlayerBullets();
            }


            AlienMode.Update(this, dt);

            alienShottimer = alienShottimer - (float)dt.TotalSeconds;

            if (alienShottimer <= 0f)
            {
                SpawnAlienBullets();
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


#if DEBUG
            Debug.Assert(Score >= 0);

            //Debug.Assert(Player.BulletCount >= 1);
            Debug.Assert(Player.BulletSpeed > 0);
            Debug.Assert(Player.BulletDamage > 0);

            foreach (var b in Bullets)
            {
                Debug.Assert(!float.IsNaN(b.Bounds.X) && !float.IsNaN(b.Bounds.Y));
            }
#endif

            if (!Player.IsAlive || Player.LivesLeft <= 0f)
                GameOverNow();

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
                    //int minHealth = 10;
                    //int health = Math.Max(minHealth, (r == 0) ? 15 : (r <= 2 ? 10 : 5));

                    //int points = 5;

                    AlienType type;

                    // assign alien type based on row ( top row is strongest, bottom row is weakest)
                    if (r < 1) type = AlienType.Squid;    // rows 0
                    else if (r < 4) type = AlienType.Crab;  // rows 2, 3
                    else type = AlienType.Octopus;            // row 4

                    // health and points based on alien type
                    int health = (type == AlienType.Squid) ? 15 : (type == AlienType.Crab) ? 10 : (type == AlienType.Octopus) ? 5 : 10;
                    int points = (type == AlienType.Squid) ? 30 : (type == AlienType.Crab) ? 20 : (type == AlienType.Octopus) ? 10 : 5;

                    float x = startX + c * (alienW + gapX);
                    float y = startY + r * (alienH + gapY);

                    Aleins.Add(new Alien { Bounds = new RectangleF(x, y, alienW, alienH), Health = health, Points = points, Type = type });

                }
            }
        }
        private void SpawnPlayerBullets()
        {

            float bulletW = 4f;
            float bulletH = 10f;

            float x = Player.Bounds.X + Player.Bounds.Width / 2f - bulletW / 2f;
            float y = Player.Bounds.Y - bulletH; // bullet will start above player 

            float speed = Player.BulletSpeed;
            int damage = Player.BulletDamage;

            // multishot conditional (3 bullets at ±20°)
            bool canUseMultishot = Player.Upg_Multishot && Player.MultishotTimer <= 0f;

            float[] angles = canUseMultishot
                ? new[] { -20f, 0f, +20f }
                : new[] { 0f };

            foreach (float ang in angles)
            {
                var (vx, vy) = FromAngleDegrees(speed, ang);

                Bullets.Add(new bullet
                {
                    Bounds = new RectangleF(x, y, bulletW, bulletH),
                    VelX = vx,
                    VelY = vy,
                    FromAlien = false,
                    Damage = damage
                });
            }

            if (canUseMultishot)
                Player.MultishotTimer = Player.MultishotCooldown;

#if DEBUG
            // test spawn marker ( output window) 
            System.Diagnostics.Debug.WriteLine($"Spawn bullet at X={x:0} Y={y:0}");
#endif
        }

        private void SpawnAlienBullets ()
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
                VelX = 0f,
                VelY = +100f,
                FromAlien = true,
                Damage = 1

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

        private static (float vx, float vy) FromAngleDegrees(float speed, float degreesFromUp)
{
    float rad = degreesFromUp * (float)(Math.PI / 180.0);
    float vx = speed * (float)Math.Sin(rad);
    float vy = -speed * (float)Math.Cos(rad); // up is negative Y in WinForms
    return (vx, vy);
}

        private void ApplyUpgradesIfNeeded()
        {
            // 100 points: stronger + faster bullets
            if (!Player.Upg_StrongerFaster && Score >= 100)
            {
                Player.Upg_StrongerFaster = true;
                Player.BulletDamage += 5;       // was 5 -> becomes 10
                Player.BulletSpeed += 80f;      // was 100 -> becomes 180
#if DEBUG
                Debug.WriteLine("Unlocked 100pt upgrade!");
#endif
            }

            // 500 points: multishot
            if (!Player.Upg_Multishot && Score >= 500)
            {
                Player.Upg_Multishot = true;     // was 1 bullet -> becomes 3
#if DEBUG
                Debug.WriteLine("Unlocked 500pt upgrade!");
              
#endif
            }

            // 1000 points: defense
            if (!Player.Upg_Defense && Score >= 1000)
            {
                Player.Upg_Defense = true;
                Player.DamageMultiplierTaken = 0.5f; // take half damage
#if DEBUG
                Debug.WriteLine("Unlocked 1000pt upgrade!");
#endif
            }



        }

#if DEBUG
        internal void DebugRecalcUpgrades() => ApplyUpgradesIfNeeded();
#endif

        //public void Update(TimeSpan dt) => Player.Update(dt, Input, Playfield);
        public void Draw(Graphics g)
        {
            if (State == GameState.Title || State == GameState.GameOver)
                return;

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

            //Score.Location = new Point(100, 30);
            //Score.Size = new Size(100, 30);
            //Score.Font = new Font("Arial", 12, FontStyle.Bold);
            //Score.ForeColor = Color.White;
            //Score.BackColor = Color.Transparent;
            //Score.BringToFront();
            // update the lives label

#if DEBUG
            //using (var font = new Font("Consolas", 10))
            //{
                //g.DrawString(
                    //$"Score: {Score}\n",
                    //$"Lives: {Player.LivesLeft:0.0}\n" +
                    //$"Upg100:{Player.Upg_StrongerFaster} Upg500:{Player.Upg_Multishot} Upg1000:{Player.Upg_Defense}\n" +
                    //$"Dmg:{Player.BulletDamage} Spd:{Player.BulletSpeed:0} \n" +
                    //$"Bullets:{Bullets.Count} Aliens:{Aleins.Count(a => a.IsAlive)}",
                    //font, Brushes.White, 5, 30);
            //}
#endif
            using (var font = new Font("Arial", 12, FontStyle.Bold))
            {
                g.DrawString($"Score: {Score}", font, Brushes.White, 10f, 10f);
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

        // initialize the speed = 100
        public float Speed = 100f;
        // firecooldown; is instaticiated 
        private float Firecooldown;
        public float MultishotCooldown = 2.0f;   // seconds between multishots
        public float MultishotTimer = 0f;

        // initialize lives = 3 (player can be damaged 3 times before death)
        public float LivesLeft = 3;
        // initialize IsInvulnerable = false (player can be damaged)
        private bool IsInvulnerable = false;
        // initialize invulnerability time = 0 (no invulnerability frames)
        private float InvulnerabilityTime = 0f;
        private float BlinkTimer = 0f; // timer for blinking effect when invulnerable
        private bool BlinkVisible = true; // controls whether the player is visible during blinking
        private const float BlinkInterval = 0.15f; // interval for blinking effect (e.g., 0.15 seconds)
        public float InvulnerabilityDuration = 2.0f;

        // --- Upgrades / stats ---
        public int BulletDamage = 5;        // base
        public float BulletSpeed = 100f;    // base

        public float DamageMultiplierTaken = 1.0f; // 1.0 = normal damage, <1 = more defense

        // tracks which upgrades are unlocked
        public bool Upg_StrongerFaster;   // 100 points
        public bool Upg_Multishot;        // 500 points
        public bool Upg_Defense;          // 1000 points







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

        // load player sprite from resources
        //private Image PlayerSprite = Properties.Resources.Player;

        // bool value showing if the player shooted
        private bool shotThisFrame = false;

        public void DebugSetLives(float lives)
        {
            LivesLeft = Math.Max(0f, lives);
            if (LivesLeft > 0f) IsAlive = true;
        }

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

            if (MultishotTimer > 0f)
            {
                MultishotTimer -= (float)dt.TotalSeconds;
                if (MultishotTimer < 0f) MultishotTimer = 0f;
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

            // load the PlayerSprite from Resources.resx
            if (Sprites.PlayerSprite != null)
            {
                g.DrawImage(Sprites.PlayerSprite, Bounds);
            }

            else
            {
                g.FillRectangle(Brushes.Lime, Bounds);    
            }

        }


        // damage update logic for the player (for testing purposes, will be changed later)
        public void TakesDamage(float amount = 1f)
        {
            if (IsInvulnerable == true || IsAlive == false || LivesLeft <= 0)
            {
                return; // player cannot take damage if they are invulnerable, already dead, or have no lives left
            }

            LivesLeft = LivesLeft - amount * DamageMultiplierTaken;  // reduce lives by 1

            // check if player is dead after taking damage
            if (LivesLeft <= 0f)
            {
                LivesLeft = 0f;  // prevent negative lives
                IsAlive = false; // player is now dead
                IsInvulnerable = true; // player becomes invulnerable because they're already dead
                InvulnerabilityTime = 0f;
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

        public AlienType Type;

        // health 
        public int Health = 10;
        // points 
        public int Points = 5;

        //private Image AlienSprite = Properties.Resources.Octopus1;

        // methods for alien 

        public override void Update(TimeSpan dt, Inputstate input, RectangleF Playfield)
        {
            // will add code to change the behavior of the alien in fleet 

        }

        public void TakeDamage(int damage)
        {
            Health = Health - damage;
            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
            }
        }


        // public override void Draw(Graphics g) => g.FillRectangle(Brushes.Green, Bounds);

        // rewritten Alien Draw
        public override void Draw(Graphics g)
        {
            Image sprite = null;

            switch (Type)
            {
                case AlienType.Octopus:
                    sprite = Sprites.Alien1Sprite;
                    break;

                case AlienType.Crab:
                    sprite = Sprites.Alien2Sprite;
                    break;

                case AlienType.Squid:
                    sprite = Sprites.Alien3Sprite;
                    break;
            }

            if (sprite != null)
            {
                g.DrawImage(sprite, Bounds);
            }

            //// load the Octopus1 sprite for the aliens
            //if (Sprites.Alien1Sprite != null)
            //{
            //    g.DrawImage(Sprites.Alien1Sprite, Bounds);
            //}
            else
            {
                g.FillRectangle(Brushes.Green, Bounds);
            }
        }
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

        private int wave = 0;

        private const float BaseSpeed = 40f;
        private const float SpeedPerWave = 8f;
        private const float MaxSpeed = 160f;

        private const float BaseDrop = 20f;

        public AlienFleetWaveMode(Alienfleet fleet)
        {
            this.fleet = fleet;
        }

        public void Start(GameWorld world)
        {

            wave = wave + 1;
            
            
            world.Aleins.Clear();
            world.SpawnAleinsGrid(rows: 5, cols:11);

            fleet.direction = 1;
            fleet.speed = Math.Min(MaxSpeed, BaseSpeed + SpeedPerWave * (wave - 1));
            fleet.dropamount = BaseDrop;

        }

        public void Update(GameWorld world, TimeSpan dt)
        {

            float bottom = GetfleetBottom(world.Aleins);

            float orginalDrop = fleet.dropamount;

            if (bottom != float.MinValue && bottom >= world.AlienStopLineY)
            {
                //fleet.dropamount = 0f;
                world.GameOverNow();
                return;
            }
            
            fleet.Update(dt, world.Playfield, world.Aleins);

            // if we want to respawn a new wave 


            fleet.dropamount = orginalDrop;

            if (world.Aleins.All(a => !a.IsAlive))
            {

                Start(world);

            }
        }

        private static float GetfleetBottom(List<Alien> aliens)
        {
            float bottom = float.MinValue;

            foreach( var a in aliens)
            {
                if (!a.IsAlive) continue;
                if(a.Bounds.Bottom > bottom) bottom = a.Bounds.Bottom;

            }

            return bottom;

        }

    }

    public sealed class AlienContinuousSpawnMode : IAlienModeController
    {

        private readonly Random random = new Random();

        public float spawnIntervalSpeed = 1.25f;

        public float fallSpeed = 15f;

        public int maxAliensOnScreen = 5;

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
                Bounds = new RectangleF(x, y, w, h),
                Health = 10,   // > 5
                Points = 20,
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

                float dy = fallSpeed * seconds;

                ac.Bounds = new RectangleF(

                    ac.Bounds.X,
                    ac.Bounds.Y + dy,
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




        public class bullet : GameObject
    { 

        public float VelX = 0f;
        public float VelY = -100f;

        public bool FromAlien; // true = alien bullet , false = player bullet

        //private float Firecooldown;

        public int Damage = 5; // player bullet does 5 damage by default

        public int Health = 3;
        public void TakeHit()
        {
            Health--;
            if (Health <= 0)
                IsAlive = false;
        }

        public override void Update(TimeSpan dt, Inputstate Input, RectangleF Playfield)
        {

            float s = (float)dt.TotalSeconds;

            Bounds = new RectangleF(Bounds.X + VelX * s, Bounds.Y + VelY * s, Bounds.Width, Bounds.Height);


            // if the bullet is outside of the playfield, delete instance
            if (Bounds.Bottom < Playfield.Top || Bounds.Top > Playfield.Bottom || Bounds.Right < Playfield.Left || Bounds.Left > Playfield.Right)
                IsAlive = false;

        }


        public override void Draw(Graphics g) {

            Brush brush = FromAlien ? Brushes.Orange : Brushes.Pink;
            
            g.FillRectangle(brush, Bounds);
        
        }

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

        public bool intersectsAnyBlock(RectangleF r)
        {
            for( int i = 0; i < Blocks.Count; i++)
            {
                if (Blocks[i].IntersectsWith(r))
                {
                    return true;
                }


            }

            return false;
        }

        public void DamageByRect(RectangleF area)
        {
            Blocks.RemoveAll(b => b.IntersectsWith(area));
            IsAlive = Blocks.Count > 0;
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

    // add logic that allows only three aliens to go past the playfield before the player dies.

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
#if DEBUG
                        Debug.WriteLine($"Hit alien with HP={target.Health}, bulletDamage={b.Damage}");
#endif

                        //int oldHealth = target.Health;
                        target.TakeDamage(b.Damage);

                        if (!target.IsAlive) // died this hit
                            world.AddScore(target.Points * world.AlienKillScoreMultiplier);
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
                   // Debug.WriteLine("Player hit!");
                    world.Player.TakesDamage(1f);

#if DEBUG
                    Debug.WriteLine($"Player hit! Lives now: {world.Player.LivesLeft}");
#endif
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


                    if(!bo.intersectsAnyBlock(b.Bounds)) continue;

                    b.IsAlive = false;
                    bo.DamageAt(hitPoint, radius: 10f);
                    break;
 
                }


            }
        }
    }


    public sealed class AlienBulletVsBoulderCollision : CollisionSystemBase
    {
        public override void Handle(GameWorld world)
        {
            foreach( var b in world.Bullets)
            {
                if (!b.IsAlive || !b.FromAlien) continue;

                var hitPoint = new PointF(b.Bounds.X + b.Bounds.Width / 2f, b.Bounds.Y + b.Bounds.Height / 2f);

                foreach ( var bo in world.boulders)
                {
                    if (!bo.IsAlive) continue;

                    if (!Hit(b.Bounds, bo.Bounds)) continue;

                    if (!bo.intersectsAnyBlock(b.Bounds)) continue;

                    b.IsAlive = false;
                    bo.DamageAt(hitPoint, radius: 8f);
                    break;


                }
            }
        }
    }

    public sealed class AlienVsBoulderCollision : CollisionSystemBase
    {
        public override void Handle(GameWorld world)
        {
            foreach (var a in world.Aleins)
            {
                if (!a.IsAlive) continue;

                foreach (var bo in world.boulders)
                {
                    if (!bo.IsAlive) continue;

                    // broad-phase: alien overlaps boulder bounds?
                    if (!Hit(a.Bounds, bo.Bounds)) continue;

                    //if (a.Bounds.Bottom < bo.Bounds.Top) continue; // not yet touching shield area

                    // remove blocks that intersect the alien rectangle
                    bo.DamageByRect(a.Bounds);
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

            

        }

    }


}

