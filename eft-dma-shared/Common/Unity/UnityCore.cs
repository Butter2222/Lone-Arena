using eft_dma_shared.Common.DMA;
using eft_dma_shared.Common.Misc.Pools;
using SkiaSharp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace eft_dma_shared.Common.Unity
{
    /// <summary>
    /// Unity Game Object Manager. Contains all Game Objects.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct GameObjectManager
    {
        public readonly ulong LastTaggedNode; // 0x0

        public readonly ulong TaggedNodes; // 0x8

        public readonly ulong LastMainCameraTaggedNode; // 0x10

        public readonly ulong MainCameraTaggedNodes; // 0x18

        public readonly ulong LastActiveNode; // 0x20

        public readonly ulong ActiveNodes; // 0x28


        /// <summary>
        /// Returns the Game Object Manager for the current UnityPlayer.
        /// </summary>
        /// <param name="unityBase">UnityPlayer Base Addr</param>
        /// <returns>Game Object Manager</returns>
        public static GameObjectManager Get(ulong unityBase)
        {
            try
            {
                var gomPtr = Memory.ReadPtr(unityBase + UnityOffsets.ModuleBase.GameObjectManager, false);
                return Memory.ReadValue<GameObjectManager>(gomPtr, false);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR Loading Game Object Manager", ex);
            }
        }

        /// <summary>
        /// Helper method to locate GOM Objects.
        /// </summary>
        public static ulong GetObjectFromList(ulong currentObjectPtr, ulong lastObjectPtr, string objectName)
        {
            var currentObject = Memory.ReadValue<BaseObject>(currentObjectPtr);
            var lastObject = Memory.ReadValue<BaseObject>(lastObjectPtr);

            if (currentObject.CurrentObject != 0x0)
            {
                while (currentObject.CurrentObject != 0x0 && currentObject.CurrentObject != lastObject.CurrentObject)
                {
                    var objectNamePtr = Memory.ReadPtr(currentObject.CurrentObject + GameObject.NameOffset);
                    var objectNameStr = Memory.ReadString(objectNamePtr, 64);
                    if (objectNameStr.Equals(objectName, StringComparison.OrdinalIgnoreCase))
                        return currentObject.CurrentObject;

                    currentObject = Memory.ReadValue<BaseObject>(currentObject.NextObjectLink); // Read next object
                }
            }
            return 0x0;
        }
    }

    /// <summary>
    /// GOM List Node.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct BaseObject
    {
        /// <summary>
        /// Previous ListNode
        /// </summary>
        public readonly ulong PreviousObjectLink; // 0x0
        /// <summary>
        /// Next ListNode
        /// </summary>
        public readonly ulong NextObjectLink; // 0x8
        /// <summary>
        /// Current GameObject
        /// </summary>
        public readonly ulong CurrentObject; // 0x10   (to Offsets.GameObject)
    };

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct MonoBehaviour // Behaviour : Component : EditorExtension : Object
    {
        public const uint InstanceIDOffset = 0x8;
        public const uint ObjectClassOffset = 0x28;
        public const uint GameObjectOffset = 0x30;
        public const uint EnabledOffset = 0x38;
        public const uint IsAddedOffset = 0x39;

        [FieldOffset((int)InstanceIDOffset)]
        public readonly int InstanceID; // m_InstanceID
        [FieldOffset((int)ObjectClassOffset)]
        public readonly ulong ObjectClass; // m_Object
        [FieldOffset((int)GameObjectOffset)]
        public readonly ulong GameObject; // m_GameObject
        [FieldOffset((int)EnabledOffset)]
        public readonly bool Enabled; // m_Enabled
        [FieldOffset((int)IsAddedOffset)]
        public readonly bool IsAdded; // m_IsAdded

        /// <summary>
        /// Return the game object of this MonoBehaviour.
        /// </summary>
        /// <returns>GameObject struct.</returns>
        public readonly GameObject GetGameObject() =>
            Memory.ReadValue<GameObject>(ObjectClass);

        /// <summary>
        /// Gets a component class from a Behaviour object.
        /// </summary>
        /// <param name="behaviour">Behaviour object to scan.</param>
        /// <param name="className">Name of class of child.</param>
        /// <returns>Child class component.</returns>
        public static ulong GetComponent(ulong behaviour, string className)
        {
            var go = Memory.ReadPtr(behaviour + GameObjectOffset);
            return eft_dma_shared.Common.Unity.GameObject.GetComponent(go, className);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct GameObject // EditorExtension : Object
    {
        public const uint InstanceIDOffset = 0x8;
        public const uint ObjectClassOffset = 0x28;
        public const uint ComponentsOffset = 0x30;
        public const uint NameOffset = 0x60;

        [FieldOffset((int)InstanceIDOffset)]
        public readonly int InstanceID; // m_InstanceID
        [FieldOffset((int)ObjectClassOffset)]
        public readonly ulong ObjectClass; // m_Object
        [FieldOffset((int)ComponentsOffset)]
        public readonly ComponentArray Components; // m_Component, dynamic_array<GameObject::ComponentPair,0> ?
        [FieldOffset((int)NameOffset)]
        public readonly ulong Name; // m_Name, String

        /// <summary>
        /// Return the name of this game object.
        /// </summary>
        /// <returns>Name string.</returns>
        public readonly string GetName() =>
            Memory.ReadString(Name, 128);

        /// <summary>
        /// Gets a component class from a Game Object.
        /// </summary>
        /// <param name="gameObject">Game object to scan.</param>
        /// <param name="className">Name of class of child.</param>
        /// <returns>Child class component.</returns>
        public static ulong GetComponent(ulong gameObject, string className)
        {
            // component list
            var componentArr = Memory.ReadValue<ComponentArray>(gameObject + ComponentsOffset);
            int size = componentArr.Size <= 0x1000 ?
                (int)componentArr.Size : 0x1000;
            using var compsBuf = SharedArray<ComponentArrayEntry>.Get(size);
            Memory.ReadBuffer(componentArr.ArrayBase, compsBuf.Span);
            foreach (var comp in compsBuf)
            {
                var compClass = Memory.ReadPtr(comp.Component + MonoBehaviour.ObjectClassOffset);
                var name = Unity.ObjectClass.ReadName(compClass);
                if (name.Equals(className, StringComparison.OrdinalIgnoreCase))
                    return compClass;
            }
            throw new Exception("Component Not Found!");
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ComponentArray
    {
        public readonly ulong ArrayBase; // To ComponentArrayEntry[]
        public readonly ulong MemLabelId;
        public readonly ulong Size;
        public readonly ulong Capacity;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct ComponentArrayEntry
    {
        [FieldOffset(0x8)]
        public readonly ulong Component;
    }

    /// <summary>
    /// Most higher level EFT Assembly Classes and Game Objects.
    /// </summary>
    public readonly struct ObjectClass
    {
        public const uint MonoBehaviourOffset = 0x10;

        public static readonly uint[] To_GameObject = new uint[] { MonoBehaviourOffset, MonoBehaviour.GameObjectOffset };
        public static readonly uint[] To_NamePtr = new uint[] { 0x0, 0x0, 0x48 };

        /// <summary>
        /// Read the Class Name from any ObjectClass that implements MonoBehaviour.
        /// </summary>
        /// <param name="objectClass">ObjectClass address.</param>
        /// <returns>Name (string) of the object class given.</returns>
        public static string ReadName(ulong objectClass, int length = 128, bool useCache = true)
        {
            try
            {
                var namePtr = Memory.ReadPtrChain(objectClass, To_NamePtr, useCache);
                return Memory.ReadString(namePtr, length, useCache);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR Reading Object Class Name", ex);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct UnityColor
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public UnityColor(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public UnityColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }

        public UnityColor(string hex)
        {
            var color = SKColor.Parse(hex);

            R = color.Red / 255f;
            G = color.Green / 255f;
            B = color.Blue / 255f;
            A = color.Alpha / 255f;
        }

        public UnityColor(SKColor color)
        {
            R = color.Red / 255f;
            G = color.Green / 255f;
            B = color.Blue / 255f;
            A = color.Alpha / 255f;
        }

        public readonly override string ToString() => $"({R * 255}, {G * 255}, {B * 255}, {A * 255})";

        public static int GetSize()
        {
            return Unsafe.SizeOf<UnityColor>();
        }

        public static uint GetSizeU()
        {
            return (uint)GetSize();
        }

        public static UnityColor Invert(UnityColor color)
        {
            float invertedR = 1f - color.R;
            float invertedG = 1f - color.G;
            float invertedB = 1f - color.B;

            return new(invertedR, invertedG, invertedB, color.A);
        }
    }

    public enum UnityKeyCode
    {
        [Description(nameof(None))]
        None,
        [Description(nameof(Backspace))]
        Backspace = 8,
        [Description(nameof(Delete))]
        Delete = 127,
        [Description(nameof(Tab))]
        Tab = 9,
        [Description(nameof(Clear))]
        Clear = 12,
        [Description(nameof(Return))]
        Return,
        [Description(nameof(Pause))]
        Pause = 19,
        [Description(nameof(Escape))]
        Escape = 27,
        [Description(nameof(Space))]
        Space = 32,
        [Description(nameof(Keypad0))]
        Keypad0 = 256,
        [Description(nameof(Keypad1))]
        Keypad1,
        [Description(nameof(Keypad2))]
        Keypad2,
        [Description(nameof(Keypad3))]
        Keypad3,
        [Description(nameof(Keypad4))]
        Keypad4,
        [Description(nameof(Keypad5))]
        Keypad5,
        [Description(nameof(Keypad6))]
        Keypad6,
        [Description(nameof(Keypad7))]
        Keypad7,
        [Description(nameof(Keypad8))]
        Keypad8,
        [Description(nameof(Keypad9))]
        Keypad9,
        [Description(nameof(KeypadPeriod))]
        KeypadPeriod,
        [Description(nameof(KeypadDivide))]
        KeypadDivide,
        [Description(nameof(KeypadMultiply))]
        KeypadMultiply,
        [Description(nameof(KeypadMinus))]
        KeypadMinus,
        [Description(nameof(KeypadPlus))]
        KeypadPlus,
        [Description(nameof(KeypadEnter))]
        KeypadEnter,
        [Description(nameof(KeypadEquals))]
        KeypadEquals,
        [Description(nameof(UpArrow))]
        UpArrow,
        [Description(nameof(DownArrow))]
        DownArrow,
        [Description(nameof(RightArrow))]
        RightArrow,
        [Description(nameof(LeftArrow))]
        LeftArrow,
        [Description(nameof(Insert))]
        Insert,
        [Description(nameof(Home))]
        Home,
        [Description(nameof(End))]
        End,
        [Description(nameof(PageUp))]
        PageUp,
        [Description(nameof(PageDown))]
        PageDown,
        [Description(nameof(F1))]
        F1,
        [Description(nameof(F2))]
        F2,
        [Description(nameof(F3))]
        F3,
        [Description(nameof(F4))]
        F4,
        [Description(nameof(F5))]
        F5,
        [Description(nameof(F6))]
        F6,
        [Description(nameof(F7))]
        F7,
        [Description(nameof(F8))]
        F8,
        [Description(nameof(F9))]
        F9,
        [Description(nameof(F10))]
        F10,
        [Description(nameof(F11))]
        F11,
        [Description(nameof(F12))]
        F12,
        [Description(nameof(F13))]
        F13,
        [Description(nameof(F14))]
        F14,
        [Description(nameof(F15))]
        F15,
        [Description(nameof(Alpha0))]
        Alpha0 = 48,
        [Description(nameof(Alpha1))]
        Alpha1,
        [Description(nameof(Alpha2))]
        Alpha2,
        [Description(nameof(Alpha3))]
        Alpha3,
        [Description(nameof(Alpha4))]
        Alpha4,
        [Description(nameof(Alpha5))]
        Alpha5,
        [Description(nameof(Alpha6))]
        Alpha6,
        [Description(nameof(Alpha7))]
        Alpha7,
        [Description(nameof(Alpha8))]
        Alpha8,
        [Description(nameof(Alpha9))]
        Alpha9,
        [Description(nameof(Exclaim))]
        Exclaim = 33,
        [Description(nameof(DoubleQuote))]
        DoubleQuote,
        [Description(nameof(Hash))]
        Hash,
        [Description(nameof(Dollar))]
        Dollar,
        [Description(nameof(Percent))]
        Percent,
        [Description(nameof(Ampersand))]
        Ampersand,
        [Description(nameof(Quote))]
        Quote,
        [Description(nameof(LeftParen))]
        LeftParen,
        [Description(nameof(RightParen))]
        RightParen,
        [Description(nameof(Asterisk))]
        Asterisk,
        [Description(nameof(Plus))]
        Plus,
        [Description(nameof(Comma))]
        Comma,
        [Description(nameof(Minus))]
        Minus,
        [Description(nameof(Period))]
        Period,
        [Description(nameof(Slash))]
        Slash,
        [Description(nameof(Colon))]
        Colon = 58,
        [Description(nameof(Semicolon))]
        Semicolon,
        [Description(nameof(Less))]
        Less,
        [Description(nameof(Equals))]
        Equals,
        [Description(nameof(Greater))]
        Greater,
        [Description(nameof(Question))]
        Question,
        [Description(nameof(At))]
        At,
        [Description(nameof(LeftBracket))]
        LeftBracket = 91,
        [Description(nameof(Backslash))]
        Backslash,
        [Description(nameof(RightBracket))]
        RightBracket,
        [Description(nameof(Caret))]
        Caret,
        [Description(nameof(Underscore))]
        Underscore,
        [Description(nameof(BackQuote))]
        BackQuote,
        [Description(nameof(A))]
        A,
        [Description(nameof(B))]
        B,
        [Description(nameof(C))]
        C,
        [Description(nameof(D))]
        D,
        [Description(nameof(E))]
        E,
        [Description(nameof(F))]
        F,
        [Description(nameof(G))]
        G,
        [Description(nameof(H))]
        H,
        [Description(nameof(I))]
        I,
        [Description(nameof(J))]
        J,
        [Description(nameof(K))]
        K,
        [Description(nameof(L))]
        L,
        [Description(nameof(M))]
        M,
        [Description(nameof(N))]
        N,
        [Description(nameof(O))]
        O,
        [Description(nameof(P))]
        P,
        [Description(nameof(Q))]
        Q,
        [Description(nameof(R))]
        R,
        [Description(nameof(S))]
        S,
        [Description(nameof(T))]
        T,
        [Description(nameof(U))]
        U,
        [Description(nameof(V))]
        V,
        [Description(nameof(W))]
        W,
        [Description(nameof(X))]
        X,
        [Description(nameof(Y))]
        Y,
        [Description(nameof(Z))]
        Z,
        [Description(nameof(LeftCurlyBracket))]
        LeftCurlyBracket,
        [Description(nameof(Pipe))]
        Pipe,
        [Description(nameof(RightCurlyBracket))]
        RightCurlyBracket,
        [Description(nameof(Tilde))]
        Tilde,
        [Description(nameof(Numlock))]
        Numlock = 300,
        [Description(nameof(CapsLock))]
        CapsLock,
        [Description(nameof(ScrollLock))]
        ScrollLock,
        [Description(nameof(RightShift))]
        RightShift,
        [Description(nameof(LeftShift))]
        LeftShift,
        [Description(nameof(RightControl))]
        RightControl,
        [Description(nameof(LeftControl))]
        LeftControl = 306,
        [Description(nameof(RightAlt))]
        RightAlt,
        [Description(nameof(LeftAlt))]
        LeftAlt,
        [Description(nameof(LeftCommand))]
        LeftCommand = 310,
        [Description(nameof(LeftApple))]
        LeftApple = 310,
        [Description(nameof(LeftWindows))]
        LeftWindows,
        [Description(nameof(RightCommand))]
        RightCommand = 309,
        [Description(nameof(RightApple))]
        RightApple = 309,
        [Description(nameof(RightWindows))]
        RightWindows = 312,
        [Description(nameof(AltGr))]
        AltGr,
        [Description(nameof(Help))]
        Help = 315,
        [Description(nameof(Print))]
        Print,
        [Description(nameof(SysReq))]
        SysReq,
        [Description(nameof(Break))]
        Break,
        [Description(nameof(Menu))]
        Menu,
        [Description(nameof(Mouse0))]
        Mouse0 = 323,
        [Description(nameof(Mouse1))]
        Mouse1,
        [Description(nameof(Mouse2))]
        Mouse2,
        [Description(nameof(Mouse3))]
        Mouse3,
        [Description(nameof(Mouse4))]
        Mouse4,
        [Description(nameof(Mouse5))]
        Mouse5,
        [Description(nameof(Mouse6))]
        Mouse6
    }
}