using System.Diagnostics;

namespace Typesafe.Snapshots.Cloner
{
    internal class NonCloningCloner<T> : ITypeCloner<T>
    {
        public T Clone(T instance)
        {
            var debuggerWrapper = new DebuggerWrapper<T>(instance);

            return debuggerWrapper;
        }
        
        [DebuggerDisplay("Not snapshot: {Value}")]
        private class DebuggerWrapper<T>
        {
            public readonly T Value;

            public DebuggerWrapper(T instance)
            {
                Value = instance;
            }

            public static implicit operator DebuggerWrapper<T>(T instance) => new DebuggerWrapper<T>(instance);

            public static implicit operator T(DebuggerWrapper<T> wrapper) => wrapper.Value;
        }

    }
}