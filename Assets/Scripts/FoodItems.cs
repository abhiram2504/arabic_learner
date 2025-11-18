using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FoodDish : MonoBehaviour
{
    [Header("Assign ALL ingredient objects here")]
    public List<GameObject> ingredientObjects; // drag ingredients in inspector

    private HashSet<GameObject> remainingIngredients;

    private void Start()
    {
        // Copy initial ingredients into a quick lookup set
        remainingIngredients = new HashSet<GameObject>(ingredientObjects);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hit = collision.gameObject;

        // Check if the collided object is one of the ingredients
        if (remainingIngredients.Contains(hit))
        {
            hit.SetActive(false);  // hide the ingredient
            remainingIngredients.Remove(hit);

            Debug.Log("Collected ingredient: " + hit.name);

            // If all ingredients are collected, reload the scene using the new API
            if (remainingIngredients.Count == 0)
            {
                Debug.Log("All ingredients collected! Reloading scene...");

                // NEW SCENE MANAGER (Unity 6 / 2023+ compatible)
                SceneManager.LoadSceneAsync(
                    SceneManager.GetActiveScene().name,
                    LoadSceneMode.Single
                );
            }
        }
    }
}
