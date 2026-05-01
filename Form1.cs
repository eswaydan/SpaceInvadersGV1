using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static SpaceInvadersGV1.GameWorld;

namespace SpaceInvadersGV1
{
    public partial class Form1 : Form
    {
        private GameState _lastState;
        private readonly GameWorld _world = new GameWorld();

        // datetime tuple (year,month,day,hour,min,sec), timespan can use when 
        // calculating difference.
        private DateTime last;

        public Form1()
        {
            InitializeComponent();


            // make sure that lives and pause are visuable  
            lblLives.BringToFront();
            lblPause.BringToFront();

            // use draw mode to make the list centered  
            lstScores.DrawMode = DrawMode.OwnerDrawFixed;
            lstScores.DrawItem += lstScores_DrawItem;

            // take note to change of game state
            _lastState = _world.State;
            UpdateUiForState();
            KeyPreview = true; // makes sure that key data is retreived
            gamePanel1.World = _world;

        
            // take note of change of frame
            last = DateTime.UtcNow; // last frame
            timer1.Interval = 16; // use a mix of timer and dt to acumulate data 
            timer1.Start();

            UpdateUiForState();
            gamePanel1.Focus(); //calls the panel to receive active keyboard input
        }


        // method that takes in the key input that is pressed/clicked by the user and do actions accordingly 
        protected override void OnKeyDown(KeyEventArgs e)
        {

            base.OnKeyDown(e);

            // Pause and unpause game on "ESC" key
            if (e.KeyCode == Keys.Escape)
            {
                // if state is playing and or paused 
                if (_world.State == GameState.Playing || _world.State == GameState.Paused)
                {
                    ClearInput();
                    _world.TogglePause(); // pause
                    UpdateUiForState();
                }
                else if (_world.State == GameState.GameOver) 
                {
                    ClearInput();
                    _world.GoToTitle();
                    UpdateUiForState();
                    gamePanel1.Invalidate(); 
                }
                gamePanel1.Focus(); // return focus to the panel after handling pause/gameover
                return; // exit early since we've handled this key
            }

            // for test purposes of alien fleet contiuous mode

#if DEBUG
            if (e.KeyCode == Keys.F)
            {
                if (_world.State != GameState.Title) return;

                _world.SelectedMode = (_world.SelectedMode == ModeChoice.FleetWaves)
                    ? ModeChoice.Continuous
                    : ModeChoice.FleetWaves;

                UpdateUiForState(); // updates lblMode
            }
#endif
            
            if (_world.State != GameState.Playing) // if state is not playing ignore taking in left, right, space data
                return;

            // checking what the user did press by the user 
            if (e.KeyCode == Keys.Left) _world.Input.Left_held = true;
            if (e.KeyCode == Keys.Right) _world.Input.Right_held = true;
            if (e.KeyCode == Keys.Space) _world.Input.Fire_held = true;

            e.Handled = true;
            // use this if unwanted movement try this
            //e.SuppressKeyPress = true;

#if DEBUG
            // Jump score to thresholds
            if (e.KeyCode == Keys.D1) _world.DebugSetScore(100);   // stronger/faster
            if (e.KeyCode == Keys.D2) _world.DebugSetScore(500);   // multishot
            if (e.KeyCode == Keys.D3) _world.DebugSetScore(1000);  // defense

            // Add score increments
            if (e.KeyCode == Keys.Q) _world.DebugAddScore(10);
            if (e.KeyCode == Keys.W) _world.DebugAddScore(100);

            // Remove lives quickly
            if (e.KeyCode == Keys.K) _world.Player.TakesDamage(1f);     // lose 1 life
            if (e.KeyCode == Keys.R) _world.StartNewGame(gamePanel1.ClientSize);

            // Reset lives
            if (e.KeyCode == Keys.B) _world.Player.DebugSetLives(3f);

            // Optional: toggle invulnerability off (if you have a setter)
#endif


        }

        
        protected override void OnKeyUp(KeyEventArgs e)
        {

            base.OnKeyUp(e);
            // checking what the user did not press by the user 
            if (e.KeyCode == Keys.Left) _world.Input.Left_held = false;
            if (e.KeyCode == Keys.Right) _world.Input.Right_held = false;
            if (e.KeyCode == Keys.Space) _world.Input.Fire_held = false;

            e.Handled = true;
            // use this if unwanted movement try this
            //e.SuppressKeyPress = true;

        }

