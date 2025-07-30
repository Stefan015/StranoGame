using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class ColorBlitRendererFeature : ScriptableRendererFeature
{
	public Shader _Shader;
	public float _Intensity;

	Material _Material;

	ColorBlitPass _RenderPass = null;

	public override void AddRenderPasses(ScriptableRenderer renderer,
		ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType == CameraType.Game)
			renderer.EnqueuePass(_RenderPass);
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer,
		in RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType == CameraType.Game)
		{
			// Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
			// ensures that the opaque texture is available to the Render Pass.
			_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
			_RenderPass.SetTarget(renderer.cameraColorTargetHandle, _Intensity);
		}
	}

	public override void Create()
	{
		_Material = CoreUtils.CreateEngineMaterial(_Shader);
		_RenderPass = new ColorBlitPass(_Material);
	}

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(_Material);
	}
}