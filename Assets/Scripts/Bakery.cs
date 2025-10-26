using UnityEngine;

public class Bakery : MonoBehaviour
{
    public CraftingRecipe currentRecipe;
    public bool isBaking { get; private set; }

    private float bakingTimer;
    [SerializeField] private SpriteRenderer productSpriteRenderer;

    void Awake()
    {
       
        isBaking = false;

        if (productSpriteRenderer != null)
        {
            productSpriteRenderer.sprite = null;
        }
        else
        {
            Debug.LogError("Sprite Renderer del prodotto non assegnato alla panetteria!");
        }
    }

    public bool StartBaking(CraftingRecipe recipeToBake)
    {
        if (isBaking || currentRecipe != null)
        {
            Debug.Log("Busy bakery!");
            return false; // Already baking
        }


        currentRecipe = recipeToBake;
        bakingTimer = 0f;
        isBaking = true;
        Debug.Log($"Started cooking {currentRecipe.recipeName}!");
        return true;
    }

    void Update()
    {
        if (!isBaking) return;

        bakingTimer += Time.deltaTime;
        if (bakingTimer >= currentRecipe.craftingTime)
        {
            isBaking = false;
            Debug.Log($"{currentRecipe.recipeName} is ready!");
            productSpriteRenderer.sprite = currentRecipe.finishedItemSprite;
        }
    }
}