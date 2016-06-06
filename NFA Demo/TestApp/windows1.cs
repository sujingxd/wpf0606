using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Aga.Diagrams;
using Aga.Diagrams.Controls;
using System.Windows.Media;
using System.Windows.Input;
namespace TestApp.Flowchart
{
    public class windows1:Window
    {
        public string strContent { get; set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            strContent = ((TextBox)this.Content).Text;
            base.OnClosing(e); 
        }
    }
}
