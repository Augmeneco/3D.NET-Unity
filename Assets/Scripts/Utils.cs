using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Text;
using System.IO;
using System.Globalization;

namespace ThreeDNet
{
    public class Utils
    {
        //public NumberFormatInfo InvarCult =  CultureInfo.InvariantCulture.NumberFormat;

        public static Vector2Int AddrToDim(IPAddress ip){
            // careful of sign extension: convert to uint first;
            // unsigned NetworkToHostOrder ought to be provided.
            long i = IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(ip.GetAddressBytes(), 0));
            return new Vector2Int((int)(i % 65536), (int)(i / 65536));
        }

        public static IPAddress DimToAddr(Vector2Int coords)
        {
            long i = ((long)coords.x + (65536 * (long)coords.y));
            return IPAddress.Parse(i.ToString());
            
            // This also works:
            // return new IPAddress((uint) IPAddress.HostToNetworkOrder(
            //    (int) address)).ToString();
        }

        public static void LogWrite(string text)
        {
            Debug.Log(text);
        }
    }

    public class Pair<T, K>
	{
		public T First { get; set; }
		public K Second { get; set; }

        public Pair(T first, K second)
        {
            this.First = first;
            this.Second = second;
        }
	}
}
