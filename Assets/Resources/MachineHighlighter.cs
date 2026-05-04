using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Collections.Generic;

public class MachineHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("Il materiale trasparente/ologramma da sovrapporre quando selezionato")]
    public Material highlightMaterial; 
    
    private bool _isSelected = false;
    private List<GameObject> _overlayObjects = new List<GameObject>();

    void Awake()
    {
        CreateOverlayObjects();
    }

    void CreateOverlayObjects()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var rend in renderers)
        {
            if (rend == null) continue;
            // Evita di creare overlay degli overlay se lo script viene riavviato
            if (rend.gameObject.name.EndsWith("_HighlightOverlay")) continue;

            Mesh mesh = null;

            if (rend is MeshRenderer)
            {
                MeshFilter mf = rend.GetComponent<MeshFilter>();
                if (mf != null) mesh = mf.sharedMesh;
            }
            else if (rend is SkinnedMeshRenderer smr)
            {
                mesh = smr.sharedMesh;
            }

            // Se abbiamo trovato una mesh, creiamo un oggetto copia "fantasma"
            if (mesh != null)
            {
                GameObject overlay = new GameObject(rend.gameObject.name + "_HighlightOverlay");
                overlay.transform.SetParent(rend.transform, false);
                overlay.transform.localPosition = Vector3.zero;
                overlay.transform.localRotation = Quaternion.identity;
                overlay.transform.localScale = Vector3.one;

                if (rend is MeshRenderer)
                {
                    MeshFilter newMf = overlay.AddComponent<MeshFilter>();
                    newMf.sharedMesh = mesh;
                    MeshRenderer newMr = overlay.AddComponent<MeshRenderer>();
                    
                    // Applica l'highlight material a TUTTE le submesh (per supportare oggetti multi-materiale)
                    Material[] mats = new Material[mesh.subMeshCount];
                    for (int i = 0; i < mats.Length; i++) mats[i] = highlightMaterial;
                    newMr.sharedMaterials = mats;
                }
                else if (rend is SkinnedMeshRenderer origSmr)
                {
                    SkinnedMeshRenderer newSmr = overlay.AddComponent<SkinnedMeshRenderer>();
                    newSmr.sharedMesh = mesh;
                    newSmr.bones = origSmr.bones;
                    newSmr.rootBone = origSmr.rootBone;
                    
                    Material[] mats = new Material[mesh.subMeshCount];
                    for (int i = 0; i < mats.Length; i++) mats[i] = highlightMaterial;
                    newSmr.sharedMaterials = mats;
                }

                overlay.SetActive(false); // Parte spento
                _overlayObjects.Add(overlay);
            }
        }
    }

    // --- METODI COMPATIBILI CON XR INTERACTION TOOLKIT ---

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        TurnOnHighlight();
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        TurnOffHighlight();
    }

    // --- LOGICA DI HIGHLIGHT ---

    public void TurnOnHighlight()
    {
        if (_isSelected || highlightMaterial == null) return;
        _isSelected = true;

        // Accende semplicemente tutti i "fantasmi" creati
        foreach (var obj in _overlayObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    public void TurnOffHighlight()
    {
        if (!_isSelected || highlightMaterial == null) return;
        _isSelected = false;

        // Spegne i "fantasmi"
        foreach (var obj in _overlayObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}
