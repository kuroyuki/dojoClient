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

        dojoCoords Sensor1, Sensor2, Act1, Act2;

        double mouseX = 0, mouseY = 0;

        public MainWindow()
        {
            InitializeComponent();
            Client = new dojoClient(IPAddress.Parse("192.168.56.101"));

            Canvas.SetLeft(rect, 100);
            Canvas.SetTop(rect, 80);


            UpdateProcess = new Thread(UpdateValues);
            UpdateProcess.Start();

            Sensor1 = new dojoCoords(0, 0);
            Sensor2 = new dojoCoords(1, 0);

            Act1 = new dojoCoords(0, 1);
            Act2 = new dojoCoords(1, 1);

            Client.RegSensor(Sensor1, 100);
            Client.RegSensor(Sensor2, 100);

            Client.RegAct(Act1, 1);
            Client.RegAct(Act2, 1);
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Workspace_MouseMove(object sender, MouseEventArgs e)
        {
            Point NewMousePoint = e.GetPosition(Workspace);

            //Here we need to sent two values of sensors X and Y of new mouse position
            mouseX = Canvas.GetLeft(rect);
            mouseY = Canvas.GetTop(rect);            
        }
        private void UpdateValues()
        {
            //waiting for shutdown of Dispatcher 
            while (this.Dispatcher.HasShutdownStarted == false)
            {
                Thread.Sleep(10);
                // Get the dispatcher from the current window, and use it to invoke
                // the update code.
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate()
                {
                    //updating sensors
                    Client.UpdateSensorValue(Sensor1, mouseX);
                    Client.UpdateSensorValue(Sensor2, mouseY);

                    //updating acts
                    double x = Canvas.GetLeft(rect);
                    double y = Canvas.GetTop(rect);
                    Canvas.SetLeft(rect, x + Client.GetActValue(Act1));
                    Canvas.SetTop(rect, y + Client.GetActValue(Act2));
                });

            }
        }
    }

}
