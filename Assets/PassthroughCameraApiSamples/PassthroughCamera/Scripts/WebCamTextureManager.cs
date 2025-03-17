// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace PassthroughCameraSamples
{
    using PCD = PassthroughCameraDebugger;

    public class WebCamTextureManager : MonoBehaviour
    {
        [SerializeField] public PassthroughCameraEye eye = PassthroughCameraEye.Left;
        [SerializeField, Tooltip("The requested resolution of the camera may not be supported by the chosen camera. In such cases, the closest available values will be used.\n\n" +
                                 "When set to (0,0), the highest supported resolution will be used.")]
        public Vector2Int requestedResolution;
        [SerializeField] public PassthroughCameraPermissions _cameraPermissions;

        /// <summary>
        /// Returns <see cref="WebCamTexture"/> reference if required permissions were granted and this component is enabled. Else, returns null.
        /// </summary>
        public WebCamTexture WebCamTexture { get; private set; }

        private bool _hasPermission;

        private void Awake()
        {
            PCD.DebugMessage(LogType.Log, $"{nameof(WebCamTextureManager)}.{nameof(Awake)}() was called");
            Assert.AreEqual(1, FindObjectsByType<WebCamTextureManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                $"PCA: Passthrough Camera: more than one {nameof(WebCamTextureManager)} component. Only one instance is allowed at a time. Current instance: {name}");
#if UNITY_ANDROID
            _cameraPermissions.AskCameraPermissions();
#endif
        }

        private void OnEnable()
        {
            PCD.DebugMessage(LogType.Log, $"PCA: {nameof(OnEnable)}() was called");
            if (!PassthroughCameraUtils.IsSupported)
            {
                PCD.DebugMessage(LogType.Log, "PCA: Passthrough Camera functionality is not supported by the current device." +
                          $" Disabling {nameof(WebCamTextureManager)} object");
                enabled = false;
                return;
            }

            _hasPermission = PassthroughCameraPermissions.HasCameraPermission == true;
            if (!_hasPermission)
            {
                PCD.DebugMessage(LogType.Error,
                    $"PCA: Passthrough Camera requires permission(s) {string.Join(" and ", PassthroughCameraPermissions.CameraPermissions)}. Waiting for them to be granted...");
                return;
            }

            PCD.DebugMessage(LogType.Log, "PCA: All permissions have been granted");
            StartCoroutine(InitializeWebCamTexture());
        }

        private void OnDisable()
        {
            PCD.DebugMessage(LogType.Log, $"PCA: {nameof(OnDisable)}() was called");
            StopCoroutine(InitializeWebCamTexture());
            if (WebCamTexture != null)
            {
                WebCamTexture.Stop();
                Destroy(WebCamTexture);
                WebCamTexture = null;
            }
        }

        private void Update()
        {
            if (!_hasPermission)
            {
                if (PassthroughCameraPermissions.HasCameraPermission != true)
                    return;

                _hasPermission = true;
                StartCoroutine(InitializeWebCamTexture());
            }
        }

        private IEnumerator InitializeWebCamTexture()
        {
            while (true)
            {
                var devices = WebCamTexture.devices;
                if (PassthroughCameraUtils.EnsureInitialized() && PassthroughCameraUtils.cameraEyeToCameraIdMap.TryGetValue(eye, out var cameraData))
                {
                    if (cameraData.index < devices.Length)
                    {
                        string deviceName = devices[cameraData.index].name;
                        var webCamTexture = requestedResolution == Vector2Int.zero ? new WebCamTexture(deviceName) : new WebCamTexture(deviceName, requestedResolution.x, requestedResolution.y);
                        // There is a bug in the current implementation of WebCamTexture: if 'Play()' is called at the same frame the WebCamTexture was created, this error is logged and the WebCamTexture object doesn't work:
                        //     Camera2: SecurityException java.lang.SecurityException: validateClientPermissionsLocked:1325: Callers from device user 0 are not currently allowed to connect to camera "66"
                        //     Camera2: Timeout waiting to open camera.
                        // Waiting for one frame is important and prevents the bug.
                        yield return null;
                        webCamTexture.Play();
                        var currentResolution = new Vector2Int(webCamTexture.width, webCamTexture.height);
                        if (requestedResolution != Vector2Int.zero && requestedResolution != currentResolution)
                        {
                            PCD.DebugMessage(LogType.Warning, $"WebCamTexture created, but '{nameof(requestedResolution)}' {requestedResolution} is not supported. Current resolution: {currentResolution}.");
                        }
                        WebCamTexture = webCamTexture;
                        PCD.DebugMessage(LogType.Log, $"WebCamTexture created, texturePtr: {WebCamTexture.GetNativeTexturePtr()}, size: {WebCamTexture.width}/{WebCamTexture.height}");
                        yield break;
                    }
                }

                PCD.DebugMessage(LogType.Error, $"Requested camera is not present in WebCamTexture.devices: {string.Join(", ", devices)}.");
                yield return null;
            }
        }
    }

    /// <summary>
    /// Defines the position of a passthrough camera relative to the headset
    /// </summary>
    public enum PassthroughCameraEye
    {
        Left,
        Right
    }
}
