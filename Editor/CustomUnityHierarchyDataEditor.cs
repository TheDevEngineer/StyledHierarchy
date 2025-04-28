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

        private Toggle iconsMainToggle;
        private Toggle headersMainToggle; 
        private Toggle treeMainToggle;
        private Toggle compactScriptIconsToggle;
        private Toggle layersEnabledToggle;
        private Toggle tagsEnabledToggle;

        private VisualElement iconsSubArea;
        private VisualElement headersSubArea;
        private VisualElement treeSubArea;
        private VisualElement iconToDisplay1;
        private VisualElement iconToDisplay2;
        private VisualElement iconToDisplay3;
        private VisualElement debugIconToDisplay1;
        private VisualElement debugIconToDisplay2;
        private VisualElement debugIconToDisplay3;

        private ColorField headerColor;
        private ColorField mainBranchColor;
        private ColorField subBranchColor;

        private TextField headersTextPrefix;

        private SerializedProperty componentIconsEnabledProperty;
        private SerializedProperty headersEnabledProperty;
        private SerializedProperty treeEnabledProperty;

        private void OnEnable()
        {
            customUnityHierarchyData = (CustomUnityHierarchyData)AssetDatabase.LoadAssetAtPath("Assets/CustomUnityHierarchyData.asset", typeof(CustomUnityHierarchyData));

            componentIconsEnabledProperty = serializedObject.FindProperty("componentIconsEnabled");
            headersEnabledProperty = serializedObject.FindProperty("headersEnabled");
            treeEnabledProperty = serializedObject.FindProperty("treeEnabled");

            if (customUnityHierarchyData == null)
            {
                return;
            }

            bool temp = customUnityHierarchyData.inspectorIcons.Count > 0;

            // If the list is > 0 so we have data in it, return.
            if (temp)
            {
                return;
            }

            // If it is 0 then pre-load a bunch of in Unity icons.
            List<string> iconsToLoad = new()
            {
                "d_cs Script Icon",
                "d_Transform Icon",
                "d_Camera Icon",
                "d_console.infoicon",
                "d_console.warnicon",
                "d_console.erroricon"
            };

            foreach (string icontoLoad in iconsToLoad)
            {
                customUnityHierarchyData.inspectorIcons.Add(new(icontoLoad, EditorGUIUtility.IconContent(icontoLoad).image));
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();

            visualTree.CloneTree(root);

            // Icons
            iconsMainToggle = root.Q<Toggle>("IconsMainToggle");
            iconsMainToggle.RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(componentIconsEnabledProperty, iconsSubArea));
            compactScriptIconsToggle = root.Q<Toggle>("CompactScriptIconsToggle");
            compactScriptIconsToggle.RegisterCallback<ChangeEvent<bool>>(evt => RepaintHierarchy());

            iconsSubArea = root.Q<VisualElement>("IconsSubArea");
            iconToDisplay1 = root.Q<VisualElement>("IconToDisplay1");
            iconToDisplay2 = root.Q<VisualElement>("IconToDisplay2");
            iconToDisplay3 = root.Q<VisualElement>("IconToDisplay3");

            iconToDisplay1.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_cs Script Icon").componentTexture as Texture2D);
            iconToDisplay2.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_Transform Icon").componentTexture as Texture2D);
            iconToDisplay3.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_Camera Icon").componentTexture as Texture2D);

            // Headers
            headersMainToggle = root.Q<Toggle>("HeadersMainToggle");
            headersMainToggle.RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(headersEnabledProperty, headersSubArea));

            headersSubArea = root.Q<VisualElement>("HeadersSubArea");

            headerColor = root.Q<ColorField>("HeaderColor");
            headerColor.RegisterCallback<ChangeEvent<Color>>(evt => RepaintHierarchy());

            headersTextPrefix = root.Q<TextField>("HeadersTextPrefix");
            headersTextPrefix.RegisterCallback<ChangeEvent<string>>(evt => RepaintHierarchy());

            // Tree
            treeMainToggle = root.Q<Toggle>("TreeMainToggle");
            treeMainToggle.RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(treeEnabledProperty, treeSubArea));

            treeSubArea = root.Q<VisualElement>("TreeSubArea");

            mainBranchColor = root.Q<ColorField>("MainBranchColor");
            mainBranchColor.RegisterCallback<ChangeEvent<Color>>(evt => RepaintHierarchy());
            subBranchColor = root.Q<ColorField>("SubBranchColor");
            subBranchColor.RegisterCallback<ChangeEvent<Color>>(evt => RepaintHierarchy());

            // Layers and tags
            layersEnabledToggle = root.Q<Toggle>("LayersMainToggle");
            layersEnabledToggle.RegisterCallback<ChangeEvent<bool>>(evt => RepaintHierarchy());
            tagsEnabledToggle = root.Q<Toggle>("TagsMainToggle");
            tagsEnabledToggle.RegisterCallback<ChangeEvent<bool>>(evt => RepaintHierarchy());

            // Debugs
            debugIconToDisplay1 = root.Q<VisualElement>("DebugIconToDisplay1");
            debugIconToDisplay2 = root.Q<VisualElement>("DebugIconToDisplay2");
            debugIconToDisplay3 = root.Q<VisualElement>("DebugIconToDisplay3");

            debugIconToDisplay1.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_console.infoicon").componentTexture as Texture2D);
            debugIconToDisplay2.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_console.warnicon").componentTexture as Texture2D);
            debugIconToDisplay3.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_console.erroricon").componentTexture as Texture2D);

            CheckForDisplayType(componentIconsEnabledProperty, iconsSubArea);
            CheckForDisplayType(headersEnabledProperty, headersSubArea);
            CheckForDisplayType(treeEnabledProperty, treeSubArea);

            return root;
        }

        private void RepaintHierarchy()
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        private void OnBoolChanged(SerializedProperty prop, VisualElement visualElement)
        {
            CheckForDisplayType(prop, visualElement);
            RepaintHierarchy();
        }

        private void CheckForDisplayType(SerializedProperty prop, VisualElement visualElement)
        {
            visualElement.style.display = prop.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}