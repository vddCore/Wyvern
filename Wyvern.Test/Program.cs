using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using HidSharp;
using HidSharp.Reports;
using Wyvern;

namespace Wyvern.Test
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var mouse = new RedragonImpactMouse();
            
            mouse.SetBrightnessLevel(BrightnessLevel.High);
            mouse.SetSolidColor(Color.CornflowerBlue);
        }

        private static void PrintBuffer(byte[] buf)
        {
            foreach (var b in buf)
            {
                Console.Write(b.ToString("X2"));
                Console.Write(" ");
            }
            
            Console.WriteLine();
        }
    }
}