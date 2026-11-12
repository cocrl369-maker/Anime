using UnityEngine;

namespace AnimeFantasy
{
    /// <summary>
    /// Startet das Spiel automatisch in jeder Szene – es muss nichts im Editor eingerichtet werden.
    /// </summary>
    static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;
            new GameObject("GameManager").AddComponent<GameManager>();
        }
    }
}
