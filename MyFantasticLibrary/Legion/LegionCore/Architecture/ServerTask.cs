﻿using ComponentsLoader;
using LegionContract;
using LegionCore.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LegionCore.Architecture
{
    internal class ServerTask: IDisposable
    {
        private LoadedComponent<LegionTask> _Task;
        private List<string> _TaskInputPaths = new List<string>();
        private string _TaskOutputPath;
        private int _CurrentTaskParameter;
        private StreamReader _DataInReader;
        private StreamWriter _DataOutWriter;
        private string OrderedOutputPath;
        private int _LastParamId;
        private List<LegionDataOut> _DataOut;
        private LoggingManager _Logger;

        public LoadedComponent<LegionTask> Task { get => _Task; }
        public bool NoParametersAvailable { get; set; }



        public ServerTask(LoadedComponent<LegionTask> task, 
            List<string> taskInputPaths, 
            string taskOutputPath,
            string taskOutputOrderedPath)
        {
            _Logger = LoggingManager.Instance;
            _Task = task;
            _TaskInputPaths = taskInputPaths;
            _TaskOutputPath = taskOutputPath;
            _CurrentTaskParameter = _LastParamId = 0;
            NoParametersAvailable = false;
            OrderedOutputPath = taskOutputOrderedPath;
            if (OrderedOutputPath != null)
                _DataOut = new List<LegionDataOut>();
            InitStreams();
        }

        private void InitStreams()
        {
            if (_Task != null && _TaskInputPaths.Count > 0
                && _TaskInputPaths.Count > 0 &&
                !string.IsNullOrEmpty(_TaskOutputPath))
            {
                _DataInReader = new StreamReader(_TaskInputPaths[0]);
                _DataOutWriter = new StreamWriter(_TaskOutputPath);

            }
        }

        internal LegionDataIn DataIn
        {
            get
            {
                LegionAttribute attr = (LegionAttribute)_Task.Singleton
                    .GetType()
                    .GetCustomAttribute(typeof(LegionAttribute));
                return (LegionDataIn)Activator.CreateInstance(attr.TypeIn);
            }
        }
        internal LegionDataOut DataOut
        {
            get
            {
                LegionAttribute attr = (LegionAttribute)_Task.Singleton
                    .GetType()
                    .GetCustomAttribute(typeof(LegionAttribute));
                return (LegionDataOut)Activator.CreateInstance(attr.TypeOut);
            }
        }


        internal void CheckNextInputParameters()
        {
            if (_DataInReader.EndOfStream)
            {
                if (_CurrentTaskParameter + 1 < _TaskInputPaths.Count)
                {
                    _CurrentTaskParameter++;
                    _DataInReader.Close();
                    _DataInReader = new StreamReader(
                       _TaskInputPaths[_CurrentTaskParameter]);
                } 
                else
                {
                    NoParametersAvailable = true;
                }
            }
               
        }

        internal LegionDataIn GetDataIn()
        {
            lock (_DataInReader)
            {
                CheckNextInputParameters();
                if(!_DataInReader.EndOfStream)
                {
                    LegionDataIn dataIn = DataIn;
                    IdManagement.SetId(dataIn, _LastParamId);
                    _LastParamId++;
                    dataIn.LoadFromStream(_DataInReader);
                    return dataIn;
                }

                return null;
            }
        }

        internal void SaveResults(IEnumerable<LegionDataOut> dataOut)
        {
            lock (_DataOutWriter)
            {
                foreach (var data in dataOut)
                {
                    _DataOutWriter.Write(IdManagement.GetId(data) + " -> ");
                    data.SaveToStream(_DataOutWriter);
                    _DataOutWriter.Flush();
                    if (_DataOut != null)
                        _DataOut.Add(data);
                }
                if (_DataOut?.Count == _LastParamId)
                    SaveResultsWithOrder();
            }
        }
        private void SaveResultsWithOrder()
        {
            lock(OrderedOutputPath)
            {
                using(StreamWriter ordered = new StreamWriter(OrderedOutputPath))
                {
                    foreach (var data in _DataOut.OrderBy(x => IdManagement.GetId(x)))
                    {
                        data.SaveToStream(ordered);
                    }
                }
                _Logger.LogInformation("[ Server ] Saved output data with order");
            }
        }
        public void Dispose()
        {
            _DataInReader?.Dispose();
            _DataOutWriter?.Dispose();
        }
    }

}
