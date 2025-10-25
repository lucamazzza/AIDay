using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropBehaviour : MonoBehaviour
{
    public CropData cropData;
    
    private SpriteRenderer spriteRenderer;
    private int currentGrowthStage = 0;
    private float growthTimer = 0f;
    private bool isFullyGrown = false;

    void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cropData == null) 
        {
            Debug.LogWarning("No crop data for this crop behavior");
            return;
        }
        currentGrowthStage = 0;
        growthTimer = 0;
        isFullyGrown = false;

        if (cropData.growthStages.Length > 0) {
            spriteRenderer.sprite = cropData.growthStages[0];
        }
        else 
        {
            Debug.LogError("CropData has no growth stages");
        }
    }

    // Update is called once per frame
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
    
    private void UpdateCropSprite()
    {
        if (currentGrowthStage < cropData.growthStages.Length)
        {
            spriteRenderer.sprite = cropData.growthStages[currentGrowthStage];
        }
    }
}
