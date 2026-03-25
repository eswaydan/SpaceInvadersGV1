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
    public partial class GamePanel : Panel
    {

        public GameWorld World {  get; set; }

        public GamePanel()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            pe.Graphics.Clear(Color.Black);
        }
    }

    public class GameWorld
    {

        public Inputstate Input = new Inputstate();

        public void Update(float dt)
        {

        }



        private TimeSpan LastFrameTime;

        Stopwatch timer = new Stopwatch();

        
        public TimeSpan CalculateDeltaTime()
        {
            TimeSpan currentTime = timer.Elapsed;

            TimeSpan dt = currentTime - LastFrameTime;

            LastFrameTime = currentTime;

            return dt;
        }



    }

    public sealed class Inputstate
    {
        public bool Left_held;
        public bool Right_held;
        public bool Fire_held;

        public bool FirePressed;
        internal bool Fire_was_held_last_frame;

        public void PreUpdateInput()
        {

        }

        private readonly GameWorld _world = new GameWorld();


        // moved to Form1
        //protected override void On_KeyDown(KeyEventArgs e) {

        //    base.On_KeyDown(e);

        //    if (e.KeyCode == Keys.Left) _world.Input.Left_held = true;
        //    if (e.KeyCode == Keys.Right) _world.Input.Right_held = true;
        //    if (e.KeyCode == Keys.Space) _world.Input.Fire_held = true;

        //    e.Handled = true;


        //    //if (e.KeyCode == Keys.Left)
        //    //{
        //    //    MessageBox.Show("The left key was pressed!");
        //    //    // Setting e.Handled to true here would prevent the base class 
        //    //    // and any controls from receiving the key press.

        //    //}


        //      //base.On_KeyDown(e);
        
        
        //}


    }




}
