using System;
using System.Linq;

namespace Typesafe.Snapshots.Registry
{
    public static class AssemblyScanner
    {
        public static void RegisterTypeCloners(ITypeRegistryBuilder typeRegistryBuilder)
        {
            if (typeRegistryBuilder == null) throw new ArgumentNullException(nameof(typeRegistryBuilder));

            var typeRegistrations =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.ImplementsInterface<ITypeClonerRegistrar>()
                select type;
            
            foreach (var typeRegistration in typeRegistrations)
            {
                var typeRegistrationInstance = Activator.CreateInstance(typeRegistration) as ITypeClonerRegistrar
                                               ?? throw new Exception(
                                                   $"Type '{typeRegistration.Name}' is not convertible to '{nameof(ITypeClonerRegistrar)}"
                                               );
                
                typeRegistrationInstance.RegisterTypeCloners(typeRegistryBuilder);
            }
        }
    }
}