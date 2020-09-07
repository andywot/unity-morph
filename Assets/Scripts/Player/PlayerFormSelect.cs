using UnityEngine;
using System;

public class PlayerFormSelect : MonoBehaviour
{
    internal enum PlayerForm
    {
        Humanoid,
        Fox,
        Fish,
        Birb,
        Unselected
    }

    private PlayerForm activeFormEnum;
    private GameObject activeForm;

    internal PlayerForm selectedFormEnum = PlayerForm.Unselected;
    private GameObject selectedForm;

    private void Start()
    {
        GetFirstActiveChild();
    }

    private void GetFirstActiveChild()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).gameObject.activeSelf)
            {
                activeForm = gameObject.transform.GetChild(i).gameObject;
                activeFormEnum = (PlayerForm)Enum.Parse(typeof(PlayerForm), activeForm.name);
                break;
            }
        }
    }

    private void ChangeActiveForm()
    {
        if (selectedFormEnum != activeFormEnum)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject currentObject = gameObject.transform.GetChild(i).gameObject;
                
                if (currentObject.name.Equals(selectedFormEnum.ToString()))
                {
                    selectedForm = currentObject;
                }
            }

            if (selectedForm != null)
            {
                selectedForm.transform.position = activeForm.transform.position + new Vector3(0, 10, 0);

                activeForm.SetActive(false);
                selectedForm.SetActive(true);

                activeForm = selectedForm;
                activeFormEnum = selectedFormEnum;

                selectedForm = null;
                selectedFormEnum = PlayerForm.Unselected;
            }
        }
    }

    public void SelectFox()
    {
        selectedFormEnum = PlayerForm.Fox;
        ChangeActiveForm();
    }

    public void SelectHuman()
    {
        selectedFormEnum = PlayerForm.Humanoid;
        ChangeActiveForm();
    }
}
