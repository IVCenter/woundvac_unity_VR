using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Original Source: http://nihilistdev.blogspot.com/2013/05/outline-in-unity-with-mesh-transparency.html
// Shader Source: http://answers.unity3d.com/questions/60155/is-there-a-shader-to-only-add-an-outline.html
public class GrabHighlight : MonoBehaviour
{
    [SerializeField]
    private bool highlightChildren = true;

    [SerializeField]
    private Color meshColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);

    private List<Material[]> normalMaterials;
    private List<GameObject> outlineObjects;
    private List<GameObject> normalObjects;
    private bool outlineIsOn = false;

    // Use this for initialization
    void Start()
    {

        normalMaterials = new List<Material[]>();
        outlineObjects = new List<GameObject>();
        normalObjects = new List<GameObject>();

        MeshRenderer[] meshRenderers = new MeshRenderer[1];

        if (highlightChildren)
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }
        else
        {
            meshRenderers[0] = this.GetComponent<MeshRenderer>();
        }

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            normalObjects.Add(meshRenderers[i].gameObject);

            Material[] materials = meshRenderers[i].materials;

            normalMaterials.Add(new Material[materials.Length]);

            for (int j = 0; j < materials.Length; j++)
            {
                normalMaterials[i][j] = new Material(materials[j]);
            }

            GameObject outlineObj = new GameObject();
            outlineObj.transform.parent = meshRenderers[i].gameObject.transform;

            outlineObj.transform.parent = meshRenderers[i].gameObject.transform;

            outlineObj.transform.position = meshRenderers[i].gameObject.transform.position;
            outlineObj.transform.rotation = meshRenderers[i].gameObject.transform.rotation;
            outlineObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            outlineObj.AddComponent<MeshFilter>();
            outlineObj.AddComponent<MeshRenderer>();
            Mesh mesh;
            mesh = (Mesh)Instantiate(meshRenderers[i].gameObject.GetComponent<MeshFilter>().mesh);
            outlineObj.GetComponent<MeshFilter>().mesh = mesh;

            materials = new Material[materials.Length];
            for (int j = 0; j < materials.Length; j++)
            {
                // TODO - null pointer exception on Stencil/Outline
                materials[j] = new Material(Shader.Find("Stencil/Outline"));
            }

            outlineObj.GetComponent<MeshRenderer>().materials = materials;

            outlineObj.SetActive(false);

            outlineObjects.Add(outlineObj);


        }
    }

    public void DrawOutline()
    {
        if (outlineIsOn) return;

        for (int i = 0; i < normalObjects.Count; i++)
        {
            Material[] materials = normalObjects[i].GetComponent<MeshRenderer>().materials;
            int materialsNum = materials.Length;
            for (int j = 0; j < materialsNum; j++)
            {
                materials[j] = new Material(Shader.Find("Outline/Transparent"));
                materials[j].SetColor("_color", meshColor);
            }

            for (int j = 0; j < outlineObjects.Count; j++)
            {
                outlineObjects[j].SetActive(true);
            }
        }

        outlineIsOn = true;
    }

    public void EraseOutline()
    {
        if (!outlineIsOn) return;

        for (int i = 0; i < normalObjects.Count; i++)
        {
            normalObjects[i].GetComponent<MeshRenderer>().materials = normalMaterials[i];

            for (int j = 0; j < outlineObjects.Count; j++)
            {
                outlineObjects[j].SetActive(false);
            }
        }

        outlineIsOn = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
