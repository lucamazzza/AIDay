using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropBehaviour : MonoBehaviour
{
    public CropData cropData;
    public bool isFullyGrown = false;
    private SpriteRenderer spriteRenderer;
    private int currentGrowthStage = 0;
    private float growthTimer = 0f;
    
    void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (cropData == null || isFullyGrown) 
        {
            return;
        } 
        growthTimer += Time.deltaTime;
        int totalStages = cropData.growthStages.Length;
        if (totalStages == 0) return;
        float timePerStage = cropData.growthTime / totalStages;
        int calculatedStage = Mathf.FloorToInt(growthTimer / timePerStage);
        int newStage = Mathf.Clamp(calculatedStage, 0, totalStages - 1);
        if (newStage != currentGrowthStage)
        {
            currentGrowthStage = newStage;
            UpdateCropSprite();
        }
        if (growthTimer >= cropData.growthTime) 
        {           
            currentGrowthStage = totalStages - 1;
            UpdateCropSprite();
            isFullyGrown = true;
        }
    }
    
    public void SetCrop(CropData newData)
    {
        if (newData == null)
        {
            Debug.LogWarning("SetCrop called with null values.");
            return;
        }
        this.cropData = newData;
        currentGrowthStage = 0;
        growthTimer = 0;
        isFullyGrown = false;
        if (cropData.growthStages.Length > 0)
        {
            spriteRenderer.sprite = cropData.growthStages[0];
        }
        else
        {
            Debug.LogError("CropData do not have grow stages!");
        }
    }

    private void UpdateCropSprite()
    {
        if (currentGrowthStage < cropData.growthStages.Length)
        {
            spriteRenderer.sprite = cropData.growthStages[currentGrowthStage];
        }
    }
}
