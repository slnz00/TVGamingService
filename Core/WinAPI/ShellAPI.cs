using System.Runtime.InteropServices;
using System;

namespace Core.WinAPI
{
    public static class ShellAPI
    {
        [ComImport]
        [Guid("00020400-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface IDispatch
        {
        }

        [ComImport]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }

        public enum ShellWindowTypeConstants
        {
            SWC_EXPLORER = 0,
            SWC_BROWSER = 1,
            SWC_3RDPARTY = 2,
            SWC_CALLBACK = 4,
            SWC_DESKTOP = 8,
        }

        public enum ShellWindowFindWindowOptions
        {
            SWFO_NEEDDISPATCH = 1,
            SWFO_INCLUDEPENDING = 2,
            SWFO_COOKIEPASSED = 4,
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
        [Guid("85CB6900-4D95-11CF-960C-0080C7F4EE85")]
        public interface IShellWindows
        {
            int Count
            {
                get;
            }

            [return: MarshalAs(UnmanagedType.IDispatch)]
            object Item([MarshalAs(UnmanagedType.Struct)] object index);

            [return: MarshalAs(UnmanagedType.IUnknown)]
            object _NewEnum();

            void Register([MarshalAs(UnmanagedType.IDispatch)] object pid, int hwnd, ShellWindowTypeConstants swClass, out int plCookie);

            void RegisterPending(int lThreadId, [MarshalAs(UnmanagedType.Struct)] in object pvarloc, [MarshalAs(UnmanagedType.Struct)] in object pvarlocRoot, ShellWindowTypeConstants swClass, out int plCookie);

            void Revoke(int lCookie);

            void OnNavigate(int lCookie, [MarshalAs(UnmanagedType.Struct)] in object pvarLoc);

            void OnActivated(int lCookie, object fActive);

            [return: MarshalAs(UnmanagedType.IDispatch)]
            object FindWindowSW([MarshalAs(UnmanagedType.Struct)] in object pvarLoc, [MarshalAs(UnmanagedType.Struct)] in object pvarLocRoot, ShellWindowTypeConstants swClass, out int phwnd, ShellWindowFindWindowOptions swfwOptions);

            void OnCreated(int lCookie, [MarshalAs(UnmanagedType.IUnknown)] object punk);

            void ProcessAttachDetach(object fAttach);
        }

        [ComImport]
        [Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39")]
        public partial class ShellWindows
        {
        }

        [ComImport]
        [Guid("000214E3-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellView
        {
            void vtableGap_GetWindow();
            void vtableGap_ContextSensitiveHelp();
            void vtableGap_TranslateAcceleratorA();
            void vtableGap_EnableModeless();
            void vtableGap_UIActivate();
            void vtableGap_Refresh();
            void vtableGap_CreateViewWindow();
            void vtableGap_DestroyViewWindow();
            void vtableGap_GetCurrentInfo();
            void vtableGap_AddPropertySheetPages();
            void vtableGap_SaveViewState();
            void vtableGap_SelectItem();

            [return: MarshalAs(UnmanagedType.Interface)]
            object GetItemObject(uint aspectOfView, ref Guid riid);
        }

        [ComImport]
        [Guid("000214E2-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellBrowser
        {
            void vtableGap_GetWindow();
            void vtableGap_ContextSensitiveHelp();
            void vtableGap_InsertMenusSB();
            void vtableGap_SetMenuSB();
            void vtableGap_RemoveMenusSB();
            void vtableGap_SetStatusTextSB();
            void vtableGap_EnableModelessSB();
            void vtableGap_TranslateAcceleratorSB();
            void vtableGap_BrowseObject();
            void vtableGap_GetViewStateStream();
            void vtableGap_GetControlWindow();
            void vtableGap_SendControlMsg();

            IShellView QueryActiveShellView();
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("1AF3A467-214F-4298-908E-06B03E0B39F9")]
        public interface IFolderView2
        {
            void vtableGap_GetCurrentViewMode();
            void vtableGap_SetCurrentViewMode();
            void vtableGap_GetFolder();
            void vtableGap_Item();
            void vtableGap_ItemCount();
            void vtableGap_Items();
            void vtableGap_GetSelectionMarkedItem();
            void vtableGap_GetFocusedItem();
            void vtableGap_GetItemPosition();
            void vtableGap_GetSpacing();
            void vtableGap_GetDefaultSpacing();
            void vtableGap_GetAutoArrange();
            void vtableGap_SelectItem();
            void vtableGap_SelectAndPositionItems();
            void vtableGap_SetGroupBy();
            void vtableGap_GetGroupBy();
            void vtableGap_SetViewProperty();
            void vtableGap_GetViewProperty();
            void vtableGap_SetTileViewProperties();
            void vtableGap_SetExtendedTileViewProperties();
            void vtableGap_SetText();
            void SetCurrentFolderFlags(uint dwMask, uint dwFlags);
            void GetCurrentFolderFlags(out uint pdwFlags);
            void vtableGap_GetSortColumnCount();
            void vtableGap_SetSortColumns();
            void vtableGap_GetSortColumns();
            void vtableGap_GetItem();
            void vtableGap_GetVisibleItem();
            void vtableGap_GetSelectedItem();
            void vtableGap_GetSelection();
            void vtableGap_GetSelectionState();
            void vtableGap_InvokeVerbOnSelection();
            void vtableGap_SetViewModeAndIconSize();
            void vtableGap_GetViewModeAndIconSize();
            void vtableGap_SetGroupSubsetCount();
            void vtableGap_GetGroupSubsetCount();
            void vtableGap_SetRedraw();
            void vtableGap_IsMoveInSameFolder();
            void vtableGap_DoRename();
        }

        public readonly static Guid SID_STopLevelBrowser = new Guid("4C96BE40-915C-11CF-99D3-00AA004AE837");
        public const uint FWF_NOICONS = 0x1000;
        public const uint CSIDL_DESKTOP = 0U;
    }
}
