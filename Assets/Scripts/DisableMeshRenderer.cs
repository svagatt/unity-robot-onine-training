using UnityEngine;

public class DisableMeshRenderer : MonoBehaviour
{
    public GameObject tool;
    public bool enableMesh = false;
    private void Start() {
        Renderer[] meshes = tool.GetComponentsInChildren<Renderer>(true);
        foreach(Renderer mesh in meshes){
            if(enableMesh == true) {
                mesh.enabled = true;
            } else {
                mesh.enabled = false;
            }
        }
    }

}
