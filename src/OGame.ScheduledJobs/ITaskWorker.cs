using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OGame.ScheduledJobs
{
    interface ITaskWorker
    {
        Task Run(CancellationToken cancellationToken);
    }
}
