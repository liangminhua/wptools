using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;

namespace WindowsPhone.Tools
{
    public class RemoteApplicationEx
    {

        public RemoteApplication RemoteApplication { get; private set; }

        private string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    Guid productId = RemoteApplication.ProductID;

                    if (!PersistedData.Current.KnownApplication.TryGetValue(productId, out _name))
                    {
                        _name = productId.ToString();
                    }
                }

                return _name;
            }
        }

        public RemoteApplicationEx(RemoteApplication remoteApplication)
        {
            RemoteApplication = remoteApplication;
        }

    }
}
