namespace Typesafe.Snapshots.Registry
{
    internal static class TypeRegistryLoader
    {
        private static readonly ITypeRegistry TypeRegistry;
        
        static TypeRegistryLoader()
        {
            var typeRegistryBuilder = new TypeRegistryBuilder();
            AssemblyScanner.RegisterTypeCloners(typeRegistryBuilder);
            TypeRegistry = typeRegistryBuilder.Build();
        }

        public static ITypeRegistry GetTypeRegistry() => TypeRegistry;
    }
}