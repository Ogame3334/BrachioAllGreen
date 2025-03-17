// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace PassthroughCameraSamples.Editor
{
    public class PassthroughCameraEditorUpdateManifest : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            UpdateAndroidManifest();
        }

        private void UpdateAndroidManifest()
        {
            string pcaManifestPermission = "horizonos.permission.HEADSET_CAMERA";
            string manifestFolder = Application.dataPath + "/Plugins/Android";
            try
            {
                // Load android manfiest file
                XmlDocument doc = new XmlDocument();
                doc.Load(manifestFolder + "/AndroidManifest.xml");

                string androidNamepsaceURI;
                XmlElement element = (XmlElement)doc.SelectSingleNode("/manifest");
                if (element == null)
                {
                    throw new System.OperationCanceledException("Could not find manifest tag in android manifest.");
                }

                // Get android namespace URI from the manifest
                androidNamepsaceURI = element.GetAttribute("xmlns:android");
                if (!string.IsNullOrEmpty(androidNamepsaceURI))
                {
                    // Check if the android manifest already has the Passthrough Camera Access permission
                    XmlNodeList nodeList = doc.SelectNodes("/manifest/uses-permission");
                    foreach (XmlElement e in nodeList)
                    {
                        string attr = e.GetAttribute("name", androidNamepsaceURI);
                        if (attr == pcaManifestPermission)
                        {
                            Debug.Log("PCA Editor: Android manifest already has the proper permissions.");
                            return;
                        }
                    }

                    if (EditorUtility.DisplayDialog("Meta Passthrough Camera Access", "\"horizonos.permission.HEADSET_CAMERA\" permission IS NOT PRESENT in AndroidManifest.xml", "Add it", "Do Not Add it"))
                    {
                        element = (XmlElement)doc.SelectSingleNode("/manifest");
                        if (element != null)
                        {
                            // Insert Passthrough Camera Access permission
                            XmlElement newElement = doc.CreateElement("uses-permission");
                            newElement.SetAttribute("name", androidNamepsaceURI, pcaManifestPermission);
                            element.AppendChild(newElement);

                            doc.Save(manifestFolder + "/AndroidManifest.xml");
                            Debug.Log("PCA Editor: Successfully modified android manifest with Passthrough Camera Access permission.");
                            return;
                        }
                        throw new System.OperationCanceledException("Could not find android namespace URI in android manifest.");
                    }
                    else
                    {
                        throw new System.OperationCanceledException("To use the Passthrough Camera Access Api you need to add the \"horizonos.permission.HEADSET_CAMERA\" permission in your AndroidManifest.xml.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new BuildFailedException("PCA Editor: " + e.Message);
            }
        }
    }
}
