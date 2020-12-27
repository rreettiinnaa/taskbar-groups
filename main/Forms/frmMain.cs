﻿using client.Classes;
using client.User_controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace client
{

    public partial class frmMain : Form
    {

        // Allow doubleBuffering drawing each frame to memory and then onto screen
        // Solves flickering issues mostly as the entire rendering of the screen is done in 1 operation after being first loaded to memory
        protected override CreateParams CreateParams

        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public Category ThisCategory { get; set; }

        public Color HoverColor { get; set; }

        private string passedDirec;
        //private Category cat;
        public Point mouseClick;

        public frmMain(string passedDirectory, int cursorPosX, int cursorPosY)
        {
            System.Runtime.ProfileOptimization.StartProfile("frmMain.Profile");
            mouseClick = new Point(cursorPosX, cursorPosY); // Consstruct point p based on passed x y mouse values
            passedDirec = passedDirectory;
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;

            //InitializeComponent();

            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes("GroupIcon.ico")))
                this.Icon = new Icon(ms);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //System.Diagnostics.Debugger.Launch();

            if (Directory.Exists(@MainPath.path + @"\config\" + passedDirec))
            {
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                ThisCategory = new Category($"config\\{passedDirec}");
                this.BackColor = ImageFunctions.FromString(ThisCategory.ColorString);
                Opacity = (1-(ThisCategory.Opacity/100));

                if (BackColor.R * 0.2126 + BackColor.G * 0.7152 + BackColor.B * 0.0722 > 255 / 2)
                    //if backcolor is light, set hover color as darker
                    HoverColor = Color.FromArgb(BackColor.A, (BackColor.R - 50), (BackColor.G - 50), (BackColor.B - 50));
                else
                    //light backcolor is light, set hover color as darker
                    HoverColor = Color.FromArgb(BackColor.A, (BackColor.R + 50), (BackColor.G + 50), (BackColor.B + 50));

                LoadCategory();
                SetLocation();
            }
            else
            {
                Application.Exit();
            }
        }

        private void SetLocation()
        {
            List<Rectangle> taskbarList = FindDockedTaskBars();
            Rectangle taskbar = new Rectangle();
            Rectangle screen = new Rectangle();

            int i = 0;
            int locationy;
            int locationx;
            if (taskbarList.Count != 0)
            {
                foreach (var scr in Screen.AllScreens) // Get what screen user clicked on
                {
                    if (scr.Bounds.Contains(mouseClick))
                    {
                        screen.X = scr.Bounds.X;
                        screen.Y = scr.Bounds.Y;
                        screen.Width = scr.Bounds.Width;
                        screen.Height = scr.Bounds.Height;
                        taskbar = taskbarList[i];
                    }
                    i++;
                }

                if (taskbar.Contains(mouseClick)) // Click on taskbar
                {
                    if (taskbar.Top == screen.Top && taskbar.Width == screen.Width)
                    {
                        // TOP
                        locationy = screen.Y + taskbar.Height + 10;
                        locationx = mouseClick.X - (this.Width / 2);
                    }
                    else if (taskbar.Bottom == screen.Bottom && taskbar.Width == screen.Width)
                    {
                        // BOTTOM
                        locationy = screen.Y + screen.Height - this.Height - taskbar.Height - 10;
                        locationx = mouseClick.X - (this.Width / 2);
                    }
                    else if (taskbar.Left == screen.Left)
                    {
                        // LEFT
                        locationy = mouseClick.X - (this.Height / 2);
                        locationx = screen.X + taskbar.Width + 10;

                    }
                    else
                    {
                        // RIGHT
                        locationy = mouseClick.X - (this.Height / 2);
                        locationx = screen.X + screen.Width - this.Width - taskbar.Width - 10;
                    }

                }
                else // not click on taskbar
                {
                    locationy = mouseClick.Y - this.Height - 20;
                    locationx = mouseClick.X - (this.Width / 2);
                }

                this.Location = new Point(locationx, locationy);
                if (this.Left < screen.Left)
                    this.Left = screen.Left + 10;
                if (this.Top < screen.Top)
                    this.Top = screen.Top + 10;
                if (this.Right > screen.Right)
                    this.Left = screen.Right - this.Width - 10;
            }
            else // not click on taskbar
            {
                foreach (var scr in Screen.AllScreens) // get what screen user clicked on
                {
                    if (scr.Bounds.Contains(mouseClick))
                    {
                        screen.X = scr.Bounds.X;
                        screen.Y = scr.Bounds.Y;
                        screen.Width = scr.Bounds.Width;
                        screen.Height = scr.Bounds.Height;
                    }
                    i++;
                }

                if (mouseClick.Y > Screen.PrimaryScreen.Bounds.Height - 35)
                    locationy = Screen.PrimaryScreen.Bounds.Height - this.Height - 45;
                else
                    locationy = mouseClick.Y - this.Height - 20;
                locationx = mouseClick.X - (this.Width / 2);

                this.Location = new Point(locationx, locationy);
                if (this.Left < screen.Left)
                    this.Left = screen.Left + 10;
                if (this.Top < screen.Top)
                    this.Top = screen.Top + 10;
                if (this.Right > screen.Right)
                    this.Left = screen.Right - this.Width - 10;
            }
        }

        private void LoadCategory()
        {
            this.Width = 0;
            this.Height = 45;
            int x = 0;
            int y = 0;
            int width = ThisCategory.Width;
            int columns = 1;

            // Check if icon caches exist for the category being loaded
            // If not then rebuild the icon cache
            if (!Directory.Exists(@MainPath.path + @"\config\" + ThisCategory.Name + @"\Icons\"))
            {
                ThisCategory.cacheIcons();
            }

            foreach (ProgramShortcut psc in ThisCategory.ShortcutList)
            {

                if (columns > width)  // creating new row if there are more psc than max width
                {
                    x = 0;
                    y += 45;
                    this.Height += 45;
                    columns = 1;
                }

                if (this.Width < ((width * 55)))
                    this.Width += (55);

                BuildShortcutPanel(x, y, psc);
                x += 55;
                columns++;
            }
        }

        private void BuildShortcutPanel(int x, int y, ProgramShortcut psc)
        {
            ucShortcut pscPanel = new ucShortcut(psc, this, ThisCategory);
            pscPanel.Location = new System.Drawing.Point(x, y);
            this.Controls.Add(pscPanel);
            pscPanel.Show();
            pscPanel.BringToFront();

            // Having some issues with the uc build, so keeping the old code below

            //this.shortcutPic = new System.Windows.Forms.PictureBox();
            //this.shortcutPic.BackColor = System.Drawing.Color.Transparent;
            //this.shortcutPic.Location = new System.Drawing.Point(25, 15);
            //this.shortcutPic.Size = new System.Drawing.Size(25, 25);
            //this.shortcutPic.BackgroundImage = ThisCategory.loadImageCache(psc.FilePath); // Use the local icon cache for the file specified as the icon image
            //this.shortcutPic.BackgroundImageLayout = ImageLayout.Stretch;
            //this.shortcutPic.TabStop = false;
            //this.shortcutPic.Click += new System.EventHandler((sender, e) => OpenFile(sender, e, psc.FilePath));
            //this.shortcutPic.Cursor = System.Windows.Forms.Cursors.Hand;
            //this.shortcutPanel.Controls.Add(this.shortcutPic);
            //this.shortcutPic.Show();
            //this.shortcutPic.BringToFront();
            //this.shortcutPic.MouseEnter += new System.EventHandler((sender, e) => this.shortcutPanel.BackColor = Color.Black);
            //this.shortcutPic.MouseLeave += new System.EventHandler((sender, e) => this.shortcutPanel.BackColor = System.Drawing.Color.Transparent);

        }


        public void OpenFile(object sender, EventArgs e, string path)
        {
            // starting program from psc panel click
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = path;

            try
            {
                proc.Start();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        public static List<Rectangle> FindDockedTaskBars()
        {
            List<Rectangle> dockedRects = new List<Rectangle>();
            foreach (var tmpScrn in Screen.AllScreens)
            {
                if (!tmpScrn.Bounds.Equals(tmpScrn.WorkingArea))
                {
                    Rectangle rect = new Rectangle();

                    var leftDockedWidth = Math.Abs((Math.Abs(tmpScrn.Bounds.Left) - Math.Abs(tmpScrn.WorkingArea.Left)));
                    var topDockedHeight = Math.Abs((Math.Abs(tmpScrn.Bounds.Top) - Math.Abs(tmpScrn.WorkingArea.Top)));
                    var rightDockedWidth = ((tmpScrn.Bounds.Width - leftDockedWidth) - tmpScrn.WorkingArea.Width);
                    var bottomDockedHeight = ((tmpScrn.Bounds.Height - topDockedHeight) - tmpScrn.WorkingArea.Height);
                    if ((leftDockedWidth > 0))
                    {
                        rect.X = tmpScrn.Bounds.Left;
                        rect.Y = tmpScrn.Bounds.Top;
                        rect.Width = leftDockedWidth;
                        rect.Height = tmpScrn.Bounds.Height;
                    }
                    else if ((rightDockedWidth > 0))
                    {
                        rect.X = tmpScrn.WorkingArea.Right;
                        rect.Y = tmpScrn.Bounds.Top;
                        rect.Width = rightDockedWidth;
                        rect.Height = tmpScrn.Bounds.Height;
                    }
                    else if ((topDockedHeight > 0))
                    {
                        rect.X = tmpScrn.WorkingArea.Left;
                        rect.Y = tmpScrn.Bounds.Top;
                        rect.Width = tmpScrn.WorkingArea.Width;
                        rect.Height = topDockedHeight;
                    }
                    else if ((bottomDockedHeight > 0))
                    {
                        rect.X = tmpScrn.WorkingArea.Left;
                        rect.Y = tmpScrn.WorkingArea.Bottom;
                        rect.Width = tmpScrn.WorkingArea.Width;
                        rect.Height = bottomDockedHeight;
                    }
                    else
                    {
                        // Nothing found!
                    }

                    dockedRects.Add(rect);
                }
            }

            if (dockedRects.Count == 0)
            {
                // Taskbar is set to "Auto-Hide".
            }

            return dockedRects;
        }
        private void frmMain_Deactivate(object sender, EventArgs e)
        {
            // closes program if user clicks outside form
            this.Close();
        }
        //
        // endregion
        //
        public System.Windows.Forms.PictureBox shortcutPic;
        public System.Windows.Forms.Panel shortcutPanel;


        //
        // END OF CLASS
        //
    }
}
