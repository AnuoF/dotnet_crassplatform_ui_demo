using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using unotest.Model;
using unotest.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace unotest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel _mainVm;
        public MainViewModel MainVM => _mainVm;

        public MainPage()
        {
            this.InitializeComponent();

            _mainVm = new MainViewModel();
        }


        public void Start()
        {
            if (MainVM.IsStarted) return;
            MainVM.IsStarted = true;

            Task.Factory.StartNew(() =>
            {
                while (MainVM.IsStarted)
                {
                    try
                    {
                        #region Spectrum Data

                        int len = MainVM.SpectrumLength;

                        float[] data = new float[len];
                        Random r = new Random();

                        for (int i = 0; i < len; i++)
                        {
                            data[i] = r.Next(10, 30);
                        }

                        int interLen = len / 2;
                        for (int i = interLen - 100; i < interLen + 100; i++)
                        {
                            data[i] = r.Next(20, 50);
                        }

                        for (int i = interLen - 50; i < interLen + 50; i++)
                        {
                            data[(int)i] = r.Next(40, 70);
                        }

                        interLen = len / 4;
                        for (int i = interLen - 100; i < interLen + 100; i++)
                        {
                            data[i] = r.Next(20, 50);
                        }

                        for (int i = interLen - 50; i < interLen + 50; i++)
                        {
                            data[(int)i] = r.Next(40, 70);
                        }

                        interLen = len / 4 * 3;
                        for (int i = interLen - 100; i < interLen + 100; i++)
                        {
                            data[i] = r.Next(20, 50);
                        }

                        for (int i = interLen - 50; i < interLen + 50; i++)
                        {
                            data[(int)i] = r.Next(40, 70);
                        }

                        SpectrumData specData = new SpectrumData(data, 950000000, 1000);


                        spectrumOxy.AddData(specData);

                        #endregion

                        #region Wave Data


                        int wavLen = 1000;

                        short[] wavBuff = new short[wavLen];
                        for (int i = 0; i < wavLen; i++)
                        {
                            wavBuff[i] = (short)(r.Next(-1000, 1000));
                        }

                        WavSampleData wavData = new WavSampleData();
                        wavData.SampleRate = 1000;
                        wavData.SampleData = wavBuff;

                        waveChartOxy.AddData(wavData);
                        #endregion

                        Thread.Sleep(MainVM.IntervalMs);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }

        public void Stop()
        {
            MainVM.IsStarted = false;
        }

        public void Clear()
        {
            waveChartOxy.Clear();
            spectrumOxy.Clear();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }




    }
}
