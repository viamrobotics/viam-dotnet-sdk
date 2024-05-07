using System;
using System.Threading.Tasks;
using MethodBoundaryAspect.Fody.Attributes;

namespace Viam.Core.Logging
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class LogInvocationAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args) =>
            Logger.GetLogger<LogInvocationAttribute>()
                  .LogMethodInvocationStart(args.Method.Name, args.Arguments);

        public override void OnExit(MethodExecutionArgs args)
        {
            // If this is an async call, we want to log when the async call is complete, not when 
            // control is returned to the caller
            if (args.ReturnValue is Task t)
            {
                // So we can add a ContinueWith to the task chain to make sure we get called at some point
                t.ContinueWith(task =>
                {
                    // If the task is completed successfully, we can just invoke the happy case
                    if (task.IsCompletedSuccessfully)
                    {
                        Logger.GetLogger<LogInvocationAttribute>()
                              .LogMethodInvocationFinish(
                                  args.Method.Name,
                                  args.Arguments);
                    }

                    // If the task is faulted, now we want to log the exception
                    if (task.IsFaulted)
                    {
                        Logger.GetLogger<LogInvocationAttribute>()
                              .LogMethodInvocationException(
                                  args.Method.Name,
                                  args.Arguments,
                                  task.Exception);
                    }

                    // If the task was canceled, we might as well log that as well
                    if (task.IsCanceled)
                    {
                        Logger.GetLogger<LogInvocationAttribute>()
                              .LogMethodInvocationCanceled(
                                  args.Method.Name,
                                  args.Arguments);
                    }
                });
            }
            else if (args.ReturnValue is ValueTask vt)
            {
                // So we can add a ContinueWith to the task chain to make sure we get called at some point
                // I don't like using AsTask because it causes an allocation
                // TODO: Fix logging for ValueTask such that we don't need AsTask()
                vt.AsTask().ContinueWith(task =>
                {
                    // If the task is completed successfully, we can just invoke the happy case
                    if (task.IsCompletedSuccessfully)
                    {
                        Logger.GetLogger<LogInvocationAttribute>()
                              .LogMethodInvocationFinish(
                                  args.Method.Name,
                                  args.Arguments);
                    }

                    // If the task is faulted, now we want to log the exception
                    if (task.IsFaulted)
                    {
                        Logger.GetLogger<LogInvocationAttribute>()
                              .LogMethodInvocationException(
                                  args.Method.Name,
                                  args.Arguments,
                                  task.Exception);
                    }

                    // If the task was canceled, we might as well log that as well
                    if (task.IsCanceled)
                    {
                        Logger.GetLogger<LogInvocationAttribute>()
                              .LogMethodInvocationCanceled(
                                  args.Method.Name,
                                  args.Arguments);
                    }
                });
            }
            else
            {
                Logger.GetLogger<LogInvocationAttribute>()
                      .LogMethodInvocationFinish(
                          args.Method.Name,
                          args.Arguments);
            }
        }

        //public override void OnException(MethodExecutionArgs arg) => Logger.GetLogger<LogInvocationAttribute>()
        //                                                                   .LogMethodInvocationException(
        //                                                                       arg.Method.Name,
        //                                                                       arg.Arguments,
        //                                                                       arg.Exception);
    }
}
