using System;
using System.Runtime.InteropServices;

namespace Core.WinAPI
{
    public static class DisplayAPI
    {
        public class DisplayAPIException : Exception
        {
            public DisplayAPIException(string method, StatusCode code)
                : base($"{method} failed with status code: {Enum.GetName(typeof(StatusCode), code)}")
            { }
        }

        public enum StatusCode : uint
        {
            SUCCESS = 0,
            ERROR_ACCESS_DENIED = 5,
            ERROR_GEN_FAILURE = 31,
            ERROR_NOT_SUPPORTED = 50,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_BAD_CONFIGURATION = 1610,
        }

        public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY
        {
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = -1,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_WIRED = 16,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_VIRTUAL = 17,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_USB_TUNNEL = 18,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = -2147483648,
        }

        [Flags]
        public enum SET_DISPLAY_CONFIG_FLAGS : uint
        {
            SDC_USE_DATABASE_CURRENT = 0x0000000F,
            SDC_TOPOLOGY_INTERNAL = 0x00000001,
            SDC_TOPOLOGY_CLONE = 0x00000002,
            SDC_TOPOLOGY_EXTEND = 0x00000004,
            SDC_TOPOLOGY_EXTERNAL = 0x00000008,
            SDC_TOPOLOGY_SUPPLIED = 0x00000010,
            SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020,
            SDC_VALIDATE = 0x00000040,
            SDC_APPLY = 0x00000080,
            SDC_NO_OPTIMIZATION = 0x00000100,
            SDC_SAVE_TO_DATABASE = 0x00000200,
            SDC_ALLOW_CHANGES = 0x00000400,
            SDC_PATH_PERSIST_IF_REQUIRED = 0x00000800,
            SDC_FORCE_MODE_ENUMERATION = 0x00001000,
            SDC_ALLOW_PATH_ORDER_CHANGES = 0x00002000,
            SDC_VIRTUAL_MODE_AWARE = 0x00008000,
            SDC_VIRTUAL_REFRESH_RATE_AWARE = 0x00020000,
        }

        public enum DISPLAYCONFIG_ROTATION
        {
            DISPLAYCONFIG_ROTATION_IDENTITY = 1,
            DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
            DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
            DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
        }

        public enum DISPLAYCONFIG_SCALING
        {
            DISPLAYCONFIG_SCALING_IDENTITY = 1,
            DISPLAYCONFIG_SCALING_CENTERED = 2,
            DISPLAYCONFIG_SCALING_STRETCHED = 3,
            DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
            DISPLAYCONFIG_SCALING_CUSTOM = 5,
            DISPLAYCONFIG_SCALING_PREFERRED = 128,
        }

        public enum DISPLAYCONFIG_SCANLINE_ORDERING
        {
            DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
            DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = 2,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
        }

        public enum DISPLAYCONFIG_MODE_INFO_TYPE
        {
            DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
            DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
            DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE = 3,
        }

        public enum DISPLAYCONFIG_PIXELFORMAT
        {
            DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
            DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
            DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
            DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
            DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
        }
        public enum DISPLAYCONFIG_TOPOLOGY_ID
        {
            DISPLAYCONFIG_TOPOLOGY_UNKNOWN = 0,
            DISPLAYCONFIG_TOPOLOGY_INTERNAL = 1,
            DISPLAYCONFIG_TOPOLOGY_CLONE = 2,
            DISPLAYCONFIG_TOPOLOGY_EXTEND = 4,
            DISPLAYCONFIG_TOPOLOGY_EXTERNAL = 8,
        }

        [Flags]
        public enum QUERY_DISPLAY_CONFIG_FLAGS : uint
        {
            QDC_ALL_PATHS = 0x00000001,
            QDC_ONLY_ACTIVE_PATHS = 0x00000002,
            QDC_DATABASE_CURRENT = 0x00000004,
            QDC_VIRTUAL_MODE_AWARE = 0x00000010,
            QDC_INCLUDE_HMD = 0x00000020,
            QDC_VIRTUAL_REFRESH_RATE_AWARE = 0x00000040,
        }

        [Flags]
        public enum DISPLAYCONFIG_PATH_FLAGS : uint
        {
            DISPLAYCONFIG_PATH_ACTIVE = 0x00000001,
            DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE = 0x00000008,
            DISPLAYCONFIG_PATH_BOOST_REFRESH_RATE = 0x00000010,
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
        {
            [FieldOffset(0)]
            private readonly uint _raw;

            [FieldOffset(0)]
            public uint value;


            public byte FriendlyNameFromEdid => (byte)(_raw & (1 << 0));

            public byte FriendlyNameForced => (byte)(_raw & (1 << 1));

            public byte EdidIdsValid => (byte)(_raw & (1 << 2));
        }

        public enum DISPLAYCONFIG_DEVICE_INFO_TYPE
        {
            DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
            DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
            DISPLAYCONFIG_DEVICE_INFO_GET_SUPPORT_VIRTUAL_RESOLUTION = 7,
            DISPLAYCONFIG_DEVICE_INFO_SET_SUPPORT_VIRTUAL_RESOLUTION = 8,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9,
            DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10,
            DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL = 11,
            DISPLAYCONFIG_DEVICE_INFO_GET_MONITOR_SPECIALIZATION = 12,
            DISPLAYCONFIG_DEVICE_INFO_SET_MONITOR_SPECIALIZATION = 13,
        }

