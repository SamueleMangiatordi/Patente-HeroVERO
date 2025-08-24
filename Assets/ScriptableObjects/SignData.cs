using UnityEngine;

[CreateAssetMenu(fileName = "SignData", menuName = "Scriptable Objects/SignData")]
public class SignData : ScriptableObject
{
    public Sprite image;
    public string signName;
    public SignType type;
    [TextArea(3, 10)]
    public string description;
}

public class SignTypeColor
{
    public static float alpha = 0.3f; // Default alpha value
    public static Color GetColor(SignType signType)
    {
        return signType switch
        {
            SignType.Pericolo => Color.red.WithAlpha(alpha),
            SignType.Obbligo => Color.blue.WithAlpha(alpha),
            SignType.Divieto => Color.yellow.WithAlpha(alpha),
            _ => Color.white.WithAlpha(alpha),
        };
    }
}