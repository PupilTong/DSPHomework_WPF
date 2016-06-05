using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using System.Windows.Controls;
using System.Linq;
using System.Numerics;

namespace DSPHomework {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void t0Plus_Click(object sender, RoutedEventArgs e) {
            if (Convert.ToInt16(t0Data.Content) < 20) {
                t0Data.Content = (Convert.ToInt16(t0Data.Content) + 1).ToString();
            }
        }

        private void t0Sub_Click(object sender, RoutedEventArgs e) {
            if (Convert.ToInt16(t0Data.Content) > -20) {
                t0Data.Content = (Convert.ToInt16(t0Data.Content) - 1).ToString();
            }
        }

        private void f0Plus_Click(object sender, RoutedEventArgs e) {
            if (Convert.ToInt16(f0Data.Content) < 100) {
                f0Data.Content = (Convert.ToInt16(f0Data.Content) + 1).ToString();
            }
        }

        private void f0Sub_Click(object sender, RoutedEventArgs e) {
            if (Convert.ToInt16(f0Data.Content) > -100) {
                f0Data.Content = (Convert.ToInt16(f0Data.Content) - 1).ToString();
            }
        }
    }

    public class MainViewModel {
        public PlotModel originData { get; private set; }
        public PlotModel IFFTedData { get; private set; }
        public MainViewModel() {
            originData = new PlotModel { Title = "理想低通滤波器频域特性" };
            IFFTedData = new PlotModel { Title = "IDTFT之后波形" };
            originData.Series.Add(new FunctionSeries((x) => IdealLowPassFilter(new Complex(x,0)).Real, -40, 40, 0.0100000001f, "IdealLowPassFilter(t)"));
            IFFTedData.Series.Add(new FunctionSeries((x) => DSPTools.IDTFT(x, IdealLowPassFilter).Real, -20, 20, 0.0100000001f, "IDTFTedData(w)"));
        }
        public Complex IdealLowPassFilter(Complex w) {
            if (Math.Abs(w.Real) <= 20) {
                return 1;
            }
            return 0;
        }
        public double SquareWave(double t) {
            /*if (t <= 20&&t>0) {
                return 1;
            }
            return 0;*/
            return Math.Cos(2 * Math.PI * t) + 2 * Math.Cos(4 * Math.PI * t) + 3 * Math.Sin(6 * Math.PI * t);
        }
    }

}
