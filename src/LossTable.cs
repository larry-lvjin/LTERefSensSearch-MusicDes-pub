using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LTE10MRXFULLRBPOWER
{
    public class LossTable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private double _freqMHz;
        public double FreqMHz
        {
            get => _freqMHz;
            set
            {
                if (_freqMHz != value)
                {
                    _freqMHz = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FreqMHz)));
                }
            }
        }
    }
}
