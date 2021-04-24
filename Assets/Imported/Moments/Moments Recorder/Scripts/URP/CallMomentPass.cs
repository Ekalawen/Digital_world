namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Draws full screen mesh using given material and pass and reading from source target.
    /// </summary>
    internal class CallMomentPass : ScriptableRenderPass
    {
        public FilterMode filterMode { get; set; }
        public CallMomentFeature.Settings settings;

        RenderTargetIdentifier source;
        RenderTargetIdentifier destination;
        int temporaryRTId = Shader.PropertyToID("_TempRT");

        int sourceId;
        int destinationId;
        bool isSourceAndDestinationSameTarget;

        string m_ProfilerTag;

        protected Moments.Recorder momentRecorder;
        RenderTexture renderTextureSource = null;
        RenderTargetIdentifier sourceTextureId;

        public CallMomentPass(string tag) {
            m_ProfilerTag = tag;
            momentRecorder = GameObject.FindObjectOfType<Moments.Recorder>();
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            blitTargetDescriptor.depthBufferBits = 0;

            isSourceAndDestinationSameTarget = settings.sourceType == settings.destinationType &&
                (settings.sourceType == BufferType.CameraColor || settings.sourceTextureId == settings.destinationTextureId);

            var renderer = renderingData.cameraData.renderer;

            if (settings.sourceType == BufferType.CameraColor) {
                sourceId = -1;
                source = renderer.cameraColorTarget;
            }
            else
            {
                sourceId = Shader.PropertyToID(settings.sourceTextureId);
                cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode);
                source = new RenderTargetIdentifier(sourceId);
            }

            if (isSourceAndDestinationSameTarget)
            {
                destinationId = temporaryRTId;
                cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
                destination = new RenderTargetIdentifier(destinationId);
            }
            else if (settings.destinationType == BufferType.CameraColor)
            {
                destinationId = -1;
                destination = renderer.cameraColorTarget;
            }
            else
            {
                destinationId = Shader.PropertyToID(settings.destinationTextureId);
                cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
                destination = new RenderTargetIdentifier(destinationId);
            }

            if(momentRecorder != null && IsMainCamera(ref renderingData)) {
                renderTextureSource = new RenderTexture(renderingData.cameraData.cameraTargetDescriptor);
                sourceTextureId = new RenderTargetIdentifier(renderTextureSource);
            }
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            Blit(cmd, source, destination);

            if (momentRecorder != null && IsMainCamera(ref renderingData)) {
                cmd.Blit(source, sourceTextureId);
                //cmd.CopyTexture(BuiltinRenderTextureType.CurrentActive, sourceTextureId);
            }

            context.ExecuteCommandBuffer(cmd);
            context.Submit();
            CommandBufferPool.Release(cmd);

            if (momentRecorder != null && IsMainCamera(ref renderingData)) {
                momentRecorder.OnCustomRenderImage(renderTextureSource);
                RenderTexture.active = null;
                renderTextureSource.Release();
            }
        }

        protected bool IsMainCamera(ref RenderingData renderingData) {
            return renderingData.cameraData.camera.name == momentRecorder.CameraName;
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destinationId != -1)
                cmd.ReleaseTemporaryRT(destinationId);

            if (source == destination && sourceId != -1)
                cmd.ReleaseTemporaryRT(sourceId);
        }
    }
}
