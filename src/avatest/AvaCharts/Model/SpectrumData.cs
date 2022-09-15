using System;
using System.Collections.Generic;
using System.Text;

namespace AvaCharts.Model
{
    public class SpectrumData
    {
        /// <summary>
        /// 初始化频谱数据
        /// </summary>
        /// <param name="data">频谱数据</param>
        /// <param name="dataLen">频谱数据长度</param>
        /// <param name="start">频谱起始频率</param>
        /// <param name="resolution">数据样点间的间距</param>
        /// <param name="validBandwidth">频谱数据有效起带宽，默认为全部带宽</param>
        public SpectrumData(float[] data, int dataLen, double start, double resolution, double validBandwidth = double.NaN)
            : this(data, dataLen, start, resolution, start, start + dataLen * resolution)
        {
            if (!double.IsNaN(validBandwidth))
            {
                this.ValidStart = this.ValidCenter - validBandwidth / 2;
                this.ValidStop = this.ValidStart + validBandwidth;
            }
        }

        public SpectrumData(float[] data, double start, double resolution, double validBandwidth = double.NaN)
            : this(data, data.Length, start, resolution, validBandwidth)
        {
        }

        /// <summary>
        /// 初始化频谱数据
        /// </summary>
        /// <param name="data">频谱数据</param>
        /// <param name="dataLen">频谱数据长度</param>
        /// <param name="start">频谱起始频率</param>
        /// <param name="resolution">数据样点间的间距</param>
        /// <param name="validStart">频谱数据有效起始频率</param>
        /// <param name="validStop">频谱数据有效结束频率</param>
        public SpectrumData(float[] data, int dataLen, double start, double resolution, double validStart, double validStop)
        {
            if (data == null) return;
            //System.Diagnostics.Debug.Assert(dataLen <= data.Length);

            this.Data = data;
            this.DataLength = dataLen;
            this.Start = start;
            this.Stop = start + dataLen * resolution;
            this.Resolution = resolution;
            this.Span = dataLen * resolution;

            this.ValidStart = validStart;
            this.ValidStop = validStop;
            this.ValidCenter = (this.ValidStart + this.ValidStop) / 2;

            //System.Diagnostics.Debug.Assert(validStart >= start);
            //System.Diagnostics.Debug.Assert(validStop <= Stop);
        }

        public SpectrumData(float[] data, double start, double resolution, double validStart, double validStop)
            : this(data, data.Length, start, resolution, validStart, validStop)
        {

        }


        public float[] Data { get; private set; }

        /// <summary>
        /// Data的数组长度，使用这个值避免Data.Length被频繁调用，影响性能
        /// </summary>
        public int DataLength { get; private set; }

        /// <summary>
        /// 对数据的时间戳信息, UTC时间
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 数据的起始频率
        /// </summary>
        public double Start { get; private set; }

        /// <summary>
        /// 数据的结束频率
        /// </summary>
        public double Stop { get; private set; }

        /// <summary>
        /// 数据频率分辨率
        /// </summary>
        public double Resolution { get; private set; }

        /// <summary>
        /// 采样中心频率
        /// 
        /// 注：该值为有效带宽的中心频点
        /// </summary>
        public double ValidCenter { get; private set; }

        /// <summary>
        /// 数据的有效起始频率
        /// </summary>
        public double ValidStart { get; private set; }

        /// <summary>
        /// 数据的有效结束频率
        /// </summary>
        public double ValidStop { get; private set; }

        /// <summary>
        /// 数据的频率区域
        /// </summary>
        public double Span { get; private set; }
    }
}
