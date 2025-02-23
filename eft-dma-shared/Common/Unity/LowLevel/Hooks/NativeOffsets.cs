namespace eft_dma_shared.Common.Unity.LowLevel.Hooks
{
    internal static class NativeOffsets
    {
        // mono-2.0-bdwgc.dll
        public const ulong mono_mprotect = 0x173C0;
        public const ulong mono_class_setup_methods = 0x3F6D0;
        public const ulong mono_marshal_free = 0x81B70;
        public const ulong mono_compile_method = 0xB6380;
        public const ulong mono_object_new = 0xB9AB0;
        public const ulong mono_type_get_object = 0xFA2D0;
        public const ulong mono_gchandle_new = 0x1189F0;
        public const ulong mono_method_signature = 0x73FE0;
        public const ulong mono_signature_get_param_count = 0x96920;
        public const ulong mono_marshal_alloc_hglobal = 0x11A540;

        // UnityPlayer.dll
        public const ulong AssetBundle_CUSTOM_LoadAsset_Internal = 0x3B4A40;
        public const ulong AssetBundle_CUSTOM_LoadFromMemory_Internal = 0x3B4E10;
        public const ulong AssetBundle_CUSTOM_Unload = 0x3B5110;
        public const ulong Behaviour_SetEnabled = 0x6500B0; // Behaviour::SetEnabled
        public const ulong GameObject_CUSTOM_Find = 0x9501B0;
        public const ulong GameObject_CUSTOM_SetActive = 0x9515E0;
        public const ulong Material_CUSTOM_CreateWithShader = 0x963820;
        public const ulong Material_CUSTOM_SetColorImpl_Injected = 0x9662D0;
        public const ulong Object_CUSTOM_DontDestroyOnLoad = 0x96F020;
        public const ulong Object_Set_Custom_PropHideFlags = 0x96FA20;
        public const ulong Shader_CUSTOM_PropertyToID = 0x9883C0;
        public const ulong mono_gc_is_incremental = 0x1849AD8; // Search name as string to get offset
    }
}
