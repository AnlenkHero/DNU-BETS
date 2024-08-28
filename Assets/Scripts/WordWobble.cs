using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class WordWobble : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationSettings
    {
        public float verticesOffset1;
        public float verticesOffset2;
        public float verticesOffset3;
        public float verticesOffset4;
        public float colorLength1;
        public float colorLength2;
        public float colorLength3;
        public float colorLength4;
        public float colorSpeed1;
        public float colorSpeed2;
        public float colorSpeed3;
        public float colorSpeed4;
        public float speedMultiplier;
        public float amplitudeMultiplier;
        public float riseDuration; 
        public float fallDuration;
        public float delayBetweenLetters;
    }

    public enum WobbleMode
    {
        SinCos,
        Spiral,
        Random,
        Pulse,
        Wave,
        Jitter,
        QueueRise,
        Custom
    }

    [SerializeField] private WobbleMode wobbleMode = WobbleMode.SinCos;

    [SerializeField] private bool useParallel; 

    [SerializeField] private AnimationSettings animationSettings = new AnimationSettings
    {
        verticesOffset1 = 3.5f,
        verticesOffset2 = 3f,
        verticesOffset3 = 1.5f,
        verticesOffset4 = 2f,
        colorLength1 = 1.2f,
        colorLength2 = 1.3f,
        colorLength3 = 1.4f,
        colorLength4 = 1.5f,
        colorSpeed1 = 0.0004f,
        colorSpeed2 = 0.0007f,
        colorSpeed3 = 0.0007f,
        colorSpeed4 = 0.0007f,
        speedMultiplier = 2f,
        amplitudeMultiplier = 10f,
        riseDuration = 0.5f,
        fallDuration = 0.5f,
        delayBetweenLetters = 0.2f
    };

    public Gradient rainbow;

    private TMP_Text _textMesh;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private List<int> _wordIndexes;
    private List<int> _wordLengths;

    void Start()
    {
        _textMesh = GetComponent<TMP_Text>();

        _wordIndexes = new List<int> { 0 };
        _wordLengths = new List<int>();

        string s = _textMesh.text;
        for (int index = s.IndexOf(' '); index > -1; index = s.IndexOf(' ', index + 1))
        {
            _wordLengths.Add(index - _wordIndexes[_wordIndexes.Count - 1]);
            _wordIndexes.Add(index + 1);
        }

        _wordLengths.Add(s.Length - _wordIndexes[_wordIndexes.Count - 1]);
    }

    void Update()
    {
        _textMesh.ForceMeshUpdate();
        var textInfo = _textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; ++i)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            float timeOffset = useParallel ? i * animationSettings.delayBetweenLetters : i;
            _vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            Vector3 offset = ApplyWobbleEffect(Time.time + timeOffset, i, wobbleMode);

            for (int j = 0; j < 4; ++j)
            {
                _vertices[charInfo.vertexIndex + j] += offset * GetOffsetByIndex(j);
            }
        }

        UpdateMeshVertices(textInfo);
        UpdateVertexColors();
    }

    private Vector3 ApplyWobbleEffect(float time, int index, WobbleMode mode)
    {
        switch (mode)
        {
            case WobbleMode.SinCos:
                return new Vector2(Mathf.Sin(time * animationSettings.speedMultiplier),
                    Mathf.Cos(time * animationSettings.speedMultiplier)) * animationSettings.amplitudeMultiplier;
            case WobbleMode.Spiral:
                return new Vector2(Mathf.Cos(time * animationSettings.speedMultiplier) * 2f,
                    Mathf.Sin(time * animationSettings.speedMultiplier) * 2f) * animationSettings.amplitudeMultiplier;
            case WobbleMode.Random:
                return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.Pulse:
                return new Vector2(0,
                    Mathf.Sin(time * animationSettings.speedMultiplier) * animationSettings.amplitudeMultiplier);
            case WobbleMode.Wave:
                return new Vector2(Mathf.Sin(time * animationSettings.speedMultiplier),
                           Mathf.Sin(time * animationSettings.speedMultiplier + time * 2)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.Jitter:
                return new Vector2(Mathf.PerlinNoise(time * animationSettings.speedMultiplier, 0),
                           Mathf.PerlinNoise(0, time * animationSettings.speedMultiplier)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.QueueRise:
                return CalculateQueueRiseOffset(time, index);
            case WobbleMode.Custom:
                return CustomWobble(time);
            default:
                return Vector2.zero;
        }
    }

    private Vector3 CalculateQueueRiseOffset(float time, int index)
    {
        float totalDuration = animationSettings.riseDuration + animationSettings.fallDuration;
        float startDelay = index * animationSettings.delayBetweenLetters;
        float elapsed = time - startDelay;

        if (elapsed < 0)
            return Vector3.zero;

        float phase = (elapsed % totalDuration) / totalDuration;

        if (phase < animationSettings.riseDuration / totalDuration)
        {
            float risePhase = phase / (animationSettings.riseDuration / totalDuration);
            return new Vector3(0, Mathf.Sin(risePhase * Mathf.PI) * animationSettings.amplitudeMultiplier, 0);
        }
        else
        {
            float fallPhase = (phase - animationSettings.riseDuration / totalDuration) /
                              (animationSettings.fallDuration / totalDuration);
            return new Vector3(0, -Mathf.Sin(fallPhase * Mathf.PI) * animationSettings.amplitudeMultiplier, 0);
        }
    }

    private Vector3 CustomWobble(float time)
    {
        // Custom wobble logic
        return new Vector2(Mathf.Sin(time), Mathf.Cos(time) * Mathf.Sin(time));
    }

    private void UpdateMeshVertices(TMP_TextInfo textInfo)
    {
        for (int i = 0; i < textInfo.meshInfo.Length; ++i)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            _textMesh.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    private void UpdateVertexColors()
    {
        _mesh = _textMesh.mesh;
        _vertices = _mesh.vertices;
        Color[] colors = _mesh.colors;

        for (int w = 0; w < _wordIndexes.Count; w++)
        { 
            int wordIndex = _wordIndexes[w];

            for (int i = 0; i < _wordLengths[w]; i++)
            {
                TMP_CharacterInfo c = _textMesh.textInfo.characterInfo[wordIndex + i];
                int index = c.vertexIndex;

                colors[index] = rainbow.Evaluate(Mathf.Repeat(
                    Time.time + _vertices[index].x * animationSettings.colorSpeed1, animationSettings.colorLength1));
                colors[index + 1] = rainbow.Evaluate(Mathf.Repeat(
                    Time.time + _vertices[index + 1].x * animationSettings.colorSpeed2,
                    animationSettings.colorLength2));
                colors[index + 2] = rainbow.Evaluate(Mathf.Repeat(
                    Time.time + _vertices[index + 2].x * animationSettings.colorSpeed3,
                    animationSettings.colorLength3));
                colors[index + 3] = rainbow.Evaluate(Mathf.Repeat(
                    Time.time + _vertices[index + 3].x * animationSettings.colorSpeed4,
                    animationSettings.colorLength4));
            }
        }

        _mesh.colors = colors;
        _textMesh.canvasRenderer.SetMesh(_mesh);
    }

    private float GetOffsetByIndex(int index)
    {
        return index switch
        {
            0 => animationSettings.verticesOffset1,
            1 => animationSettings.verticesOffset2,
            2 => animationSettings.verticesOffset3,
            _ => animationSettings.verticesOffset4,
        };
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WordWobble))]
    public class WordWobbleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var wordWobble = (WordWobble)target;

            EditorGUI.BeginChangeCheck();

            if (wordWobble.wobbleMode != WobbleMode.QueueRise)
            {
                wordWobble.wobbleMode = (WobbleMode)EditorGUILayout.EnumPopup("Wobble Mode", wordWobble.wobbleMode);
                wordWobble.useParallel = EditorGUILayout.Toggle("Use Parallel", wordWobble.useParallel);

                wordWobble.animationSettings.verticesOffset1 = EditorGUILayout.FloatField("Vertices Offset 1",
                    wordWobble.animationSettings.verticesOffset1);
                wordWobble.animationSettings.verticesOffset2 = EditorGUILayout.FloatField("Vertices Offset 2",
                    wordWobble.animationSettings.verticesOffset2);
                wordWobble.animationSettings.verticesOffset3 = EditorGUILayout.FloatField("Vertices Offset 3",
                    wordWobble.animationSettings.verticesOffset3);
                wordWobble.animationSettings.verticesOffset4 = EditorGUILayout.FloatField("Vertices Offset 4",
                    wordWobble.animationSettings.verticesOffset4);

                wordWobble.animationSettings.colorLength1 =
                    EditorGUILayout.FloatField("Color Length 1", wordWobble.animationSettings.colorLength1);
                wordWobble.animationSettings.colorLength2 =
                    EditorGUILayout.FloatField("Color Length 2", wordWobble.animationSettings.colorLength2);
                wordWobble.animationSettings.colorLength3 =
                    EditorGUILayout.FloatField("Color Length 3", wordWobble.animationSettings.colorLength3);
                wordWobble.animationSettings.colorLength4 =
                    EditorGUILayout.FloatField("Color Length 4", wordWobble.animationSettings.colorLength4);

                wordWobble.animationSettings.colorSpeed1 =
                    EditorGUILayout.FloatField("Color Speed 1", wordWobble.animationSettings.colorSpeed1);
                wordWobble.animationSettings.colorSpeed2 =
                    EditorGUILayout.FloatField("Color Speed 2", wordWobble.animationSettings.colorSpeed2);
                wordWobble.animationSettings.colorSpeed3 =
                    EditorGUILayout.FloatField("Color Speed 3", wordWobble.animationSettings.colorSpeed3);
                wordWobble.animationSettings.colorSpeed4 =
                    EditorGUILayout.FloatField("Color Speed 4", wordWobble.animationSettings.colorSpeed4);

                wordWobble.animationSettings.speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier",
                    wordWobble.animationSettings.speedMultiplier);
                wordWobble.animationSettings.amplitudeMultiplier = EditorGUILayout.FloatField("Amplitude Multiplier",
                    wordWobble.animationSettings.amplitudeMultiplier);
                wordWobble.rainbow =
                    EditorGUILayout.GradientField("Rainbow Gradient", wordWobble.rainbow);
            }
            else
            {
                DrawDefaultInspector();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}