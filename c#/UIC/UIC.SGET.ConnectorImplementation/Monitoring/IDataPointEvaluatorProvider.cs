using UIC.Framework.Interfaces.Edm.Value;
using UIC.SGET.ConnectorImplementation.Monitoring.Evaluation;

namespace UIC.SGET.ConnectorImplementation.Monitoring
{
    public interface IDataPointEvaluatorProvider
    {
        IDataPointEvaluator ProvideFor(DatapointValue val, DataPointEvaluatorParam param);
    }
}
