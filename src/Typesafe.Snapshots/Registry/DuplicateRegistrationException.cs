using System;

namespace Typesafe.Snapshots.Registry
{
    public class DuplicateRegistrationException : Exception
    {
        public DuplicateRegistrationException(Type type) : base($"Cloner for type '{type.Name}' has already been registered") { }
    }
}