using UnityEngine;

namespace ParticleSim {

	public class FluidSimulation : ScriptableObject {

		public const int MAX_PARTICLES = 65535;

		private int _particleCount = 0;
		public int particleCount { get { return _particleCount; } }

	}

}