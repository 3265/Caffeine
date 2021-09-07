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
        private static bool _enabled = false; // Backing field
        public static bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                switch (value)
                {
                    case true:
                        ResetTimer();
                        break;
                    case false:
                        _timer?.Stop();
                        break;
                }
            }
        }
        private static System.Timers.Timer _timer;
        private static LASTINPUTINFO _lastInput;
        private static readonly Rectangle _bounds = Screen.PrimaryScreen.Bounds;
        private static readonly Random _rng = new Random();
        private static readonly Array _vkeys = typeof(VirtualKey).GetEnumValues();

        static AfkMode() // Constructor
        {
            _lastInput.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)); // Set cbSize parameter in struct with size of this struct
        }
        private static async void DelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Win32API.GetLastInputInfo(ref _lastInput); // Sets field LastInput.dwTime (ticks of last input)
                long idletime = (long)Win32API.GetTickCount() - (long)_lastInput.dwTime; // Get difference between Current Ticks & Ticks of Last Input (Idle Time)
                if (idletime > _timer.Interval) // // Check to see if user is idle, proceed with simulating keyboard/mouse input
                {
                    Win32API.SendKey((VirtualKey)_vkeys.GetValue(_rng.Next(0, _vkeys.Length))); // Send Key Press with randomly selected key
                    Win32API.MoveMouse(_rng.Next(0, _bounds.Width), _rng.Next(0, _bounds.Height)); // Move mouse to random area of primary screen
                    await Task.Delay(500); // 500 ms delay before resetting timer
                }
            }
            catch { } // Silently ignore any exceptions (This program is designed to run quietly, and should cause no disturbance or provide any evidence that it is running when user is AFK)
            finally
            {
                if (Enabled) ResetTimer(); // Set next timer (if still enabled)
            }
        }
        private static void ResetTimer()
        {
            _timer = new System.Timers.Timer(_rng.Next(45000, 65000)); // 45 - 65 sec, random
            _timer.AutoReset = false;
            _timer.Elapsed += DelayTimer_Elapsed; // Set elapsed event method
            _timer.Start(); // start timer
        }
    }
}