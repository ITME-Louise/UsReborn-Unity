using UnityEngine;

namespace BKPureNature
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ParticleSystem))]
    public class TerrainParticleFollow : MonoBehaviour
    {
        public Terrain terrain;
        public float yOffset = 0.0f;
        private new ParticleSystem particleSystem;
        private ParticleSystem.Particle[] particles;

        void Start()
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        void LateUpdate()
        {
            if (terrain == null || particleSystem == null) return;

            int particleCount = particleSystem.particleCount;
            if (particles == null || particles.Length < particleCount)
                particles = new ParticleSystem.Particle[particleCount];

            particleSystem.GetParticles(particles);

            // Check the particle system’s simulation space
            bool isWorldSpace = particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World;

            Vector3 particleSystemPosition = particleSystem.transform.position;

            for (int i = 0; i < particleCount; i++)
            {
                Vector3 position = particles[i].position;

                if (isWorldSpace)
                {
                    // For World Space: use the particle's position directly
                    float terrainHeight = terrain.SampleHeight(position) + terrain.transform.position.y + yOffset;
                    particles[i].position = new Vector3(position.x, terrainHeight, position.z);
                }
                else
                {
                    // For Local Space: adjust based on particle system position
                    Vector3 worldPosition = position + particleSystemPosition;
                    float terrainHeight = terrain.SampleHeight(worldPosition) + terrain.transform.position.y + yOffset;
                    particles[i].position = new Vector3(worldPosition.x - particleSystemPosition.x, terrainHeight - particleSystemPosition.y, worldPosition.z - particleSystemPosition.z);
                }
            }

            particleSystem.SetParticles(particles, particleCount);
        }

        void OnValidate()
        {
            if (particleSystem == null)
                particleSystem = GetComponent<ParticleSystem>();

            particleSystem.Clear();
        }
    }
}
