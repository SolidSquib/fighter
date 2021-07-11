using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Speaker")]
public class SDialogueSpeaker : ScriptableObject
{
    [SerializeField] Sprite _portrait;
    [SerializeField] Color _color;
    [SerializeField] string _name;

    public Sprite portrait { get { return _portrait; } }
    public Color color { get { return _color; } }
    public string speakerName { get { return _name; } }
}
