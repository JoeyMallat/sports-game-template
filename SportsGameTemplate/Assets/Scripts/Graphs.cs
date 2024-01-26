using UnityEngine;

public class Graphs : MonoBehaviour
{
    public static Graphs Instance;

    public AnimationCurve SkillImportanceGraph;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
