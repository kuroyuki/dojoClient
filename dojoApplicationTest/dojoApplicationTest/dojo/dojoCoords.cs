using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dojoApplicationTest.dojo
{
    /*
     * struct to support coordinates of nodes    
     */
    struct dojoCoords
    {
        //there are  X.Y coords only, all sensors and acts should have Z=0    
        public UInt32 X, Y;
        
        public dojoCoords(UInt32 x, UInt32 y)
        {
            X = x; Y = y; 
        }
       
        public byte[] GetBytes()
        {
            byte[] data = new byte[8];
          
            byte[] dataX = BitConverter.GetBytes(X);
            Array.Reverse(dataX);
            dataX.CopyTo(data, 0);

            byte[] dataY = BitConverter.GetBytes(Y);
            Array.Reverse(dataY);
            dataY.CopyTo(data, 4);

            return data;
        }
    }
    /*
     * struct to store data in tables.    
     * fields using different depends on sensor or act node
     */
    struct dojoData
    {
        public double Value, Threshold;
        
        public dojoData(double val, double thres)
        {
            Value = val;
            Threshold = thres;
        }       
    }
}
