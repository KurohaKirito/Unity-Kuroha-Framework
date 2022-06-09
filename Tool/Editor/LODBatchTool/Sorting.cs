using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using String = Script.Effect.Editor.AssetTool.Util.RunTime.StringUtil;

public class Sorting : EditorWindow {
    [MenuItem("GameObject/Scene Tool/排序选中物体 升序 %[", false, 13)]
    private static void SortHierarchyAscending() {
        var transforms = Selection.transforms;
        SortSelectedAscending(transforms, true);
    }
    
    [MenuItem("GameObject/Scene Tool/排序选中物体 降序 %]", false, 14)]
    private static void SortHierarchyDescending() {
        var transforms = Selection.transforms;
        SortSelectedAscending(transforms, false);
    }
    
    [MenuItem("GameObject/Scene Tool/排序子物体 升序 %,", false, 15)]
    private static void SortSelectedAscending() {
        var transforms = Selection.GetTransforms(SelectionMode.Deep | SelectionMode.Editable);
        SortSelectedAscending(transforms, true);
    }
    
    [MenuItem("GameObject/Scene Tool/排序子物体 降序 %.", false, 16)]
    private static void SortSelectedDescending() {
        var transforms = Selection.GetTransforms(SelectionMode.Deep | SelectionMode.Editable);
        SortSelectedAscending(transforms, false);
    }
    
    private static void SortSelectedAscending(in Transform[] transforms, bool isAsc) {
        var newIndex = GetLowestIndex(transforms);
        
        for (var i = 0; i < transforms.Length; i++) {
            for (var j = i + 1; j < transforms.Length; j++) {
                if (String.CompareByNumber(transforms[i].name, transforms[j].name, isAsc) > 0) {
                    var t = transforms[i];
                    transforms[i] = transforms[j];
                    transforms[j] = t;
                }
            }
        }
        
        foreach (var transform in transforms) {
            if (transform != transform.root) {
                transform.SetSiblingIndex(newIndex);
                newIndex++;
            }
        }

        Debug.Log(isAsc ? "排序完成 - 升序" : "排序完成 - 降序");
    }

    private static int GetLowestIndex(IEnumerable<Transform> transforms) {
        return transforms.Select(transform => transform.GetSiblingIndex()).Prepend(9999).Min();
    }
}