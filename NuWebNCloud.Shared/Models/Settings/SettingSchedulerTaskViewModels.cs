using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class SettingSchedulerTaskViewModels
    {
        public List<SettingSchedulerTaskModels> ListSchedulerTaskModels { get; set; }

        public SettingSchedulerTaskViewModels()
        {
            ListSchedulerTaskModels = new List<SettingSchedulerTaskModels>();
        }
    }
}
