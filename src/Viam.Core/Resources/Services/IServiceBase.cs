namespace Viam.Core.Resources.Services
{
    public interface IServiceBase
    {
        public string ServiceName { get; }
        public SubType SubType { get; }
    }
}
