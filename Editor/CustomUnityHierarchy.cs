using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CustomUnityHierarchy.CustomUnityHierarchyData;

namespace CustomUnityHierarchy
{
    [InitializeOnLoad]
    public static class CustomUnityHierarchy
    {
        private static readonly CustomUnityHierarchyData customUnityHierarchyData;
        private static Texture tagTexture, layerTexture;
        private static readonly GUIStyle guiStyle = new()
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12,
        };
        private static int longestIconCount;
        private static readonly bool firstTimeSettingGuiStyleColor;

        static CustomUnityHierarchy()
        {
            customUnityHierarchyData = LoadCustomUnityHierarchyData();
            tagTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/iamagamedev.custom-unity-hierarchy/Tag.png", typeof(Texture));
            layerTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/iamagamedev.custom-unity-hierarchy/Layer.png", typeof(Texture));
            firstTimeSettingGuiStyleColor = true;
            EditorApplication.hierarchyWindowItemOnGUI += Draw;
            EditorApplication.hierarchyChanged += HierarchyChanged;
        }

        private static CustomUnityHierarchyData LoadCustomUnityHierarchyData()
        {
            var asset = (CustomUnityHierarchyData)AssetDatabase.LoadAssetAtPath("Assets/CustomUnityHierarchyData.asset", typeof(CustomUnityHierarchyData));
            if (asset == null)
            {
                try
                {
                    asset = ScriptableObject.CreateInstance<CustomUnityHierarchyData>();
                    AssetDatabase.CreateAsset(asset, "Assets/CustomUnityHierarchyData.asset");
                    AssetDatabase.SaveAssets();
                    OnlyDebugIfDebugsEnabled("Didn't find a CustomUnityHierarchyData, so creating one.", true, InfoType.Log);
                }
                catch
                {
                    OnlyDebugIfDebugsEnabled("Failed to create a CustomUnityHierarchyData, please manually make one and rename to 'CustomUnityHierarchyData'.", true, InfoType.Error);
                }
            }
            return asset;
        }

        private static void Draw(int instanceID, Rect selectionRect)
        {
            // We can assume this is the name of the scene which we don't want to draw for so "SampleScene".
            if (EditorUtility.InstanceIDToObject(instanceID) == null)
            {
                return;
            }
            Object objectReference = EditorUtility.InstanceIDToObject(instanceID);
            GameObject gameObject = objectReference as GameObject;

            if (firstTimeSettingGuiStyleColor)
            {
                guiStyle.normal.textColor = customUnityHierarchyData.textColor;
            }

            // Draw the background color for the Header.
            if (gameObject.name.Contains(customUnityHierarchyData.headerPrefix))
            {
                DrawHeader(gameObject, selectionRect);
            }

            // Offset to the right (by using xMax later) by 0.
            int offset = 0;

            // Draw the rest if the tag isn't "Header".
            DrawTreeView(objectReference, selectionRect);

            // Draw component icons
            offset = DrawComponentIcons(objectReference, selectionRect, offset);

            GUI.color = new Color(1, 1, 1, 1);

            // This int will be used for if we are drawing tags and we need to draw a layer later if the tag fails.
            int currentOffset = offset;
            offset = DrawTags(objectReference, offset, selectionRect); 

            // Wasn't able to draw tags even though its enabled so don't try and draw the layers.
            if (customUnityHierarchyData.tagsEnabled && currentOffset == offset)
            {
                return;
            }
            DrawLayers(objectReference, offset, selectionRect);
        }

        private static int DrawTags(Object objectReference, int offset, Rect selectionRect)
        {
            if (customUnityHierarchyData.tagsEnabled)
            {
                GameObject gameObject = objectReference as GameObject;
                if (!customUnityHierarchyData.activeTags.Contains(gameObject.tag))
                {
                    customUnityHierarchyData.activeTags.Add(gameObject.tag);
                }
                return DrawTagsAndLayers(gameObject, customUnityHierarchyData.activeTags, offset, selectionRect, tagTexture, gameObject.tag);
            }
            return offset;
        }

        private static int DrawLayers(Object objectReference, int offset, Rect selectionRect)
        {
            if (customUnityHierarchyData.layersEnabled)
            {
                GameObject gameObject = objectReference as GameObject;
                string layerName = LayerMask.LayerToName(gameObject.layer);
                if (!customUnityHierarchyData.activeLayers.Contains(layerName))
                {
                    customUnityHierarchyData.activeLayers.Add(layerName);
                }
                return DrawTagsAndLayers(gameObject, customUnityHierarchyData.activeLayers, offset, selectionRect, layerTexture, layerName);
            }
            return offset;
        }

