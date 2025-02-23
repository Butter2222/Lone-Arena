namespace eft_dma_shared.Common.Unity
{
    public readonly struct UnityOffsets
    {
        public readonly struct ModuleBase
        {
            public const uint GameObjectManager = 0x17FFD28; // to eft_dma_radar.GameObjectManager
            public const uint AllCameras = 0x179F500; // Lookup in IDA 's_AllCamera'
            public const uint ManagerContext = 0x17FFAE0; // Lookup in IDA
            public const uint InputManager = ManagerContext + 1 * 0x8; // ManagerContext[1]
            public const uint GfxDevice = 0x1847810; // g_MainGfxDevice , Type GfxDeviceClient
        }
        public readonly struct UnityInputManager
        {
            public const uint CurrentKeyState = 0x58; // 0x50 + 0x8
            public const uint ThisFrameKeyDown = 0x78; // 0x70 + 0x8
            public const uint ThisFrameKeyUp = 0x98; // 0x90 + 0x8
        }
        public readonly struct TransformInternal
        {
            public const uint TransformAccess = 0x38; // to TransformHierarchy
        }
        public readonly struct TransformAccess
        {
            public const uint Vertices = 0x18; // MemList<TrsX>
            public const uint Indices = 0x20; // MemList<int>
        }
        public readonly struct SkinnedMeshRenderer // SkinnedMeshRenderer : Renderer
        {
            public const uint Renderer = 0x10; // Renderer : Unity::Component
        }
        public readonly struct Renderer // Renderer : Unity::Component
        {
            public const uint Materials = 0x148; // m_Materials : dynamic_array<PPtr<Material>,0>
            public const uint Count = 0x158; // Extends from m_Materials type (0x20 length?)
        }

        public readonly struct Camera // NOTE: Add previous struct size to get final offset
        {
            private const uint State = 0x40; // Camera::CopiableState (m_State)
            public const uint ViewMatrix = State + 0x9C; // Matrix4x4 (m_WorldToClipMatrix)
            public const uint FOV = State + 0x11C; // float (m_FieldOfView)
            public const uint LastPosition = State + 0x3EC; // float (m_LastPosition)
            public const uint NearClip = State + 0x3FC; // float (m_NearClip)
            public const uint AspectRatio = State + 0x488; // float (m_Aspect)
            public const uint OcclusionCulling = State + 0x4BC; // bool (m_OcclusionCulling)
        }

        public readonly struct GfxDeviceClient
        {
            public const uint Viewport = 0x2A28; // m_Viewport      RectT<int> ?
        }

        public readonly struct UnityAnimator // Animator        struc ; (sizeof=0x6A0, align=0x8, copyof_18870)
        {
            public const uint Speed = 0x47C; // 0000047C m_Speed
        }

        public readonly struct SSAA // Unity.Postprocessing.Runtime Assembly in UNISPECT
        {
            public const uint OpticMaskMaterial = 0x58;
        }
    }
}
