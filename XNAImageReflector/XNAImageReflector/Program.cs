using System;
using System.Collections.Generic;
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
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            XNAWinForm form = new XNAWinForm();
            form.RefreshMode = XNAWinForm.eRefreshMode.OnPanelPaint;
            Application.Run(form);
        }
    }
}