using System.Collections.Generic;
using UnityEngine;

namespace UPRTools.Editor
{
    public static class TreeElementUtility
    {
        public static void TreeToList<T>(T root, IList<T> result) where T : TreeElement
        {
            if (result == null)
            {
                result = new List<T>();
            }
            
            result.Clear();

            var stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                result.Add(current);

                if (current.children != null && current.children.Count > 0)
                {
                    foreach (var child in current.children)
                    {
                        stack.Push(child as T);
                    }
                }
            }
        }
        
        public static T ListToTree<T>(IList<T> list) where T : TreeElement
        {
            ValidateDepthValues(list);

            // Clear old states
            foreach (var element in list)
            {
                element.parent = null;
                element.children = null;
            }

            // Set child and parent references using depth info
            for (var parentIndex = 0; parentIndex < list.Count; parentIndex++)
            {
                var parent = list[parentIndex];
                var alreadyHasValidChildren = parent.children != null;
                if (alreadyHasValidChildren)
                {
                    continue;
                }

                var parentDepth = parent.depth;
                var childCount = 0;

                // Count children based depth value, we are looking at children until it's the same depth as this object
                for (var i = parentIndex + 1; i < list.Count; i++)
                {
                    if (list[i].depth == parentDepth + 1)
                    {
                        childCount++;
                    }

                    if (list[i].depth <= parentDepth)
                    {
                        break;
                    }
                }

                // Fill child array
                List<TreeElement> childList = null;
                if (childCount != 0)
                {
                    // Allocate once
                    childList = new List<TreeElement>(childCount);
                    childCount = 0;
                    for (var i = parentIndex + 1; i < list.Count; i++)
                    {
                        if (list[i].depth == parentDepth + 1)
                        {
                            list[i].parent = parent;
                            childList.Add(list[i]);
                            childCount++;
                        }

                        if (list[i].depth <= parentDepth)
                        {
                            break;
                        }
                    }
                }

                parent.children = childList;
            }

            return list[0];
        }

        private static void ValidateDepthValues<T>(IList<T> list) where T : TreeElement
        {
            if (list.Count == 0)
            {
                Debug.LogError("list should have items, count is 0, check before calling ValidateDepthValues");
            }

            if (list[0].depth != -1)
            {
                Debug.LogError($"list item at index 0 should have a depth of -1. Depth is: {list[0].depth}");
            }

            for (var i = 0; i < list.Count - 1; i++)
            {
                var depth = list[i].depth;
                var nextDepth = list[i + 1].depth;
                if (nextDepth > depth && nextDepth - depth > 1)
                {
                    Debug.LogError($"Invalid depth info in input list. Depth cannot increase more than 1 per row. Index {i} has depth {depth} while index {i + 1} has depth {nextDepth}");
                }
            }

            for (var i = 1; i < list.Count; ++i)
            {
                if (list[i].depth < 0)
                {
                    Debug.LogError($"Invalid depth value for item at index {i}");
                }
            }

            if (list.Count > 1 && list[1].depth != 0)
            {
                Debug.LogError("Input list item at index 1 is assumed to have a depth of 0");
            }
        }
        
        public static void UpdateDepthValues<T>(T root) where T : TreeElement
        {
            if (root == null)
            {
                Debug.LogError("The root is null");
            }
            else if (!root.HasChildren)
            {
                return;
            }

            var stack = new Stack<TreeElement>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.children != null)
                {
                    foreach (var child in current.children)
                    {
                        child.depth = current.depth + 1;
                        stack.Push(child);
                    }
                }
            }
        }

        private static bool IsChildOf<T>(T child, in IList<T> elements) where T : TreeElement
        {
            while (child != null)
            {
                child = child.parent as T;
                if (elements.Contains(child))
                {
                    return true;
                }
            }

            return false;
        }

        public static IList<T> FindCommonAncestorsWithinList<T>(IList<T> elements) where T : TreeElement
        {
            if (elements.Count == 1)
            {
                return new List<T>(elements);
            }

            var result = new List<T>(elements);
            result.RemoveAll(g => IsChildOf(g, elements));
            return result;
        }
    }
}