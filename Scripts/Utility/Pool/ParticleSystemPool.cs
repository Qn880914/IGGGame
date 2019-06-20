using UnityEngine;

namespace IGG.Utility
{
    public class ParticleSystemPool
    {
        private static readonly ObjectPool<ParticleSystem> s_ListParticleSystem = new ObjectPool<ParticleSystem>(null, l => l.Clear());

        public static ParticleSystem Get()
        {
            return s_ListParticleSystem.Get();
        }

        public static void Release(ParticleSystem toRelease)
        {
            s_ListParticleSystem.Release(toRelease);
        }
    }
}
