using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SMX
{
    /// <summary>
    /// Author: Iñaki Ayucar (http://graphicdna.blogspot.com)
    /// Date: 23/11/2007
    /// 
    /// This software is distributed "for free" for any non-commercial usage. The software is provided “as-is.” 
    /// You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.
    /// </summary>
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Set the visited property to True. This will change
            //the color of the link.
            e.Link.Visited = true;

            //Open Internet Explorer to the correct url.
            System.Diagnostics.Process.Start("IExplore.exe", this.linkLabel1.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Set the visited property to True. This will change
            //the color of the link.
            e.Link.Visited = true;

            //Open Internet Explorer to the correct url.
            System.Diagnostics.Process.Start("IExplore.exe", this.linkLabel2.Text);
        }
    }
}