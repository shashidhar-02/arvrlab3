using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assignment3.Editor
{
    [InitializeOnLoad]
    public static class McpAutoStarter
    {
        static McpAutoStarter()
        {
            EditorApplication.delayCall += () =>
            {
                _ = ConnectBridge();
            };
        }

        [MenuItem("Assignment 3/Connect MCP Bridge", priority = 3)]
        public static async Task ConnectBridge()
        {
            try
            {
                EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
                EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);

                var locatorType = Type.GetType("MCPForUnity.Editor.Services.MCPServiceLocator, MCPForUnity.Editor");
                if (locatorType != null)
                {
                    var bridgeProp = locatorType.GetProperty("Bridge", BindingFlags.Public | BindingFlags.Static);
                    if (bridgeProp != null)
                    {
                        var bridgeObj = bridgeProp.GetValue(null);
                        if (bridgeObj != null)
                        {
                            var startMethod = bridgeObj.GetType().GetMethod("StartAsync", BindingFlags.Public | BindingFlags.Instance);
                            if (startMethod != null)
                            {
                                var task = (Task<bool>)startMethod.Invoke(bridgeObj, null);
                                bool connected = await task;
                                Debug.Log($"<color=green>[MCP] Unity Bridge connected: {connected}</color>");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[MCP AutoStarter] " + ex.Message);
            }
        }
    }
}
