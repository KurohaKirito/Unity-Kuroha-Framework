using System.IO;
using Kuroha.Framework.Utility.Editor;
using Kuroha.Framework.Utility.RunTime;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Kuroha.Tool.AssetSearchTool.Editor.Data
{
    public enum AssetType
    {
        Prefab,
        Model,
        Texture,
        Material,
        ScriptableObject,
        Sprite,
        Atlas,
        Txt,
        Script,
        AnimationClip,
        AnimatorController,
        Shader,
        Scene
    }

    public class AssetData : ThreadPoolUtil.ITask
    {
        /// <summary>
        /// 资源的相对路径
        /// </summary>
        public readonly string relativePath;

        /// <summary>
        /// 资源的绝对路径
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// 资源的文件内容
        /// </summary>
        public string fileContentText;

        /// <summary>
        /// 资源的初始化标志
        /// </summary>
        private bool isInit;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="filePath"></param>
        public AssetData(string relativePath, string filePath)
        {
            this.relativePath = relativePath;
            this.filePath = filePath;
        }

        /// <summary>
        /// 读取资源的数据内容
        /// </summary>
        public void Execute()
        {
            if (isInit == false)
            {
                fileContentText = File.ReadAllText(filePath);
                isInit = true;
            }
        }

        /// <summary>
        /// 获得 Unity 资源的类型
        /// </summary>
        /// <param name="asset">资源</param>
        /// <param name="path">资源的路径</param>
        /// <returns></returns>
        public static AssetType GetAssetType(UnityEngine.Object asset, string path)
        {
            switch (asset)
            {
                case GameObject _:
                    return path.EndsWith(".FBX") ? AssetType.Model : AssetType.Prefab;

                case Texture _:
                    return AssetType.Texture;

                case Material _:
                    return AssetType.Material;

                case ScriptableObject _:
                    return AssetType.ScriptableObject;

                case Sprite _:
                    return AssetType.Sprite;

                case SpriteAtlas _:
                    return AssetType.Atlas;

                case TextAsset _:
                    return path.EndsWith(".cs") ? AssetType.Script : AssetType.Txt;

                case AnimationClip _:
                    return AssetType.AnimationClip;

                case RuntimeAnimatorController _:
                    return AssetType.AnimatorController;

                case Shader _:
                    return AssetType.Shader;

                case SceneAsset _:
                    return AssetType.Scene;

                default:
                    return AssetType.Prefab;
            }
        }

        /// <summary>
        /// 任务是否完成
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            return isInit;
        }
    }
}