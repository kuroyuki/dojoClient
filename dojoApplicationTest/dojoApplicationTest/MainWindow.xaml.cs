using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Threading;
using System.Windows.Threading;

using dojoApplicationTest.dojo;

namespace dojoApplicationTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        dojoClient Client;
        Thread UpdateProcess;

        dojoCoords Sensor1, Sensor2, Sensor3, Sensor4,  UpAct, DownAct, LeftAct, RightAct;

        double mouseX = 0, mouseY = 0;

        bool IsMoveEnable = false;

        public MainWindow()
        {
            InitializeComponent();
            Client = new dojoClient(IPAddress.Parse("192.168.56.101"));

            UpdateProcess = new Thread(UpdateValues);
            UpdateProcess.IsBackground = true;
            UpdateProcess.Start();

            Sensor1 = new dojoCoords(0, 0);
            Sensor2 = new dojoCoords(1, 0);
            Sensor3 = new dojoCoords(2, 0);
            Sensor4 = new dojoCoords(3, 0);

            UpAct = new dojoCoords(0, 1);
            DownAct = new dojoCoords(1, 1);
            LeftAct = new dojoCoords(2, 1);
            RightAct = new dojoCoords(3, 1);

            Client.RegSensor(Sensor1, 1);
            Client.RegSensor(Sensor2, 1);
            Client.RegSensor(Sensor3, 1);
            Client.RegSensor(Sensor4, 1);

            Client.RegAct(UpAct, 1);
            Client.RegAct(DownAct, 1);
            Client.RegAct(LeftAct, 1);
            Client.RegAct(RightAct, 1);
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void UpdateValues()
        {
            //waiting for shutdown of Dispatcher 
            while (true)
            {
                Thread.Sleep(20);
                // Get the dispatcher from the current window, and use it to invoke
                // the update code.
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate()
                {
                    //updating sensors
                    Point rectPoint = new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect));

                    Point leftTopTarget = new Point(Canvas.GetLeft(target), Canvas.GetTop(target));
                    Point rightBottomTarget = new Point(leftTopTarget.X + target.Width, leftTopTarget.Y + target.Height);

                    double[] squares = new double[4];
                    int counter = 0;

                    foreach (UIElement el in rect.Children)
                    {
                        //Get Coord for leftTop corner of rect in absolute
                        Point rectLeftTop = new Point(rectPoint.X + Canvas.GetLeft(el), rectPoint.Y+Canvas.GetTop(el));
                        
                        //Get Coord for rightBottom corner of rect in absolute
                        Point rectRightBottom = new Point(rectLeftTop.X + 60, rectLeftTop.Y + 60);

                        double XSide;
                        double Xdiff = (leftTopTarget.X - rectLeftTop.X );
                        if((leftTopTarget.X + target.Width) > 60)
                            XSide = 60 - Xdiff;
                        else if(Xdiff>=0)
                            XSide =  target.Width;
                        else XSide = target.Width + Xdiff;

                        
                        double YSide;
                        double Ydiff = (leftTopTarget.Y - rectLeftTop.Y );
                        if((leftTopTarget.Y + target.Height) > 60)
                           YSide = 60 - Ydiff;
                        else if(Ydiff>=0)
                            YSide =  target.Height;
                        else YSide = target.Height + Ydiff;

                        double Square = XSide * YSide/1600;
                        if (Square >= 0)
                        {                            
                            squares[counter] = Square;
                        }
                        else
                        {
                            squares[counter] = 0;
                        }
                        counter++;
                    }
                    Client.UpdateSensorValue(Sensor1, squares[0]);
                    Client.UpdateSensorValue(Sensor2, squares[1]);
                    Client.UpdateSensorValue(Sensor3, squares[2]);
                    Client.UpdateSensorValue(Sensor4, squares[3]);


                    //updating acts
                    double x = Canvas.GetLeft(rect);
                    double y = Canvas.GetTop(rect);

                    double up = Client.GetActValue(UpAct);
                    double down = Client.GetActValue(DownAct);

                    double left = Client.GetActValue(LeftAct);
                    double right = Client.GetActValue(RightAct);

                    Canvas.SetLeft(rect, x - left + right);
                    Canvas.SetTop(rect, y - up + down);
                });

            }
        }    


        private void target_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMoveEnable = true;
        }

        private void target_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMoveEnable = false;
        }
       
        private void target_LostFocus(object sender, RoutedEventArgs e)
        {
            IsMoveEnable = false;
        }

        private void Workspace_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMoveEnable)
            {
                Point newPos = e.GetPosition(Workspace);
                Canvas.SetLeft(target, newPos.X);
                Canvas.SetTop(target, newPos.Y);
            }
        }

        private void Workspace_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMoveEnable = false;
        }
    }

}
