using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;

namespace DSPHomework {
    static class DSPTools {
        static private Ft boostFFTFt;
        static double boostFFTStartTime, boostFFTEndTime;
        static int boostFFTN;
        static Complex[] boostFFTData;
        /// <summary>
        /// 定义代表f（x）的函数委托
        /// </summary>
        /// <param name="t">时间自变量</param>
        /// <returns>因变量</returns>
        public delegate double Ft(double t);
        /// <summary>
        /// 定义代表X（w）的函数委托
        /// </summary>
        /// <param name="w">自变量</param>
        /// <returns>因变量</returns>
        public delegate Complex Xw(Complex w);
        /// <summary>
        /// 离散逼近定积分
        /// </summary>
        /// <param name="low">积分下限</param>
        /// <param name="high">积分上限</param>
        /// <param name="step">积分精度(x轴步进)</param>
        /// <param name="f">被积函数</param>
        /// <returns>积分值</returns>
        private static double Integral(double low, double high, double step, Ft f) {
            double sum = 0;
            for (int i = 0; i <= (high - low) / step; i++) {
                sum += f(low + i * step);
            }
            sum *= step;
            return sum;
        }
        /// <summary>
        /// 离散时间傅里叶反变换
        /// </summary>
        /// <param name="t">时域横坐标</param>
        /// <param name="X">系统函数</param>
        /// <returns></returns>
        public static Complex IDTFT(double t,Xw X) {
            double real = Integral(-Math.PI, Math.PI, 0.01, (omega) => Math.Cos(t * omega * X(Complex.Exp(new Complex(0,omega))).Magnitude));
            double imag = Integral(-Math.PI, Math.PI, 0.01, (omega) => Math.Sin(t * omega * X(Complex.Exp(new Complex(0, omega))).Magnitude));
            Complex result = new Complex(real, imag) / Math.PI / 2;
            return result;
        }
        /// <summary>
        /// 快速傅里叶变换
        /// </summary>
        /// <param name="w">频域横坐标</param>
        /// <param name="F">系统时域函数</param>
        /// <param name="startTime">采样开始时间</param>
        /// <param name="endTime">采样结束时间</param>
        /// <param name="N">采样点数</param>
        /// <returns>频域对应值</returns>
        public static Complex FFT(double w,Ft F,double startTime,double endTime,int N) {
            if (endTime > startTime) {//数据合法性校验
                double T = (endTime - startTime) / N;//采样周期
                //求最接近的2的幂
                int SequenceNumber = (int)Math.Pow(2, Math.Ceiling(Math.Log(N) / Math.Log(2)));
                Complex[] Data = new Complex[SequenceNumber];
                Data.Initialize();
                //生成采样数据
                for (int i = 0; i < N; i++) {
                    Data[i] = new Complex(F(i * T + startTime), 0);
                }
                //如果这一次的和上次的数据不一样，那就重新FFT
                if (startTime != boostFFTStartTime && endTime != boostFFTEndTime && N != boostFFTN && F != boostFFTFt) {
                    //FFT采样数据
                    Data = SubFFT(Data, false);
                    Data = Data.Select((x) => x * 2 / N).ToArray();
                    //Data = FFTShift(Data);
                    //保存这一次的，为了加速计算
                    boostFFTStartTime = startTime;
                    boostFFTEndTime = endTime;
                    boostFFTN = N;
                    boostFFTFt = F;
                    boostFFTData = Data;
                }
                else {
                    Data = boostFFTData;
                }
                //f=采样频率/2*(-1+2/SequenceNumber*n)
                int n = (int)(SequenceNumber * (2*w*T+1 ) / 2);
                if (n < Data.Length && n >= 0) {
                    return Data[n];
                }
                else {
                    return 0;
                }
            }
            else {
                throw new Exception("数据上界小于下界");
            }
        }
        /// <summary>
        /// FFT输出数据是两端为0，中心为最大，所以需要调整
        /// </summary>
        /// <param name="Data">数据</param>
        /// <returns></returns>
        public static Complex[] FFTShift(Complex[] Data) {
            Complex tempComp = new Complex();
            for (int i = 0; i < Data.Length/2; i++) {
                tempComp = Data[Data.Length / 2 + i];
                Data[Data.Length / 2 + i] = Data[i];
                Data[i] = tempComp;
            }
            return Data;
        }
        /// <summary>
        /// 傅立叶变换或反变换，递归实现多级蝶形运算
        /// 作为反变换输出需要再除以序列的长度
        /// ！注意：输入此类的序列长度必须是2^n
        /// </summary>
        /// <param name="input">复数输入序列</param>
        /// <param name="invert">false=正变换，true=反变换</param>
        /// <returns>傅立叶变换或反变换后的序列</returns>
        private static Complex[] SubFFT(Complex[] input, bool invert) {
            ///输入序列只有一个元素，输出这个元素并返回
            if (input.Length == 1) {
                return new Complex[] { input[0] };
            }
            ///输入序列的长度
            int length = input.Length;
            ///输入序列的长度的一半
            int half = length / 2;
            ///有输入序列的长度确定输出序列的长度
            Complex[] output = new Complex[length];
            ///正变换旋转因子的基数
            double fac = -2.0 * Math.PI / length;
            ///反变换旋转因子的基数是正变换的相反数
            if (invert) {
                fac = -fac;
            }
            ///序列中下标为偶数的点
            Complex[] evens = new Complex[half];
            for (int i = 0; i < half; i++) {
                evens[i] = input[2 * i];
            }
            ///求偶数点FFT或IFFT的结果，递归实现多级蝶形运算
            Complex[] evenResult = SubFFT(evens, invert);
            ///序列中下标为奇数的点
            Complex[] odds = new Complex[half];
            for (int i = 0; i < half; i++) {
                odds[i] = input[2 * i + 1];
            }
            ///求偶数点FFT或IFFT的结果，递归实现多级蝶形运算
            Complex[] oddResult = SubFFT(odds, invert);
            for (int k = 0; k < half; k++) {
                ///旋转因子
                double fack = fac * k;

                ///进行蝶形运算
                Complex oddPart = oddResult[k] * new Complex(Math.Cos(fack), Math.Sin(fack));
                output[k] = evenResult[k] + oddPart;
                output[k + half] = evenResult[k] - oddPart;
            }

            ///返回FFT或IFFT的结果
            return output;
        }
    }
}
