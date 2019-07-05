
using UnityEngine;
using UnityEditor;
public class TestDropFlat : EditorWindow
{
    private static int xResolution;  //store the x axis (width) of terrain
    private static int zResolution;
    static float[,] heightMapBackup;
    public static GameObject objectToInstantiate;
    private static Vector3 lastPlaced;
    private static Vector3 hP;
    public static Vector3 placePos;  // The place to put the obj
    public static Quaternion placeRot;
    private static float zOff = 0.5f;
    private static float xOff = 0.5f;
    private static float yOff = 0.5f;
    public static Bounds bounds;
    private static Vector3 flat = new Vector3(0.0f, 1.0f, 0.0f);
    private static Collider col;
    private static bool isLastPlaced;
    private static Vector3 currObjPlace;

    [MenuItem("Tools/TestDropFlat")]
    
    private static void OnEnable()  //called every time window is opened static is used because this is in an editor window
    {
        GetWindow<TestDropFlat>().Show();
        xResolution = Terrain.activeTerrain.terrainData.heightmapWidth;  //get the width of the current terrain
        zResolution = Terrain.activeTerrain.terrainData.heightmapHeight;  //get the height (z value) of the current terrain
        //this gets the heights (y value) of all the terrain points starting from 0, 0 to the full width and height of the terrain
        heightMapBackup = Terrain.activeTerrain.terrainData.GetHeights(0, 0, xResolution, zResolution);
        //SceneView.onSceneGUIDelegate -= OnSceneGUI;  //2018 and older code
        //SceneView.onSceneGUIDelegate += OnSceneGUI;
        var ver = Application.unityVersion.Substring(2, 2);
        var verNum = int.Parse(ver);
        if (verNum > 18)
        {
            Debug.Log("code for Unity version 2019 is commented out ");
        }
        SceneView.duringSceneGui += OnSceneGUI;
        //Debug.Log("OnEnable");
    }

    private void OnDisable()  //will not be called IF STATIC!
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        //SceneView.onSceneGUIDelegate -= OnSceneGUI;
        //SceneView.onSceneGUIDelegate = null;
        Event.current = null;
        //place = false;
        //Debug.Log("Disable");
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()  //what you see
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        objectToInstantiate = EditorGUILayout.ObjectField("Prefab from Project", objectToInstantiate, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.yellow;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Undo ALL Flatten terrain"))
        {
            RestoreHeight();
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = Color.red;
        EditorGUILayout.Space();
        if (GUILayout.Button("Undo last = Edit -> Undo"))
        {
            Undo.PerformUndo();
        }
        EditorGUILayout.Space();
    }

    private static void OnSceneGUI(SceneView sceneView)  //What it does 
    {
            Event e = Event.current;
            if ((e.type == EventType.MouseDown) && e.button == 1 && objectToInstantiate != null)
            {
                RaycastHit hit;
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    //Debug.Log("first raycast   " + hit.point);
                    GameObject placedObject = PrefabUtility.InstantiatePrefab(objectToInstantiate) as GameObject;
                    col = placedObject.GetComponent<Collider>();
                    if (col.GetType() != typeof(BoxCollider))
                    {
                        placedObject.AddComponent<BoxCollider>();
                    }

                    bounds = placedObject.GetComponent<BoxCollider>().bounds;
                    xOff = bounds.size.x / 2;
                    zOff = bounds.size.z / 2;
                    yOff = bounds.size.y / 2;
                    Debug.Log("bounds y   " + yOff);

                    placedObject.transform.position = new Vector3(hit.point.x, hit.point.y + 20, hit.point.z);
                    Vector3 sendPoint = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    //Transform trns = placedObject.transform;

                    raiselowerTerrainArea(sendPoint, 1, 1, 0, 2);

                    Undo.RegisterCreatedObjectUndo(placedObject, "Undo placedObject");
                    Selection.activeGameObject = placedObject;  //keeps the placed object selected                    
                    
                    placedObject.transform.position = new Vector3(hit.point.x, hit.point.y + 20, hit.point.z);                
                    ObjectRayCast(placedObject);
                    placedObject.transform.rotation = placedObject.transform.rotation;
                }
            } //end mouseclick     
    }

    void RestoreHeight()
    {
        Terrain.activeTerrain.terrainData.SetHeights(0, 0, heightMapBackup);  //reset terrain heights
        //Terrain.activeTerrain.terrainData.SetAlphamaps(0, 0, alphaMapBackup);
    }
    
