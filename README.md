<h1>StyledHierarchy</h1>
<h2>What's the project?</h2>
<p>This is an importable custom Unity package (written in c#), that changes the look of the Unity hierarchy.&nbsp;</p>
<h3>Feature list:</h3>
<ul><li>Component display, (with compact script(s) if multiple script(s) are present),</li><li>Tree view, (my favourite feature,) this draws branches with a main and sub-colour which are both colour customisable.</li><li>GameObject header, a priority based list of colours and prefixes to draw as a background colour for a GameObject.</li><li>Layers & Tag text display which just writes what layer and tag are on each GameObject.</li></ul>
<h2>Installation guide:</h2>
<ol><li>Open the Unity project you want to import the package into.</li></ol>
<h4>Import via git URL (requires Git):</h4>
<ol><li>Click Window &gt; Package Manager &gt; + Icon.<br>
</li><li>Add Package from git URL.</li><li>Copy and paste this link:</li><li>https://github.com/TheDevEngineer/StyledHierarchy.git</li></ol>
<h4>Import via download:</h4>
<ol><li>Download the Unity package.<br>
</li><li>Right click in your Project View.
</li><li>Click "Import Package" &gt; "Custom Package".<br>
</li><li>Double click the Unity Package in it's downloaded folder.</li></ol>
<h2>Why create it?</h2>
<p>This was an educational project which taught me how to first use Unity editor scripting techniques. The project makes the Unity hierarchy look less bland and also helps with coding new projects, for example, if you are using "Component.CompareTag("enemy")" and notice that on the enemy GameObject it states "Enemy" it will fail the string check. This project helps in other ways too by seeing what components are attached to all GameObjects.</p>
<h2>Learning outcomes:</h2>
<ul><li>Unity Editor scripting with use of custom methods such as:<ul><li>InitializeOnLoad,</li><li>EditorApplication.hierarchyWindowItemOnGUI,</li><li>GUIStyle,</li><li>and more.</li></ul></li><li>UXML visual tree style sheets&nbsp;(allows changing the inspector to hide and display custom values with the Unity UI Toolkit).</li><li>Enhanced ScriptableObject usage by learning to use&nbsp;classes to store/cache multiple data to prevent repeated checking which in turn saves&nbsp;resources..</li></ul>
<h2>Source code&nbsp;& Bug reporting:</h2>
<p>You can find the source code here: https://github.com/IAmAGameDev/StyledHierarchy, along with bug reports which can be reported at the same link.</p>

