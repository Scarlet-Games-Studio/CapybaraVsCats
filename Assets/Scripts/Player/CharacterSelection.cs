public static class CharacterSelection
{
    public enum Character { Hiro, Mika, Edge }
    static Character selected = Character.Hiro;
    public static Character Selected
    {
        get => selected;
        set
        {
            selected = value;
            string key = "CharacterUses_" + value;
            UnityEngine.PlayerPrefs.SetInt(key, UnityEngine.PlayerPrefs.GetInt(key, 0) + 1);
            UnityEngine.PlayerPrefs.SetString("LastSelectedCharacter", value.ToString());
            UnityEngine.PlayerPrefs.Save();
        }
    }

    public static Character MostUsed
    {
        get
        {
            Character best = Character.Hiro;
            int uses = -1;
            foreach (Character character in System.Enum.GetValues(typeof(Character)))
            {
                int current = UnityEngine.PlayerPrefs.GetInt("CharacterUses_" + character, 0);
                if (current > uses) { uses = current; best = character; }
            }
            return best;
        }
    }
}
