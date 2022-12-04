﻿using UnityEngine;

[ExecuteInEditMode]
public class FXAATest : MonoBehaviour {

	public PostProcessingEffect fxaa;
	public bool debugOceanMask;

	private void OnRenderImage (RenderTexture src, RenderTexture dest) {
		if (debugOceanMask) {
			Graphics.Blit (FindObjectOfType<OceanMaskRenderer> ().oceanMaskTexture, dest, new Material (Shader.Find ("Unlit/Texture")));
			return;
		} else {
			Graphics.Blit (src, dest, new Material (Shader.Find ("Unlit/Texture")));
		}
	}
}