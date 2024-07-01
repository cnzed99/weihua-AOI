using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace WH.Controls
{
    /// <summary>
    /// WelComePage.xaml 的交互逻辑
    /// </summary>
    public partial class WelComePage : Window
    {
        public Action<string> useraction;

        private bool unclicked = true;
        public WelComePage(List<string> recentpros,string Title)
        {
            InitializeComponent();
            this.lb_Title.Text = Title;
            Style buttonStyle = Resources["PageButton"] as Style;
            Style PathStyle = Resources["lbText"] as Style;
            int i = 0;
            foreach (var pro in recentpros)
            {
                if (!CheckExist(pro)) continue;
                var btnPro1 = new Button();
                var lbPro1Path = new TextBlock();
                // 
                // lbPro1Path
                // 
                lbPro1Path.Style = PathStyle;


                lbPro1Path.Margin = new Thickness(0, 25 + 46 * i, 0, 0);
                //lbPro1Path.Width = 305;
                lbPro1Path.Height = 23;
                lbPro1Path.FontSize = 10;
                //lbPro1Path.FontStyle = FontStyles.Italic;
                lbPro1Path.Foreground = Brushes.LightGray;
                lbPro1Path.HorizontalAlignment = HorizontalAlignment.Left;
                lbPro1Path.VerticalAlignment = VerticalAlignment.Top;
                // 
                // btnPro1
                // 
                btnPro1.Style = buttonStyle;
                btnPro1.Margin = new Thickness(0, 46 * i, 0, 0);
                Binding db = new Binding();
                db.Source = ProjContainer;
                db.Path = new PropertyPath("ActualWidth");
                btnPro1.SetBinding(WidthProperty, db);
                //btnPro1.Width = 305;
                btnPro1.Height = 25;
                btnPro1.HorizontalAlignment = HorizontalAlignment.Left;
                btnPro1.VerticalAlignment = VerticalAlignment.Top;
                btnPro1.Click += BtnPro1_Click;

                btnPro1.Content = System.IO.Path.GetFileNameWithoutExtension(pro);
                btnPro1.Tag = pro;
                lbPro1Path.Text = pro;
                this.ProjContainer.Children.Add(btnPro1);
                this.ProjContainer.Children.Add(lbPro1Path);
                ProjContainer.Height += 48;
                this.Height += 48;
                //label1.Location = new Point(label1.Location.X, label1.Location.Y + 6);
                //label2.Location = new Point(label2.Location.X, label2.Location.Y + 6);
                //label3.Location = new Point(label3.Location.X, label3.Location.Y + 6);
                //btnCreateNew.Location = new Point(btnCreateNew.Location.X, btnCreateNew.Location.Y + 6);
                //btnOpen.Location = new Point(btnOpen.Location.X, btnOpen.Location.Y + 6);
                //btnExite.Location = new Point(btnExite.Location.X, btnExite.Location.Y + 4);
                //alphaFormMarker2.Location = new Point(alphaFormMarker2.Location.X, alphaFormMarker2.Location.Y + 4);
                i++;
            }
            

        }

        private void BtnPro1_Click(object sender, RoutedEventArgs e)
        {
            if (unclicked)
            {

                if (sender is Button btn && (string)btn.Content != string.Empty)
                {
                    unclicked = false;
                    Sure(btn.Tag.ToString());
                    //useraction?.Invoke(btn.Tag.ToString());
                    
                }
            }
        }

        private bool CheckExist(string filename)
        {
            return File.Exists(filename);
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (unclicked)
            {
                unclicked = false;
                Sure("openfile");
                //useraction?.Invoke("openfile");
               
            }


        }
        private void btn_MainFrm_Click(object sender, RoutedEventArgs e)
        {
            if (unclicked)
            {
                unclicked = false;
                Sure("mainform");
                 //useraction?.Invoke("mainform");
               
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            Storyboard std = this.Resources["OnCloseWindow"] as Storyboard;

            std.Completed += delegate { this.Close(); Environment.Exit(0); };

            std.Begin();
        }


        private void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
      
       private void Sure(string command)
        {
            Storyboard std = this.Resources["OnCloseWindow"] as Storyboard;

            std.Completed += delegate {
                this.Close();
                useraction?.Invoke(command);
            };

            std.Begin();
        }
       
    }
}
