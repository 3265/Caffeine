using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Caffeine
{
    internal static class AfkMode
    {
        private static bool _Enabled;
        public static bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
                switch (value)
                {
                    case true:
                        ResetTimer();
                        break;
                    case false:
                        DelayTimer?.Stop();
                        break;
                }
            }
        }
        private static System.Timers.Timer DelayTimer;
        private static LASTINPUTINFO LastInput;
        private static readonly Rectangle ScreenBounds;
        private static readonly Random Rng;
        private static readonly Array Vkeys;

        static AfkMode() // Constructor
        {
            LastInput.cbSize = (uint)Marshal.SizeOf(LastInput); // Set cbSize parameter in struct with size of this struct
            ScreenBounds = Screen.PrimaryScreen.Bounds;
            Rng = new Random();
            Vkeys = typeof(VirtualKey).GetEnumValues();
        }
        private static async void DelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!Enabled) return;
                Win32API.GetLastInputInfo(ref LastInput); // Sets field LastInput.dwTime (ticks of last input)
                long idletime = (long)Win32API.GetTickCount() - (long)LastInput.dwTime; // Get difference between Current Ticks & Ticks of Last Input (Idle Time)
                if (idletime > DelayTimer.Interval) // // Check to see if user is idle, proceed with simulating keyboard/mouse input
                {
                    Win32API.SendKey((VirtualKey)Vkeys.GetValue(Rng.Next(0, Vkeys.Length))); // Send Key Press with randomly selected key
                    Win32API.SetCursorPos(Rng.Next(0, ScreenBounds.Width), Rng.Next(0, ScreenBounds.Height)); // Move mouse to random area of primary screen
                    await Task.Delay(500); // 500 ms delay before resetting timer
                }
            }
            catch { } // Silently ignore any exceptions (This program is designed to run quietly, and should cause no disturbance or provide any evidence that it is running when user is AFK)
            finally
            {
                ResetTimer(); // Set next timer
            }
        }
        private static void ResetTimer()
        {
            if (!Enabled) return;
            DelayTimer = new System.Timers.Timer(Rng.Next(45000, 65000)); // 45 - 65 sec, random
            DelayTimer.AutoReset = false;
            DelayTimer.Elapsed += DelayTimer_Elapsed; // Set elapsed event method
            DelayTimer.Start(); // start timer
        }
    }
}