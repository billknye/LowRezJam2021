using Microsoft.Xna.Framework;

namespace LowRezJam2021
{
    public class Particle
    {
        public Color StartColor;
        public Color EndColor;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Created;

        public Particle()
        {
            Created = float.MinValue;
        }
    }
}
