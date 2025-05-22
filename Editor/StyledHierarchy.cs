using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static StyledHierarchy.StyledHierarchyData;

namespace StyledHierarchy
{
    [InitializeOnLoad]
    public static class StyledHierarchy
    {
        private static readonly StyledHierarchyData styledHierarchyData;
        private static readonly GUIStyle guiStyle = new()
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12,
        };
        private static int longestIconCount;
        private static readonly bool firstTimeSettingGuiStyleColor;

        /// <summary>
        /// Called on first load / reloading the editor (saving a script etc).
        /// </summary>
        static StyledHierarchy()
        {
            styledHierarchyData = LoadStyledHierarchyData();

            // Loads the tag and layer texture from a "Resources" folder.
            styledHierarchyData.tagTexture = (Texture)Resources.Load("tag");
            styledHierarchyData.layerTexture = (Texture)Resources.Load("layer");

            firstTimeSettingGuiStyleColor = true;

            // APIs that Unity pre-built in that we can utilise.
            EditorApplication.hierarchyWindowItemOnGUI += Draw;
            EditorApplication.hierarchyChanged += HierarchyChanged;
        }

        /// <summary>
        /// A method that is called manually to load/create the Scriptable Object data.
        /// </summary>
        /// <returns></returns> Returns the loaded/created asset.
        private static StyledHierarchyData LoadStyledHierarchyData()
        {
            var asset = (StyledHierarchyData)AssetDatabase.LoadAssetAtPath("Assets/StyledHierarchyData.asset", typeof(StyledHierarchyData));
            if (asset == null)
            {
                try
                {
                    asset = ScriptableObject.CreateInstance<StyledHierarchyData>();
                    AssetDatabase.CreateAsset(asset, "Assets/StyledHierarchyData.asset");
                    AssetDatabase.SaveAssets();
                    Debug.Log("Didn't find a StyledHierarchyData, so creating one.");
                }
                catch
                {
                    Debug.Log("Failed to create a StyledHierarchyData, please manually make one and rename to 'StyledHierarchyData'.");
                }
            }
            return asset;
        }

        /// <summary>
        /// Main method that is hooked to the Unity API EditorApplication.hierarchyWindowItemOnGUI += Draw.
        /// </summary>
        /// <param name="instanceID"></param> InstanceID (object int) for what is being drawn.
        /// <param name="rect"></param> A "Rect" (rectangle X Y Width Height) for what is being drawn.
        private static void Draw(int instanceID, Rect rect)
        {
            // We can assume this is the name of the scene which we don't want to draw for so "SampleScene".
            if (EditorUtility.InstanceIDToObject(instanceID) == null)
            {
                return;
            }

            // Converting the instanceID to a Object then into a GameObject.
            Object objectReference = EditorUtility.InstanceIDToObject(instanceID);
            GameObject gameObject = objectReference as GameObject;

            // Stores the original textColor that is used by Unity.
            if (firstTimeSettingGuiStyleColor)
            {
                guiStyle.normal.textColor = styledHierarchyData.textColor;
            }

            // Draw the background color for the Header.
            DrawHeader(gameObject.name, rect);

            // Offset to the right (by using xMax later) by 0.
            int offset = 0;

            // Draw the rest if the tag isn't "Header".
            DrawTreeView(gameObject, rect);

            // Draw component icons.
            offset = DrawComponentIcons(objectReference, rect, offset);

            // Resets the color after each component is drawn (fixes a bug where a gameobject is shown as disabled when it wasn't).
            GUI.color = new Color(1, 1, 1, 1);

            // This int will be used for if we are drawing tags and we need to draw a layer later if the tag fails.
            int currentOffset = offset;

            if (styledHierarchyData.tagsEnabled)
            {
                offset = TagsAndLayersCombined(gameObject, gameObject.tag, styledHierarchyData.activeTags, offset, rect, styledHierarchyData.tagTexture);
            }

            // Wasn't able to draw tags even though its enabled so don't try and draw the layers.
            if (styledHierarchyData.tagsEnabled && currentOffset == offset)
            {
                return;
            }

            // Finally draws layers.
            if (styledHierarchyData.layersEnabled)
            {
                string layer = LayerMask.LayerToName(gameObject.layer);
                TagsAndLayersCombined(gameObject, layer, styledHierarchyData.activeLayers, offset, rect, styledHierarchyData.layerTexture);
            }
        }

