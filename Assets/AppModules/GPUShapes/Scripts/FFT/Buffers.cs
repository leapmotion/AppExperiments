using UnityEngine;
using System.Collections;


public static class Buffers
{
	private const RenderTextureFormat _FORMAT 	= RenderTextureFormat.ARGB32;
	public const RenderTextureReadWrite MODE 	= RenderTextureReadWrite.sRGB;

	private static RenderBuffer[] _render_buffer = null;
	public static RenderBuffer[] color 
	{ 
		get
		{
			if(_render_buffer == null)
			{
				Allocate();
			}

			return _render_buffer;
		}
		set
		{ 
			color = value;
		}
	}


	private static RenderTexture[] _render_texture = null;
	public static RenderTexture[] render_texture 
	{
		get
		{
			return _render_texture;
		}
		set
		{
			render_texture = value;
		}
	}


	public static RenderBuffer depth 
	{
		get
		{
			return _render_texture[Main.BUFFERS].depthBuffer;
		}
	}


	public static RenderTexture target 
	{
		get
		{
			return _render_texture[Main.BUFFERS];
		}
		set
		{
			render_texture[Main.BUFFERS] = value;
		}
	}


	public static void Allocate()
	{
		_render_texture			= new RenderTexture[Main.BUFFERS + 1];			
		_render_buffer 			= new RenderBuffer[Main.BUFFERS + 1];

		for(int i = 0; i < _render_buffer.Length; i++)
		{
			if(_render_texture[i] == null)
			{
				_render_texture[i] = Create(Main.SCALE, Main.SCALE);
			}

			color[i] = render_texture[i].colorBuffer;
		}
	}


	private static RenderTexture Create(int width, int height)
	{
		RenderTexture render_texture 		= new RenderTexture(width, height, 0, _FORMAT, MODE);
		render_texture.anisoLevel			= 0;
		render_texture.wrapMode				= TextureWrapMode.Clamp;
		render_texture.filterMode 			= FilterMode.Point;
		render_texture.enableRandomWrite 	= false;
		render_texture.useMipMap 			= false;
		render_texture.Create();

		return	render_texture;
	}


	public static bool Allocated()
	{
		return _render_texture != null;
	}


	public static void Destroy()
	{
		if(_render_texture != null)
		{
			for(int i = 0; i < _render_texture.Length; i++)
			{
				if(_render_texture[i] == RenderTexture.active)
				{
					_render_texture[i].Release();
				}

				_render_texture[i].DiscardContents();	

				RenderTexture.DestroyImmediate(_render_texture[i]);
			}
		}
	}
}
