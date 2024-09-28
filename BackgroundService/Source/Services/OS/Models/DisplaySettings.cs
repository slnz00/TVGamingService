using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using static Core.WinAPI.DisplayAPI;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class DisplaySettings
    {
        public delegate void ModifyPathAction(ref DISPLAYCONFIG_PATH_INFO path);

        public List<DISPLAYCONFIG_PATH_INFO> Paths = new List<DISPLAYCONFIG_PATH_INFO>();
        public List<DISPLAYCONFIG_MODE_INFO> Modes = new List<DISPLAYCONFIG_MODE_INFO>();

        public DisplaySettings() { }

        public DisplaySettings(
            DISPLAYCONFIG_PATH_INFO[] paths,
            uint pathsCount,
            DISPLAYCONFIG_MODE_INFO[] modes,
            uint modesCount
        )
        {
            Paths = paths
                .Take(Convert.ToInt32(pathsCount))
                .ToList();

            Modes = modes
                .Take(Convert.ToInt32(modesCount))
                .ToList();
        }

        public DisplaySettings Clone()
        {
            var clone = new DisplaySettings();

            clone.Paths = new List<DISPLAYCONFIG_PATH_INFO>(Paths);
            clone.Modes = new List<DISPLAYCONFIG_MODE_INFO>(Modes);

            return clone;
        }

        public void Reset()
        {
            ResetPaths();
            ResetModes();
        }

        public void ResetPaths()
        {
            for (int i = 0; i < Paths.Count; i++)
            {
                ModifyPath(i, (ref DISPLAYCONFIG_PATH_INFO path) =>
                {
                    BitUtils.SetBit(
                        ref path.flags,
                        (uint)DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
                        false
                    );

                    path.sourceInfo.statusFlags = 0;
                    path.targetInfo.statusFlags = 0;
                });
            }
        }

        public void ResetModes()
        {
            Modes.Clear();

            for (int i = 0; i < Paths.Count; i++)
            {
                ModifyPath(i, (ref DISPLAYCONFIG_PATH_INFO path) =>
                {
                    path.sourceInfo.idx.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                    path.targetInfo.idx.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                });
            }
        }

        public void ActivatePath(uint sourceId, uint targetId)
        {
            ModifyPath(sourceId, targetId, (ref DISPLAYCONFIG_PATH_INFO path) =>
            {
                BitUtils.SetBit(
                    ref path.flags,
                    (uint)DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
                    true
                );

                path.sourceInfo.statusFlags = 1;
                path.targetInfo.statusFlags = 1;
            });
        }

        public void InactivatePath(uint sourceId, uint targetId)
        {
            ModifyPath(sourceId, targetId, (ref DISPLAYCONFIG_PATH_INFO path) =>
            {
                BitUtils.SetBit(
                    ref path.flags,
                    (uint)DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
                    false
                );

                path.sourceInfo.statusFlags = 0;
                path.targetInfo.statusFlags = 0;
            });
        }

        public void ModifyPath(uint sourceId, uint targetId, ModifyPathAction action)
        {
            var pathIndex = FindPathIndex(sourceId, targetId);
            var path = Paths[pathIndex];

            action(ref path);

            Paths[pathIndex] = path;
        }

        public void ModifyPath(int pathIndex, ModifyPathAction action)
        {
            var path = Paths[pathIndex];

            action(ref path);

            Paths[pathIndex] = path;
        }

        public void GetModesForPath(uint sourceId, uint targetId, out DISPLAYCONFIG_MODE_INFO? sourceMode, out DISPLAYCONFIG_MODE_INFO? targetMode)
        {
            var pathIndex = FindPathIndex(sourceId, targetId);
            var sourceModeIdx = Paths[pathIndex].sourceInfo.idx.modeInfoIdx;
            var targetModeIdx = Paths[pathIndex].targetInfo.idx.modeInfoIdx;

            if (sourceModeIdx != DISPLAYCONFIG_PATH_MODE_IDX_INVALID)
            {
                sourceMode = Modes[(int)sourceModeIdx];
            }
            else
            {
                sourceMode = null;
            }

            if (targetModeIdx != DISPLAYCONFIG_PATH_MODE_IDX_INVALID)
            {
                targetMode = Modes[(int)targetModeIdx];
            }
            else
            {
                targetMode = null;
            }
        }

        public void RemoveModesForPath(uint sourceId, uint targetId)
        {
            var modeIndexesToRemove = new List<int>();
            var pathIndex = FindPathIndex(sourceId, targetId);
            var sourceModeIdx = Paths[pathIndex].sourceInfo.idx.modeInfoIdx;
            var targetModeIdx = Paths[pathIndex].targetInfo.idx.modeInfoIdx;

            if (sourceModeIdx != DISPLAYCONFIG_PATH_MODE_IDX_INVALID)
            {
                modeIndexesToRemove.Add((int)sourceModeIdx);
            }
            if (targetModeIdx != DISPLAYCONFIG_PATH_MODE_IDX_INVALID)
            {
                modeIndexesToRemove.Add((int)targetModeIdx);
            }

            foreach (var i in modeIndexesToRemove.OrderByDescending(i => i))
            {
                Modes.RemoveAt(i);
            }
        }

        public void SetModesForPath(uint sourceId, uint targetId, DISPLAYCONFIG_MODE_INFO? sourceMode, DISPLAYCONFIG_MODE_INFO? targetMode)
        {
            RemoveModesForPath(sourceId, targetId);

            ModifyPath(sourceId, targetId, (ref DISPLAYCONFIG_PATH_INFO path) =>
            {
                if (sourceMode != null)
                {
                    var mode = (DISPLAYCONFIG_MODE_INFO)sourceMode;

                    path.sourceInfo.idx.modeInfoIdx = (uint)Modes.Count;

                    mode.id = path.sourceInfo.id;
                    mode.adapterId = path.sourceInfo.adapterId;
                    Modes.Add(mode);
                }

                if (targetMode != null)
                {
                    var mode = (DISPLAYCONFIG_MODE_INFO)targetMode;

                    path.targetInfo.idx.modeInfoIdx = (uint)Modes.Count;

                    mode.id = path.targetInfo.id;
                    mode.adapterId = path.targetInfo.adapterId;
                    Modes.Add(mode);
                }
            });
        }

        public int FindPathIndex(uint sourceId, uint targetId)
        {
            for (int i = 0; i < Paths.Count; i++)
            {
                var source = Paths[i].sourceInfo;
                var target = Paths[i].targetInfo;
                var isSelectedPath = source.id == sourceId && target.id == targetId;

                if (isSelectedPath)
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException($"Display path does not exist for: source #{sourceId} - target #{targetId}");
        }
    }
}
