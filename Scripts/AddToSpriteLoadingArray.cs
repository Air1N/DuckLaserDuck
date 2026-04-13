using UnityEngine;

public class AddToSpriteLoadingArray : MonoBehaviour
{

        public SpriteLoadingManager loadingManager;
        // Start is called before the first frame update
        void Start()
        {
                GameObject character = GameObject.Find("char");
                if (character) loadingManager = character.GetComponent<SpriteLoadingManager>();
                if (loadingManager) loadingManager.objects.Add(gameObject);
        }

        void OnDestroy()
        {
                if (loadingManager) loadingManager.objects.Remove(gameObject);
        }

}
