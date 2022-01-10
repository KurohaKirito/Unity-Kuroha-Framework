using System.Collections.Generic;
using System.IO;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.ItemListView;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.LODBatchTool {
    public class GunSetLOD {
        private static void SetLOD(CheckItemInfo itemData, ref List<EffectCheckReportInfo> reportInfos) {
            var fullPath = Application.dataPath + "/" + itemData.path;
            var direction = new DirectoryInfo(fullPath);
            var files = direction.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files) {
                if (file.Name.EndsWith(".meta")) {
                    continue;
                }

                var assetPath = $"Assets/{itemData.path}/{file.Name}";
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                var lodGroup = asset.GetComponent<LODGroup>();
                if (lodGroup == null || lodGroup.lodCount != 2) {
                    Debug.LogError(assetPath);
                    continue;
                }

                var control = asset.GetComponent<WeaponEquipControl>();
                if (control == null) {
                    Debug.LogError("缺少 WeaponEquipControl ");
                    continue;
                }

                var lods = lodGroup.GetLODs();
                var renders = asset.GetComponentsInChildren<Renderer>(true);
                var lod1List = new List<Renderer>();
                foreach (var render in renders) {
                    //var meshFilter = renders[j].GetComponent<MeshFilter>();
                    var particle = render.GetComponent<ParticleSystem>();
                    if (particle != null) {
                        continue;
                    }

                    var transformName = render.transform.name;

                    switch (control.WeaponID) {
                        //枪管，枪托
                        case WeaponEnum.S686:
                            if (transformName.Contains("Def_Stock") || transformName == "Def_Barrel") {
                                lod1List.Add(render);
                            }

                            break;
                        //RPG
                        case WeaponEnum.KSG:
                        case WeaponEnum.P18C:
                        case WeaponEnum.P92:
                        case WeaponEnum.P1911:
                            if (transformName.Contains("_LOD_1") || transformName == "Def_Slide") {
                                lod1List.Add(render);
                            }

                            break;
                        //RPG
                        case WeaponEnum.RPG:
                            if (transformName.Contains("_LOD_1") || transformName == "Projectile") {
                                lod1List.Add(render);
                            }

                            break;
                        //主枪体,枪托
                        case WeaponEnum.MK14:
                        case WeaponEnum.S12K:
                        case WeaponEnum.TommyGun:
                        case WeaponEnum.Vector:
                            if (transformName.Contains("_LOD_1") || transformName == "Def_Stock") {
                                lod1List.Add(render);
                            }

                            break;
                        //主枪体
                        case WeaponEnum.S1897:
                        case WeaponEnum.M24:
                        case WeaponEnum.Kar98:
                        case WeaponEnum.AWM:
                        case WeaponEnum.Bow:
                        case WeaponEnum.MicroUZI:
                        case WeaponEnum.P90:
                            if (transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                        //弹夹，枪托，主枪体
                        case WeaponEnum.M416:
                        case WeaponEnum.AK12:
                            if (transformName == "Def_Magazine" || transformName == "Def_Stock" || transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                        //弹夹，枪管，主枪体
                        case WeaponEnum.DP28:
                            if (transformName == "Def_Magazine" || transformName == "Def_Barrel" || transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                        //弹夹，主枪体
                        case WeaponEnum.VSS:
                        case WeaponEnum.SLR:
                        case WeaponEnum.SKS:
                        case WeaponEnum.Mini14:
                        case WeaponEnum.M249:
                        case WeaponEnum.UMP9:
                        case WeaponEnum.AKM:
                        case WeaponEnum.AUG:
                        case WeaponEnum.Groza:
                        case WeaponEnum.QBZ:
                        case WeaponEnum.SCARL:
                        case WeaponEnum.R1895:
                            if (transformName == "Def_Magazine" || transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                        //枪托，主枪体，枪管
                        case WeaponEnum.M16A4:
                        case WeaponEnum.MiniGun:
                        case WeaponEnum.SignalGun:
                            if (transformName == "Def_Stock" || transformName == "Def_Barrel" || transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                        case WeaponEnum.ParticleCannon:
                            if (transformName == "Def_Magazine" || transformName.Contains("axis") || transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                        case WeaponEnum.RailGun:
                            if (transformName == "Def_Magazine" || transformName == "Def_UpBarrel" || transformName == "Def_DownBarrel" || transformName.Contains("_LOD_1")) {
                                lod1List.Add(render);
                            }

                            break;
                    }
                }

                if (lod1List.Count == 0) {
                    continue;
                }

                var defCull = 0.02f;
                var lod1Vale = 0.1f;

                switch (control.WeaponID) {
                    case WeaponEnum.MicroUZI:
                        defCull = 0.01f;
                        break;
                    case WeaponEnum.Bow:
                        lod1Vale = 0.05f;
                        break;
                }

                lods[0] = new LOD(lod1Vale, renders);
                lods[1] = new LOD(defCull, lod1List.ToArray());
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
        }
    }
}