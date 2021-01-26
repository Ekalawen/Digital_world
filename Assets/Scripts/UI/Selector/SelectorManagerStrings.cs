﻿using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "SelectorManagerStrings", menuName = "SelectorManager")]
public class SelectorManagerStrings : ScriptableObject {

    public LocalizedString pathGoodUnlockedTitle;
    public LocalizedString pathGoodUnlockedTexte;
    public LocalizedString pathGoodLockedTitle;
    public LocalizedString pathGoodLockedTexte;
    public LocalizedString pathFalseLockedTitle;
    public LocalizedString pathFalseLockedTexte;
    public LocalizedString pathFalseUnlockedTitre;
    public LocalizedString pathFalseUnlockedTexte;

    public LocalizedString blocs;
    public LocalizedString victoires;

    public LocalizedString dataHackeesTitle;
    public LocalizedString dataHackeesJamaisHackeesTexte;

    public LocalizedString palierTotalTexte;
    public LocalizedString palierNextPalierDernier;
    public LocalizedString palierNextPalierProchain;

    public LocalizedString traceDisplayer;

    public LocalizedString meilleurScoreTitre;
    public LocalizedString meilleurScoreTexte;

    public LocalizedString palierDebloqueTitre;
    public LocalizedString palierDebloqueTexte;
    public LocalizedString palierDebloqueUnPalier;

    public LocalizedString niveauDebloqueTitre;
    public LocalizedString niveauDebloqueTexte;

    public LocalizedString niveauVerouilleTitre;
    public LocalizedString niveauVerouilleTotalTexte;
    public LocalizedString niveauVerouilleUnSeulNiveau;
    public LocalizedString niveauVerouilleUnNiveauParmiPlusieurs;

    public LocalizedString tutoriel;
    public LocalizedString tutorielRecommandeTitre;
    public LocalizedString tutorielRecommandeTexte;

}
