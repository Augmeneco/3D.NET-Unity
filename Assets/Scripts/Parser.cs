using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

namespace ThreeDNet.Client
{
    public class Parser
    {
        public static void CreateSceneFromJson(Vector2Int coords, string json)
        {
            dynamic sceneParsed = JsonConvert.DeserializeObject(json);

            ThreeDNet.Engine.World world = ThreeDNet.Engine.World.getInstance();

            GameObject scene = null;
            lock(world.Scenes)
            {
                scene = world.Scenes[coords];
            }
            if (sceneParsed.ContainsKey("childs"))
                ParseSceneObject(sceneParsed["childs"], scene, coords);
            UnityThread.executeInUpdate( () => { // UnityThread
                scene.SetActive(true);
            });
        }
        static void ParseSceneObject(dynamic childs, GameObject scene, Vector2Int coords)
        {
            foreach (dynamic child in childs)
            {
                GameObject obj = null;
                UnityThread.executeInUpdate( () => { // UnityThread
                    obj = new GameObject();
                    // if ((string)child["type"] == "cube"){
                    //     obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // }
                    obj.transform.SetParent(scene.transform);
                    obj.name = (string)child["tag"];
                }); // UnityThread
                if (child["components"].ContainsKey("transform"))
                {
                    UnityThread.executeInUpdate( () => { // UnityThread
                        try
                        {
                            dynamic trans = child["components"]["transform"];
                            obj.transform.localPosition = new Vector3((float)trans["x"], (float)trans["y"], (float)trans["z"]);
                            obj.transform.localScale = new Vector3((float)trans["scaleX"], (float)trans["scaleY"], (float)trans["scaleZ"]);
                            obj.transform.localRotation = new Quaternion((float)trans["pitch"], (float)trans["yaw"], (float)trans["roll"], 0);
                        }
                        catch (System.Exception e)
                        {
                            Utils.LogWrite(e.ToString());
                        }
                    }); // UnityThread
                }
                if (child["components"].ContainsKey("model")) 
                {
                    try
                    {
                        dynamic model = child["components"]["model"];
                        if (model.ContainsKey("uri"))
                        {
                            // ObjectLoader loader = obj.AddComponent<ObjectLoader>();
                            // string uri = Client.RestoreUri(Utils.DimToAddr(coords).ToString(), (string)model["uri"]);
                            // string resp = Client.Send(uri).Text;
                            // loader.Load(resp);
                            
                            string uri = Client.RestoreUri(Utils.DimToAddr(coords).ToString(), (string)model["uri"]);
                            string data = Client.Send(uri).Text;

                            FileReader.ObjectFile objFile = FileReader.ReadObjectFile (data);

                            UnityThread.executeInUpdate( () => {
                                MeshFilter filter = obj.AddComponent<MeshFilter> ();
                                
                                Mesh mesh = new Mesh ();

                                List<int[]> triplets = new List<int[]> ();
                                List<int> submeshes = new List<int> ();

                                for (int i = 0; i < objFile.f.Count; i += 1) {
                                    for (int j = 0; j < objFile.f [i].Count; j += 1) {
                                        triplets.Add (objFile.f [i] [j]);
                                    }
                                    submeshes.Add (objFile.f [i].Count);
                                }

                                Vector3[] vertices = new Vector3[triplets.Count];
                                Vector3[] normals = new Vector3[triplets.Count];
                                Vector2[] uvs = new Vector2[triplets.Count];

                                for (int i = 0; i < triplets.Count; i += 1) {
                                    vertices [i] = objFile.v [triplets [i] [0] - 1];
                                    normals [i] = objFile.vn [triplets [i] [2] - 1];
                                    if (triplets [i] [1] > 0)
                                        uvs [i] = objFile.vt [triplets [i] [1] - 1];
                                }

                                mesh.name = objFile.o;
                                mesh.vertices = vertices;
                                mesh.normals = normals;
                                mesh.uv = uvs;
                                mesh.subMeshCount = submeshes.Count;

                                int vertex = 0;
                                for (int i = 0; i < submeshes.Count; i += 1) {
                                    int[] triangles = new int[submeshes [i]];
                                    for (int j = 0; j < submeshes [i]; j += 1) {
                                        triangles [j] = vertex;
                                        vertex += 1;
                                    }
                                    mesh.SetTriangles (triangles, i);
                                }

                                mesh.RecalculateBounds ();
                                mesh.Optimize ();

                                filter.mesh = mesh;
                            });

                            MeshRenderer renderer = null;
                            UnityThread.executeInUpdate( () => { // UnityThread
                                renderer = obj.AddComponent<MeshRenderer> ();
                            }); // UnityThread
                            if (objFile.mtllib != null)
                            {

                                string mtlData = Client.Send(objFile.mtllib).Text;
                                Material[] materials = new Material[objFile.usemtl.Count];

                                FileReader.MaterialFile mtl = FileReader.ReadMaterialFile (mtlData);
                    
                                for (int i = 0; i < objFile.usemtl.Count; i += 1) {
                                    int index = mtl.newmtl.IndexOf (objFile.usemtl [i]);

                                    byte[] img = Client.Send(Client.RestoreUri(Utils.DimToAddr(coords).ToString(), mtl.mapKd[index])).Raw;
                                    UnityThread.executeInUpdate( () => { // UnityThread
                                        Texture2D texture = new Texture2D (1, 1);
                                        texture.LoadImage(img);

                                        materials [i] = new Material (Shader.Find ("Diffuse"));
                                        materials [i].name = mtl.newmtl [index];
                                        materials [i].mainTexture = texture;
                                    }); // UnityThread
                                }
                                UnityThread.executeInUpdate( () => { // UnityThread
                                    renderer.materials = materials;
                                });
                            }
                            else
                            {
                                UnityThread.executeInUpdate( () => { // UnityThread
                                    renderer.materials = new Material[1];
                                });
                            }
                        }
                        else if (model.ContainsKey("verticies"))
                        {

                        }
                        if (model.ContainsKey("texture_uri"))
                        {
                            byte[] img = Client.Send(Client.RestoreUri(Utils.DimToAddr(coords).ToString(), (string)model["texture_uri"])).Raw;
                            UnityThread.executeInUpdate( () => { // UnityThread
                                Material[] materials = new Material[1];

                                Texture2D texture = new Texture2D (1, 1);
                                //texture.alphaIsTransparency = true;
                                texture.LoadImage(img);
                                //ImageConversion.LoadImage(texture, img);
                                //System.IO.File.WriteAllBytes(@"/home/lanode/img.png", img);

                                materials[0] = new Material(Shader.Find("Diffuse"));
                                materials[0].name = "texture_uri";
                                materials[0].mainTexture = texture;

                                obj.GetComponent<MeshRenderer>().materials = materials; 
                            }); // UnityThread
                        }
                    }
                    catch (System.Exception e)
                    {
                        Utils.LogWrite(e.ToString());
                    }
                }
                if (child["components"].ContainsKey("light")) 
                {
                    UnityThread.executeInUpdate( () => { // UnityThread
                        try
                        {
                            Light lightComp = obj.AddComponent<Light>();
                        }
                        catch (System.Exception e)
                        {
                            Utils.LogWrite(e.ToString());
                        }
                    });
                }
            }
        }
    }
}
