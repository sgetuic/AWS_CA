using System;
using System.Threading;
using UIC.Framework.Interfaces;
using UIC.Framework.Interfaces.Edm;
using UIC.Framework.Interfaces.Edm.Value;
using UIC.Framework.Interfaces.Project;
using UIC.SGET.ConnectorImplementation.Monitoring.Evaluation;
using UIC.Util.Logging;

namespace UIC.SGET.ConnectorImplementation.Monitoring {
    internal class DatapointMonitor : IDisposable {
       private readonly IDataPointEvaluatorProvider _evaluatorProvider;
        private readonly UniversalIotConnector _connector;
        private readonly ILogger _logger;
        private bool _isDisposed;
        private IDataPointEvaluator _evaluator;
        private readonly ProjectDatapointTask _dataPointTask;
        private readonly EmbeddedDriverModule _edm;

        public DatapointMonitor(ProjectDatapointTask dataPointTask, IDataPointEvaluatorProvider evaluatorProvider, UniversalIotConnector connector, ILogger logger, EmbeddedDriverModule edm)
        {
            _dataPointTask = dataPointTask ?? throw new ArgumentNullException("dataPointTask");
            _evaluatorProvider = evaluatorProvider ?? new DataPointEvaluatorProvider();
            _connector = connector ?? throw new ArgumentNullException("connector");
            _logger = logger ?? throw new ArgumentNullException("logger");
            _edm = edm ?? throw new ArgumentNullException("edm");
            (new Thread(Target)).Start();
        }

        
        private void Target()
        {
            try {
                long pollIntervall = _dataPointTask.PollIntervall;

                if (pollIntervall <= 0)
                {
                    _logger.Warning("Set poll intervall is to short: " + pollIntervall);
                    pollIntervall = 10;
                }
                _logger.Information("Start Monitoring with intervall in seconds: " + pollIntervall);

                while (!_isDisposed) {
                    if (_dataPointTask.Definition == null) {
                        _logger.Warning("Skip Datapoint evaluation because the EDM might not be installed correctly");
                    } else {
                        try
                        {
                            _logger.Information("ReadDatapoint: " + _dataPointTask.Definition);
                            DatapointValue val = _edm.GetValueFor(_dataPointTask.Definition);
                            if (Evaluate(val))
                            {
                                _connector.Push(val);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Warning(e.Message);
                        }
                    }
                    for (int i = 0; i < pollIntervall; i++) {
                        if (_isDisposed)
                            break;
                        Thread.Sleep(1000);
                    }
                    
                }
            } catch (Exception e) {
                _logger.Error(e);
            }
        }

        private bool Evaluate(DatapointValue val)
        {
            if (_evaluator == null)
            {
                _evaluator = _evaluatorProvider.ProvideFor(val, new DataPointEvaluatorParam(_dataPointTask.ReportingCondition));
            }
            return _evaluator.ShouldSendToCloud(val);
        }

      

        public void Dispose()
        {
            _isDisposed = true;

        }
    }
}
