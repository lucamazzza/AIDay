using UnityEngine;
using System.Collections.Generic; 


[System.Serializable]
public class Ingredient
{
    public CropData item; 
    public int amount;    
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Farm/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName; 
    public float craftingTime;  

    public List<Ingredient> ingredients;

    public Sprite finishedItemSprite;
}