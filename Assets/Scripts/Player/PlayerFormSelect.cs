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

    private GameObject previousSelectedForm = null;

    private LayerMask collisionMask;

    private bool m_Start;

    private void Start()
    {
        GetFirstActiveChild();
        collisionMask = LayerMask.GetMask("Ground");
        m_Start = true;
    }

    private void Update()
    {
        if (selectedForm != null)
        {
            CheckIfColliding();
        }
    }

    private void GetFirstActiveChild()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf && child.tag != "FormSwitchCheck")
            {
                activeForm = child.gameObject;
                activeFormEnum = (PlayerForm)Enum.Parse(typeof(PlayerForm), activeForm.name);
            }
        }
    }

    private void ChangeActiveForm()
    {
        foreach (Transform child in transform)
        {
            if (child.name.Equals(selectedFormEnum.ToString()))
            {
                selectedForm = child.gameObject;
            }
        }

        
        bool canMorph = !CheckIfColliding();
        

        if (canMorph)
        {
            activeForm.SetActive(false);
            selectedForm.SetActive(true);

            previousSelectedForm = activeForm;

            activeForm = selectedForm;
            activeFormEnum = selectedFormEnum;

            selectedForm = null;
            selectedFormEnum = PlayerForm.Unselected;
        }
        else
        {
            Debug.Log("Cannot Morph!");
        }
    }

    private bool CheckIfColliding()
    {
        Vector2 skinWidth = new Vector2(-.05f, -.05f);
        Collider2D colliders = Physics2D.OverlapBox(selectedForm.transform.position, (Vector2)selectedForm.transform.localScale + skinWidth, 0f, collisionMask);

        return colliders != null;
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

    private void OnDrawGizmos()
    {
        if (m_Start && selectedForm != null && false)
        {
            Vector2 skinWidth = new Vector2(-.03f, -.03f);
            Gizmos.color = new Color(1, 0, 0, .3f);
            Gizmos.DrawCube(selectedForm.transform.position, (Vector2)selectedForm.transform.localScale + skinWidth);
        }
    }
}