        public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
            public int size;
            public LUID adapterId;
            public uint id;
        }

        public interface IDisplayDeviceInfo
        {
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_SOURCE_DEVICE_NAME : IDisplayDeviceInfo
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string adapterDevicePath;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME : IDisplayDeviceInfo
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public ushort edidManufactureId;
            public ushort edidProductCodeId;
            public uint connectorInstance;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string monitorFriendlyDeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string monitorDevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_TARGET_PREFERRED_MODE : IDisplayDeviceInfo
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public uint width;
            public uint height;
            public DISPLAYCONFIG_TARGET_MODE targetMode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_ADAPTER_NAME : IDisplayDeviceInfo
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string adapterDevicePath;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DisplayConfigSetTargetPersistence : IDisplayDeviceInfo
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            private uint _raw;

            public bool BootPersistenceOn
            {
                get { return (_raw & (1 << 0)) != 0; }
                set
                {
                    if (value) _raw |= 1; // set first bit as 1
                    else _raw &= ~(uint)1; // set first bit as 0
                }
            }
        }

        public struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        public struct DISPLAYCONFIG_TARGET_MODE
        {
            public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        public struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        public struct POINTL
        {
            public int x;
            public int y;
        }

        public struct RECTL
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public struct DISPLAYCONFIG_SOURCE_MODE
        {
            public uint width;
            public uint height;
            public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
            public POINTL position;
        }

        public struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO
        {
            public POINTL PathSourceSize;
            public RECTL DesktopImageRegion;
            public RECTL DesktopImageClip;
        }

        public struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;

            public uint flags;
        }

        public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;
            public DISPLAYCONFIG_RATIONAL hSyncFreq;
            public DISPLAYCONFIG_RATIONAL vSyncFreq;
            public DISPLAYCONFIG_2DREGION activeSize;
            public DISPLAYCONFIG_2DREGION totalSize;
            public SignalInfoUnion signalInfo;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;

            [StructLayout(LayoutKind.Explicit)]
            public struct SignalInfoUnion
            {
                [FieldOffset(0)]
                public AdditionalSignalInfoUnion additionalSignalInfo;

                [FieldOffset(0)]
                public uint videoStandard;

                public struct AdditionalSignalInfoUnion
                {
                    public uint _bitfield;
                }
            }
        }

        public struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public IDXUnion idx;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public DISPLAYCONFIG_ROTATION rotation;
            public DISPLAYCONFIG_SCALING scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
            public int targetAvailable;
            public uint statusFlags;

            [StructLayout(LayoutKind.Explicit)]
            public struct IDXUnion
            {
                [FieldOffset(0)]
                public uint modeInfoIdx;

                [FieldOffset(0)]
                public IDXSubUnion inner;

