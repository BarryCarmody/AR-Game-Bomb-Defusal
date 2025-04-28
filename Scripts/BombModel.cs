using UnityEngine;

public class BombModel : MonoBehaviour
{
    public GameObject modelPrefab; 
    public float distanceFromCamera = 2f; 

    private GameObject instantiatedModel;

    public void ShowModel()
    {
        if (instantiatedModel == null)
        {
            instantiatedModel = Instantiate(modelPrefab);
            var renderer = instantiatedModel.GetComponent<Renderer>();
            if (renderer != null)
            {
                SetRenderQueue(renderer.material, 3000); 
            }
        }

        PositionModel();
    }

    public void HideModel()
    {
        if (instantiatedModel != null)
        {
            Destroy(instantiatedModel);
            instantiatedModel = null;
        }
    }

    void Update()
    {
        if (instantiatedModel != null)
        {
            PositionModel();
        }
    }

    private void PositionModel()
    {
        if (Camera.main == null) return; 

        instantiatedModel.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;

        instantiatedModel.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(270, 0, 0);
    }

    private void SetRenderQueue(Material material, int queue)
    {
        if (material != null)
        {
            material.renderQueue = queue;
        }
    }


}
