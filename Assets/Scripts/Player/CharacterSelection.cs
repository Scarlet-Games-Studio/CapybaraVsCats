public static class CharacterSelection
{
    public enum Character { Hiro, Mika, Edge }

    const string FirstChosenKey = "FirstChosenCharacter";
    const string MostPlayedKey = "MostPlayedCharacter";
    const string LastSelectedKey = "LastSelectedCharacter";
    const string GamesPlayedPrefix = "CharacterGames_";

    static Character selected = ReadCharacter(LastSelectedKey, Character.Hiro);

    public static Character Selected
    {
        get => selected;
        set
        {
            selected = value;

            // Escolher um personagem nao equivale a jogar uma partida com ele.
            UnityEngine.PlayerPrefs.SetString(LastSelectedKey, value.ToString());

            // Enquanto ainda nao existem partidas registradas, o primeiro clique
            // define o personagem exibido como favorito no Lobby.
            if (!UnityEngine.PlayerPrefs.HasKey(FirstChosenKey))
            {
                UnityEngine.PlayerPrefs.SetString(FirstChosenKey, value.ToString());
                UnityEngine.PlayerPrefs.SetString(MostPlayedKey, value.ToString());
            }

            UnityEngine.PlayerPrefs.Save();
        }
    }

    public static void RecordGameStarted()
    {
        string selectedGamesKey = GamesPlayedPrefix + selected;
        int selectedGames = UnityEngine.PlayerPrefs.GetInt(selectedGamesKey, 0) + 1;
        UnityEngine.PlayerPrefs.SetInt(selectedGamesKey, selectedGames);

        Character initialFavorite = ReadCharacter(FirstChosenKey, selected);
        Character currentFavorite = ReadCharacter(MostPlayedKey, initialFavorite);
        int favoriteGames = UnityEngine.PlayerPrefs.GetInt(GamesPlayedPrefix + currentFavorite, 0);

        // Em empate, mantem o favorito atual. Ele so muda quando outro personagem
        // realmente tiver sido usado em mais partidas.
        if (selected != currentFavorite && selectedGames > favoriteGames)
            UnityEngine.PlayerPrefs.SetString(MostPlayedKey, selected.ToString());

        UnityEngine.PlayerPrefs.Save();
    }

    public static Character MostUsed
    {
        get => ReadCharacter(MostPlayedKey, ReadCharacter(FirstChosenKey, selected));
    }

    static Character ReadCharacter(string key, Character fallback)
    {
        string savedValue = UnityEngine.PlayerPrefs.GetString(key, string.Empty);
        return System.Enum.TryParse(savedValue, out Character character) ? character : fallback;
    }
}
