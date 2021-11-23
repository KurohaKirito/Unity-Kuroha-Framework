using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.ItemListView;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Check {
    public static class CheckParticleSystem {
        /// <summary>
        /// ParticleSystem
        /// </summary>
        public static readonly string[] checkOptions = {
            "RenderMode", "CastShadows", "ReceiveShadows", "MeshTrisLimit", "MeshUV", "CollisionAndTrigger", "Prewarm", "SubEmittersError", "ZeroSurface"
        };

        /// <summary>
        /// 粒子系统检查类型
        /// </summary>
        public enum CheckOptions {
            RenderMode,
            CastShadows,
            ReceiveShadows,
            MeshTrisLimit,
            MeshUV,
            CollisionAndTrigger,
            Prewarm,
            SubEmittersError,
            ZeroSurface
        }

        /// <summary>
        /// 检测 RenderMode 时的子检查项
        /// </summary>
        public static readonly string[] renderModeOptions = Enum.GetNames(typeof(ParticleSystemRenderMode));

        /// <summary>
        /// 对粒子组件进行检测
        /// </summary>
        /// <param name="itemData">待检测的资源信息</param>
        /// <param name="reportInfos">检测结果</param>
        public static void Check(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos) {
            if (itemData.path.StartsWith("Assets")) {
                var assetGuids = AssetDatabase.FindAssets("t:Prefab", new[] {
                    itemData.path
                });

                for (var index = 0; index < assetGuids.Length; index++) {
                    ProgressBar.DisplayProgressBar("特效检测工具", $"Particle 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);

                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                    var pattern = itemData.assetWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(assetPath)) {
                            continue;
                        }
                    }

                    switch ((CheckOptions)itemData.checkType) {
                        case CheckOptions.SubEmittersError:
                            CheckSubEmittersError(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.RenderMode:
                            CheckRenderMode(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.Prewarm:
                            CheckPrewarm(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.CastShadows:
                            CheckCastShadows(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.ReceiveShadows:
                            CheckReceiveShadows(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.MeshTrisLimit:
                            CheckMeshTrisLimit(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.MeshUV:
                            CheckMeshUV(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.CollisionAndTrigger:
                            CheckCollisionAndTrigger(assetPath, itemData, ref reportInfos);
                            break;

                        case CheckOptions.ZeroSurface:
                            CheckZeroSurface(assetPath, itemData, ref reportInfos);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            } else {
                DebugUtil.LogError("路径必须以 Assets 开头!");
            }
        }

        /// <summary>
        /// 检测: RenderMode
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckRenderMode(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var particles = asset.GetComponentsInChildren<ParticleSystem>(true);

                // 翻译参数
                var parameter = ParticleSystemRenderMode.Billboard;
                switch (Convert.ToInt32(item.parameter)) {
                    case 0:
                        parameter = ParticleSystemRenderMode.Billboard;
                        break;
                    case 1:
                        parameter = ParticleSystemRenderMode.Stretch;
                        break;
                    case 2:
                        parameter = ParticleSystemRenderMode.HorizontalBillboard;
                        break;
                    case 3:
                        parameter = ParticleSystemRenderMode.VerticalBillboard;
                        break;
                    case 4:
                        parameter = ParticleSystemRenderMode.Mesh;
                        break;
                    case 5:
                        parameter = ParticleSystemRenderMode.None;
                        break;
                }

                foreach (var particle in particles) {
                    // 正则
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(particle.gameObject.name)) {
                            continue;
                        }
                    }

                    var renderMode = particle.GetComponent<ParticleSystemRenderer>().renderMode;
                    if (renderMode != parameter) {
                        var content = $"粒子特效 Renderer Mode 错误: {assetPath} 子物体: {particle.name} 的渲染方式: {renderMode} >>> {parameter}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleRendererMode, content, item));
                    }
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: 预热设置
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckPrewarm(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null)) {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var particles = asset.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var particle in particles) {
                // 正则
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false) {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(particle.gameObject.name)) {
                        continue;
                    }
                }

                var isOpen = particle.main.prewarm;
                if (isOpen == false) {
                    continue;
                }

                var content = $"粒子特效 Prewarm 设置错误: {assetPath} 子物体: {particle.name} Prewarm: true >>> false";
                report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticlePrewarm, content, item));
            }
        }

        /// <summary>
        /// 检测: CastShadows
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckCastShadows(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var particles = asset.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var particle in particles) {
                    // 正则
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(particle.gameObject.name)) {
                            continue;
                        }
                    }

                    var castShadows = particle.GetComponent<ParticleSystemRenderer>().shadowCastingMode;
                    if (castShadows != ShadowCastingMode.Off) {
                        var content = $"粒子应强制关闭阴影投射: {assetPath} 子物体: {particle.name} 的阴影投射应设置为: {ShadowCastingMode.Off}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleCastShadows, content, item));
                    }
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: ReceiveShadows
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckReceiveShadows(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var particles = asset.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var particle in particles) {
                    // 正则
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(particle.gameObject.name)) {
                            continue;
                        }
                    }

                    var receiveShadows = particle.GetComponent<ParticleSystemRenderer>().receiveShadows;
                    if (receiveShadows) {
                        var content = $"粒子强制关闭阴影接收: {assetPath} 子物体: {particle.name} 的阴影接收应设置为: false";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleReceiveShadows, content, item));
                    }
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: MeshTrisLimit
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMeshTrisLimit(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var particles = asset.GetComponentsInChildren<ParticleSystem>(true);
                var maxTris = Convert.ToInt32(item.parameter);

                foreach (var particle in particles) {
                    // 正则
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(particle.gameObject.name)) {
                            continue;
                        }
                    }

                    var renderer = particle.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null && renderer.mesh != null) {
                        var triangle = renderer.mesh.triangles.Length / 3;
                        if (renderer.renderMode == ParticleSystemRenderMode.Mesh && triangle > maxTris) {
                            var content = $"粒子的渲染模式为 Mesh 并且面数大于 {maxTris}, 路径: {assetPath} 子物体: {particle.name} 的网格面数超出限制! {triangle}";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleMeshTrisLimit, content, item));
                        }
                    }
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: 网格 UV 数据
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckMeshUV(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var particles = asset.GetComponentsInChildren<ParticleSystem>(true);

                foreach (var particle in particles) {
                    // 正则
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(particle.gameObject.name)) {
                            continue;
                        }
                    }

                    var renderer = particle.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null && renderer.mesh != null) {
                        var mesh = renderer.mesh;
                        var isError = false;
                        var tips = string.Empty;

                        if (mesh.uv3.Length > 0) {
                            isError = true;
                            tips += $"uv3: {mesh.uv3.Length}; ";
                        }

                        if (mesh.uv4.Length > 0) {
                            isError = true;
                            tips += $"uv4: {mesh.uv4.Length}; ";
                        }

                        if (mesh.tangents.Length > 0) {
                            isError = true;
                            tips += $"tangents: {mesh.tangents.Length}; ";
                        }

                        if (mesh.normals.Length > 0) {
                            isError = true;
                            tips += $"normals: {mesh.normals.Length}; ";
                        }

                        if (isError) {
                            tips = tips.Substring(0, tips.Length - 2);
                            var content = $"Mesh UV 错误: {assetPath} 子物件: {particle.name} 当前 Mesh: {tips} >>> 去除";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleMeshVertexData, content, item));
                        }
                    }
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: CollisionAndTrigger
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckCollisionAndTrigger(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null) == false) {
                var particles = asset.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var particle in particles) {
                    // 正则
                    var pattern = item.objectWhiteRegex;
                    if (string.IsNullOrEmpty(pattern) == false) {
                        var regex = new Regex(pattern);
                        if (regex.IsMatch(particle.gameObject.name)) {
                            continue;
                        }
                    }

                    var collision = particle.collision.enabled;
                    var trigger = particle.trigger.enabled;

                    if (collision || trigger) {
                        var content = $"特效需要关闭 Collision 和 Trigger, 路径: {assetPath} : 子物件: {particle.name}";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleCollisionAndTrigger, content, item));
                    }
                }
            } else {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
            }
        }

        /// <summary>
        /// 检测: SubEmitters
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckSubEmittersError(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null)) {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var particles = asset.GetComponentsInChildren<ParticleSystem>(true);

            // 遍历每一个粒子系统
            foreach (var particle in particles) {
                // 正则
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false) {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(particle.gameObject.name)) {
                        continue;
                    }
                }

                // 获取当前粒子系统的所有子粒子系统
                var allSubParticleSystems = particle.GetComponentsInChildren<ParticleSystem>(true);

                // 获取当前粒子系统 Sub-Emitter 的个数
                var subEmittersCount = particle.subEmitters.subEmittersCount;

                // 遍历所有的 Sub-Emitter
                for (var index = 0; index < subEmittersCount; index++) {
                    // 依次获取 Sub-Emitter 设置
                    var subEmitter = particle.subEmitters.GetSubEmitterSystem(index);

                    // 错误 1: Sub-Emitters 为空
                    if (ReferenceEquals(subEmitter, null)) {
                        var content = $"粒子特效的 Sub-Emitters 为空! : {assetPath} 子物体: {particle.name} 的第 {index + 1} 个设置为空!";
                        report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleSubEmittersError, content, item));
                    } else {
                        // 错误 2: Sub-Emitters 引用了非子物体的粒子系统
                        // 错误 3: Sub-Emitters 引用了子物体的粒子系统, 但是子物体是 '未激活' 状态的.
                        var isError = true;
                        foreach (var subParticleSystem in allSubParticleSystems) {
                            if (subEmitter == subParticleSystem && subParticleSystem.gameObject.activeSelf) {
                                isError = false;
                            }
                        }

                        if (isError) {
                            var content = $"粒子特效 Sub-Emitters 设置错误: {assetPath} 子物体: {particle.name} 的第 {index + 1} 个设置引用的 {subEmitter.name} 粒子系统不是子系统!";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleSubEmittersError, content, item));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测: ZeroSurface
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="item">检查项</param>
        /// <param name="report">检查结果</param>
        private static void CheckZeroSurface(string assetPath, CheckItemInfo item, ref List<EffectCheckReportInfo> report) {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (ReferenceEquals(asset, null)) {
                DebugUtil.Log($"未读取到资源, 路径为: {assetPath}");
                return;
            }

            var particles = asset.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var particle in particles) {
                // 正则
                var pattern = item.objectWhiteRegex;
                if (string.IsNullOrEmpty(pattern) == false) {
                    var regex = new Regex(pattern);
                    if (regex.IsMatch(particle.gameObject.name)) {
                        continue;
                    }
                }

                if (particle.shape.enabled) {
                    if (particle.shape.shapeType == ParticleSystemShapeType.Mesh) {
                        var mesh = particle.shape.mesh;
                        if (ReferenceEquals(mesh, null)) {
                            var content = $"特效的发射器类型为 Mesh, 但是没有指定 Mesh! {assetPath} 子物件 {particle.name}";
                            report.Add(EffectCheckReport.AddReportInfo(asset, assetPath, EffectCheckReportInfo.EffectCheckReportType.ParticleZeroSurface, content, item));
                        } else if (mesh.isReadable == false) {
                            // 根据 Mesh 获取到的路径不是 Mesh 的路径, 而是 Mesh 所在的模型文件的路径
                            var modelPath = AssetDatabase.GetAssetPath(mesh);
                            var content = $"特效的发射器类型为 Mesh, 但是 Mesh 没有开启读写! {assetPath} 子物件 {particle.name}, 模型路径 {modelPath}";
                            report.Add(EffectCheckReport.AddReportInfo(mesh, modelPath, EffectCheckReportInfo.EffectCheckReportType.ParticleZeroSurface, content, item));
                        }
                    }
                }
            }
        }
    }
}