        private static int DrawTagsAndLayers(GameObject gameObject, List<string> list, int offset, Rect selectionRect, Texture icon, string textToDisplay)
        {
            // Find out what the longest tag is for example "WWWWWWW" is longer than "a".
            float longestWidth = list.Select(x => guiStyle.CalcSize(new GUIContent(x)).x).Max();

            // If the data we want to display doesn't fit, just return.
            if (!CanIDisplayMoreData(GameObjectNameLength(gameObject), selectionRect.width, offset, longestWidth + 30, selectionRect))
            {
                return offset;
            }

            if (tagTexture == null || layerTexture == null)
            {
                tagTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/iamagamedev.custom-unity-hierarchy/Tag.png", typeof(Texture));
                layerTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/iamagamedev.custom-unity-hierarchy/Layer.png", typeof(Texture));
            }

            // Draw the icon (tag) for example.
            Rect newRect = new(selectionRect.xMax - longestWidth - 10 - offset, selectionRect.y, 16, 16);
            GUI.DrawTexture(newRect, icon);

            // Draw the texture "Player" for example.
            newRect = new(selectionRect.xMax - longestWidth + 10 - offset, selectionRect.y - 2, longestWidth, 16);
            GUI.Label(newRect, textToDisplay, guiStyle);

            // Return the new offset for future
            return offset + (int)longestWidth + 30;
        }

        private static float GameObjectNameLength(Object objectReference)
        {
            GUIContent temp2 = new(objectReference.name);
            Vector2 lengthOfGameobjectText = guiStyle.CalcSize(temp2);
            return lengthOfGameobjectText.x;
        }

        private static int DrawComponentIcons(Object objectReference, Rect selectionRect, int offset)
        {
            if (!customUnityHierarchyData.componentIconsEnabled)
            {
                return offset;
            }

            int objectReferenceID = objectReference.GetInstanceID();

            // Get the cached item if there even is one.
            var cachedGameObject = customUnityHierarchyData.gameObjectCache
                .FirstOrDefault(x => x.instanceID == objectReferenceID);

            GameObject gameObject = objectReference as GameObject;

            // A bunch of data needed to either add to the cache or update the cache.
            Component[] objectComponents = gameObject.GetComponents<Component>();
            List<string> componentTypes = new();
            foreach (Component component in objectComponents)
            {
                string componentToAdd = "Null";
                if (component != null)
                {
                    componentToAdd = component.GetType().Name;
                }
                componentTypes.Add(componentToAdd);
            }

            // Did we find a cache item, if yes update it, if no make it.
            if (cachedGameObject == null)
            {
                GameObjectCache newCacheEntry = new(objectReferenceID, objectComponents.Length, componentTypes, gameObject.tag, gameObject.layer, gameObject.activeInHierarchy);
                customUnityHierarchyData.gameObjectCache.Add(newCacheEntry);
                cachedGameObject = newCacheEntry;
            }
            else
            {
                cachedGameObject.componentCount = objectComponents.Length;
                cachedGameObject.componentTypes = componentTypes;
                cachedGameObject.tag = gameObject.tag;
                cachedGameObject.layer = gameObject.layer;
            }

            // Can we actually draw the component icons within the space we need?
            if (!CanIDisplayMoreData(GameObjectNameLength(objectReference), selectionRect.width, offset, ((objectComponents.Length - 1) * 25) + 20, selectionRect))
            {
                return offset;
            }

            // Will use this later.
            bool hasDrawnAScript = false;

            for (int i = 0; i < cachedGameObject.componentCount; i++)
            {
                // Null component found, this breaks this as we can't gettype a null component so
                if (cachedGameObject.componentTypes[i] == "Null")
                {
                    OnlyDebugIfDebugsEnabled($"Null component found at: {objectReference.name} skipping rendering.", true, InfoType.Warning);
                    continue;
                }

                // Minus the offset, 16by16 icon at y height.
                Rect newRect = new(selectionRect.xMax - offset, selectionRect.y, 16, 16);

                // See if the component is enabled or disabled
                bool isEnabled = objectComponents[i] is not Behaviour behaviour || behaviour.enabled;

                // If it is a mesh render for some reason I can't use .enabled I need to check if the render is enabled.
                if (!objectComponents[i].gameObject.activeInHierarchy || !isEnabled)
                {
                    GUI.color = new Color(1, 1, 1, 0.3f);
                }
                else if (objectComponents[i].GetType() == typeof(MeshRenderer))
                {
                    Renderer renderer = (MeshRenderer)objectComponents[i];
                    if (!renderer.enabled)
                    {
                        GUI.color = new Color(1, 1, 1, 0.3f);
                    }
                }
                if (objectComponents[i].GetType().Name.Contains("Collider") && !objectComponents[i].GetType().Name.Contains("2D"))
                {
                    Collider renderer = (Collider)objectComponents[i];
                    if (!renderer.enabled)
                    {
                        GUI.color = new Color(1, 1, 1, 0.3f);
                    }
                }

                Texture componentTexture = null;
                foreach (var componentAndTexture in customUnityHierarchyData.componentsAndTextures)
                {
                    if (componentAndTexture.componentName == objectComponents[i].GetType().Name)
                    {
                        componentTexture = componentAndTexture.componentTexture;
                    }
                }

                if (componentTexture == null)
                {
                    GUIContent componentContent = EditorGUIUtility.ObjectContent(objectComponents[i], objectComponents[i].GetType());
                    componentTexture = componentContent.image as Texture2D;
                    customUnityHierarchyData.componentsAndTextures.Add(new(objectComponents[i].GetType().Name, componentTexture));
                }

                if (customUnityHierarchyData.compactScriptIcons &&
                    hasDrawnAScript && componentTexture.name == "d_cs Script Icon")
                {
                    continue;
                }

                // Draws the componentTexture at a calculate Rect newRect position.
                GUI.DrawTexture(newRect, componentTexture);

                // If we haven't drawn a script and it is a script enable the bool.
                if (!hasDrawnAScript && componentTexture.name == "d_cs Script Icon")
                {
                    hasDrawnAScript = true;
                }

                // Set the color of the box to draw to be white no alpha value change.
                GUI.color = new Color(1, 1, 1, 1);

                // Add a XXXXX pixel offset.
                offset += 20;
                if (longestIconCount < offset)
                {
                    longestIconCount = offset;
                }
            }
            return longestIconCount;
        }

