using System;
using MethodBoundaryAspect.Fody.Attributes;

namespace Viam.Core.Logging
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class LogCallAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            Logger.GetLogger<LogCallAttribute>().LogMethodInvocationStart(args.Method.Name, args.Arguments);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Logger.GetLogger<LogCallAttribute>().LogMethodInvocationFinish(args.Method.Name, args.Arguments);
        }
    }
}
