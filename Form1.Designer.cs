namespace SpaceInvadersGV1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.gamePanel1 = new SpaceInvadersGV1.GamePanel();
            this.lblPause = new System.Windows.Forms.Label();
            this.lblScoresTitle = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnTitle = new System.Windows.Forms.Button();
            this.lblFinalScore = new System.Windows.Forms.Label();
            this.lblGameOver = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnMode = new System.Windows.Forms.Button();
            this.lblMode = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lstScores = new System.Windows.Forms.ListBox();
            this.btnPause = new System.Windows.Forms.Button();
            this.lblLives = new System.Windows.Forms.Label();
            this.gamePanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // gamePanel1
            // 
            this.gamePanel1.Controls.Add(this.lblPause);
            this.gamePanel1.Controls.Add(this.lblScoresTitle);
            this.gamePanel1.Controls.Add(this.btnExit);
            this.gamePanel1.Controls.Add(this.btnTitle);
            this.gamePanel1.Controls.Add(this.lblFinalScore);
            this.gamePanel1.Controls.Add(this.lblGameOver);
            this.gamePanel1.Controls.Add(this.btnStart);
            this.gamePanel1.Controls.Add(this.btnMode);
            this.gamePanel1.Controls.Add(this.lblMode);
            this.gamePanel1.Controls.Add(this.lblTitle);
            this.gamePanel1.Controls.Add(this.lstScores);
            this.gamePanel1.Controls.Add(this.btnPause);
            this.gamePanel1.Controls.Add(this.lblLives);
            this.gamePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamePanel1.Location = new System.Drawing.Point(0, 0);
            this.gamePanel1.Margin = new System.Windows.Forms.Padding(4);
            this.gamePanel1.Name = "gamePanel1";
            this.gamePanel1.Size = new System.Drawing.Size(1067, 554);
            this.gamePanel1.TabIndex = 0;
            this.gamePanel1.World = null;
            // 
            // lblPause
            // 
            this.lblPause.AutoSize = true;
            this.lblPause.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPause.Location = new System.Drawing.Point(442, 178);
            this.lblPause.Name = "lblPause";
            this.lblPause.Size = new System.Drawing.Size(183, 46);
            this.lblPause.TabIndex = 13;
            this.lblPause.Text = "PAUSED";
            // 
            // lblScoresTitle
            // 
            this.lblScoresTitle.AutoSize = true;
            this.lblScoresTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScoresTitle.Location = new System.Drawing.Point(464, 335);
            this.lblScoresTitle.Name = "lblScoresTitle";
            this.lblScoresTitle.Size = new System.Drawing.Size(139, 24);
            this.lblScoresTitle.TabIndex = 12;
            this.lblScoresTitle.Text = "LeaderBoard:";
            // 
            // btnExit
            // 
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.Location = new System.Drawing.Point(548, 248);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(194, 68);
            this.btnExit.TabIndex = 11;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnTitle
            // 
            this.btnTitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTitle.Location = new System.Drawing.Point(325, 248);
            this.btnTitle.Name = "btnTitle";
            this.btnTitle.Size = new System.Drawing.Size(194, 68);
            this.btnTitle.TabIndex = 5;
            this.btnTitle.Text = "Main Menu";
            this.btnTitle.UseVisualStyleBackColor = true;
            this.btnTitle.Click += new System.EventHandler(this.btnTitle_Click);
            // 
            // lblFinalScore
            // 
            this.lblFinalScore.AutoSize = true;
            this.lblFinalScore.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFinalScore.Location = new System.Drawing.Point(434, 194);
            this.lblFinalScore.Name = "lblFinalScore";
            this.lblFinalScore.Size = new System.Drawing.Size(123, 24);
            this.lblFinalScore.TabIndex = 10;
            this.lblFinalScore.Text = "Final Score:";
            // 
            // lblGameOver
            // 
            this.lblGameOver.AutoSize = true;
            this.lblGameOver.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGameOver.Location = new System.Drawing.Point(402, 95);
            this.lblGameOver.Name = "lblGameOver";
            this.lblGameOver.Size = new System.Drawing.Size(263, 46);
            this.lblGameOver.TabIndex = 9;
            this.lblGameOver.Text = "GAME OVER";
            // 
            // btnStart
            // 
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(325, 248);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(194, 68);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnMode
            // 
            this.btnMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMode.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMode.Location = new System.Drawing.Point(596, 189);
            this.btnMode.Name = "btnMode";
            this.btnMode.Size = new System.Drawing.Size(119, 35);
            this.btnMode.TabIndex = 8;
            this.btnMode.Text = "Change Mode";
            this.btnMode.UseVisualStyleBackColor = true;
            this.btnMode.Click += new System.EventHandler(this.btnMode_Click);
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMode.Location = new System.Drawing.Point(352, 194);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(189, 24);
            this.lblMode.TabIndex = 7;
            this.lblMode.Text = "Mode: Fleet Waves";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(350, 95);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(367, 46);
            this.lblTitle.TabIndex = 6;
            this.lblTitle.Text = "SPACE INVADERS";
            // 
            // lstScores
            // 
            this.lstScores.BackColor = System.Drawing.Color.Black;
            this.lstScores.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstScores.ForeColor = System.Drawing.Color.White;
            this.lstScores.FormattingEnabled = true;
            this.lstScores.ItemHeight = 21;
            this.lstScores.Location = new System.Drawing.Point(325, 370);
            this.lstScores.Name = "lstScores";
            this.lstScores.Size = new System.Drawing.Size(417, 172);
            this.lstScores.TabIndex = 4;
            // 
            // btnPause
            // 
            this.btnPause.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPause.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPause.ForeColor = System.Drawing.Color.White;
            this.btnPause.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnPause.Location = new System.Drawing.Point(481, 12);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(104, 44);
            this.btnPause.TabIndex = 3;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // lblLives
            // 
            this.lblLives.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLives.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblLives.Location = new System.Drawing.Point(924, 18);
            this.lblLives.Name = "lblLives";
            this.lblLives.Size = new System.Drawing.Size(124, 30);
            this.lblLives.TabIndex = 1;
            this.lblLives.Text = "Lives";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.gamePanel1);
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.gamePanel1.ResumeLayout(false);
            this.gamePanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GamePanel gamePanel1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblLives;
        private System.Windows.Forms.ListBox lstScores;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnTitle;
        private System.Windows.Forms.Button btnMode;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFinalScore;
        private System.Windows.Forms.Label lblGameOver;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblScoresTitle;
        private System.Windows.Forms.Label lblPause;
    }
}