        /// <summary>
        /// Merged a old method that was duplicated into this that just takes in different params.
        /// </summary>
        /// <param name="gameObject"></param> GameObject we are drawing for.
        /// <param name="text"></param> The text that needs to be displayed.
        /// <param name="hashSet"></param> The HashSet it is getting added too.
        /// <param name="offset"></param> The current X offset.
        /// <param name="rect"></param> The current rect (rectangle) location.
        /// <param name="icon"></param> The icon to draw before (tag or layer).
        /// <returns></returns>
        private static int TagsAndLayersCombined(GameObject gameObject, string text, HashSet<string> hashSet, int offset, Rect rect, Texture icon)
        {
            hashSet.Add(text);
            return DrawTagsAndLayers(gameObject, hashSet, offset, rect, icon, text);
        }

        /// <summary>
        /// Draws the icon then the layer if it fits.
        /// </summary>
        /// <param name="gameObject"></param> GameObject we are drawing for.
        /// <param name="hashSet"></param> The HashSet so it can calculate the longest word displayed.
        /// <param name="offset"></param> The current X offset.
        /// <param name="rect"></param> The current rect (rectangle) location.
        /// <param name="icon"></param> The icon to draw before (tag or layer).
        /// <param name="textToDisplay"></param>
        /// <returns></returns>
        private static int DrawTagsAndLayers(GameObject gameObject, HashSet<string> hashSet, int offset, Rect rect, Texture icon, string textToDisplay)
        {
            // Find out what the longest tag is for example "WWWWWWW" is longer than "a".
            float longestWidth = hashSet.Select(x => guiStyle.CalcSize(new GUIContent(x)).x).Max();

            // If the data we want to display doesn't fit, just return.
            if (!CanIDisplayMoreData(GameObjectNameLength(gameObject), rect.width, offset, longestWidth + 30))
            {
                return offset;
            }

            // Null check.
            if (styledHierarchyData.tagTexture == null || styledHierarchyData.layerTexture == null)
            {
                // Trys to find the tag or layer texture in a Resources folder.
                styledHierarchyData.tagTexture = (Texture)Resources.Load("tag");
                styledHierarchyData.layerTexture = (Texture)Resources.Load("layer");
                // If still null log and return.
                if (styledHierarchyData.layerTexture == null || styledHierarchyData.layerTexture == null)
                {
                    Debug.LogError("Failed to get references to the Tag/Layer images");
                    return offset;
                }
            }

            // Draw the icon (tag) for example.
            Rect newRect = new(rect.xMax - longestWidth - 10 - offset, rect.y, 16, 16);
            GUI.DrawTexture(newRect, icon);

            // Draw the texture "Player" for example.
            newRect = new(rect.xMax - longestWidth + 10 - offset, rect.y - 2, longestWidth, 16);
            GUI.Label(newRect, textToDisplay, guiStyle);

            // Return the new offset for future
            return offset + (int)longestWidth + 30;
        }

        /// <summary>
        /// Returns a X Length by calculating how big a rectangle (guiStyle) is based on what text we are displaying.
        /// </summary>
        /// <param name="objectReference"></param> objectReference (to get the name).
        /// <returns></returns> The x length.
        private static float GameObjectNameLength(Object objectReference)
        {
            GUIContent temp2 = new(objectReference.name);
            Vector2 lengthOfGameobjectText = guiStyle.CalcSize(temp2);
            return lengthOfGameobjectText.x;
        }

        /// <summary>
        /// Method to draw component icons in a loop for each component on a object.
        /// </summary>
        /// <param name="objectReference"></param> The objectReference that the components are on.
        /// <param name="rect"></param> The current rect (rectangle) location.
        /// <param name="offset"></param> The current X offset.
        /// <returns></returns> The new X offset.
        private static int DrawComponentIcons(Object objectReference, Rect rect, int offset)
        {
            // If disabled return the old offset.
            if (!styledHierarchyData.componentIconsEnabled)
            {
                return offset;
            }

            // Gets the objectReferenceID.
            int objectReferenceID = objectReference.GetInstanceID();

            // Get the cached item if there even is one.
            var cachedGameObject = styledHierarchyData.gameObjectCache
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

            // Did we find a cache item, if no make it, if yes update it.
            if (cachedGameObject == null)
            {
                GameObjectCache newCacheEntry = new(objectReferenceID, objectComponents.Length, componentTypes, gameObject.tag, gameObject.layer, gameObject.activeInHierarchy);
                styledHierarchyData.gameObjectCache.Add(newCacheEntry);
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
            if (!CanIDisplayMoreData(GameObjectNameLength(objectReference), rect.width, offset, ((objectComponents.Length - 1) * 25) + 20))
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
                    Debug.Log($"Null component found at: {objectReference.name} skipping rendering.");
                    continue;
                }

                // Minus the offset, 16by16 icon at y height.
                Rect newRect = new(rect.xMax - offset, rect.y, 16, 16);

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

                // Obtain the componentTexture from the componentTexture cache passing in a string.
                Texture componentTexture = null;
                foreach (var componentAndTexture in styledHierarchyData.componentsAndTextures)
                {
                    if (componentAndTexture.componentName == objectComponents[i].GetType().Name)
                    {
                        componentTexture = componentAndTexture.componentTexture;
                    }
                }

                // If it is null, cache the component name and texture by retriving it with EditorGUIUtility.ObjectContent.
                if (componentTexture == null)
                {
                    GUIContent componentContent = EditorGUIUtility.ObjectContent(objectComponents[i], objectComponents[i].GetType());
                    componentTexture = componentContent.image as Texture2D;
                    styledHierarchyData.componentsAndTextures.Add(new(objectComponents[i].GetType().Name, componentTexture));
                }

                // If compact script icons is enabled and we are about to draw a script skip this component.
                if (styledHierarchyData.compactScriptIcons &&
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

                // Add a 20 pixel offset.
                offset += 20;
                if (longestIconCount < offset)
                {
                    longestIconCount = offset;
                }
            }
            return longestIconCount;
        }

