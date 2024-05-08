namespace Viam.Core.Resources.Components
{
    public interface IServiceBase
    {
        public string ServiceName { get; }
        public SubType SubType { get; }
    }
}
