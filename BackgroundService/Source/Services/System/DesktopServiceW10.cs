using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.System.API.VirtualDesktop;
using BackgroundService.Source.Services.System.Models;
using BackgroundService.Source.Services.System.Models.VirtualDesktop;
using Core.Utils;

namespace BackgroundService.Source.Services.System
{
    internal class DesktopServiceW10 : DesktopService
    {
        private static readonly string VIRTUAL_DESKTOP_PATH = InternalSettings.PATH_VIRTUAL_DESKTOP_W10;

        private readonly object threadLock = new object();

        public DesktopServiceW10(ServiceProvider services) : base(services) { }

        public override void CreateAndSwitchToDesktop(string desktopName)
        {
            Logger.Debug($"Switching desktop to: {desktopName}");

            ExecVirtualDesktopBinary($"/New /Name:{desktopName} /Switch");
        }

        public override void RemoveDesktop(string desktopName)
        {
            Logger.Debug($"Removing desktop: {desktopName}");

            ExecVirtualDesktopBinary($"/Remove:{desktopName}");
        }

        public override string GetCurrentDesktopName()
        {
            var resourceManager = new VirtualDesktopAPIW10.ResourceManager();

            try
            {
                var currentDesktop = resourceManager.GetCurrentVirtualDesktop();
                var desktopInfo = new VirtualDesktopInfoW10(0, currentDesktop);

                return desktopInfo.Name;
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }

        public override List<WindowComponent> GetWindowsOnDesktop(string desktopName)
        {
            var resourceManager = new VirtualDesktopAPIW10.ResourceManager();
            var windows = new List<WindowComponent>();

            try
            {
                var views = GetApplicationViews(resourceManager);
                var desktops = GetVirtualDesktops(resourceManager);

                views.ForEach(view =>
                {
                    view.GetThumbnailWindow(out var windowHandle);

                    var desktop = desktops.Find(d => d.OwnsView(view));

                    var isOnDesktop = desktop != null && desktop.Name == desktopName;
                    var isVisible = IsViewVisible(view);

                    if (isVisible && isOnDesktop)
                    {
                        windows.Add(new WindowComponent("Window", windowHandle));
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get windows on desktop (name: {desktopName}): {ex}");
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }

            return windows;
        }

        private bool IsViewVisible(VirtualDesktopAPIW10.IApplicationView view)
        {
            try
            {
                view.GetVisibility(out var visibility);
                return visibility == 1;
            }
            catch
            {
                Logger.Warn("Failed to get visibility for view");
                return false;
            }
        }

        private List<VirtualDesktopInfoW10> GetVirtualDesktops(VirtualDesktopAPIW10.ResourceManager resourceManager)
        {
            var virtualDesktops = resourceManager.GetVirtualDesktops();

            return Enumerable
                .Range(0, virtualDesktops.Count)
                .Select(i => new VirtualDesktopInfoW10(i, VirtualDesktopAPIW10.DesktopManager.GetDesktop(i)))
                .ToList();
        }

        private List<VirtualDesktopAPIW10.IApplicationView> GetApplicationViews(VirtualDesktopAPIW10.ResourceManager resourceManager)
        {
            return resourceManager.GetApplicationViews();
        }

        private void ExecVirtualDesktopBinary(string args)
        {
            Logger.Debug($"Exec virtual desktop, args: {args}");

            ProcessUtils.StartProcess(VIRTUAL_DESKTOP_PATH, args, ProcessWindowStyle.Hidden, true);
        }
    }
}
