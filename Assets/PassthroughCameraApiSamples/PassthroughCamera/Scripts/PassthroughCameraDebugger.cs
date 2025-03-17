// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace PassthroughCameraSamples
{
    public static class PassthroughCameraDebugger
    {
        public enum DebuglevelEnum
        {
            ALL,
            NONE,
            ONLY_ERROR,
            ONLY_LOG,
            ONLY_WARNING
        }

        public static DebuglevelEnum debugLevel = DebuglevelEnum.ALL;

        /// <summary>
        /// Send debug information to Unity console based on DebugType and DebugLevel
        /// </summary>
        /// <param name="mType"></param>
        /// <param name="message"></param>
        public static void DebugMessage(UnityEngine.LogType mType, string message)
        {
            switch (mType)
            {
                case LogType.Error:
                    if (debugLevel == DebuglevelEnum.ALL || debugLevel == DebuglevelEnum.ONLY_ERROR)
                    {
                        Debug.LogError(message);
                    }
                    break;
                case LogType.Log:
                    if (debugLevel == DebuglevelEnum.ALL || debugLevel == DebuglevelEnum.ONLY_LOG)
                    {
                        Debug.Log(message);
                    }
                    break;
                case LogType.Warning:
                    if (debugLevel == DebuglevelEnum.ALL || debugLevel == DebuglevelEnum.ONLY_WARNING)
                    {
                        Debug.LogWarning(message);
                    }
                    break;
            }
        }
    }
}
