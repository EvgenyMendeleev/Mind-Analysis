﻿using System;
using System.CodeDom;

namespace NeuroTGAM
{
    public class BrainInfo : ICloneable
    {
        public uint Attention { get; set; } = 0;
        public uint Meditation { get; set; } = 0;
        public uint AlphaLow { get; set; } = 0;
        public uint AlphaHigh { get; set; } = 0;
        public uint GammaLow { get; set; } = 0;
        public uint GammaHigh { get; set; } = 0;
        public uint BetaLow { get; set; } = 0;
        public uint BetaHigh { get; set; } = 0;
        public uint Theta { get; set; } = 0;
        public uint Delta { get; set; } = 0;
        public uint Second { get; set; } = 0;

        public object Clone() => MemberwiseClone();
    }
}
