using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Assets.Scripts.RGFileImport;
using Assets.Scripts.RGFileImport.RGGFXImport;

public class ModelViewer2 : MonoBehaviour
{
    [SerializeField] private string redguardPath;
    [SerializeField] private ModelViewer2_GUI gui;
    [SerializeField] private GameObject objectRoot;
    [SerializeField] private GameObject cameraRoot;
    [SerializeField] private float scrollSpeed = 5;
    

    private GameObject objectRootGenerated;
    
    void Start()
    {
        ViewerMode_Levels();
    }
    
    // Mode to for viewing full levels
    public void ViewerMode_Levels()
    {
        RG3DStore.path_to_game = redguardPath;
        RGTexStore.path_to_game = redguardPath;
        print("Using Folder " + RG3DStore.path_to_game);
        
        // Switch the GUI to level mode
        gui.UpdateUI_Levels();
    }
    
    // Mode to viewing individual Models
    public void ViewerMode_Models()
    {
        RG3DStore.path_to_game = redguardPath;
        RGTexStore.path_to_game = redguardPath;
        print("Using Folder " + RG3DStore.path_to_game);
        DirectoryInfo dirInfo = new DirectoryInfo(RG3DStore.path_to_game + "/fxart");
        
        // Switch the GUI to model mode
        gui.UpdateUI_Models(dirInfo.GetFiles("*.3DC"));
    }
    
    // Mode to viewing textures
    public void ViewerMode_Textures()
    {
        RG3DStore.path_to_game = redguardPath;
        RGTexStore.path_to_game = redguardPath;
        print("Using Folder " + RG3DStore.path_to_game);
        
        // Switch the GUI to texture mode
        gui.UpdateUI_Textures();
    }
    
    // Spawns the terrain
    public void SetModel_wld(string name_wld, string texbsi, string name_col)
    {
        // Load the WLD data
        string filename_wld = new string(redguardPath + $"/maps/{name_wld}.WLD");
        RG2Mesh.UnityData_WLD data_WLD = RG2Mesh.WLD2Mesh(filename_wld, name_col);
        
        // Build the GameObject
        GameObject obj_wld = new GameObject();
        obj_wld.name = "Terrain";
        obj_wld.transform.SetParent(objectRootGenerated.transform);
        
        obj_wld.AddComponent<MeshFilter>().mesh = data_WLD.mesh;
        obj_wld.AddComponent<MeshRenderer>().SetMaterials(data_WLD.materials);
    }
    
    // Spawns 3D or ROB objects
    public void add3DToScene(string prefix, string name_3d, string name_pal,Vector3 position, Vector3 eulers)
    {
        RG2Mesh.UnityData_3D data_3D = RG2Mesh.f3D2Mesh(name_3d, name_pal);

        GameObject spawned = new GameObject($"{prefix}_{name_3d}");
        
        objectRootGenerated.transform.SetParent(objectRoot.transform);
        objectRootGenerated.name = "Root_Generated_" + name_3d;
        spawned.transform.SetParent(objectRootGenerated.transform);
        
        MeshRenderer meshRenderer = spawned.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = spawned.AddComponent<MeshFilter>();

        meshFilter.mesh = data_3D.mesh;
        meshRenderer.SetMaterials(data_3D.materials);

        spawned.transform.position = position;
        spawned.transform.Rotate(eulers);
 
    }
    void LoadRGM(string filename, string name_col)
    {
        List<RGRGMStore.RGRGMData> RGM_MPSOs = RGRGMStore.LoadMPSO(filename);
        List<RGRGMStore.RGRGMData> RGM_MPOBs = RGRGMStore.LoadMPOB(filename);
        for(int i=0;i<RGM_MPOBs.Count;i++)
        {
            try
            {
                add3DToScene($"B{i:D3}", RGM_MPOBs[i].name2, name_col, RGM_MPOBs[i].position, RGM_MPOBs[i].rotation);
            }
            catch(Exception ex)
            {
                Debug.LogWarning($"ERR: B{i:D3}: {ex.Message}");
            }
        }
        for(int i=0;i<RGM_MPSOs.Count;i++)
        {
            add3DToScene($"S{i:D3}", RGM_MPSOs[i].name, name_col, RGM_MPSOs[i].position, RGM_MPSOs[i].rotation);
        }
    }

