/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Core.WinAPI
{
    public static class AudioAPI
    {
        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public KeyEventF dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mouse;
            [FieldOffset(0)] public KeyboardInput keyboard;
            [FieldOffset(0)] public HardwareInput hardware;
        }

        [Flags]
        public enum InputType : int
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF : uint
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }

        public struct Input
        {
            public InputType type;
            public InputUnion union;
        }

        public static class PKEY
        {
            public static readonly Guid PKEY_AudioEndpoint_FormFactor = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEndpoint_ControlPanelPageProvider = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEndpoint_Association = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEndpoint_PhysicalSpeakers = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEndpoint_GUID = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEndpoint_Disable_SysFx = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEndpoint_FullRangeSpeakers = new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e);
            public static readonly Guid PKEY_AudioEngine_DeviceFormat = new Guid(0xf19f064d, 0x82c, 0x4e27, 0xbc, 0x73, 0x68, 0x82, 0xa1, 0xbb, 0x8e, 0x4c);
            public static readonly PropertyKey PKEY_DeviceInterface_FriendlyName = new PropertyKey { fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), pid = 14 };
        }

        [Flags]
        public enum EDeviceState : uint
        {
            DEVICE_STATE_ACTIVE = 0x00000001,
            DEVICE_STATE_UNPLUGGED = 0x00000002,
            DEVICE_STATE_NOTPRESENT = 0x00000004,
            DEVICE_STATEMASK_ALL = 0x00000007
        }

        [Flags]
        internal enum CLSCTX : uint
        {
            INPROC_SERVER = 0x1,
            INPROC_HANDLER = 0x2,
            LOCAL_SERVER = 0x4,
            INPROC_SERVER16 = 0x8,
            REMOTE_SERVER = 0x10,
            INPROC_HANDLER16 = 0x20,
            RESERVED1 = 0x40,
            RESERVED2 = 0x80,
            RESERVED3 = 0x100,
            RESERVED4 = 0x200,
            NO_CODE_DOWNLOAD = 0x400,
            RESERVED5 = 0x800,
            NO_CUSTOM_MARSHAL = 0x1000,
            ENABLE_CODE_DOWNLOAD = 0x2000,
            NO_FAILURE_LOG = 0x4000,
            DISABLE_AAA = 0x8000,
            ENABLE_AAA = 0x10000,
            FROM_DEFAULT_CONTEXT = 0x20000,
            INPROC = INPROC_SERVER | INPROC_HANDLER,
            SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
            ALL = SERVER | INPROC_HANDLER
        }

        public enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2,
            EDataFlow_enum_count = 3
        }

        public enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2,
            ERole_enum_count = 3
        }

        internal enum EStgmAccess
        {
            STGM_READ = 0x00000000,
            STGM_WRITE = 0x00000001,
            STGM_READWRITE = 0x00000002
        }

        public struct PropertyKey
        {
            public Guid fmtid;
            public int pid;
        };

        public struct Blob
        {
            public int Length;
            public IntPtr Data;

            // Dummy method for avoiding CS0649 warning.
            public void Clear()
            {
                Length = 0;
                Data = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PropVariant
        {
            [FieldOffset(0)] public short vt;
            [FieldOffset(2)] public short wReserved1;
            [FieldOffset(4)] public short wReserved2;
            [FieldOffset(6)] public short wReserved3;
            [FieldOffset(8)] public sbyte cVal;
            [FieldOffset(8)] public byte bVal;
            [FieldOffset(8)] public short iVal;
            [FieldOffset(8)] public ushort uiVal;
            [FieldOffset(8)] public int lVal;
            [FieldOffset(8)] public uint ulVal;
            [FieldOffset(8)] public long hVal;
            [FieldOffset(8)] public ulong uhVal;
            [FieldOffset(8)] public float fltVal;
            [FieldOffset(8)] public double dblVal;
            [FieldOffset(8)] public Blob blobVal;
            [FieldOffset(8)] public DateTime date;
            [FieldOffset(8)] public bool boolVal;
            [FieldOffset(8)] public int scode;
            [FieldOffset(8)] public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
            [FieldOffset(8)] public IntPtr everything_else;

            internal byte[] GetBlob()
            {
                byte[] Result = new byte[blobVal.Length];
                for (int i = 0; i < blobVal.Length; i++)
                {
                    Result[i] = Marshal.ReadByte((IntPtr)((long)(blobVal.Data) + i));
                }
                return Result;
            }

            public object Value
            {
                get
                {
                    VarEnum ve = (VarEnum)vt;
                    switch (ve)
                    {
                        case VarEnum.VT_I1:
                            return bVal;
                        case VarEnum.VT_I2:
                            return iVal;
                        case VarEnum.VT_I4:
                            return lVal;
                        case VarEnum.VT_I8:
                            return hVal;
                        case VarEnum.VT_INT:
                            return iVal;
                        case VarEnum.VT_UI4:
                            return ulVal;
                        case VarEnum.VT_LPWSTR:
                            return Marshal.PtrToStringUni(everything_else);
                        case VarEnum.VT_BLOB:
                            return GetBlob();
                    }
                    return "FIXME Type = " + ve.ToString();
                }
            }
        }

        [ComImport]
        [Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
        internal class COM_PolicyConfigClient
        {
        }

        [Guid("00000000-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPolicyConfig10
        {
            [PreserveSig]
            int GetMixFormat(string pszDeviceName, IntPtr ppFormat);

            [PreserveSig]
            int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);

            [PreserveSig]
            int ResetDeviceFormat(string pszDeviceName);

            [PreserveSig]
            int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);

            [PreserveSig]
            int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

            [PreserveSig]
            int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);

            [PreserveSig]
            int GetShareMode(string pszDeviceName, IntPtr pMode);

            [PreserveSig]
            int SetShareMode(string pszDeviceName, IntPtr mode);

            [PreserveSig]
            int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

            [PreserveSig]
            int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

            [PreserveSig]
            int SetDefaultEndpoint(string pszDeviceName, ERole role);

            [PreserveSig]
            int SetEndpointVisibility(string pszDeviceName, bool bVisible);
        }

        [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPolicyConfig
        {
            [PreserveSig]
            int GetMixFormat(string pszDeviceName, IntPtr ppFormat);

            [PreserveSig]
            int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);

            [PreserveSig]
            int ResetDeviceFormat(string pszDeviceName);

            [PreserveSig]
            int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);

            [PreserveSig]
            int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

            [PreserveSig]
            int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);

            [PreserveSig]
            int GetShareMode(string pszDeviceName, IntPtr pMode);

            [PreserveSig]
            int SetShareMode(string pszDeviceName, IntPtr mode);

            [PreserveSig]
            int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

            [PreserveSig]
            int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);

            [PreserveSig]
            int SetDefaultEndpoint(string pszDeviceName, ERole role);

            [PreserveSig]
            int SetEndpointVisibility(string pszDeviceName, bool bVisible);
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            [PreserveSig]
            int EnumAudioEndpoints(EDataFlow dataFlow, EDeviceState StateMask, out IMMDeviceCollection device);
            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
            [PreserveSig]
            int GetDevice(string pwstrId, out IMMDevice ppDevice);
            [PreserveSig]
            int RegisterEndpointNotificationCallback(IntPtr pClient);
            [PreserveSig]
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class COM_MMDeviceEnumerator
        {
        }

        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceCollection
        {
            [PreserveSig]
            int GetCount(out uint pcDevices);
            [PreserveSig]
            int Item(uint nDevice, out IMMDevice Device);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            [PreserveSig]
            int OpenPropertyStore(EStgmAccess stgmAccess, out IPropertyStore propertyStore);
            [PreserveSig]
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            [PreserveSig]
            int GetState(out EDeviceState pdwState);
        }

        [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPropertyStore
        {
            [PreserveSig]
            int GetCount(out Int32 count);
            [PreserveSig]
            int GetAt(int iProp, out PropertyKey pkey);
            [PreserveSig]
            int GetValue(ref PropertyKey key, out PropVariant pv);
            [PreserveSig]
            int SetValue(ref PropertyKey key, ref PropVariant propvar);
            [PreserveSig]
            int Commit();
        };

        [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMEndpoint
        {
            [PreserveSig]
            int GetDataFlow(out EDataFlow pDataFlow);
        };

        public class PolicyConfigClient
        {
            private readonly IPolicyConfig _PolicyConfig;
            private readonly IPolicyConfig10 _PolicyConfig10;

            public PolicyConfigClient()
            {
                _PolicyConfig = new COM_PolicyConfigClient() as IPolicyConfig;
                if (_PolicyConfig != null)
                    return;

                _PolicyConfig10 = new COM_PolicyConfigClient() as IPolicyConfig10;
            }

            public void SetDefaultEndpoint(string devID, ERole eRole)
            {
                if (_PolicyConfig != null)
                {
                    Marshal.ThrowExceptionForHR(_PolicyConfig.SetDefaultEndpoint(devID, eRole));
                    return;
                }
                if (_PolicyConfig10 != null)
                {
                    Marshal.ThrowExceptionForHR(_PolicyConfig10.SetDefaultEndpoint(devID, eRole));
                }
            }
        }

        public class PropertyStoreProperty
        {
            private PropertyKey _PropertyKey;
            private PropVariant _PropValue;

            internal PropertyStoreProperty(PropertyKey key, PropVariant value)
            {
                _PropertyKey = key;
                _PropValue = value;
            }

            public PropertyKey Key
            {
                get
                {
                    return _PropertyKey;
                }
            }

            public object Value
            {
                get
                {
                    return _PropValue.Value;
                }
            }
        }

        public class PropertyStore
        {
            private readonly IPropertyStore _Store;

            public int Count
            {
                get
                {
                    Marshal.ThrowExceptionForHR(_Store.GetCount(out int Result));
                    return Result;
                }
            }

            public PropertyStoreProperty this[int index]
            {
                get
                {
                    var key = Get(index);
                    Marshal.ThrowExceptionForHR(_Store.GetValue(ref key, out PropVariant result));
                    return new PropertyStoreProperty(key, result);
                }
            }

            public bool Contains(Guid guid)
            {
                for (int i = 0; i < Count; i++)
                {
                    PropertyKey key = Get(i);
                    if (key.fmtid == guid)
                        return true;
                }
                return false;
            }

            public PropertyStoreProperty this[Guid guid]
            {
                get
                {
                    for (int i = 0; i < Count; i++)
                    {
                        PropertyKey key = Get(i);
                        if (key.fmtid == guid)
                        {
                            Marshal.ThrowExceptionForHR(_Store.GetValue(ref key, out PropVariant result));
                            return new PropertyStoreProperty(key, result);
                        }
                    }
                    return null;
                }
            }

            public PropertyKey Get(int index)
            {
                Marshal.ThrowExceptionForHR(_Store.GetAt(index, out PropertyKey key));
                return key;
            }

            public PropVariant GetValue(int index)
            {
                PropertyKey key = Get(index);
                Marshal.ThrowExceptionForHR(_Store.GetValue(ref key, out PropVariant result));
                return result;
            }

            public bool Contains(PropertyKey compareKey)
            {
                for (int i = 0; i < Count; i++)
                {
                    PropertyKey key = Get(i);
                    if (key.fmtid == compareKey.fmtid && key.pid == compareKey.pid)
                        return true;
                }
                return false;
            }

            public PropertyStoreProperty this[PropertyKey queryKey]
            {
                get
                {
                    for (int i = 0; i < Count; i++)
                    {
                        PropertyKey key = Get(i);
                        if (key.fmtid == queryKey.fmtid && key.pid == queryKey.pid)
                        {
                            Marshal.ThrowExceptionForHR(_Store.GetValue(ref key, out PropVariant result));
                            return new PropertyStoreProperty(key, result);
                        }
                    }
                    return null;
                }
            }

            internal PropertyStore(IPropertyStore store)
            {
                _Store = store;
            }
        }

        public class MMDevice
        {
            private readonly IMMDevice _RealDevice;

            private PropertyStore _PropertyStore;

            private void GetPropertyInformation()
            {
                Marshal.ThrowExceptionForHR(_RealDevice.OpenPropertyStore(EStgmAccess.STGM_READ, out IPropertyStore propstore));
                _PropertyStore = new PropertyStore(propstore);
            }

            public PropertyStore Properties
            {
                get
                {
                    if (_PropertyStore == null)
                        GetPropertyInformation();
                    return _PropertyStore;
                }
            }

            private EDataFlow? _DataFlow;

            public EDataFlow DataFlow
            {
                get
                {
                    if (_DataFlow == null)
                    {
                        IMMEndpoint ep = _RealDevice as IMMEndpoint;
                        ep.GetDataFlow(out var result);
                        _DataFlow = result;
                    }

                    return (EDataFlow)_DataFlow;
                }
            }

            private string _FriendlyName;

            public string FriendlyName
            {
                get
                {
                    try
                    {
                        if (_FriendlyName != null)
                        {
                            return _FriendlyName;
                        }

                        if (_PropertyStore == null)
                        {
                            GetPropertyInformation();
                        }

                        if (_PropertyStore.Contains(PKEY.PKEY_DeviceInterface_FriendlyName))
                        {
                            _FriendlyName = (string)_PropertyStore[PKEY.PKEY_DeviceInterface_FriendlyName].Value;

                            return _FriendlyName;
                        }

                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public string ID
            {
                get
                {
                    Marshal.ThrowExceptionForHR(_RealDevice.GetId(out string Result));
                    return Result;
                }
            }

            public EDeviceState State
            {
                get
                {
                    Marshal.ThrowExceptionForHR(_RealDevice.GetState(out EDeviceState Result));
                    return Result;

                }
            }

            internal MMDevice(IMMDevice realDevice)
            {
                _RealDevice = realDevice;
            }

        }

        public class MMDeviceEnumerator
        {
            private readonly IMMDeviceEnumerator _realEnumerator = new COM_MMDeviceEnumerator() as IMMDeviceEnumerator;

            public MMDeviceCollection EnumerateAudioEndPoints(EDataFlow dataFlow, EDeviceState dwStateMask)
            {
                Marshal.ThrowExceptionForHR(_realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out IMMDeviceCollection result));
                return new MMDeviceCollection(result);
            }

            public MMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role)
            {
                Marshal.ThrowExceptionForHR(_realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out IMMDevice _Device));
                return new MMDevice(_Device);
            }

            public MMDevice GetDevice(string ID)
            {
                Marshal.ThrowExceptionForHR(_realEnumerator.GetDevice(ID, out IMMDevice _Device));
                return new MMDevice(_Device);
            }

            public MMDeviceEnumerator()
            {
                if (System.Environment.OSVersion.Version.Major < 6)
                {
                    throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
                }
            }
        }

        public class MMDeviceCollection : IEnumerable<MMDevice>
        {
            private readonly List<MMDevice> list;
            private readonly IMMDeviceCollection collection;

            public int Count => list.Count;

            public MMDevice this[int index] => list[index];

            internal MMDeviceCollection(IMMDeviceCollection parent)
            {
                collection = parent;

                var count = GetCollectionSize();

                list = new List<MMDevice>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(GetCollectionItem(i));
                }
            }

            private int GetCollectionSize()
            {
                Marshal.ThrowExceptionForHR(collection.GetCount(out uint result));

                return (int)result;
            }

            private MMDevice GetCollectionItem(int index)
            {
                collection.Item((uint)index, out IMMDevice result);

                return new MMDevice(result);
            }

            public IEnumerator<MMDevice> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }
        }
    }
}
