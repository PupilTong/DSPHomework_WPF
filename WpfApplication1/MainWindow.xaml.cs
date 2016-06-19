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

     
    }

    public class MainViewModel {
        public PlotModel originFuncT { get; private set; }
        public PlotModel originFuncF { get; private set; }
        public PlotModel FliterFuncT { get; private set; }
        public PlotModel FliterFuncF { get; private set; }
        public PlotModel ProcedFuncT { get; private set; }
        public PlotModel AimFuncT { get; private set; }
        public MainViewModel() {
            originFuncT = new PlotModel { Title = "理想滤波器的幅频响应" };
            FliterFuncT = new PlotModel { Title = "理想滤波器的时域冲激响应" };
            FliterFuncF = new PlotModel { Title = "滤波器时域截断后" };


            System.Func<double,double> idtftedFliter = (x) => DSPTools.IDTFT(x, IdealLowPassFilter).Real;
            originFuncT.Series.Add(new FunctionSeries((x)=>IdealLowPassFilter(new Complex(x*Math.PI,0)).Real, -10, 10, 0.010000000f, "幅频响应"));
            FliterFuncT.Series.Add(new FunctionSeries(idtftedFliter, -10, 10, 0.0100000001f, "IDTFTedData(w)"));
            FliterFuncF.Series.Add(new FunctionSeries((x) => DSPTools.FFT(x/Math.PI/2, idtftedFliter, -50, 50, 2048).Magnitude, -6, 6, 2 / 2048f, "幅频响应"));

            

        }

        public Complex IdealLowPassFilter(Complex w) {
            if (Math.Abs(w.Real) <= 5 * Math.PI) {
                return Complex.FromPolarCoordinates(1,0);
            }
            return 0;
        }
        public double originFunc(double t) {
            /*if (t <= 20&&t>0) {
                return 1;
            }
            return 0;*/
            return Math.Cos(2 * Math.PI * t) + 2 * Math.Cos(4 * Math.PI * t) + 3 * Math.Sin(6 * Math.PI * t);
        }
    }

}
