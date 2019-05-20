using System.ServiceModel;

namespace ServerLib
{
    [ServiceContract(CallbackContract = typeof(IServerServiceCallback))]
    public interface IServerService
    {
        [OperationContract]
        string[] GetFilters();

        [OperationContract(IsOneWay = true)]
        void StartFilter(byte[] image, string filterName);

        [OperationContract(IsOneWay = true)]
        void StopFilter();
    }

    public interface IServerServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void GetProgress(int percent);

        [OperationContract(IsOneWay = true)]
        void GetImage(byte[] image);

        [OperationContract(IsOneWay = true)]
        void StopWorking();
    }
}
