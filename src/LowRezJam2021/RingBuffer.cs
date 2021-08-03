using System;

namespace LowRezJam2021
{
    public class RingBuffer<T> where T : new()
    {
        protected T[] items;
        protected int nextFree;

        public RingBuffer(int count)
        {
            items = new T[count];

            for (int i = 0; i < count; i++)
            {
                items[i] = new T();
            }
        }

        protected void Add(Action<T> change)
        {
            change(items[nextFree]);
            nextFree = (nextFree + 1) % items.Length;
        }
    }
}
