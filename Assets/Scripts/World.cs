using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeDNet.Client;
using System.Linq;

namespace ThreeDNet.Engine
{
    public class World
    {
        private static World instance;

        private World()
        {
            this.Scenes = new Dictionary<Vector2Int, GameObject>();
        }

        public static World getInstance()
        {
            if (instance == null)
                instance = new World();
            return instance;
        }

        public Dictionary<Vector2Int, GameObject> Scenes;
        Vector2Int ActiveScene;

        public void AddScene(Vector2Int coord, GameObject scene)
        {
            UnityThread.executeInUpdate( () => { // UnityThread
                scene.transform.position = new Vector3((ActiveScene.x-coord.x)*20+10, -0.5f, (ActiveScene.y-coord.y)*20+10);
                scene.SetActive(true);
            });
            this.Scenes.Add(coord, scene);
        }
        
        void CreateScene(Vector2Int coord)
        {
            GameObject scene = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scene.SetActive(false);
            scene.transform.position = new Vector3((coord.x-ActiveScene.x)*-20+10, -0.5f, (coord.y-ActiveScene.y)*-20+10);
            scene.transform.localScale = new Vector3(20, 1, 20);

            this.Scenes.Add(coord, scene);
        }

        public void DeleteFarScenes(Vector2Int coord) {
            List<Vector2Int> NearScenes = new List<Vector2Int>();
            for (int x = coord.x - 3; x <= coord.x + 3; x++)
                for (int y = coord.y - 3; y <= coord.y + 3; y++)
                    if ((x >= 0) && (x <= 65536) &&
                        (y >= 0) && (y <= 65536))
                        NearScenes.Add(new Vector2Int(x, y));
            IEnumerable<Vector2Int> FarScenes = new List<Vector2Int>(Scenes.Keys).Except(NearScenes);   
            foreach (Vector2Int scene in FarScenes) 
            {
                Scenes.Remove(scene);
            }
        }

        public void ChangeActiveScene(Vector2Int coord)
        {
            ActiveScene = coord;
            Client.Client client = Client.Client.getInstance();
            if (Scenes.ContainsKey(coord))
                Scenes[coord].transform.position = new Vector3(10, -0.5f, 10);
            else
            {
                CreateScene(coord);
                client.AddSceneToLoad(Utils.DimToAddr(coord));
            }

            DeleteFarScenes(coord);

            for (int x = coord.x - 3; x <= coord.x + 3; x++)
                for (int y = coord.y - 3; y <= coord.y + 3; y++)
                    if ((x >= 0) && (x <= 65536) &&
                        (y >= 0) && (y <= 65536))
                        if (!coord.Equals(new Vector2Int(x, y)))
                            if (Scenes.ContainsKey(new Vector2Int(x, y)))
                                Scenes[new Vector2Int(x, y)].transform.position = new Vector3((coord.x-x)*-20+10, -0.5f, (coord.y-y)*-20+10);
                            else
                            {
                                CreateScene(new Vector2Int(x, y));
                                client.AddSceneToLoad(Utils.DimToAddr(new Vector2Int(x, y)));
                            }
        }

        // void Update(){
        //     // отрисовка всех чанков
        // }
    }
}