                public struct IDXSubUnion
                {
                    public uint bitfield;
                }
            }
        }

        public struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public IDXUnion idx;
            public uint statusFlags;

            [StructLayout(LayoutKind.Explicit)]
            public struct IDXUnion
            {
                [FieldOffset(0)]
                public uint modeInfoIdx;

                [FieldOffset(0)]
                public IDXSubUnion inner;

                public struct IDXSubUnion
                {
                    public uint bitfield;
                }
            }
        }

        public struct DISPLAYCONFIG_MODE_INFO
        {
            public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
            public uint id;
            public LUID adapterId;
            public AnonymousUnion union;

            [StructLayout(LayoutKind.Explicit)]
            public struct AnonymousUnion
            {
                [FieldOffset(0)]
                public DISPLAYCONFIG_TARGET_MODE targetMode;

                [FieldOffset(0)]
                public DISPLAYCONFIG_SOURCE_MODE sourceMode;

                [FieldOffset(0)]
                public DISPLAYCONFIG_DESKTOP_IMAGE_INFO desktopImageInfo;
            }
        }

        public const uint DISPLAYCONFIG_PATH_MODE_IDX_INVALID = 0xffffffff;

        public static class PInvoke
        {
            [DllImport("USER32.dll", ExactSpelling = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            public static extern StatusCode GetDisplayConfigBufferSizes(
                QUERY_DISPLAY_CONFIG_FLAGS flags,
                ref uint numPathArrayElements,
                ref uint numModeInfoArrayElements
            );

            [DllImport("USER32.dll", ExactSpelling = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            public static extern unsafe StatusCode QueryDisplayConfig(
                QUERY_DISPLAY_CONFIG_FLAGS flags,
                ref uint numPathArrayElements,
                DISPLAYCONFIG_PATH_INFO* pathArray,
                ref uint numModeInfoArrayElements,
                DISPLAYCONFIG_MODE_INFO* modeInfoArray,
                IntPtr currentTopologyId
            );

            [DllImport("USER32.dll", ExactSpelling = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            public static extern unsafe StatusCode QueryDisplayConfig(
                QUERY_DISPLAY_CONFIG_FLAGS flags,
                ref uint numPathArrayElements,
                DISPLAYCONFIG_PATH_INFO* pathArray,
                ref uint numModeInfoArrayElements,
                DISPLAYCONFIG_MODE_INFO* modeInfoArray,
                out DISPLAYCONFIG_TOPOLOGY_ID currentTopologyId
            );

            [DllImport("USER32.dll", ExactSpelling = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            public static extern unsafe StatusCode SetDisplayConfig(
                uint numPathArrayElements,
                DISPLAYCONFIG_PATH_INFO* pathArray,
                uint numModeInfoArrayElements,
                DISPLAYCONFIG_MODE_INFO* modeInfoArray,
                SET_DISPLAY_CONFIG_FLAGS flags
            );

            [DllImport("USER32.dll", ExactSpelling = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            public static extern StatusCode DisplayConfigGetDeviceInfo(IntPtr requestPacket);

            [DllImport("USER32.dll", ExactSpelling = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            public static extern StatusCode DisplayConfigSetDeviceInfo(IntPtr requestPacket);
        }

        public static void GetDisplayConfigBufferSizes(
            QUERY_DISPLAY_CONFIG_FLAGS flags,
            out uint numPathArrayElements,
            out uint numModeInfoArrayElements
        )
        {
            numPathArrayElements = 0; numModeInfoArrayElements = 0;

            var result = PInvoke.GetDisplayConfigBufferSizes(flags, ref numPathArrayElements, ref numModeInfoArrayElements);

            if (result != StatusCode.SUCCESS)
            {
                throw new DisplayAPIException(nameof(PInvoke.GetDisplayConfigBufferSizes), result);
            }
        }

        public static void QueryDisplayConfig(
            QUERY_DISPLAY_CONFIG_FLAGS flags,
            out uint numPathArrayElements,
            out DISPLAYCONFIG_PATH_INFO[] pathArray,
            out uint numModeInfoArrayElements,
            out DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId
        )
        {
            pathArray = null; modeInfoArray = null;

            GetDisplayConfigBufferSizes(flags, out numPathArrayElements, out numModeInfoArrayElements);

            pathArray = new DISPLAYCONFIG_PATH_INFO[numPathArrayElements];
            modeInfoArray = new DISPLAYCONFIG_MODE_INFO[numModeInfoArrayElements];

            unsafe
            {
                fixed (DISPLAYCONFIG_PATH_INFO* pPathArray = pathArray)
                {
                    fixed (DISPLAYCONFIG_MODE_INFO* pModeArray = modeInfoArray)
                    {
                        var result = PInvoke.QueryDisplayConfig(flags, ref numPathArrayElements, pPathArray, ref numModeInfoArrayElements, pModeArray, currentTopologyId);

                        if (result != StatusCode.SUCCESS)
                        {
                            throw new DisplayAPIException(nameof(PInvoke.QueryDisplayConfig), result);
                        }
                    }
                }
            }
        }

        public static void SetDisplayConfig(
            uint numPathArrayElements,
            ref DISPLAYCONFIG_PATH_INFO[] pathArray,
            uint numModeInfoArrayElements,
            ref DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            SET_DISPLAY_CONFIG_FLAGS flags
        )
        {
            unsafe
            {
                fixed (DISPLAYCONFIG_PATH_INFO* pPathArray = pathArray)
                {
                    fixed (DISPLAYCONFIG_MODE_INFO* pModeArray = modeInfoArray)
                    {
                        var result = PInvoke.SetDisplayConfig(numPathArrayElements, pPathArray, numModeInfoArrayElements, pModeArray, flags);

                        if (result != StatusCode.SUCCESS)
                        {
                            throw new DisplayAPIException(nameof(PInvoke.SetDisplayConfig), result);
                        }
                    }
                }
            }
        }

        public static void DisplayConfigGetDeviceInfo<T>(ref T deviceInfo) where T : IDisplayDeviceInfo
        {
            var result = MarshalStructureAndCall(ref deviceInfo, PInvoke.DisplayConfigGetDeviceInfo);

            if (result != StatusCode.SUCCESS)
            {
                throw new DisplayAPIException(nameof(PInvoke.DisplayConfigGetDeviceInfo), result);
            }
        }

        public static void DisplayConfigSetDeviceInfo<T>(ref T deviceInfo) where T : IDisplayDeviceInfo
        {
            var result = MarshalStructureAndCall(ref deviceInfo, PInvoke.DisplayConfigSetDeviceInfo);

            if (result != StatusCode.SUCCESS)
            {
                throw new DisplayAPIException(nameof(PInvoke.DisplayConfigSetDeviceInfo), result);
            }
        }

        private static StatusCode MarshalStructureAndCall<T>(
            ref T deviceInfo,
            Func<IntPtr, StatusCode> func
        ) where T : IDisplayDeviceInfo
        {
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(deviceInfo));

            try
            {
                Marshal.StructureToPtr(deviceInfo, ptr, false);

                var result = func(ptr);

                deviceInfo = (T)Marshal.PtrToStructure(ptr, deviceInfo.GetType());

                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
