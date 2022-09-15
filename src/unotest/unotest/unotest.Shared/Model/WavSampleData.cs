using System;
using System.Collections.Generic;
using System.Text;

namespace unotest.Model
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
