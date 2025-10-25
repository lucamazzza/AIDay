using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Plant Crop")]
public class CropData : ScriptableObject
{
    [Header("Crop Settings")]
    public string cropName = "New Crop";
    [Tooltip("Total time for the crop to grow")]
    public float growthTime = 60f;
    [Header("Growth Stages")]
    [Tooltip("Sprites for each growth stage")]
    public Sprite[] growthStages;
        
}
