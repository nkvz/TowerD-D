using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CubeConfig
{
    [SerializeField] private CubeType _type;
    [PreviewField] [SerializeField] private Sprite _sprite;

    public CubeType Type => _type;
    public Sprite Sprite => _sprite;
}