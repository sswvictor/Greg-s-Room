using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
public class FengShuiRuleEditor : EditorWindow
{
    private FengShuiRulesData targetRules;
    private Vector2 scrollPosition;
    private bool[] ruleSetFoldouts = new bool[0];
    private bool[][] directionalFoldouts = new bool[0][];

    [MenuItem("Greg's Room/Feng Shui Rule Editor")]
    public static void ShowWindow()
    {
        GetWindow<FengShuiRuleEditor>("Feng Shui Rules");
    }

    private void OnEnable()
    {
        if (targetRules != null)
        {
            InitializeFoldouts();
        }
    }

    private void InitializeFoldouts()
    {
        ruleSetFoldouts = new bool[targetRules.ruleSets.Count];
        directionalFoldouts = new bool[targetRules.ruleSets.Count][];
        
        for (int i = 0; i < targetRules.ruleSets.Count; i++)
        {
            ruleSetFoldouts[i] = false;
            directionalFoldouts[i] = new bool[targetRules.ruleSets[i].directionalRules.Count];
            
            for (int j = 0; j < directionalFoldouts[i].Length; j++)
            {
                directionalFoldouts[i][j] = false;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Feng Shui Rules Editor", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        targetRules = (FengShuiRulesData)EditorGUILayout.ObjectField(
            "Rules Data", targetRules, typeof(FengShuiRulesData), false);
        
        if (EditorGUI.EndChangeCheck() && targetRules != null)
        {
            InitializeFoldouts();
        }
        
        if (GUILayout.Button("Create New", GUILayout.Width(100)))
        {
            CreateNewRulesAsset();
        }
        EditorGUILayout.EndHorizontal();
        
        if (targetRules == null)
        {
            EditorGUILayout.HelpBox("Please select or create a FengShuiRulesData asset", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Add New Rule Set"))
        {
            AddNewRuleSet();
        }
        
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Display and edit rule sets
        for (int i = 0; i < targetRules.ruleSets.Count; i++)
        {
            DrawRuleSet(i);
        }
        
        EditorGUILayout.EndScrollView();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetRules);
        }
    }
    
    private void DrawRuleSet(int index)
    {
        FengShuiRuleSet ruleSet = targetRules.ruleSets[index];
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with delete button
        EditorGUILayout.BeginHorizontal();
        
        // Foldout for rule set
        ruleSetFoldouts[index] = EditorGUILayout.Foldout(
            ruleSetFoldouts[index], 
            $"Rule Set {index+1}: {ruleSet.objectType} in {ruleSet.roomType}", 
            true, 
            EditorStyles.foldoutHeader);
        
        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Delete Rule Set", 
                $"Are you sure you want to delete the rule set for {ruleSet.objectType} in {ruleSet.roomType}?", 
                "Yes", "No"))
            {
                targetRules.ruleSets.RemoveAt(index);
                InitializeFoldouts();
                GUIUtility.ExitGUI();
                return;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (ruleSetFoldouts[index])
        {
            EditorGUI.indentLevel++;
            
            // Rule set properties
            ruleSet.objectType = EditorGUILayout.TextField("Object Type", ruleSet.objectType);
            ruleSet.roomType = EditorGUILayout.TextField("Room Type", ruleSet.roomType);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add Direction Rule"))
            {
                AddNewDirectionalRule(ruleSet);
                
                // Resize foldout arrays
                bool[] newDirectionalFoldouts = new bool[ruleSet.directionalRules.Count];
                if (directionalFoldouts[index] != null)
                {
                    System.Array.Copy(directionalFoldouts[index], newDirectionalFoldouts, 
                        Mathf.Min(directionalFoldouts[index].Length, newDirectionalFoldouts.Length));
                }
                directionalFoldouts[index] = newDirectionalFoldouts;
            }
            
            EditorGUILayout.Space();
            
            // Display directional rules
            for (int j = 0; j < ruleSet.directionalRules.Count; j++)
            {
                DrawDirectionalRule(ruleSet, j, index);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDirectionalRule(FengShuiRuleSet ruleSet, int ruleIndex, int setIndex)
    {
        DirectionalRule dirRule = ruleSet.directionalRules[ruleIndex];
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with delete button
        EditorGUILayout.BeginHorizontal();
        
        // Foldout for directional rule
        string directionName = GetDirectionName(dirRule.mainDirection);
        directionalFoldouts[setIndex][ruleIndex] = EditorGUILayout.Foldout(
            directionalFoldouts[setIndex][ruleIndex], 
            $"Direction {ruleIndex+1}: {directionName}", 
            true);
        
        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Delete Direction Rule", 
                $"Are you sure you want to delete this direction rule?", 
                "Yes", "No"))
            {
                ruleSet.directionalRules.RemoveAt(ruleIndex);
                
                // Resize foldout array
                bool[] newDirectionalFoldouts = new bool[ruleSet.directionalRules.Count];
                if (directionalFoldouts[setIndex].Length > 0)
                {
                    System.Array.Copy(directionalFoldouts[setIndex], newDirectionalFoldouts, 
                        Mathf.Min(directionalFoldouts[setIndex].Length, newDirectionalFoldouts.Length));
                }
                directionalFoldouts[setIndex] = newDirectionalFoldouts;
                
                GUIUtility.ExitGUI();
                return;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (directionalFoldouts[setIndex][ruleIndex])
        {
            EditorGUI.indentLevel++;
            
            // Direction property
            EditorGUILayout.LabelField("Direction Vector");
            EditorGUI.indentLevel++;
            dirRule.mainDirection = EditorGUILayout.Vector3Field("", dirRule.mainDirection);
            EditorGUI.indentLevel--;
            
            // Direction presets
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Direction Presets");
            if (GUILayout.Button("North", EditorStyles.miniButtonLeft))
                dirRule.mainDirection = Vector3.forward;
            if (GUILayout.Button("East", EditorStyles.miniButtonMid))
                dirRule.mainDirection = Vector3.right;
            if (GUILayout.Button("South", EditorStyles.miniButtonMid))
                dirRule.mainDirection = Vector3.back;
            if (GUILayout.Button("West", EditorStyles.miniButtonRight))
                dirRule.mainDirection = Vector3.left;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Position rules header
            EditorGUILayout.LabelField("Position Rules", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add Position Rule"))
            {
                AddNewPositionRule(dirRule);
            }
            
            if (dirRule.positionRules.Count == 0)
            {
                EditorGUILayout.HelpBox("No position rules defined. Add some using the button above.", MessageType.Info);
            }
            
            // Draw position rules
            for (int k = 0; k < dirRule.positionRules.Count; k++)
            {
                DrawPositionRule(dirRule, k);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPositionRule(DirectionalRule dirRule, int ruleIndex)
    {
        PositionRule posRule = dirRule.positionRules[ruleIndex];
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Rule header with delete button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Rule {ruleIndex+1}: {posRule.positionType}", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            dirRule.positionRules.RemoveAt(ruleIndex);
            GUIUtility.ExitGUI();
            return;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Position type dropdown
        posRule.positionType = (PositionType)EditorGUILayout.EnumPopup("Position", posRule.positionType);
        
        // Quality and score
        EditorGUILayout.BeginHorizontal();
        posRule.zoneQuality = (ZoneQuality)EditorGUILayout.EnumPopup("Quality", posRule.zoneQuality);
        posRule.scoreModifier = EditorGUILayout.IntField("Score Modifier", posRule.scoreModifier);
        EditorGUILayout.EndHorizontal();
        
        // Feedback message
        posRule.feedbackMessage = EditorGUILayout.TextField("Feedback Message", posRule.feedbackMessage);
        
        EditorGUILayout.EndVertical();
    }
    
    private void CreateNewRulesAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Feng Shui Rules Asset",
            "FengShuiRules",
            "asset",
            "Please enter a name for the feng shui rules asset");
            
        if (string.IsNullOrEmpty(path))
            return;
            
        FengShuiRulesData rulesAsset = ScriptableObject.CreateInstance<FengShuiRulesData>();
        AssetDatabase.CreateAsset(rulesAsset, path);
        AssetDatabase.SaveAssets();
        
        targetRules = rulesAsset;
        InitializeFoldouts();
    }
    
    private void AddNewRuleSet()
    {
        FengShuiRuleSet newRuleSet = new FengShuiRuleSet();
        newRuleSet.objectType = "New_Object";
        newRuleSet.roomType = "New_Room";
        newRuleSet.directionalRules = new List<DirectionalRule>();
        targetRules.ruleSets.Add(newRuleSet);
        
        // Resize foldout arrays
        bool[] newRuleSetFoldouts = new bool[targetRules.ruleSets.Count];
        bool[][] newDirectionalFoldouts = new bool[targetRules.ruleSets.Count][];
        
        if (ruleSetFoldouts != null)
        {
            System.Array.Copy(ruleSetFoldouts, newRuleSetFoldouts, 
                Mathf.Min(ruleSetFoldouts.Length, newRuleSetFoldouts.Length));
        }
        
        for (int i = 0; i < newDirectionalFoldouts.Length; i++)
        {
            if (i < targetRules.ruleSets.Count - 1 && i < directionalFoldouts.Length && directionalFoldouts[i] != null)
            {
                newDirectionalFoldouts[i] = directionalFoldouts[i];
            }
            else
            {
                newDirectionalFoldouts[i] = new bool[0];
            }
        }
        
        ruleSetFoldouts = newRuleSetFoldouts;
        directionalFoldouts = newDirectionalFoldouts;
        
        // Set the new foldout to open
        ruleSetFoldouts[ruleSetFoldouts.Length - 1] = true;
    }
    
    private void AddNewDirectionalRule(FengShuiRuleSet ruleSet)
    {
        DirectionalRule newDir = new DirectionalRule();
        newDir.mainDirection = Vector3.forward;
        newDir.positionRules = new List<PositionRule>();
        ruleSet.directionalRules.Add(newDir);
    }
    
    private void AddNewPositionRule(DirectionalRule dirRule)
    {
        PositionRule newRule = new PositionRule(
            PositionType.Center,
            ZoneQuality.Acceptable,
            0,
            "Neutral placement"
        );
        
        dirRule.positionRules.Add(newRule);
    }
    
    private string GetDirectionName(Vector3 direction)
    {
        if (direction.normalized == Vector3.forward.normalized)
            return "North";
        if (direction.normalized == Vector3.right.normalized)
            return "East";
        if (direction.normalized == Vector3.back.normalized)
            return "South";
        if (direction.normalized == Vector3.left.normalized)
            return "West";
            
        return $"Custom ({direction.x:F1}, {direction.y:F1}, {direction.z:F1})";
    }
}
#endif 