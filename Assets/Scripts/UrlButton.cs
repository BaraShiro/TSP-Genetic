using UnityEngine;

/// <summary>
/// A script for a button that opens a URL.
/// </summary>
/// <remarks>The URL is hardcoded because I'm lazy.</remarks>
public class UrlButton : MonoBehaviour
{

    /// <summary>
    /// Opens a browser and goes to https://www.kenney.nl
    /// </summary>
    public void GoToURL()
    {
        Application.OpenURL("https://www.kenney.nl/");
    }
}
