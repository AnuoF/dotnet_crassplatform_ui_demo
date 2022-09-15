using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaCharts.Model
{
    /// <summary>
    /// 波形数据类
    /// </summary>
    public class WavSampleData
    {
        /// <summary>
        /// 采样率
        /// </summary>
        public long SampleRate;
        /// <summary>
        /// 波形数据
        /// </summary>
        public short[] SampleData;
    }
}