    private static void ObjectRayCast(GameObject go)
    {
        RaycastHit hit1;  //be sure center is over a part of the flat spot

        if(Physics.Raycast(go.transform.position, -go.transform.up, out hit1))
        {
            if(hit1.normal != flat)
            {
                go.transform.position = new Vector3(go.transform.position.x + xOff, go.transform.position.y + 20, go.transform.position.z + zOff);
            }
        }

        RaycastHit hit2; //cast from -x edge       
        var nextSend = new Vector3(go.transform.position.x - xOff, go.transform.position.y +20, go.transform.position.z);

        if (Physics.Raycast(nextSend, -go.transform.up, out hit2))
        {
                if (Mathf.Abs(hit2.normal.x) > 0.09f)
                {
                    go.transform.position = new Vector3(hit2.point.x + xOff + 0.7f, hit2.point.y + 20, hit2.point.z);
                    Debug.Log(".7 added to x  " );
                    //Debug.Log("cur pos =   " + go.transform.position);
                }
        }

        RaycastHit hit4;  //cast from + z edge
        var nextSend2 = new Vector3(go.transform.position.x, go.transform.position.y, go.transform.position.z + zOff); //ray from +z of object

        if (Physics.Raycast(nextSend2, -go.transform.up, out hit4))
        {
            if (Mathf.Abs(hit4.normal.z) > 0.09f)
            {
                go.transform.position = new Vector3(hit4.point.x, hit4.point.y + 20, hit4.point.z - zOff - .4f); //move it -z                
                Debug.Log("z moved -zoff - .3 ");
            }
        }

        RaycastHit hitx2;  //cast from + x edge
        var nextSendx2 = new Vector3(go.transform.position.x + xOff, go.transform.position.y, go.transform.position.z);

        if (Physics.Raycast(nextSendx2, -go.transform.up, out hitx2))
        {
            if (Mathf.Abs(hitx2.normal.x) > 0.09f) // && moved == false)
            {
                go.transform.position = new Vector3(hitx2.point.x - xOff- 0.3f, hitx2.point.y + 20, hitx2.point.z);
                Debug.Log(".3 - to x   ");
                Debug.Log("cur pos =   " + go.transform.position);
            }
        }

        RaycastHit hitz2;  //Cast from - Z edge
        var nextSendz2 = new Vector3(go.transform.position.x, go.transform.position.y, go.transform.position.z - zOff);

        if (Physics.Raycast(nextSendz2, -go.transform.up, out hitz2))
        {
            if (Mathf.Abs(hitz2.normal.z) > 0.09f) // && moved == false)
            {
                go.transform.position = new Vector3(hitz2.point.x, hitz2.point.y + 20, hitz2.point.z + zOff + .4f);
                Debug.Log("+ .3  to z   ");
            }
        }

        RaycastHit hit5;
        if (Physics.Raycast(go.transform.position, -go.transform.up, out hit5))
        {
            go.transform.position = new Vector3(hit5.point.x, hit5.point.y + yOff, hit5.point.z);
            Debug.Log("final object raycast  " + hit5.point + hit5.normal + yOff);
            Debug.Log("bounds y   " + yOff);
        }

    }

    private static void raiselowerTerrainArea(Vector3 point, int lenx, int lenz, int smooth, float incdec)
    {
        //Modified From http://answers.unity3d.com/questions/420634/how-do-you-dynamically-alter-terrain.html        
        incdec *= 0.0001f; //NOTE: Maximum possible terrain height is between 0-1 so needs to be small number to raise/lower  
        
        int areax;
        int areaz;
        smooth += 1;
        //https://answers.unity.com/questions/9248/how-to-translate-world-coordinates-to-terrain-coor.html
        float relativeHitTerX = point.x / Terrain.activeTerrain.terrainData.size.x;
        float relativeHitTerZ = point.z / Terrain.activeTerrain.terrainData.size.z;

        float relativeTerCoordX = Terrain.activeTerrain.terrainData.heightmapWidth * relativeHitTerX;
        float relativeTerCoordZ = Terrain.activeTerrain.terrainData.heightmapHeight * relativeHitTerZ;
        
        float smoothing;
        // now you have the relative point of your terrain, but need to round down (floor) this because the terrain points are integers
        int terX = Mathf.FloorToInt(relativeTerCoordX);
        int terZ = Mathf.FloorToInt(relativeTerCoordZ);
        
        lenx += smooth;
        lenz += smooth;        
        
        if (terX < 1) terX = 1;
        if (terX > xResolution) terX = xResolution;
        if (terZ < 1) terZ = 1;
        if (terZ > zResolution) terZ = zResolution;

        float[,] heights = Terrain.activeTerrain.terrainData.GetHeights(terX, terZ, lenx, lenz);
        float y = heights[lenx / 2, lenz / 2];
        y += incdec;  //the heights of lenx and z will be changed by y

        for (smoothing = 1; smoothing < (smooth + 1); smoothing++)
        {
            float multiplier = smoothing / smooth;
            for (areax = (int)(smoothing / 2); areax < lenx - (smoothing / 2); areax++)
            {
                for (areaz = (int)(smoothing / 2); areaz < lenz - (smoothing / 2); areaz++)
                {
                    if ((areax > -1) && (areaz > -1) && (areax < xResolution) && (areaz < zResolution))
                    {
                        heights[areax, areaz] = Mathf.Clamp(y * multiplier, 0, 1);
                    }
                }
            }
        }
        Terrain.activeTerrain.terrainData.SetHeights(terX, terZ, heights);
        
    }  //end raiselowerterrain

    //http://blog.almostlogical.com/2010/06/10/real-time-terrain-deformation-in-unity3d/   

}
