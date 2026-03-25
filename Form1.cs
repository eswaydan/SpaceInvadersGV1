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

        public Form1()
        {
            InitializeComponent();
            GamePanel gamePanel1 = new GamePanel();
            gamePanel1.Show();

            gamePanel1.World = _world;
    }

        private void On_KeyDown(KeyEventArgs e)
        {

            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Left) _world.Input.Left_held = true;
            if (e.KeyCode == Keys.Right) _world.Input.Right_held = true;
            if (e.KeyCode == Keys.Space) _world.Input.Fire_held = true;

            e.Handled = true;


            //if (e.KeyCode == Keys.Left)
            //{
            //    MessageBox.Show("The left key was pressed!");
            //    // Setting e.Handled to true here would prevent the base class 
            //    // and any controls from receiving the key press.

            //}


            //base.On_KeyDown(e);


        }

        private void On_KeyUp(KeyEventArgs e)
        {

            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Left) _world.Input.Left_held = false;
            if (e.KeyCode == Keys.Right) _world.Input.Right_held = false;
            if (e.KeyCode == Keys.Space) _world.Input.Fire_held = false;

            e.Handled = true;


            //if (e.KeyCode == Keys.Left)
            //{
            //    MessageBox.Show("The left key was pressed!");
            //    // Setting e.Handled to true here would prevent the base class 
            //    // and any controls from receiving the key press.

            //}


            //base.On_KeyDown(e);


        }
    }
}
