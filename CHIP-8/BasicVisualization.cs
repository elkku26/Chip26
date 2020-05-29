﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace visualize
{

    public class BasicVisualizer
    {
        public static void Visualize(int[,] arr)
        {
            int rowLength = arr.GetLength(0);
            int colLength = arr.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    Console.Write("{0} ", arr[i, j]);
                }
                //Console.Write(Environment.NewLine + Environment.NewLine);
            }
            return;
        }
    }
}