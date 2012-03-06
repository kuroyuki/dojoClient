﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace dojoApplicationTest.dojo
{

    class dojoConnection
    {
        //public
        public const int UDP_NODE_DATA = 0, UDP_ACT_REG = 1, UDP_SENSOR_REG = 2;

        //internal 
        UdpClient UDP_Client;

        IPEndPoint LocalEndPoint;

        const int UDP_SERVER_PORT = 49654;        

        Queue<byte[]> FifoForSend;
        Dictionary<dojoCoords, dojoData> ActTable;

        bool SenderBusy = false;

        public dojoConnection(IPAddress remoteIP, Dictionary<dojoCoords, dojoData> actTable)
        {
            LocalEndPoint = new IPEndPoint(remoteIP, UDP_SERVER_PORT);
            //Initiate remote connection to server
            UDP_Client = new UdpClient();
            UDP_Client.Connect(LocalEndPoint);

            FifoForSend = new Queue<byte[]>();

            //Start receive loop
            UDP_Client.BeginReceive(new AsyncCallback(ReceiveLoop), null);

            ActTable = actTable;
            
        }
        public void SendPacket(byte[] packet)
        {
            FifoForSend.Enqueue(packet);

            if (!SenderBusy)
            {
                SenderBusy = true;
                byte[] send = FifoForSend.Dequeue();
                UDP_Client.BeginSend(send, send.Length, new AsyncCallback(SendLoop), null);
            }
        }
        void ReceiveLoop(IAsyncResult ar)
        {
            //Packet from remote host received
            Byte[] receiveBytes = UDP_Client.EndReceive(ar, ref LocalEndPoint);

            /*             Received packets parsing            */
            //node data packet
            if (receiveBytes[0] == UDP_NODE_DATA)
            {
                //Get X coord from packet
                byte[] dataX = new byte[4];
                Array.Copy(receiveBytes, 1, dataX, 0, 4);
                Array.Reverse(dataX);

                //Get Y coord from packet
                byte[] dataY = new byte[4];
                Array.Copy(receiveBytes, 5, dataY, 0, 4);
                Array.Reverse(dataY);

                dojoCoords actCoords = new dojoCoords(BitConverter.ToUInt32(dataX, 0), BitConverter.ToUInt32(dataY, 0));
                dojoData actData;

                
                //get act from table
                if (ActTable.TryGetValue(actCoords, out actData))
                {
                    //update act Value with Threshold
                    actData.Value += actData.Threshold;
                    //save new value in act table
                    ActTable[actCoords] = actData;
                }               
            }

            //Start receive next one
            UDP_Client.BeginReceive(new AsyncCallback(ReceiveLoop), null);
        }
        void SendLoop(IAsyncResult ar)
        {
            SenderBusy = false;
            //packet was send
            UDP_Client.EndSend(ar);
          
            //check for next packet in FIFO
            if (FifoForSend.Count > 0)
            {
                SenderBusy = true;
                byte[] send = FifoForSend.Dequeue();
                UDP_Client.BeginSend(send, send.Length, new AsyncCallback(SendLoop), null);
            }
        }
    }
}
