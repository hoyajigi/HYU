using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using System.Text;

namespace HYU
{
    public partial class Page1 : PhoneApplicationPage
    {
        public Page1()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            String id;
            base.OnNavigatedTo(e);
            id = NavigationContext.QueryString["id"];
            byte[] b = Convert.FromBase64String(id);
            id = UTF8Encoding.UTF8.GetString(b, 0, b.Length); ;
            webBrowser1.Navigate(new Uri(id));

        } 
    }
}