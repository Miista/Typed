using System;

namespace Typesafe.Snapshots.Cloner
{
    internal class GuidCloner : ITypeCloner<Guid>
    {
        public Guid Clone(Guid instance)
        {
            return new Guid(instance.ToByteArray());
        }
    }
}