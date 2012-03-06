using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace dojoApplicationTest.dojo
{
    class dojoClient
    {
        dojoConnection Connection;
        Dictionary<dojoCoords, dojoData> SensorTable, ActTable;
        byte[] NodesDataPacket;

        Thread CheckTables;
        
        /*         APIs         */
        //constructor
        public dojoClient(IPAddress serverIPAddr)
        { 
            SensorTable = new Dictionary<dojoCoords, dojoData>();
            ActTable = new Dictionary<dojoCoords, dojoData>();

            NodesDataPacket = new byte[1];
            NodesDataPacket[0] = dojoConnection.UDP_NODE_DATA;

            CheckTables = new Thread(CheckSensors);
            CheckTables.IsBackground = true;
            CheckTables.Start();

            Connection = new dojoConnection(serverIPAddr, ActTable);
        }        

        //save sensor in local table and inform server about its presence
        public void RegSensor(dojoCoords node, double thres)
        {
            dojoData data = new dojoData(0, thres);
            SensorTable.Add(node, data);

            //Form packet 
            byte[] packet = new byte[9];
            packet[0] = dojoConnection.UDP_SENSOR_REG;
            node.GetBytes().CopyTo(packet, 1);

            //Send it
            Connection.SendPacket(packet);
        }
        //save act in local table and inform server about its presence 
        public void RegAct(dojoCoords node, double val)
        {
            dojoData data = new dojoData(0, val);
            ActTable.Add(node, data);

            //Form packet 
            byte[] packet = new byte[9];
            packet[0] = dojoConnection.UDP_ACT_REG ;
            node.GetBytes().CopyTo(packet, 1);

            //Send it
            Connection.SendPacket(packet);
        }
        //updating sensor value with new one. should be called by user application
        public void UpdateSensorValue(dojoCoords node, double newValue)
        {
            dojoData nodeValue;
            if (SensorTable.TryGetValue(node, out nodeValue))
            {
                //Add new value to node sum
                nodeValue.Value = nodeValue.Value + newValue;
                SensorTable[node] = nodeValue;
            }
        }
        //returning act value
        public double GetActValue(dojoCoords node)
        {
            dojoData nodeValue;
            double retVal = 0;

            if (ActTable.TryGetValue(node, out nodeValue))
            {
                retVal = nodeValue.Value;

                //Add new value to node sum
                nodeValue.Value = 0;
                ActTable[node] = nodeValue;

            }
            return retVal;
        }
        //updating act value if ap occured. should be called by dojoConnection
        public void UpdateAct(dojoCoords act, double newValue)
        {
            dojoData data = new dojoData(ActTable[act].Value + newValue, ActTable[act].Threshold);
            ActTable[act] = data;
        }


        /* internal functions */
        void CheckSensors()
        {
            //let all systems to be start (UDP, user and so on).
            Thread.Sleep(1000);

            //infinite loop
            while (true)
            {
                //temp dict
                Dictionary<dojoCoords, dojoData> tempTable = new Dictionary<dojoCoords, dojoData>();

                try
                {
                    //populate temp dict with modified values
                    foreach (KeyValuePair<dojoCoords, dojoData> kv in SensorTable)
                    {
                        if (kv.Value.Value >= kv.Value.Threshold)
                        {
                            dojoData data = new dojoData(0, kv.Value.Threshold);
                            tempTable.Add(kv.Key, data);
                            AddToPacket(kv.Key);
                        }
                        else
                            tempTable.Add(kv.Key, kv.Value);
                    }                
                    //swap dicts
                    SensorTable = tempTable;

                    //sensors should not be updated very frequently (udp flood will be)
                    Thread.Sleep(10);
                }
                // InvalidOperationException catched - it means that we modified SensorTable during foreach, so we will ignore it
                catch (InvalidOperationException ex)
                {
                    continue;
                }
            }
               
        }        
        void AddToPacket(dojoCoords node)
        {    
            /*
            byte[] newPacket = new byte[NodesDataPacket.Length + 8];

            NodesDataPacket.CopyTo(newPacket, 0);
            coords.GetBytes().CopyTo(newPacket, NodesDataPacket.Length);

            NodesDataPacket = newPacket;
            if (NodesDataPacket.Length > 1500)
            {
                Connection.SendPacket(NodesDataPacket);
                NodesDataPacket = new byte[1];
                NodesDataPacket[0] = 0;
            }
             * */

            //Form packet 
            byte[] packet = new byte[9];
            packet[0] = dojoConnection.UDP_NODE_DATA;
            node.GetBytes().CopyTo(packet, 1);

            //Send it
            Connection.SendPacket(packet);
        }
    }
}