        /// <summary>
        /// Calculates if there is more room to display more data.
        /// </summary>
        private static bool CanIDisplayMoreData(float lengthOfText, float width, float currentOffset, float requiredSpace)
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

        /// <summary>
        /// Draws the cool blue and green tree view.
        /// </summary>
        /// <param name="objectReference"></param> The current objectReference.
        /// <param name="rect"></param> The current rect.
        private static void DrawTreeView(GameObject gameObject, Rect rect)
        {
            // Checks if tree view is enabled.
            if (!styledHierarchyData.treeEnabled)
            {
                return;
            }

            if (gameObject.transform.childCount == 0)
            {
                // Draws the main line down.
                Rect newRect = new(rect.x - 8, rect.y, 2, 16);
                EditorGUI.DrawRect(newRect, styledHierarchyData.mainBranchColor);

                // Draws the line across from the middle.
                newRect = new(rect.x - 6, rect.y + 7, 7, 2);
                EditorGUI.DrawRect(newRect, styledHierarchyData.mainBranchColor);
            }
            // Checks if there is a parent and repeats.
            DrawTreeViewRecursive(gameObject, new Rect(rect.x - 8, rect.y, rect.width, rect.height));
        }

        /// <summary>
        /// Checks if it finds a parent, if so, draw a line with a new offset for every time we do have a parent.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="newRect"></param>
        private static void DrawTreeViewRecursive(GameObject gameObject, Rect newRect)
        {
            // No parent was found return.
            if (gameObject.transform.parent == null)
            {
                return;
            }

            // Add a offset for the default (blue) line by -14 pixels left.
            newRect = new(newRect.x - 14, newRect.y, 2, 16);

            // Draw the Rect.
            EditorGUI.DrawRect(newRect, styledHierarchyData.subBranchColor);

            // Call ourselves (recursion).
            DrawTreeViewRecursive(gameObject.transform.parent.gameObject, newRect);
        }

        /// <summary>
        /// Draws the red (default color) header on X prefix.
        /// </summary>
        /// <param name="gameObject"></param> gameObject we are drawing a header for.
        /// <param name="rect"></param> the gameObjects rect area.
        private static void DrawHeader(string gameObjectName, Rect rect)
        {
            // Returns if headers is disabled or nothing is in the prefix.
            if (!styledHierarchyData.headersEnabled ||
                styledHierarchyData.prefixAndColor.Count == 0 ||
                !styledHierarchyData.prefixAndColor.Any(x => gameObjectName.Contains(x.headerPrefix)))
            {
                return;
            }

            // Get the color from the first instance that the prefix applies to this gameObject name.
            Color newColor = styledHierarchyData.prefixAndColor.First(x => gameObjectName.Contains(x.headerPrefix)).headerColor;

            // Adjust the rect.
            Rect newRect = new(rect.x, rect.y, rect.width + 50, rect.height);

            // Make it transparent.
            newColor.a = newColor.a > 0.25f ? 0.25f : newColor.a;

            // Draw the Rect.
            EditorGUI.DrawRect(newRect, newColor);
        }

        /// <summary>
        /// When we create / rename / delete / add a component to a gameobject.
        /// </summary>
        private static void HierarchyChanged()
        {
            // Crear the active tag and layer list.
            styledHierarchyData.activeTags.Clear();
            styledHierarchyData.activeLayers.Clear();
            // Purge the gameObjectCache (this might need to be reconsidered if performance heavy but so far works).
            styledHierarchyData.gameObjectCache.RemoveAll(x => x == null || !EditorUtility.InstanceIDToObject(x.instanceID));
        }
    }
}