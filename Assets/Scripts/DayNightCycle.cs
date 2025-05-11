using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Material daySkybox;    // Material del Skybox de día
    public Material nightSkybox;  // Material del Skybox de noche
    public float cycleDuration = 60f;  // Duración del ciclo completo (día+noche) en segundos

    private float timer;  // Temporizador para gestionar el tiempo del ciclo
    private Material dynamicSkybox;  // Material para el skybox que se actualiza

    private bool isDay = true;  // Variable para verificar si es de día o de noche

    void Start()
    {
        // Crear una copia del skybox para modificar en tiempo real
        dynamicSkybox = new Material(daySkybox);
        RenderSettings.skybox = dynamicSkybox;
    }

    void Update()
    {
        timer += Time.deltaTime;  // Aumentamos el temporizador con el tiempo transcurrido

        // Verificamos si es necesario cambiar de día a noche o de noche a día
        if (timer >= cycleDuration)
        {
            // Si el ciclo completo ha pasado, alternamos entre día y noche
            isDay = !isDay;
            timer = 0f;  // Reiniciamos el temporizador para el siguiente ciclo
        }

        // Interpolamos entre el Skybox de día y de noche
        if (isDay)
        {
            // Si es de día, interpolamos hacia el Skybox de día
            dynamicSkybox.Lerp(nightSkybox, daySkybox, timer / cycleDuration);
        }
        else
        {
            // Si es de noche, interpolamos hacia el Skybox de noche
            dynamicSkybox.Lerp(daySkybox, nightSkybox, timer / cycleDuration);
        }

        // Aplicamos el skybox dinámico actualizado
        RenderSettings.skybox = dynamicSkybox;

        // Actualizamos la iluminación global
        DynamicGI.UpdateEnvironment();

        // Debug para ver el cambio de materiales
        Debug.Log(isDay ? "Es de día" : "Es de noche");
    }
}
