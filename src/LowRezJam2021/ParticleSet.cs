using System;

namespace LowRezJam2021
{
    public class ParticleSet : RingBuffer<Particle>
    {
        int minScanIndex;
        float currentTime;
        float maxScanTime;

        public ParticleSet(int count, float lifeTime)
            : base(count)
        {
            LifeTime = lifeTime;
        }

        public float LifeTime { get; }

        public void Add(Action<Particle> change)
        {
            base.Add(n =>
            {
                change(n);
                n.Created = currentTime;
            });

            maxScanTime = currentTime + LifeTime;
        }

        public void Scan(float time, Action<Particle> with)
        {
            currentTime = time;

            if (time > maxScanTime)
                return;

            var minTime = time - LifeTime;

            while (items[minScanIndex].Created < minTime)
            {
                minScanIndex = (minScanIndex + 1) % items.Length;
            }

            int index = minScanIndex;
            while (index != nextFree)
            {
                var item = items[index];
                with(item);
                index = (index + 1) % items.Length;
            }
        }
    }
}
