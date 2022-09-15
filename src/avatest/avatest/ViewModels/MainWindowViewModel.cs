using AvaCharts.Model;
using Avalonia.Controls;
using avatest.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace avatest.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Window _mainWindow;
        public string Greeting => "Welcome to Avalonia!";

        private bool _scottEnable = false;
        private bool _oxyEnable = false;
        private bool _skiaEnable = true;

        private string _chineseContent = "你好！我是中文测试！谢谢！TextBox";
        public string ChineseContent
        {
            get => _chineseContent;
            set { this.RaiseAndSetIfChanged(ref _chineseContent, value); }
        }

        private int _intervalMs = 200;
        public int IntervalMs
        {
            get { return _intervalMs; }
            set
            {
                if (value < 20) return;
                this.RaiseAndSetIfChanged(ref _intervalMs, value);
            }
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

        private bool _isScottWave = true;
        public bool IsScottWave
        {
            get { return _isScottWave; }
            set { this.RaiseAndSetIfChanged(ref _isScottWave, value); }
        }

        private bool _isScottSpectrum = true;
        public bool IsScottSpectrum
        {
            get { return _isScottSpectrum; }
            set { this.RaiseAndSetIfChanged(ref _isScottSpectrum, value); }
        }

        private bool _isScottSpectrogram = true;
        public bool IsScottSpectrogram
        {
            get { return _isScottSpectrogram; }
            set { this.RaiseAndSetIfChanged(ref _isScottSpectrogram, value); }
        }

        private bool _isOxyWave = true;
        public bool IsOxyWave
        {
            get { return _isOxyWave; }
            set { this.RaiseAndSetIfChanged(ref _isOxyWave, value); }
        }

        private bool _isOxySpectrum = true;
        public bool IsOxySpectrum
        {
            get { return _isOxySpectrum; }
            set { this.RaiseAndSetIfChanged(ref _isOxySpectrum, value); }
        }

        private bool _isOxySpectrogram = true;
        public bool IsOxySpectrogram
        {
            get { return _isOxySpectrogram; }
            set { this.RaiseAndSetIfChanged(ref _isOxySpectrogram, value); }
        }

        private bool _isSkiaWave;
        public bool IsSkiaWave
        {
            get => _isSkiaWave;
            set { this.RaiseAndSetIfChanged(ref _isSkiaWave, value); }
        }

        private bool _isSkiaSpectrum;
        public bool IsSkiaSpectrum
        {
            get => _isSkiaSpectrum;
            set { this.RaiseAndSetIfChanged(ref _isSkiaSpectrum, value); }
        }

        private bool _isSkiaSpectrogram;
        public bool IsSkiaSpectrogram
        {
            get => _isSkiaSpectrogram;
            set { this.RaiseAndSetIfChanged(ref _isSkiaSpectrogram, value); }
        }

        public Action<SpectrumData> SpectrumAddDataAction { get; set; }
        public Action SpectrumClearAction { get; set; }
        public Action<SpectrumData> SpectrogramAddDataAction { get; set; }
        public Action SpectrogramClearAction { get; set; }
        public Action<WavSampleData> WavSampleAddDataAction { get; set; }
        public Action WavSampleClearAction { get; set; }


        public Action<SpectrumData> SpectrumAddDataOxyAction { get; set; }
        public Action SpectrumClearOxyAction { get; set; }
        public Action<SpectrumData> SpectrogramAddDataOxyAction { get; set; }
        public Action SpectrogramClearOxyAction { get; set; }
        public Action<WavSampleData> WavSampleAddDataOxyAction { get; set; }
        public Action WavSampleClearOxyAction { get; set; }

        public Action<WavSampleData> WavSampleAddDataSkiaAction { get; set; }
        public Action WavSampleClearSkiaAction { get; set; }
        public Action<SpectrumData> SpectrumAddDataSkiaAction { get; set; }
        public Action SpectrumClearSkiaAction { get; set; }
        public Action<SpectrumData> SpectrogramAddDataSkiaAction { get; set; }
        public Action SpectrogramClearSkiaAction { get; set; }



        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;

            IsScottSpectrogram = IsScottSpectrum = IsScottWave = _scottEnable;
            IsOxySpectrogram = IsOxySpectrum = IsOxyWave = _oxyEnable;
            IsSkiaSpectrogram = IsSkiaSpectrum = IsSkiaWave = _skiaEnable;
        }

        public void ShowMessage()
        {
            //var msg = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Title", "Hello Avalonia!");
            ////msg.Show();
            //msg.ShowDialog(_mainWindow);
        }

        public void EnDisableScott()
        {
            _scottEnable = !_scottEnable;
            IsScottSpectrogram = IsScottSpectrum = IsScottWave = _scottEnable;
        }

        public void EnDisableOxy()
        {
            _oxyEnable = !_oxyEnable;
            IsOxySpectrogram = IsOxySpectrum = IsOxyWave = _oxyEnable;
        }

        public void EnDisableSkia()
        {
            _skiaEnable = !_skiaEnable;
            IsSkiaSpectrogram = IsSkiaSpectrum = IsSkiaWave = _skiaEnable;
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

                        if (IsScottSpectrum)
                            SpectrumAddDataAction?.Invoke(specData);
                        if (IsOxySpectrum)
                            SpectrumAddDataOxyAction?.Invoke(specData);

                        if (IsScottSpectrogram)
                            SpectrogramAddDataAction?.Invoke(specData);
                        if (IsOxySpectrogram)
                            SpectrogramAddDataOxyAction?.Invoke(specData);

                        if (IsSkiaSpectrum)
                            SpectrumAddDataSkiaAction?.Invoke(specData);
                        if (IsSkiaSpectrogram)
                            SpectrogramAddDataSkiaAction?.Invoke(specData);
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

                        if (IsScottWave)
                            WavSampleAddDataAction?.Invoke(wavData);
                        if (IsOxyWave)
                            WavSampleAddDataOxyAction?.Invoke(wavData);

                        if (_isSkiaWave)
                            WavSampleAddDataSkiaAction?.Invoke(wavData);
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

            WavSampleClearOxyAction?.Invoke();
            SpectrumClearOxyAction?.Invoke();
            SpectrogramClearOxyAction?.Invoke();

            WavSampleClearSkiaAction?.Invoke();
            SpectrumClearSkiaAction?.Invoke();
            SpectrogramClearSkiaAction?.Invoke();
        }


    }




}
