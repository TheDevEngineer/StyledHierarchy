using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUnityHierarchy
{
    [CustomEditor(typeof(CustomUnityHierarchyData))]
    public class CustomUnityHierarchyDataEditor : Editor
    {
        private CustomUnityHierarchyData customUnityHierarchyData;

        public VisualTreeAsset visualTree;

        private VisualElement iconsSubArea;
        private VisualElement headersSubArea;
        private VisualElement treeSubArea;

        private SerializedProperty componentIconsEnabledProperty;
        private SerializedProperty headersEnabledProperty;
        private SerializedProperty treeEnabledProperty;

        /// <summary>
        /// When the Scriptable Object is clicked this method is called.
        /// </summary>
        private void OnEnable()
        {
            // Gets reference to the scriptableObject.
            customUnityHierarchyData = target as CustomUnityHierarchyData;

            componentIconsEnabledProperty = serializedObject.FindProperty("componentIconsEnabled");
            headersEnabledProperty = serializedObject.FindProperty("headersEnabled");
            treeEnabledProperty = serializedObject.FindProperty("treeEnabled");

            // Return check if the scriptableObject is not found.
            if (customUnityHierarchyData == null)
            {
                return;
            }

            bool inspectorIconsGreaterThanZero = customUnityHierarchyData.inspectorIcons.Count > 0;

            // If the list is > 0 so we have data in it, return.
            if (inspectorIconsGreaterThanZero)
            {
                return;
            }

            // If it is 0 then pre-load a bunch of in Unity icons.
            List<string> iconsToLoad = new()
            {
                "d_cs Script Icon",
                "d_Transform Icon",
                "d_Camera Icon",
            };

            // Foreach icon to load, load it and cache it.
            foreach (string icontoLoad in iconsToLoad)
            {
                customUnityHierarchyData.inspectorIcons.Add(new(icontoLoad, EditorGUIUtility.IconContent(icontoLoad).image));
            }
        }

        /// <summary>
        /// Called after OnEnable, this method draws the data on the inspector.
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();

            // Clones the visual tree UXML.
            visualTree.CloneTree(root);

            // Icons
            // Registers a ChangeEvent for the icons toggle.
            root.Q<Toggle>("IconsMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(componentIconsEnabledProperty, iconsSubArea));
            root.Q<Toggle>("CompactScriptIconsToggle").RegisterCallback<ChangeEvent<bool>>(evt => RepaintHierarchy());

            // Gets reference to the sub area which will be enabled on icon toggle.
            iconsSubArea = root.Q<VisualElement>("IconsSubArea");

            // Changes the icons to display by using the inspectorIcons it cached earlier.
            var iconToDisplay1 = root.Q<VisualElement>("IconToDisplay1");
            var iconToDisplay2 = root.Q<VisualElement>("IconToDisplay2");
            var iconToDisplay3 = root.Q<VisualElement>("IconToDisplay3");
            iconToDisplay1.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_cs Script Icon").componentTexture as Texture2D);
            iconToDisplay2.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_Transform Icon").componentTexture as Texture2D);
            iconToDisplay3.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_Camera Icon").componentTexture as Texture2D);

            // Headers
            // Registers a ChangeEvent for the headers toggle.
            root.Q<Toggle>("HeadersMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(headersEnabledProperty, headersSubArea));

            // Gets reference to the sub area which will be enabled on headers toggle.
            headersSubArea = root.Q<VisualElement>("HeadersSubArea");

            // ChangeEvent for the header color.
            root.Q<ColorField>("HeaderColor").RegisterCallback<ChangeEvent<Color>>(evt => RepaintHierarchy());

            // ChangeEvent for the header prefix.
            root.Q<TextField>("HeadersTextPrefix").RegisterCallback<ChangeEvent<string>>(evt => RepaintHierarchy());

            // Tree View
            // Registers a ChangeEvent for the Tree View toggle.
            root.Q<Toggle>("TreeMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(treeEnabledProperty, treeSubArea));

            // Gets reference to the sub area which will be enabled on Tree View toggle.
            treeSubArea = root.Q<VisualElement>("TreeSubArea");

            // ChangeEvent for the tree colors.
            root.Q<ColorField>("MainBranchColor").RegisterCallback<ChangeEvent<Color>>(evt => RepaintHierarchy());
            root.Q<ColorField>("SubBranchColor").RegisterCallback<ChangeEvent<Color>>(evt => RepaintHierarchy());

            // Layers and tags
            root.Q<Toggle>("LayersMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => RepaintHierarchy());
            root.Q<Toggle>("TagsMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => RepaintHierarchy());

            // Layers and tags icons
            root.Q<VisualElement>("TextureToDisplay1").style.backgroundImage = new StyleBackground(customUnityHierarchyData.layerTexture as Texture2D);
            root.Q<VisualElement>("TextureToDisplay2").style.backgroundImage = new StyleBackground(customUnityHierarchyData.tagTexture as Texture2D);

            // Hides/Displays the sub areas based on their values.
            CheckForDisplayType(componentIconsEnabledProperty, iconsSubArea);
            CheckForDisplayType(headersEnabledProperty, headersSubArea);
            CheckForDisplayType(treeEnabledProperty, treeSubArea);

            return root;
        }

        /// <summary>
        /// RepaintHierarchy method.
        /// </summary>
        private void RepaintHierarchy()
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// When a boolean is changed, change the subarea to hidden/shown and repaintTheHierarchy.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="visualElement"></param>
        private void OnBoolChanged(SerializedProperty prop, VisualElement visualElement)
        {
            CheckForDisplayType(prop, visualElement);
            RepaintHierarchy();
        }

        /// <summary>
        /// When first loaded we check if any sub area should be hidden.
        /// </summary>
        /// <param name="prop"></param> Property which has a value of true/false.
        /// <param name="visualElement"></param> visualElement to hide (none) or display (flex).
        private void CheckForDisplayType(SerializedProperty prop, VisualElement visualElement)
        {
            visualElement.style.display = prop.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
