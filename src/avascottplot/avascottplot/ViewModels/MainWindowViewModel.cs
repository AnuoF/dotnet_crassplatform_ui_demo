using Avalonia.Controls;
using ReactiveUI;
using scottplotcharts.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace avascottplot.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Window _mainWindow;
        public string Greeting => "Welcome to Avalonia!";

        private int _intervalMs = 200;
        public int IntervalMs
        {
            get { return _intervalMs; }
            set { this.RaiseAndSetIfChanged(ref _intervalMs, value); }
        }

        private bool _isStarted = false;
        public bool IsStarted
        {
            get { return _isStarted; }
            set { this.RaiseAndSetIfChanged(ref _isStarted, value); }
        }

        private int _dataLength = 4096;
        public int DataLength
        {
            get { return _dataLength; }
            set { this.RaiseAndSetIfChanged(ref _dataLength, value); }
        }

        private int _displayDataLength = 500;
        public int DisplayDataLength
        {
            get { return _displayDataLength; }
            set { this.RaiseAndSetIfChanged(ref _displayDataLength, value); }
        }

        private bool _isWave = true;
        public bool IsWave
        {
            get { return _isWave; }
            set { this.RaiseAndSetIfChanged(ref _isWave, value); }
        }

        private bool _isSpectrum = true;
        public bool IsSpectrum
        {
            get { return _isSpectrum; }
            set { this.RaiseAndSetIfChanged(ref _isSpectrum, value); }
        }

        private bool _isSpectrogram = true;
        public bool IsSpectrogram
        {
            get { return _isSpectrogram; }
            set { this.RaiseAndSetIfChanged(ref _isSpectrogram, value); }
        }

        public Action<SpectrumData> SpectrumAddDataAction { get; set; }
        public Action SpectrumClearAction { get; set; }
        public Action<SpectrumData> SpectrogramAddDataAction { get; set; }
        public Action SpectrogramClearAction { get; set; }
        public Action<WavSampleData> WavSampleAddDataAction { get; set; }
        public Action WavSampleClearAction { get; set; }


        public MainWindowViewModel()
        {
        }

        public void ShowMessage()
        {
            //var msg = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Title", "Hello Avalonia!");
            ////msg.Show();
            //msg.ShowDialog(_mainWindow);
        }

        public void Start()
        {
            if (IsStarted) return;
            IsStarted = true;
            Task.Factory.StartNew(() =>
            {
                while (IsStarted)
                {
                    try
                    {
                        #region Spectrum Data

                        int len = _dataLength;

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

                        if (IsSpectrum)
                            SpectrumAddDataAction?.Invoke(specData);

                        if (IsSpectrogram)
                            SpectrogramAddDataAction?.Invoke(specData);
                        #endregion

                        #region Wave Data

                        int wavLen = 1000;

                        short[] wavBuff = new short[wavLen];
                        for (int i = 0; i < wavLen; i++)
                        {
                            wavBuff[i] = (short)(r.Next(-1000, 1000));
                        }

                        if (IsWave)
                        {
                            WavSampleData wavData = new WavSampleData();
                            wavData.SampleRate = 1000;
                            wavData.SampleData = wavBuff;
                            WavSampleAddDataAction?.Invoke(wavData);
                        }
                        #endregion

                        Thread.Sleep(IntervalMs);
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
            IsStarted = false;
        }

        public void Clear()
        {
            SpectrogramClearAction?.Invoke();
            SpectrumClearAction?.Invoke();
            WavSampleClearAction?.Invoke();
        }


    }
}
