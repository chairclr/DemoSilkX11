using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DemoSilkX11
{
    // Abstract away pointers because this is c#
    // we don't need a hundred pointers floating around
    public unsafe class Ref<T>
        where T : unmanaged 
    {
        private T* value;

        public static implicit operator IntPtr(Ref<T> val)
        {
            return (IntPtr)val.Get();
        }
        public static implicit operator T*(Ref<T> val)
        {
            return val.Get();
        }
        public static implicit operator T**(Ref<T> val)
        {
            return val.GetAddressOf();
        }
        public static implicit operator void**(Ref<T> val)
        {
            return (void**)val.GetAddressOf();
        }

        public T* Get()
        {
            return value;
        }
        public TAsType* Get<TAsType>() 
            where TAsType : unmanaged
        {
            return (TAsType*)value;
        }
        public T** GetAddressOf()
        {
            fixed (T** v = &value)
            {
                return v;
            }
        }

        public ref T Value => ref Unsafe.AsRef<T>(value);

        public Ref()
        {
            this.value = null;
        }
        public Ref(ref T value)
        {
            fixed (T* ptr = &value)
            {
                this.value = ptr;
            }
        }
        public Ref(T* value)
        {
            this.value = value;
        }
        public Ref(void* value)
        {
            this.value = (T*)value;
        }
    }
}
