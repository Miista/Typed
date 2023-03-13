namespace Typesafe.Snapshots.Cloner
{
    internal class StringCloner : ITypeCloner<string>
    {
        public string Clone(string instance)
        {
            return string.Copy((string) instance);
        }
    }
}