    public void Load3DC(string filename)
    {
        Destroy(objectRootGenerated);
        
        objectRootGenerated = new GameObject();
        
        RG3DStore.LoadMeshIntermediate3DC(filename);
        add3DToScene(filename +"_", filename, "OBSERVAT", Vector3.zero, Vector3.zero);

        FrameObject();
    }

    private void FrameObject()
    {
        var bounds = GetMaxBounds(objectRootGenerated);

        // Move camera root to the center
        cameraRoot.transform.position = bounds.center;

        // Set camera distance
        float distance = bounds.size.magnitude;
        cameraRoot.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, distance);
    }
    
    
    // Get bounding box of all spawned objects combined
    Bounds GetMaxBounds(GameObject g) {
        var renderers = g.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(g.transform.position, Vector3.zero);
        var b = renderers[0].bounds;
        foreach (Renderer r in renderers) {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
    
    // Stupid Hardcoded ROB Loading functions
    public void LoadROB(string filename)
    {
        // Destroy last objects
        Destroy(objectRootGenerated);
        
        objectRootGenerated = new GameObject();
        
        // ROBs without RGM currently dont work:
        // INVENTRY
        // MENU
        // PALATEST
        // TEMPTEST
        
        switch (filename)
        {
            default:
                break;

            case "BELLTOWR":
                RG3DStore.LoadMeshIntermediatesROB("BELLTOWR");
                LoadRGM(redguardPath + "/maps/BELLTOWR.RGM", "ISLAND");
                break;

            case "BRENNANS":
                RG3DStore.LoadMeshIntermediatesROB("BRENNANS");
                LoadRGM(redguardPath + "/maps/BRENNANS.RGM", "ISLAND");
                break;

            case "CARTOGR":
                RG3DStore.LoadMeshIntermediatesROB("CARTOGR");
                LoadRGM(redguardPath + "/maps/CARTOGR.RGM", "ISLAND");
                break;

            case "CATACOMB":
                RG3DStore.LoadMeshIntermediatesROB("CATACOMB");
                LoadRGM(redguardPath + "/maps/CATACOMB.RGM", "ISLAND");
                break;

            case "CAVERNS":
                RG3DStore.LoadMeshIntermediatesROB("CAVERNS");
                LoadRGM(redguardPath + "/maps/CAVERNS.RGM", "ISLAND");
                break;

            case "DRINT":
                RG3DStore.LoadMeshIntermediatesROB("DRINT");
                LoadRGM(redguardPath + "/maps/DRINT.RGM", "ISLAND");
                break;

            case "EXTPALAC":
                RG3DStore.LoadMeshIntermediatesROB("EXTPALAC");
                SetModel_wld("ISLAND", "302", "ISLAND");
                LoadRGM(redguardPath + "/maps/EXTPALAC.RGM", "ISLAND");
                break;

            case "GERRICKS":
                RG3DStore.LoadMeshIntermediatesROB("GERRICKS");
                LoadRGM(redguardPath + "/maps/GERRICKS.RGM", "ISLAND");
                break;

            case "HARBOTWR":
                RG3DStore.LoadMeshIntermediatesROB("HARBTOWR");
                LoadRGM(redguardPath + "/maps/HARBTOWR.RGM", "ISLAND");
                break;

            case "HIDEINT":
                RG3DStore.LoadMeshIntermediatesROB("HIDEINT");
                LoadRGM(redguardPath + "/maps/HIDEINT.RGM", "ISLAND");
                break;

            case "HIDEOUT":
                RG3DStore.LoadMeshIntermediatesROB("HIDEOUT");
                SetModel_wld("HIDEOUT", "302", "HIDEOUT");
                LoadRGM(redguardPath + "/maps/HIDEOUT.RGM", "ISLAND");
                break;

            case "INVENTRY":
                // RG3DStore.LoadMeshIntermediatesROB("INVENTRY");
                // LoadRGM(redguardPath + "/maps/INVENTRY.RGM", "ISLAND");
                break;

            case "ISLAND":
                RG3DStore.LoadMeshIntermediatesROB("ISLAND");
                SetModel_wld("ISLAND", "302", "ISLAND");
                LoadRGM(redguardPath + "/maps/ISLAND.RGM", "ISLAND");
                break;

            case "JAILINT":
                RG3DStore.LoadMeshIntermediatesROB("JAILINT");
                LoadRGM(redguardPath + "/maps/JAILINT.RGM", "ISLAND");
                break;

            case "JFFERS":
                RG3DStore.LoadMeshIntermediatesROB("JFFERS");
                LoadRGM(redguardPath + "/maps/JFFERS.RGM", "ISLAND");
                break;

            case "MENU":
                // RG3DStore.LoadMeshIntermediatesROB("MENU");
                // LoadRGM(redguardPath + "/maps/MENU.RGM", "ISLAND");
                break;

            case "MGUILD":
                RG3DStore.LoadMeshIntermediatesROB("MGUILD");
                LoadRGM(redguardPath + "/maps/MGUILD.RGM", "ISLAND");
                break;

            case "NECRISLE":
                RG3DStore.LoadMeshIntermediatesROB("NECRISLE");
                SetModel_wld("NECRISLE", "302", "NECRO");
                LoadRGM(redguardPath + "/maps/NECRISLE.RGM", "NECRO");
                break;

            case "NECRTOWR":
                RG3DStore.LoadMeshIntermediatesROB("NECRTOWR");
                LoadRGM(redguardPath + "/maps/NECRTOWR.RGM", "NECRO");
                break;

            case "OBSERVE":
                RG3DStore.LoadMeshIntermediatesROB("OBSERVE");
                LoadRGM(redguardPath + "/maps/OBSERVE.RGM", "ISLAND");
                break;

            case "PALACE":
                RG3DStore.LoadMeshIntermediatesROB("PALACE");
                LoadRGM(redguardPath + "/maps/PALACE.RGM", "ISLAND");
                break;

            case "PALATEST":
                // RG3DStore.LoadMeshIntermediatesROB("PALATEST");
                // LoadRGM(redguardPath + "/maps/PALATEST.RGM", "ISLAND");
                break;

            case "ROLLOS":
                RG3DStore.LoadMeshIntermediatesROB("ROLLOS");
                LoadRGM(redguardPath + "/maps/ROLLOS.RGM", "ISLAND");
                break;

            case "SILVER1":
                RG3DStore.LoadMeshIntermediatesROB("SILVER1");
                LoadRGM(redguardPath + "/maps/SILVER1.RGM", "ISLAND");
                break;

            case "SILVER2":
                RG3DStore.LoadMeshIntermediatesROB("SILVER2");
                LoadRGM(redguardPath + "/maps/SILVER2.RGM", "ISLAND");
                break;

            case "SMDEN":
                RG3DStore.LoadMeshIntermediatesROB("SMDEN");
                LoadRGM(redguardPath + "/maps/SMDEN.RGM", "ISLAND");
                break;

            case "START":
                RG3DStore.LoadMeshIntermediatesROB("START");
                LoadRGM(redguardPath + "/maps/START.RGM", "ISLAND");
                break;

            case "TAVERN":
                RG3DStore.LoadMeshIntermediatesROB("TAVERN");
                LoadRGM(redguardPath + "/maps/TAVERN.RGM", "ISLAND");
                break;

            case "TEMPLE":
                RG3DStore.LoadMeshIntermediatesROB("TEMPLE");
                LoadRGM(redguardPath + "/maps/TEMPLE.RGM", "ISLAND");
                break;

            case "TEMPTEST":
                // RG3DStore.LoadMeshIntermediatesROB("TEMPTEST");
                // LoadRGM(redguardPath + "/maps/TEMPTEST.RGM", "ISLAND");
                break;

            case "VILE":
                RG3DStore.LoadMeshIntermediatesROB("VILE");
                LoadRGM(redguardPath + "/maps/VILE.RGM", "ISLAND");
                break;
        }
        
        FrameObject();
    }
    
    void OnGUI()
    {
        GameObject _camera = cameraRoot.transform.GetChild(0).gameObject;
            
        // Calculate Zoom value
        Vector3 pos = _camera.transform.localPosition;
        float multiplier = _camera.transform.localPosition.z/100 * scrollSpeed *-1;
        pos.z += Input.mouseScrollDelta.y * multiplier;
        
        // Clamp the zoom range
        pos.z = Mathf.Clamp(pos.z, 1, 2000);
        
        // Move the camera by the new position
        _camera.transform.localPosition = pos;
    }
}
