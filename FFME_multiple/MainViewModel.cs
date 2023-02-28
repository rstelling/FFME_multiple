using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FFME_multiple
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public event EventHandler SliderChanged;


        private double m_DurationMs;
        public double DurationMs
        {
            get { return m_DurationMs; }
            set
            {
                if (value != m_DurationMs)
                {
                    m_DurationMs = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private int m_TimelineSliderValue;
        public int TimelineSliderValue
        {
            get { return m_TimelineSliderValue; }
            set
            {
                if (value != m_TimelineSliderValue)
                {
                    m_TimelineSliderValue = value;
                    NotifyPropertyChanged();
                    SliderChanged?.Invoke(TimelineSliderValue, null);
                }
            }
        }


    }
}
