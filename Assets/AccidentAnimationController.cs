using System.Collections;
using UnityEngine;

public class AccidentAnimationController : MonoBehaviour
{
    [SerializeField] private Transform teleportPoint;
    [SerializeField] private GameObject mainCarPrefab;

    [Tooltip("Il componente VRCameraFader che gestisce la dissolvenza della telecamera.")]
    public VRCameraFader cameraFader;

    [Tooltip("La durata in secondi della dissolvenza (fade-out e fade-in).")]
    public float fadeDuration = 2f;

    private Coroutine _fadingCoroutine = null;

    private SimplifiedCarController _carController;
   
    private void Start()
    {
        _carController = GetComponent<SimplifiedCarController>();
        if (_carController == null)
        {
            Debug.LogError("SimplifiedCarController non trovato sul GameObject!");
        }
    }

    /// <summary>
    /// Questo metodo viene sovrascritto per gestire la richiesta di teletrasporto
    /// aggiungendo la logica di dissolvenza prima e dopo.
    /// </summary>
    public void FadingTeleport(Transform teleportPoint)
    {
        if (_fadingCoroutine != null)
        {
            // Se c'� gi� una dissolvenza in corso, interrompi la coroutine precedente
            StopCoroutine(_fadingCoroutine);
        }

        // Avvia la coroutine per la sequenza completa: fade-out, teletrasporto, fade-in
        _fadingCoroutine = StartCoroutine(FadingTeleportCoroutine(teleportPoint));
    }

    // Metodo che restituisce la coroutine per il teletrasporto
    // Il chiamante pu� usare 'yield return' su questo metodo per attendere il completamento
    public IEnumerator WaitFadingTeleport(Transform teleportPoint)
    {
        // Se c'� gi� una dissolvenza in corso, la interrompe
        if (_fadingCoroutine != null)
        {
            StopCoroutine(_fadingCoroutine);
        }

        // Avvia e restituisce la coroutine. Il chiamante attender� il suo completamento.
        _fadingCoroutine = StartCoroutine(FadingTeleportCoroutine(teleportPoint));
        yield return _fadingCoroutine;
    }

    private IEnumerator FadingTeleportCoroutine(Transform teleportPoint)
    {
        // Esegui la dissolvenza a nero
        yield return StartCoroutine(cameraFader.FadeToBlack(fadeDuration));

        // Ora che lo schermo � nero, esegui il teletrasporto
        _carController.TeleportCar(teleportPoint.position, teleportPoint.rotation, 0, true);
        yield return new WaitForSeconds(fadeDuration);

        // Esegui la dissolvenza da nero a trasparente
        yield return StartCoroutine(cameraFader.FadeFromBlack(fadeDuration));

        _fadingCoroutine = null;

    }
}
