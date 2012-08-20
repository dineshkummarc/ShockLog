﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShockLog
{
    public partial class MainWindow : Form
    {
        #region DllImport
        // Import Win32 API system menu functions
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;
        public const Int32 IDM_ABOUT = 1000;
        #endregion

        #region Private Fields
        private Logger logger = new Logger();
        #endregion

        #region Form Load and Close
        public MainWindow()
        {
            // Set UI font to system font
            this.Font = SystemFonts.MessageBoxFont;
            // Initialise UI
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Add 'About' to System Menu
            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(sysMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(sysMenuHandle, 6, MF_BYPOSITION, IDM_ABOUT, "About ShockLog...");
            // Add event handlers
            logger.PeakLevelMeterUpdate += new Logger.LevelEventHandler(LevelUpdate);
            logger.StatusChange += new EventHandler(StatusUpdate);
            // Load settings
            bitrateUpDown.Value = Properties.Settings.Default.Bitrate;
            lengthUpDown.Value = Properties.Settings.Default.Length;
            if (Properties.Settings.Default.Folder == String.Empty) // If a folder has not been set, set to My Music
            {
                folderLabel.Text = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
            }
            else // Else if folder has been set, use it
            {
                folderLabel.Text = Properties.Settings.Default.Folder;
            }
            organiseCheckBox.Checked = Properties.Settings.Default.OrganiseFolder;
            clearCheckBox.Checked = Properties.Settings.Default.DeleteOld;
            clearUpDown.Value = Properties.Settings.Default.DeleteTime;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings
            Properties.Settings.Default.Bitrate = (int)bitrateUpDown.Value;
            Properties.Settings.Default.Length = (int)lengthUpDown.Value;
            Properties.Settings.Default.Folder = folderLabel.Text;
            Properties.Settings.Default.OrganiseFolder = organiseCheckBox.Checked;
            Properties.Settings.Default.DeleteOld = clearCheckBox.Checked;
            Properties.Settings.Default.DeleteTime = (int)clearUpDown.Value;
            Properties.Settings.Default.Save();
        }
        #endregion

        /// <summary>
        /// WndProc override to respond to About window click on System Menu
        /// </summary>
        /// <param name="m">WndProc message</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND) // If WndProc message is system menu item click
            {
                switch (m.WParam.ToInt32()) // For item clicked
                {
                    case IDM_ABOUT: // If about item
                        // Open about window
                        AboutWindow aboutWindow = new AboutWindow();
                        aboutWindow.ShowDialog();
                        return;
                    default:
                        break;
                }
            }
            // Run base function
            base.WndProc(ref m);
        }

        #region Buttons
        /// <summary>
        /// Starts or stops the logger depending on the logger state
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void stopStartButton_Click(object sender, EventArgs e)
        {
            if (logger.CurrentStatus == Logger.Status.NOTLOGGING) // If not logging
            {
                logger.Start(); // Start the logger
            }
            else if (logger.CurrentStatus == Logger.Status.LOGGING) // If currently logging
            {
                logger.Stop(); // Stop the logger
            }
        }

        /// <summary>
        /// Changes window size based on whether check box is checked, revealing or hiding options
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void expanderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked) // If checked to show more options
            {
                this.Height = 330;
            }
            else // Else if not checked
            {
                this.Height = 150;
            }
        }
        #endregion

        /// <summary>
        /// Update levels on the volume meters
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void LevelUpdate(object sender, Logger.LevelEventArgs e)
        {
            if (IsHandleCreated) // If the window has a handle
            {
                // Invoke on form thread
                BeginInvoke((MethodInvoker)delegate
                {
                    leftVolumeMeter.Amplitude = (float)e.LeftLevel;
                    rightVolumeMeter.Amplitude = (float)e.RightLevel;
                });
            }
        }

        /// <summary>
        /// Update the window to reflect encoder status
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void StatusUpdate(object sender, EventArgs e)
        {
            if (logger.CurrentStatus == Logger.Status.LOGGING) // If logging
            {
                // Change text to logging
                statusLabel.Text = "Logging";
                stopStartButton.Text = "Stop Logging";
                // Disable options
                fileSeperator.Enabled = false;
                bitrateLabel.Enabled = false;
                bitrateUpDown.Enabled = false;
                lengthLabel.Enabled = false;
                lengthUpDown.Enabled = false;
                folderSeperator.Enabled = false;
                folderLabel.Enabled = false;
                browseButton.Enabled = false;
                organiseCheckBox.Enabled = false;
                clearCheckBox.Enabled = false;
                clearUpDown.Enabled = false;
                clearLabel.Enabled = false;
            }
            else if (logger.CurrentStatus == Logger.Status.NOTLOGGING) // If not logging
            {
                // Change text to not logging
                statusLabel.Text = "Not Logging";
                stopStartButton.Text = "Start Logging";
                // Enable options
                fileSeperator.Enabled = true;
                bitrateLabel.Enabled = true;
                bitrateUpDown.Enabled = true;
                lengthLabel.Enabled = true;
                lengthUpDown.Enabled = true;
                folderSeperator.Enabled = true;
                folderLabel.Enabled = true;
                browseButton.Enabled = true;
                organiseCheckBox.Enabled = true;
                clearCheckBox.Enabled = true;
                clearUpDown.Enabled = true;
                clearLabel.Enabled = true;
            }
        }
    }
}
