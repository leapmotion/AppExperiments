using UnityEngine;
using System.Collections;

public static class Materials
{
	private static Material[] _material = new Material[2];

	public static void Allocate()
	{
		if (Buffers.Allocated ())
		{
			for (int i = 0; i < Materials.material.Length; i++)
			{
				Materials.display.SetTexture ("_RenderTarget" + i.ToString (), Buffers.render_texture[i]);
			}
		}
	}


	public static Material[] material 
	{
		get
		{
			return _material;
		}
		set
		{
			material = value;
		}
	}


	public static Material display
	{
		get
		{
			if(_material[0] == null)
			{
				_material[0]	= new Material(Shader.Find("Display"));
			}

			return _material[0];
		}
	}


	public static void Destroy()
	{
		if(_material != null)
		{
			for(int i = 0; i < _material.Length; i++)
			{
				Material.DestroyImmediate(_material[i]);
			}
		}
	}
}
