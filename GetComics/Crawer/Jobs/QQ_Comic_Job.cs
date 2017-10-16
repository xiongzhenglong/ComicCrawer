using Chloe;
using Chloe.SqlServer;
using Framework.Common.Extension;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [DisallowConcurrentExecution]
    public class QQ_Comic_Job:IJob
    {
        MsSqlContext dbcontext;
        public QQ_Comic_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

        }

        public void Execute(IJobExecutionContext context)
        {
            IQuery<User> q = context.Query<User>();
        }
    }
}
