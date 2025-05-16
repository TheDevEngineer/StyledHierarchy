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
        public VisualTreeAsset visualTreeScrollViewItem;

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
            VisualElement iconToDisplay1 = root.Q<VisualElement>("IconToDisplay1");
            VisualElement iconToDisplay2 = root.Q<VisualElement>("IconToDisplay2");
            VisualElement iconToDisplay3 = root.Q<VisualElement>("IconToDisplay3");
            iconToDisplay1.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_cs Script Icon").componentTexture as Texture2D);
            iconToDisplay2.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_Transform Icon").componentTexture as Texture2D);
            iconToDisplay3.style.backgroundImage = new StyleBackground(customUnityHierarchyData.inspectorIcons.First(x => x.componentName == "d_Camera Icon").componentTexture as Texture2D);

            // Headers
            // Registers a ChangeEvent for the headers toggle.
            root.Q<Toggle>("HeadersMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(headersEnabledProperty, headersSubArea));

            // Gets reference to the sub area which will be enabled on headers toggle.
            headersSubArea = root.Q<VisualElement>("HeadersSubArea");

            // A button to add another element to the list.
            Button addAnotherElementToList = root.Q<Button>("AddAnotherElementToList");
            // This is a work-around to prevent a bug where click events would fire for unknown reasons.
            addAnotherElementToList.clickable.activators.Clear();
            addAnotherElementToList.RegisterCallback<MouseDownEvent>(e => AddAnotherElementToList(root));

            MakeScrollView(root);

            // Tree View
            // Registers a ChangeEvent for the Tree View toggle.
            root.Q<Toggle>("TreeMainToggle").RegisterCallback<ChangeEvent<bool>>(evt => OnBoolChanged(treeEnabledProperty, treeSubArea));

            // Gets reference to the sub area which will be enabled on Tree View toggle.
            treeSubArea = root.Q<VisualElement>("TreeSubArea");

            VisualElement mainBranchExample = root.Q<VisualElement>("MainBranchExample");
            VisualElement subBranchExample = root.Q<VisualElement>("SubBranchExample");
            // ChangeEvent for the tree colors.
            root.Q<ColorField>("MainBranchColor").RegisterCallback<ChangeEvent<Color>>(evt => UpdateBranchExamplesAndRepaint(mainBranchExample, evt.newValue));
            root.Q<ColorField>("SubBranchColor").RegisterCallback<ChangeEvent<Color>>(evt => UpdateBranchExamplesAndRepaint(subBranchExample, evt.newValue));

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
        /// Updates the scroll view of prefixs and colors to use for the headers.
        /// </summary>
        /// <param name="root"></param> 
        private void MakeScrollView(VisualElement root)
        {
            // Gets the ScrollView reference from root.
            ScrollView headerScrollView = root.Q<ScrollView>("HeadersScrollView");
            // Finds the list property and stores it.
            SerializedProperty headersPrefixAndColors = serializedObject.FindProperty("prefixAndColor");
            // Force clears the ScrollView data.
            headerScrollView.Clear();

            for (int i = 0; i < customUnityHierarchyData.prefixAndColor.Count; i++)
            {
                // Get the reference/data of the specific item in the list that I am currently working with for example i = 0 so index 0
                CustomUnityHierarchyData.PrefixAndColor prefixAndColorItemData = customUnityHierarchyData.prefixAndColor[i];
                SerializedProperty prefixAndColorItemProperty = headersPrefixAndColors.GetArrayElementAtIndex(i);

                headerScrollView.Add(ReturnNewVisualElementItem(root, prefixAndColorItemProperty, prefixAndColorItemData));
            }
        }

        private VisualElement ReturnNewVisualElementItem(VisualElement root, SerializedProperty prefixAndColorItemProperty, CustomUnityHierarchyData.PrefixAndColor prefixAndColorItemData)
        {
            // Creates a temporary new VisualElement from a VisualTreeAsset.
            VisualElement tempVisualElementItem = new();
            visualTreeScrollViewItem.CloneTree(tempVisualElementItem);

            // Set the prefix callback and binding path.
            TextField prefix = tempVisualElementItem.Q<TextField>("Prefix");
            prefix.RegisterValueChangedCallback(e => RepaintHierarchy());
            SerializedProperty headerPrefixProp = prefixAndColorItemProperty.FindPropertyRelative("headerPrefix");
            prefix.bindingPath = headerPrefixProp.propertyPath;
            prefix.value = headerPrefixProp.stringValue;

            // Set the color callback and binding path.
            ColorField color = tempVisualElementItem.Q<ColorField>("Color");
            color.RegisterValueChangedCallback(e => RepaintHierarchy());
            SerializedProperty headerColorProp = prefixAndColorItemProperty.FindPropertyRelative("headerColor");
            color.bindingPath = headerColorProp.propertyPath;
            color.value = headerColorProp.colorValue;

            // Move buttons
            Button moveUpButton = tempVisualElementItem.Q<Button>("MoveUp");
            // This is a work-around to prevent a bug where click events would fire for unknown reasons.
            moveUpButton.clickable.activators.Clear();
            moveUpButton.RegisterCallback<MouseDownEvent>(e => MoveElementInList(root, prefixAndColorItemData, tempVisualElementItem, -1));
            Button moveDownButton = tempVisualElementItem.Q<Button>("MoveDown");
            // This is a work-around to prevent a bug where click events would fire for unknown reasons.
            moveDownButton.clickable.activators.Clear();
            moveDownButton.RegisterCallback<MouseDownEvent>(e => MoveElementInList(root, prefixAndColorItemData, tempVisualElementItem, 1));

            // Delete button
            Button deleteElementFromList = tempVisualElementItem.Q<Button>("Delete");
            // This is a work-around to prevent a bug where click events would fire for unknown reasons.
            deleteElementFromList.clickable.activators.Clear();
            deleteElementFromList.RegisterCallback<MouseDownEvent>(e => DeleteElementFromList(root, prefixAndColorItemData));

            return tempVisualElementItem;
        }
        
        private void MoveElementInList(VisualElement root, CustomUnityHierarchyData.PrefixAndColor prefixAndColorItemData, VisualElement tempVisualElementItem, int upOrDown)
        {
            ScrollView scrollView = root.Q<ScrollView>("HeadersScrollView");
            int index = scrollView.IndexOf(tempVisualElementItem);
            index += upOrDown;
            if (index == -1 && upOrDown == -1 ||
                index == scrollView.childCount && upOrDown == 1)
            {
                return; // Out of bounds in the array
            }
            scrollView.Remove(tempVisualElementItem);
            scrollView.Insert(index, tempVisualElementItem);
            customUnityHierarchyData.prefixAndColor.Remove(prefixAndColorItemData);
            customUnityHierarchyData.prefixAndColor.Insert(index, prefixAndColorItemData);
            // Update the scripable object manually.
            SaveNewScriptableObjectData();
        }

        private void AddAnotherElementToList(VisualElement root)
        {
            // Add a new element to the list.
            customUnityHierarchyData.prefixAndColor.Add(new("Replace With A Prefix.", Color.green));
            // Update the scripable object manually.
            SaveNewScriptableObjectData();

            // Finds the list property and stores it.
            SerializedProperty headersPrefixAndColors = serializedObject.FindProperty("prefixAndColor");

            // Get the reference/data of the specific item in the list that I am currently working with.
            CustomUnityHierarchyData.PrefixAndColor prefixAndColorItemData = customUnityHierarchyData.prefixAndColor[customUnityHierarchyData.prefixAndColor.Count - 1];
            SerializedProperty prefixAndColorItemProperty = headersPrefixAndColors.GetArrayElementAtIndex(customUnityHierarchyData.prefixAndColor.Count - 1);

            root.Q<ScrollView>("HeadersScrollView").Add(ReturnNewVisualElementItem(root, prefixAndColorItemProperty, prefixAndColorItemData));
        }

        private void DeleteElementFromList(VisualElement root, CustomUnityHierarchyData.PrefixAndColor prefixAndColorItemData)
        {
            root.Q<ScrollView>("HeadersScrollView").RemoveAt(customUnityHierarchyData.prefixAndColor.IndexOf(prefixAndColorItemData));

            // Removes this element from the list.
            customUnityHierarchyData.prefixAndColor.Remove(prefixAndColorItemData);
            // Updates and saves the ScriptableObject.
            SaveNewScriptableObjectData();
        }

        private void SaveNewScriptableObjectData()
        {
            // Update the scripable object manually.
            serializedObject.Update();
            EditorUtility.SetDirty(customUnityHierarchyData);
            AssetDatabase.SaveAssets();
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

        /// <summary>
        /// Updates the example branches based on the current tree view colours.
        /// </summary>
        /// <param name="elementToUpdate"></param> The VisualElement to change the background image color of.
        /// <param name="newValue"></param> The new Color value to set it to.
        private void UpdateBranchExamplesAndRepaint(VisualElement elementToUpdate, Color newValue)
        {
            elementToUpdate.style.unityBackgroundImageTintColor = newValue;
            RepaintHierarchy();
        }
    }
}
