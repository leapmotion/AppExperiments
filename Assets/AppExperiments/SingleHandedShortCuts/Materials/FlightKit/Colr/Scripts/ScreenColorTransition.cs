using UnityEngine;

namespace Colr {
	
	public class ScreenColorTransition : MonoBehaviour {
		public Color color;

		public bool fadeIn;
		[Range(0f, 10f)]
		public float duration = 1.0f;

		private Material material;
		private float alpha;
		private float delta;

		public void BeginFadeIn(float time, Color color) {
			this.color = color;
			BeginFadeIn(time);
		}

		void Awake() {
            var shader = Shader.Find("Hidden/ScreenColorTransition");
            if (shader == null)
            {
                enabled = false;
                return;
            }
            
			material = new Material(shader);
            if (material == null)
            {
                enabled = false;
                return;
            }

			if (fadeIn)	{
				alpha = 1.0f;
				delta = -1.0f / duration;
			} else {
				alpha = 0.0f;
				enabled = false;
			}
		}

		void Update () {
			alpha = Mathf.Clamp01(alpha + delta * Time.deltaTime);
			if (alpha == 0.0f) {
				enabled = false;	
			}
		}

		public void BeginFadeOut() {
			delta = 1.0f / duration;
			enabled = true;
		}

		public void BeginFadeOut(float duration) {
			delta = 1.0f / duration;
			enabled = true;
		}

		public void BeginFadeOut (float time, Color color) {
			this.color = color;
			BeginFadeOut(time);
		}

		public void BeginFadeIn ()
		{
			delta = -1.0f / duration;
			enabled = true;
		}

		public void BeginFadeIn (float duration) {
			delta = -1.0f / duration;
			enabled = true;
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			material.color = color;
			material.SetFloat("_Alpha", alpha);
			Graphics.Blit(source, destination, material);
		}
	}

}
