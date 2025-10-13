using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components
{
    public interface IComponentServiceBase
    {
        public static abstract SubType SubType { get; }
    }
}