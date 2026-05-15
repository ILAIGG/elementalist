using System;
using System.Diagnostics.Metrics;
using Godot;

public class ExperienceSystem
{
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXP { get; private set; } = 0f;

    //XP necesaria para subir al siguiente nivel. Escala con el nivel actual para que sea progresivamente más difícil subir de nivel
    public float XPToNextLevel => 50f * Mathf.Pow(CurrentLevel, 1.2f);

    //Eventos que otros sistemas pueden escuchar
    public event Action<float, float> OnXPChanged; // XP actual, XP necesaria
    public event Action<int> OnLevelUp; //Cuando el jugador sube de nivel

    public void AddXP(float amount)
    {
        CurrentXP += amount;
        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);

        //Verifica si el jugador sube de nivel. Se utiliza un while en caso de que el jugador consiga suficiente XP como para subir más de un nivel de golpe.
        while (CurrentXP >= XPToNextLevel)
        {
            CurrentXP -= XPToNextLevel;
            CurrentLevel++;
            OnLevelUp?.Invoke(CurrentLevel);
            OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
        }
    }
}