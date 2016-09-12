using System.Collections.Generic;

namespace Nimbus.Configuration.Settings
{
    public class EnablePartitioningSetting : Setting<bool>
    {
        public override bool Default
        {
            get { return false; }
        }
    }
}