        // dt now is in form 1 so when player presses pause in game, game can still get user's key input
        private void timer1_Tick(object sender, EventArgs e)
        {

            lblLives.BackColor = Color.Transparent;
            lblLives.ForeColor = Color.White;
            lblLives.BringToFront();


            var current = DateTime.UtcNow; //datetime tuple (year,month,day,hour,min,sec)

            TimeSpan dt = current - last;
            last = current;

            if(dt > TimeSpan.FromMilliseconds(50)) // ngl idk what this does (please search what this does) 
                dt = TimeSpan.FromMilliseconds(50);

            if (_world.State == GameState.Playing)
            {
                _world.Update(dt);

            }

            // added lives label

            // update the lives label
            lblLives.Text = $"Lives: {_world.Player.LivesLeft:0.0}";

            // check change in states ( if states did change) 
            if (_world.State != _lastState)
            {
                _lastState = _world.State;

                if (_world.State == GameState.GameOver) // if state changed to gameover
                {
                    
                    PopulateScoreboard(); // add score to score board 
                    lblFinalScore.Text = $"Final Score: {_world.Score}"; // display the current session score in gameover state
                }

                UpdateUiForState(); // check again for change in state --> update UI accordingly
       
            }

            gamePanel1.Invalidate(); // to see movement

#if DEBUG
            // test cases
            int alienBullets = _world.Bullets.Count(x => x.FromAlien);
            this.Text = $"L:{_world.Input.Left_held} R:{_world.Input.Right_held} X:{_world.Player.Bounds.X:0} S:{_world.Input.Fire_held} AlienBullets:{alienBullets}  Blocks:{_world.boulders.Sum(b => b.Blocks.Count)} Bullets:{_world.Bullets.Count} Lives:{_world.Player.LivesLeft} Score: {_world.Score}";
            //this.Text = $"L:{_world.Input.Left_held} R:{_world.Input.Right_held} X:{_world.Player.Bounds.X:0} S:{_world.Input.Fire_held} BCount:{_world.Bullets.Count :0}";
            //this.Text = $"L:{_world.Input.Left_held} R:{_world.Input.Right_held} X:{_world.bullet.Bounds.Y:0}";
            //this.Text = $"Lives:{_world.Player.LivesLeft}  Bullets:{_world.Bullets.Count}";
#endif
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            ClearInput(); 
            lblLives.BringToFront(); 
            _world.StartNewGame(gamePanel1.ClientSize); // build game display 
            UpdateUiForState(); // check for game state change 
            gamePanel1.Invalidate(); // see movement 

            gamePanel1.Focus(); // gain focus to input from game panel 
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            ClearInput();
            _world.TogglePause(); // build pause display 
            UpdateUiForState(); // check game state 
            gamePanel1.Focus();
        }

        private void UpdateUiForState()
        {
            // UI game states 
            bool onTitle = _world.State == GameState.Title;
            bool playingOrPaused = _world.State == GameState.Playing || _world.State == GameState.Paused;
            bool onGameOver = _world.State == GameState.GameOver;
            bool paused = _world.State == GameState.Paused;

            // HUD
            lblPause.Visible = paused;
            lblLives.Visible = playingOrPaused;
            btnPause.Visible = playingOrPaused;

            // Title controls
            lblTitle.Visible = onTitle;      // "SPACE INVADERS" 
            lblMode.Visible = onTitle;
            btnMode.Visible = onTitle;
            btnStart.Visible = onTitle;


            if (onTitle)
                lblMode.Text = $"Mode: {_world.SelectedMode}";



            // GameOver controls
            lblScoresTitle.Visible = onGameOver;
            lstScores.Visible = onGameOver;
            btnTitle.Visible = (paused || onGameOver);
            lblGameOver.Visible = onGameOver;
            lblFinalScore.Visible = onGameOver; 
            btnExit.Visible = (onTitle || onGameOver || paused);
        }


        // method deals with how the leaderboard will show up in display
        private void lstScores_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;

            string text = lstScores.Items[e.Index].ToString();

            using (var brush = new SolidBrush(e.ForeColor))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                e.Graphics.DrawString(text, e.Font, brush, e.Bounds, format);
            }

            e.DrawFocusRectangle();
        }

        // add scores to the list of sessionscores collected throughout game session
        private void PopulateScoreboard()
        {
            lstScores.BeginUpdate();
            try // getting the top score --> display scores in order of highest score to lowest
            {
                lstScores.Items.Clear();
                for (int i = 0; i < Math.Min(10, _world.SessionScores.Count); i++)
                    lstScores.Items.Add($"{i + 1}.  {_world.SessionScores[i]}");
            }
            finally
            {
                lstScores.EndUpdate(); // always end updating the list of scores 
            }
        }


        // helper method clear user key input 
        private void ClearInput()
        {
            _world.Input.Left_held = false;
            _world.Input.Right_held = false;
            _world.Input.Fire_held = false;
        }

        private void btnTitle_Click(object sender, EventArgs e)
        {
            _world.GoToTitle(); // build title screen components 
            UpdateUiForState();
            gamePanel1.Invalidate();
            gamePanel1.Focus();
        }


        private void btnMode_Click(object sender, EventArgs e)
        {
            if (_world.State != GameState.Title) return;

            // use ternary operator to change the mode of gameplay based on modechoice 
            _world.SelectedMode = (_world.SelectedMode == ModeChoice.FleetWaves)
                ? ModeChoice.Continuous
                : ModeChoice.FleetWaves;

            lblMode.Text = $"Mode: {_world.SelectedMode}"; // display mode chosen
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close(); // exit the program
        }
    }
}
