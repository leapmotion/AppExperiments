using UnityEngine;

namespace Leap.Unity.Animation.Examples {

	public class PulsatorControlTest : MonoBehaviour {

		public ScalePulsatorBehaviour pulsatorBehaviour;
		private Pulsator pulsator { get { return (Pulsator)pulsatorBehaviour; } }

		public KeyCode warmUpKey = KeyCode.W;
		public KeyCode pulsekey = KeyCode.P;

		private void Update() {
			var shouldBeWarm = Input.GetKey(warmUpKey);
			var shouldPulse = Input.GetKeyDown(pulsekey);

			if (shouldBeWarm && !pulsator.isWarm) {
				pulsator.WarmUp();
			}
			if (!shouldBeWarm && pulsator.isWarm) {
				pulsator.Relax();
			}

			// Pulsating is always transient state, that can then return to either the
			// warm state or the non-warm state. To constantly pulsate, the pulsator
			// would need to experience many Pulse() events.
			if (shouldPulse) {
				pulsator.Pulse();
			}
		}

	}

}