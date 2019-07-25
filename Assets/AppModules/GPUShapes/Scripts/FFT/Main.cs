using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour 
{

	public const int BUFFERS	                  = 1;
	public const int SCALE 		                  = 1024;

	public FFTWindow window					            = FFTWindow.BlackmanHarris;
	public static FFTWindow fft_window		      = FFTWindow.BlackmanHarris;
	private static float[] _fft_uniform;
	public Material inspector_material;

  public ProceduralShape display_object;

  void Awake()
	{
		_fft_uniform 		                          = new float[1000];

		FFT.Initalize();
	}

	void Start()
	{
	
	}

	void FixedUpdate()
	{
		fft_window = window;

    if (Buffers.Allocated()) 
    {
      FFT.FixedUpdate();
      SetFFTUniforms();
    }
  }

	private void SetFFTUniforms()
	{		
		for(int i = 0; i < _fft_uniform.Length; i++)
		{
			_fft_uniform[i] = FFT.spectrum[0][i]; 
		}

    display_object.material.SetFloatArray("_FFT", _fft_uniform);
   // Materials.display.SetFloat ("_Frame", (float)Time.frameCount);
		// Materials.display.SetFloat ("_Amplitude", Mathf.Clamp01(amplitude));
		// Materials.display.SetFloat ("_Scale", Mathf.Clamp01(amplitude));
	}


	public static void OnApplicationQuit()
	{
	}
}