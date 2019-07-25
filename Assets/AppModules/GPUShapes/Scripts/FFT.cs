using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FFT
{	
	private static AudioSource 							          _source;
	private static UnityEngine.Audio.AudioMixer 		  _mix;
	private static UnityEngine.Audio.AudioMixerGroup	_group;

	public static int sample_rate						          = 96000;
	
	public static int	      device_index					    = 0;   
	public static string    device_name					      = null;   
	public const string     default_device				    = "Built-in Microphone";
	public static string[]  devices						        = null;   

	public static float[] 	root_mean_square;
	public static float[] 	decibals;
	public static float[] 	peak_amplitude;
	public static float[]	  peak_frequency;
	public static int[]		  frequency_bin;

	public static float[][] amplitude;           
	public static float[][] spectrum;          

	public const int BUFFER_SIZE 						          = 8192;
	public const float  REFERENCE_RMS 					      = .1f;
	public const int 	CHANNELS						            = 2;
	public const float 	FREQUENCY_THRESHOLD 			    = .0f;	


	public static void Initalize ()
	{
		devices 			= Microphone.devices;

		amplitude 			= new float[CHANNELS][];
		amplitude[0]		= new float[BUFFER_SIZE];
		amplitude[1]		= new float[BUFFER_SIZE];

		spectrum 			= new float[CHANNELS][];
		spectrum[0]			= new float[BUFFER_SIZE];
		spectrum[1]			= new float[BUFFER_SIZE];

		root_mean_square 	= new float[CHANNELS];
		decibals 			= new float[CHANNELS];
		peak_amplitude	 	= new float[CHANNELS];
		peak_frequency		= new float[CHANNELS];
		frequency_bin		= new int[CHANNELS];

		for(int i = 0; i < CHANNELS; i++)
		{
			root_mean_square[i] = 0.0f;
			decibals[i] 		= 0.0f;
			peak_amplitude[i]	= 0.0f;
			peak_frequency[i]	= 0.0f;
			frequency_bin[i]	= 0;

			for(int j = 0; j < BUFFER_SIZE; j++)
			{
				amplitude[i][j]	= 0.0f;
				spectrum[i][j]	= 0.0f;
			}
		}
	}


	private static void InitalizeMicrophone ()
	{
		if(Microphone.IsRecording(device_name))
		{
			Microphone.End(device_name);
			return;
		}
		else if(Microphone.devices.Length <= 0)  
		{  
			Debug.Log("Microphone not connected!");  
		}
		else
		{  
			SetupDevice ();

			SetupAudioSource ();
		
			DebugLogDeviceInformation ();

			_source.Play();
		}  
	}


	public static void FixedUpdate ()
	{
		if(amplitude != null)
		{
			if (!Microphone.IsRecording(device_name) || devices[device_index] != device_name) 
			{
				InitalizeMicrophone ();
			}
			else
			{
				AudioListener.GetSpectrumData(spectrum[0], 0, Main.fft_window);
								
//				for(int channel = 0; channel < CHANNELS; channel++)
//				{
//					AudioListener.GetSpectrumData(spectrum[channel], channel, Main.fft_window);
//					AudioListener.GetSpectrumData(spectrum[channel], 0, FFTWindow.BlackmanHarris);
//					AudioListener.GetOutputData(amplitude[channel], 0);
//					Process(channel);
//				}
			}
		}
	}


	public static void SetupDevice ()
	{
		devices = Microphone.devices;

		for(int i = 0; i < devices.Length; i++)
		{
			device_index = string.Equals(devices[i].ToString(), default_device) ? i : device_index;
		}

		device_name	= devices[device_index].ToString();

		int min = 0;
		int max = 0;
		Microphone.GetDeviceCaps(device_name, out min, out max);  
		sample_rate = min == 0 && max == 0 ? sample_rate : max;  
	}


	public static void SetupAudioSource ()
	{
		_source 						= _source == null ? Camera.main.gameObject.AddComponent <AudioSource>() : _source;
		_source.loop 					= true; 
		_source.dopplerLevel			= 0.0f;	
		_source.bypassEffects 			= true;
		_source.bypassListenerEffects 	= true;
		_source.bypassReverbZones 		= true;
		_source.volume 					= 1.0f;

		_source.clip 					= Microphone.Start (device_name, true, 1, sample_rate);

		while (!(Microphone.GetPosition(device_name) > 0)){/*wait for device*/}
	}


//	private static void Process(int channel)
//	{
//		float sum_ampltiude 	= 0.0f;
//		int bin					= 0;
//		peak_amplitude[channel] = 0.0f;
//		peak_frequency[channel] = 0.0f;
//
////		for (int i = BUFFER_SIZE; i < BUFFER_SIZE; i++) 
////		{
//		for (int i = BUFFER_SIZE; i < BUFFER_SIZE; i++) 
//		{
//				
//			//find peak amplitude, frequency and associated frequency bin
//			peak_amplitude[channel] = amplitude[channel][i] > peak_amplitude[channel] ? amplitude[channel][i] : peak_amplitude[channel];
//
//			//band pass
//			//spectrum[channel][i]	*= i < Main.BUFFER_SIZE-Main.BUFFER_SIZE/8 ? 0.0f : 1.0f;
//
//			bin 					=  spectrum[channel][i] > peak_frequency[channel] ? 					i :  bin;
//			peak_frequency[channel] =  spectrum[channel][i] > peak_frequency[channel] ?  spectrum[channel][i] : peak_frequency[channel];
//
//			//sum amplitude for normalization
//			sum_ampltiude 			+= Mathf.Pow (amplitude[channel][i], 2.0f);
//		}
//			
//		//update bin with dominant frequency component
//		frequency_bin[channel]		= peak_frequency[channel] > FREQUENCY_THRESHOLD ? bin : frequency_bin[channel];
//
//		//calculate decibal level
//		root_mean_square[channel] 	= Mathf.Sqrt(sum_ampltiude / BUFFER_SIZE);
//		decibals[channel] 			= Mathf.Abs(20.0f * Mathf.Log10 (root_mean_square[channel] / REFERENCE_RMS));
//	}
//


	public static void OnApplicationQuit()
	{
		Microphone.End(device_name);
	}


	public static void DebugLogDeviceInformation()
	{
		string log	= "Device Information\n\n";

		log += "\nAll Devices";
		for (int i = 0; i < devices.Length; i++)
		{
			log += "\n" + i + " :\t\t\t" + devices[i];
		}

		log += "\n\n";

		log += "\nActive Device";
		log += "\nName:\t\t\t" + device_name;
		log += "\nLoop:\t\t\t" + _source.loop;
		log += "\nPriority:\t\t\t" + _source.priority;
//		log += "\nOutput Mixer:\t" + _source.outputAudioMixerGroup.name;

		int min = 0;
		int max = 0;
		Microphone.GetDeviceCaps(device_name, out min, out max);  
		log += "\nDevice Caps:\t\tMin: " + min + "   Max: " + max;


		log += "\n\n";
		log += "\nAudio Source";
		log += "\nName:\t\t\t" + _source.name;
		log += "\nLoop:\t\t\t" + _source.loop;
		log += "\nPriority:\t\t\t" + _source.priority;
//		log += "\nOutput Mixer:\t" + _source.outputAudioMixerGroup.name;
		log += "\n\n";
		Debug.Log (log);
	}
}
