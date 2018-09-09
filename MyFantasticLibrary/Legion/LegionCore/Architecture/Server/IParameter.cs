﻿using LegionContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace LegionCore.Architecture.Server
{
    public interface IParameter: IDisposable
    {
        bool ParametersAvailable();
        LegionDataIn GetNormalDataIn(LegionDataIn DataInCore, ref int id);
        void Open();
        void Close();
    }
}