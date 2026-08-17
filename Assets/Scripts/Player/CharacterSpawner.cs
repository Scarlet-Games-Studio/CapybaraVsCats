using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] GameObject mikaPrefab;

    void Awake()
    {
        if (CharacterSelection.Selected != CharacterSelection.Character.Mika || mikaPrefab == null) return;
        GameObject current = GameObject.FindGameObjectWithTag("Player");
        if (current == null || current.name.ToLowerInvariant().Contains("mika")) return;
        current.tag = "Untagged";
        GameObject mika = Instantiate(mikaPrefab, current.transform.position, current.transform.rotation);
        mika.name = "Mika";
        Destroy(current);
    }
}
