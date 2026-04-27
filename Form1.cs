using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceInvadersGV1
{
    public partial class Form1 : Form
    {

        private readonly GameWorld _world = new GameWorld();

        // datetime tuple (year,month,day,hour,min,sec), timespan can use when 
        // calculating difference.
        private DateTime last;

        public Form1()
        {
            InitializeComponent();

            KeyPreview = true; // makes sure that key data is retreived
            gamePanel1.World = _world;

            _world.StartNewGame(gamePanel1.ClientSize);

            last = DateTime.UtcNow; // last frame
            timer1.Interval = 16; // use a mix of timer and dt to acumulate data 
            timer1.Start();

            gamePanel1.Focus(); //calls the panel to receive active keyboard input
            //gamePanel1.Controls.Add(_world.Lives);  // add lives to gamePanel
        }



        protected override void OnKeyDown(KeyEventArgs e)
        {

            base.OnKeyDown(e);

            // for test purposes of alien fleet contiuous mode

            if (e.KeyCode == Keys.F)
            {

                // toggle between modes and restart alien setup

                if (_world.AlienMode is AlienContinuousSpawnMode)
                {
                    _world.SetAlienMode(new AlienFleetWaveMode(_world.GetFleet()));
                }
                else
                {
                    _world.SetAlienMode(new AlienContinuousSpawnMode());
                }

                gamePanel1.Invalidate();
                return;
            }


            // checking what the user did press by the user 
            if (e.KeyCode == Keys.Left) _world.Input.Left_held = true;
            if (e.KeyCode == Keys.Right) _world.Input.Right_held = true;
            if (e.KeyCode == Keys.Space) _world.Input.Fire_held = true;

            e.Handled = true;
            // use this if unwanted movement try this
            //e.SuppressKeyPress = true;


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

            var current = DateTime.UtcNow; //datetime tuple (year,month,day,hour,min,sec)

            TimeSpan dt = current - last;
            last = current;

            if(dt > TimeSpan.FromMilliseconds(50)) // ngl idk what this does (please search what this does) 
                dt = TimeSpan.FromMilliseconds(50);

            _world.Update(dt);

            gamePanel1.Invalidate(); // to see movement

            // use this for test cases
            int alienBullets = _world.Bullets.Count(x => x.FromAlien);
            this.Text = $"L:{_world.Input.Left_held} R:{_world.Input.Right_held} X:{_world.Player.Bounds.X:0} S:{_world.Input.Fire_held} AlienBullets:{alienBullets}  Blocks:{_world.boulders.Sum(b => b.Blocks.Count)} Bullets:{_world.Bullets.Count} Lives:{_world.Player.LivesLeft} Score: {_world.Score}";
            //this.Text = $"L:{_world.Input.Left_held} R:{_world.Input.Right_held} X:{_world.Player.Bounds.X:0} S:{_world.Input.Fire_held} BCount:{_world.Bullets.Count :0}";
            //this.Text = $"L:{_world.Input.Left_held} R:{_world.Input.Right_held} X:{_world.bullet.Bounds.Y:0}";
            //this.Text = $"Lives:{_world.Player.LivesLeft}  Bullets:{_world.Bullets.Count}";
        }
    }
}
