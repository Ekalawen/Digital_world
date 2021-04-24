namespace UnityEngine.Rendering.Universal
{
    public enum BufferType
    {
        CameraColor,
        Custom
    }

    public class CallMomentFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

            // public int blitMaterialPassIndex = -1; // I think I don't use that anymore, but not sure so I let it here ?
            public BufferType sourceType = BufferType.CameraColor;
            public BufferType destinationType = BufferType.CameraColor;
            public string sourceTextureId = "_SourceTexture";
            public string destinationTextureId = "_DestinationTexture";
        }

        public Settings settings = new Settings();
        CallMomentPass callMomentPass;

        public override void Create() {
#if UNITY_EDITOR
            callMomentPass = new CallMomentPass(name);
#endif
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
#if UNITY_EDITOR
            callMomentPass.renderPassEvent = settings.renderPassEvent;
            callMomentPass.settings = settings;
            renderer.EnqueuePass(callMomentPass);
#endif
        }
    }
}

