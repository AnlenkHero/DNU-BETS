using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordWobble : MonoBehaviour
{
    [SerializeField]
    private float verticesOffset1=3.5f;
    [SerializeField]
    private float verticesOffset2=3f;
    [SerializeField]
    private float verticesOffset3=1.5f;
    [SerializeField]
    private float verticesOffset4=2f;
    [SerializeField]
    private float colorLength1=1.2f;
    [SerializeField]
    private float colorLength2=1.3f;
    [SerializeField]
    private float colorLength3=1.4f;
    [SerializeField]
    private float colorLength4=1.5f;
    TMP_Text textMesh;

    Mesh mesh;

    Vector3[] vertices;

    List<int> wordIndexes;
    List<int> wordLengths;

    public Gradient rainbow;
    
    void Start()
    {
        textMesh = GetComponent<TMP_Text>();

        wordIndexes = new List<int>{0};
        wordLengths = new List<int>();

        string s = textMesh.text;
        for (int index = s.IndexOf(' '); index > -1; index = s.IndexOf(' ', index + 1))
        {
            wordLengths.Add(index - wordIndexes[wordIndexes.Count - 1]);
            wordIndexes.Add(index + 1);
        }
        wordLengths.Add(s.Length - wordIndexes[wordIndexes.Count - 1]);
    }
    
    void Update()
    {
        textMesh.ForceMeshUpdate();

        var textInfo = textMesh.textInfo;
        

        for (int i = 0; i < textInfo.characterCount; ++i)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
            {
                continue;
            }

            vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            for (int j = 0; j < 4; ++j)
            {
                Vector3 offset = Wobble(Time.time + i);
                
                var orig = vertices[charInfo.vertexIndex + j];
                //vertices[charInfo.vertexIndex+j]=orig+new Vector3(0,Mathf.Sin(Time.time*2f+orig.x*0.01f)*10f,0);
                vertices[charInfo.vertexIndex + j] += offset*verticesOffset1;
                vertices[charInfo.vertexIndex + j+1] += offset*verticesOffset2;
                vertices[charInfo.vertexIndex + j+2] += offset*verticesOffset3;
                vertices[charInfo.vertexIndex + j+3] += offset*verticesOffset4;

            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; ++i)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textMesh.UpdateGeometry(meshInfo.mesh,i);
        }
        
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        Color[] colors = mesh.colors;

        for (int w = 0; w < wordIndexes.Count; w++)
        {
            int wordIndex = wordIndexes[w];
            Vector3 offset = Wobble(Time.time + w);

            for (int i = 0; i < wordLengths[w]; i++)
            {
                TMP_CharacterInfo c = textMesh.textInfo.characterInfo[wordIndex+i];

                int index = c.vertexIndex;

                colors[index] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index].x*0.0004f, colorLength1));
                colors[index + 1] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 1].x*0.0007f, colorLength2));
                colors[index + 2] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 2].x*0.0007f, colorLength3));
                colors[index + 3] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 3].x*0.0007f, colorLength4));

            }
            mesh.colors = colors;
            textMesh.canvasRenderer.SetMesh(mesh);
        }
    }

    Vector2 Wobble(float time) {
        return new Vector2(Mathf.Sin(time*3.3f), Mathf.Cos(time*2.5f));
    }
}