        private static bool CanIDisplayMoreData(float lengthOfText, float width, float currentOffset, float requiredSpace, Rect TEMP)
        {
            if (lengthOfText > width)
            {
                return false;
            }
            if (lengthOfText + currentOffset + requiredSpace > width)
            {
                return false;
            }
            return true;
        }

        private static void DrawTreeView(Object objectReference, Rect selectionRect)
        {
            if (!customUnityHierarchyData.treeEnabled)
            {
                return;
            }

            GameObject gameObject = objectReference as GameObject;
            if (gameObject.transform.childCount == 0)
            {
                // Draws the main line down.
                Rect newRect = new(selectionRect.x - 8, selectionRect.y, 2, 16);
                EditorGUI.DrawRect(newRect, customUnityHierarchyData.mainBranchColor);

                // Draws the line across from the middle.
                newRect = new(selectionRect.x - 6, selectionRect.y + 7, 7, 2);
                EditorGUI.DrawRect(newRect, customUnityHierarchyData.mainBranchColor);
            }
            // Checks if there is a parent and repeats.
            DrawTreeViewRecursive(gameObject, new Rect(selectionRect.x - 8, selectionRect.y, selectionRect.width, selectionRect.height));
        }

        private static void DrawTreeViewRecursive(GameObject gameObject, Rect newRect)
        {
            if (gameObject.transform.parent != null)
            {
                newRect = new(newRect.x - 14, newRect.y, 2, 16);
                EditorGUI.DrawRect(newRect, customUnityHierarchyData.subBranchColor);
                DrawTreeViewRecursive(gameObject.transform.parent.gameObject, newRect);
            }
        }

        private static void DrawHeader(GameObject gameObject, Rect selectionRect)
        {
            if (!customUnityHierarchyData.headersEnabled)
            {
                return;
            }

            // Offset the box and draw the box.
            Rect newRect = new(selectionRect.x, selectionRect.y, selectionRect.width + 50, selectionRect.height);
            Color newColor = customUnityHierarchyData.headerColor;
            newColor.a = newColor.a > 0.25f ? 0.25f : newColor.a;
            EditorGUI.DrawRect(newRect, newColor);
        }

        // When we create / rename / delete / add a component to a gameobject.
        private static void HierarchyChanged()
        {
            customUnityHierarchyData.activeTags.Clear();
            customUnityHierarchyData.activeLayers.Clear();
            customUnityHierarchyData.gameObjectCache.RemoveAll(x => x == null || !EditorUtility.InstanceIDToObject(x.instanceID));
        }

        private static void OnlyDebugIfDebugsEnabled(string debugString, bool overrideCheck, InfoType infoType)
        {
            if (overrideCheck || customUnityHierarchyData.debugsEnabled)
            {
                switch (infoType)
                {
                    case InfoType.Log:
                        Debug.Log($"Custom Unity Hierarchy Error: {debugString}");
                        break;
                    case InfoType.Warning:
                        Debug.LogWarning($"Custom Unity Hierarchy Error: {debugString}");
                        break;
                    case InfoType.Error:
                        Debug.LogError($"Custom Unity Hierarchy Error: {debugString}");
                        break;
                }
            }
        }
    }
}

public enum InfoType
{
    Log,
    Warning,
    Error
}