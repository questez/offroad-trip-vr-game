using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class UINavigationManager : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Selectable[] FirstButtons;

    bool OffInput;

    private void Update()
    {
        if (inputControllerReader.HatSwitch.y == 1 || inputControllerReader.Plus)
        {
            if (!OffInput)
            {
                NavigateToUp();
                StartCoroutine(OffInputDelay());
            }
        }
        else if (inputControllerReader.HatSwitch.y == -1 || inputControllerReader.Minus)
        {
            if (!OffInput)
            {
                NavigateToDown();
                StartCoroutine(OffInputDelay());
            }
        }
    }

    private void NavigateToUp()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstButton();
            return;
        }

        Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

        Selectable next = current.FindSelectableOnUp();

        if (next != null && next.IsInteractable())
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
        }
    }

    private void NavigateToDown()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstButton();
            return;
        }

        Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

        Selectable next = current.FindSelectableOnDown();

        if (next != null && next.IsInteractable())
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
        }
    }

    private void SelectFirstButton()
    {
        foreach (Selectable selectable in FirstButtons)
        {
            if (selectable.IsActive() && selectable.IsInteractable())
            {
                selectable.Select();
            }
        }
    }

    private IEnumerator OffInputDelay()
    {
        OffInput = true;
        yield return new WaitForSecondsRealtime(0.25f);
        OffInput = false;
    }
}
