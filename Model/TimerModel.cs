using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Kursach.Model
{
    internal class TimerModel : DispatcherTimer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        double elapsedTime;
        public double ElapsedTime
        {
            get => Math.Round(elapsedTime, 1);
            set
            {
                elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            ElapsedTime += 0.1;
        }

        public TimerModel()
        {
            Interval = TimeSpan.FromMilliseconds(100);
            this.Tick += OnTimerTick;
            ElapsedTime = 0;
        }
    }
}
