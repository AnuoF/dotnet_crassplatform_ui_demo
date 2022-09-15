using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Uno.UI.Common;
using unotest.Model;

namespace unotest.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string p = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));



        private bool _isStarted;
        public bool IsStarted
        {
            get => _isStarted;
            set
            {
                if (_isStarted != value)
                {
                    _isStarted = value;
                    RaisePropertyChanged(nameof(IsStarted));
                }
            }
        }

        private int _spectrumLength = 4096;
        public int SpectrumLength
        {
            get => _spectrumLength;
            set
            {
                if (_spectrumLength != value)
                {
                    _spectrumLength = value;
                    RaisePropertyChanged(nameof(SpectrumLength));
                }
            }
        }

        private int _intervalMs = 100;
        public int IntervalMs
        {
            get => _intervalMs;
            set
            {
                if (_intervalMs != value)
                {
                    _intervalMs = value;
                    RaisePropertyChanged(nameof(IntervalMs));
                }
            }
        }

        public ICommand StartCommand => new DelegateCommand(Start);
        public ICommand StopCommand => new DelegateCommand(Stop);
        public ICommand ClearCommand => new DelegateCommand(Clear);


        public Action<SpectrumData> SpectrumAddDataOxyAction { get; set; }
        public Action SpectrumClearOxyAction { get; set; }
        public Action<SpectrumData> SpectrogramAddDataOxyAction { get; set; }
        public Action SpectrogramClearOxyAction { get; set; }
        public Action<WavSampleData> WavSampleAddDataOxyAction { get; set; }
        public Action WavSampleClearOxyAction { get; set; }


        public MainViewModel()
        {

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

                        int len = SpectrumLength;

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

                        SpectrumAddDataOxyAction?.Invoke(specData);
                        SpectrogramAddDataOxyAction?.Invoke(specData);

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

                
                        WavSampleAddDataOxyAction?.Invoke(wavData);
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
            WavSampleClearOxyAction?.Invoke();
            SpectrumClearOxyAction?.Invoke();
        }

    }
}
